using System;
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
#if NETSTANDARD2_0
			this.Boundary = boundary ?? ("NextPart=_" + Guid.NewGuid ().ToString ().Replace ("-", string.Empty));
#else
			this.Boundary = boundary ?? ("NextPart=_" + Guid.NewGuid ().ToString ().Replace ("-", string.Empty, StringComparison.Ordinal));
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
					(ContentTransferEncoding)this.Parts.Max (item => (int)item.RequiredTransferEncoding);

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
		public async Task LoadAsync (
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

			this.Parts.Clear ();
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

		/// <summary>
		/// Saves this entity in the specified binary data destination.
		/// </summary>
		/// <param name="destination">The binary data destination, in which this entity will be saved.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public async Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken = default)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

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

		// Получает разграничитель частей в виде массива байтов.
		private byte[] GetNextBoundary ()
		{
			var nextBoundary = new byte[this.Boundary.Length + 6]; // "\r\n--" + Boundary + "\r\n"
			int pos = 0;
			nextBoundary[pos++] = HeaderDecoder.CarriageReturnLinefeed[0];
			nextBoundary[pos++] = HeaderDecoder.CarriageReturnLinefeed[1];
			nextBoundary[pos++] = (byte)'-';
			nextBoundary[pos++] = (byte)'-';
			AsciiCharSet.GetBytes (this.Boundary.AsSpan (), nextBoundary.AsSpan (pos));
			pos += this.Boundary.Length;
			nextBoundary[pos++] = HeaderDecoder.CarriageReturnLinefeed[0];
			nextBoundary[pos] = HeaderDecoder.CarriageReturnLinefeed[1];
			return nextBoundary;
		}

		// Получает разграничитель последней части в виде массива байтов.
		private byte[] GetEndBoundary (byte[] nextBoundary)
		{
			var endBoundary = new byte[this.Boundary.Length + 8]; // "\r\n--" + Boundary + "--\r\n"
			var pos = this.Boundary.Length + 4;
			Array.Copy (nextBoundary, endBoundary, pos);
			endBoundary[pos++] = (byte)'-';
			endBoundary[pos++] = (byte)'-';
			endBoundary[pos++] = HeaderDecoder.CarriageReturnLinefeed[0];
			endBoundary[pos] = HeaderDecoder.CarriageReturnLinefeed[1];
			return endBoundary;
		}
	}
}
