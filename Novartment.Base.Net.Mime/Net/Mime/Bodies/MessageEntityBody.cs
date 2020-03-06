using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности, содержащее вложенное интернет-сообщение.
	/// </summary>
	public class MessageEntityBody :
		IEntityBody
	{
		private MailMessage _nestedMessage = null;

		/// <summary>
		/// Инициализирует новый экземпляр класса MessageEntityBody.
		/// </summary>
		public MessageEntityBody ()
		{
		}

		/// <summary>Получает кодировку передачи содержимого тела сущности.</summary>
		public ContentTransferEncoding TransferEncoding => _nestedMessage?.RequiredTransferEncoding.GetSuitableCompositeMediaTypeTransferEncoding () ??
			ContentTransferEncoding.Unspecified;

		/// <summary>
		/// Получает или устанавливает вложенное сообщение.
		/// </summary>
		public MailMessage Message
		{
			get => _nestedMessage;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException (nameof (value));
				}

				Contract.EndContractBlock ();

				_nestedMessage = value;
			}
		}

		/// <summary>
		/// Очищает тело сущности.
		/// </summary>
		public void Clear ()
		{
			_nestedMessage = null;
		}

		/// <summary>
		/// Загружает тело сущности из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных, содержащий тело сущности.</param>
		/// <param name="subBodyFactory">Фабрика, позволяющая создавать тело вложенных сущностей с указанными параметрами.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию загрузки.</returns>
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

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			this.Message = new MailMessage ();
			return this.Message.LoadAsync (source, subBodyFactory, Entity.DefaultType, Entity.DefaultSubtype, cancellationToken);
		}

		/// <summary>
		/// Saves this entity in the specified binary data destination.
		/// </summary>
		/// <param name="destination">The binary data destination, in which this entity will be saved.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken = default)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			if (this.Message == null)
			{
				throw new InvalidOperationException ("Required property 'Message' not specified.");
			}

			return this.Message.SaveAsync (destination, cancellationToken);
		}
	}
}
