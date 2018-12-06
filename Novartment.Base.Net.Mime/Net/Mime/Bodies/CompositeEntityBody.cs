using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Text;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности, содержащее список частей (вложенных сущностей),
	/// автоматически вычисляющее кодировку передачи содержимого,
	/// подходящую для всех частей.
	/// </summary>
	public class CompositeEntityBody :
		ICompositeEntityBody
	{
		private readonly ContentMediaType _defaultPartsMediaType; // медиа-тип по умолчанию для вложенных сущностей
		private readonly string _defaultPartsMediaSubtype; // медиа-подтип по умолчанию для вложенных сущностей

		/// <summary>
		/// Инициализирует новый экземпляр класса CompositeEntityBody с указанным разграничителем частей.
		/// </summary>
		/// <param name="boundary">Разграничитель частей сущности согласно требованиям RFC 1341 часть 7.2.1,
		/// либо null для автоматического генерирования разграничителя.</param>
		public CompositeEntityBody (string boundary = null)
			: this (Entity.DefaultType, Entity.DefaultSubtype, boundary)
		{
			// RFC 2046 часть 5.1 'Multipart Media Type'
			// the absence of a Content-Type header usually indicates that the corresponding body has a content-type of "text/plain; charset=US-ASCII".
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса CompositeEntityBody
		/// с указанными медиа типом по-умолчанию для вложенных сущностей и разграничителем частей.
		/// </summary>
		/// <param name="defaultPartsMediaType">Медиа тип по-умолчанию для вложенных сущностей.</param>
		/// <param name="defaultPartsMediaSubtype">Медиа подтип по-умолчанию для вложенных сущностей.</param>
		/// <param name="boundary">Разграничитель частей сущности согласно требованиям RFC 1341 часть 7.2.1,
		/// либо null для автоматического генерирования разграничителя.</param>
		protected CompositeEntityBody (
			ContentMediaType defaultPartsMediaType,
			string defaultPartsMediaSubtype,
			string boundary)
		{
			_defaultPartsMediaType = defaultPartsMediaType;
			_defaultPartsMediaSubtype = defaultPartsMediaSubtype;

			// Уникальный разграничитель согласно требованиям RFC 1341 часть 7.2.1.</returns>
			// boundary := 0*69<bchars> bcharsnospace
			// bchars := bcharsnospace / " "
			// bcharsnospace := DIGIT / ALPHA / "'" / "(" / ")" / "+" / "_" / "," / "-" / "." / "/" / ":" / "=" / "?"
#if NETCOREAPP2_2
			this.Boundary = boundary ?? ("NextPart=_" + Guid.NewGuid ().ToString ().Replace ("-", string.Empty, StringComparison.Ordinal));
#else
			this.Boundary = boundary ?? ("NextPart=_" + Guid.NewGuid ().ToString ().Replace ("-", string.Empty));
#endif
		}

		/// <summary>Получает коллекцию частей (дочерних сущностей), которые содержатся в теле сущности.</summary>
		public IAdjustableList<Entity> Parts { get; } = new ArrayList<Entity> ();

		/// <summary>Получает кодировку передачи содержимого тела сущности.</summary>
		public ContentTransferEncoding TransferEncoding
		{
			get
			{
				var maxPartsTransferEncoding = (this.Parts.Count < 1) ?
					ContentTransferEncoding.Unspecified :
					(ContentTransferEncoding)this.Parts.Max (item => (int)item.TransferEncoding);

				return maxPartsTransferEncoding.GetSuitableCompositeMediaTypeTransferEncoding ();
			}
		}

		/// <summary>Получает разграничитель, разделяющий на части сущность с множественным содержимым.</summary>
		public string Boundary { get; }

		/// <summary>
		/// Очищает тело сущности.
		/// </summary>
		public void Clear ()
		{
			this.Parts.Clear ();
		}

		/// <summary>
		/// Загружает тело сущности из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных, содержащий тело сущности.</param>
		/// <param name="subBodyFactory">Фабрика, позволяющая создавать тело вложенных сущностей с указанными параметрами.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public Task LoadAsync (
			IBufferedSource source,
			Func<EssentialContentProperties, IEntityBody> subBodyFactory,
			CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (subBodyFactory == null)
			{
				throw new ArgumentNullException (nameof (subBodyFactory));
			}

			Contract.EndContractBlock ();

			this.Parts.Clear ();

			return LoadAsyncStateMachine ();

			async Task LoadAsyncStateMachine ()
			{
				var bodyPartsSource = new BodyPartSource (this.Boundary, source);
				while (await bodyPartsSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false))
				{
					var entity = new Entity ();
					await entity.LoadAsync (
						bodyPartsSource,
						subBodyFactory,
						_defaultPartsMediaType,
						_defaultPartsMediaSubtype,
						cancellationToken).ConfigureAwait (false);
					this.Parts.Add (entity);
					if (bodyPartsSource.LastBoundaryClosed)
					{
						await bodyPartsSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Сохраняет тело сущности в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="destination">Получатель двоичных данных, в который будет сохранено тело сущности.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию сохранения.</returns>
		public Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken = default)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			return SaveAsyncStateMachine ();

			async Task SaveAsyncStateMachine ()
			{
				var nextBoundary = GetNextBoundary ();
				var endBoundary = GetEndBoundary (nextBoundary);

				foreach (var entity in this.Parts)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					await destination.WriteAsync (nextBoundary, cancellationToken).ConfigureAwait (false);
					await entity.SaveAsync (destination, cancellationToken).ConfigureAwait (false);
				}

				await destination.WriteAsync (endBoundary, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Получает разграничитель частей в виде массива байтов.
		/// </summary>
		/// <returns>Разграничитель частей в виде массива байтов.</returns>
		private byte[] GetNextBoundary ()
		{
			var nextBoundary = new byte[this.Boundary.Length + 6]; // "\r\n--" + Boundary + "\r\n"
			int size = 0;
			nextBoundary[size++] = HeaderDecoder.CarriageReturnLinefeed[0];
			nextBoundary[size++] = HeaderDecoder.CarriageReturnLinefeed[1];
			nextBoundary[size++] = (byte)'-';
			nextBoundary[size++] = (byte)'-';
			AsciiCharSet.GetBytes (this.Boundary.AsSpan (), nextBoundary.AsSpan (size));
			size += this.Boundary.Length;
			nextBoundary[size++] = HeaderDecoder.CarriageReturnLinefeed[0];
			nextBoundary[size] = HeaderDecoder.CarriageReturnLinefeed[1];
			return nextBoundary;
		}

		// Получает разграничитель последней части в виде массива байтов.
		private byte[] GetEndBoundary (byte[] nextBoundary)
		{
			var endBoundary = new byte[this.Boundary.Length + 8]; // "\r\n--" + Boundary + "--\r\n"
			var size = this.Boundary.Length + 4;
			Array.Copy (nextBoundary, endBoundary, size);
			endBoundary[size++] = (byte)'-';
			endBoundary[size++] = (byte)'-';
			endBoundary[size++] = HeaderDecoder.CarriageReturnLinefeed[0];
			endBoundary[size] = HeaderDecoder.CarriageReturnLinefeed[1];
			return endBoundary;
		}
	}
}
