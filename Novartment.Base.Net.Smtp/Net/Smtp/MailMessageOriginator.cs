using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Инициатор SMTP-транзакции по передаче одного почтового сообщения.
	/// </summary>
	public static class MailMessageOriginator
	{
		/// <summary>
		/// Асинхронно создаёт и выполняет транзакцию по передаче почтового сообщения.
		/// Может использоваться как исполнитель транзакций при создании экземпляра <see cref="Novartment.Base.Net.Smtp.SmtpOriginatorProtocol"/>.
		/// </summary>
		/// <param name="message">Сообщение для передачи.</param>
		/// <param name="transactionHandlerFactory">Фабрика для создания обработчиков транзакций.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая процесс выполнения транзакций.</returns>
		public static async Task PerformTransferTransaction (
			this IMailMessage<AddrSpec> message,
			TransactionHandlerFactory transactionHandlerFactory,
			CancellationToken cancellationToken = default)
		{
			// TODO: предусмотреть отправку писем не подразумевающих ответ (уведомлений о доставке), то есть без указания returnPath
			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}

			if (transactionHandlerFactory == null)
			{
				throw new ArgumentNullException (nameof (transactionHandlerFactory));
			}

			if (message.Recipients.Count < 1)
			{
				throw new InvalidOperationException ("No recipients specified.");
			}

			// Парралельно идут два асинхронных процесса:
			// 1. Запись данных в message.SaveAsync().
			// 3. Чтение этих же данных в transaction.TransferDataAndFinishAsync().
			// Посредником/медиатором служит канал BufferedChannel.
			using var transactionHandler = transactionHandlerFactory.Invoke (message.RequiredTransferEncoding);
			var returnPath = (message.Originators.Count > 0) ? message.Originators[0] : null;
			await transactionHandler.StartAsync (returnPath, cancellationToken).ConfigureAwait (false);
			foreach (var recipient in message.Recipients)
			{
				await transactionHandler.TryAddRecipientAsync (recipient, cancellationToken).ConfigureAwait (false);
			}

			var channel = new BufferedChannel (new byte[8192]); // TcpClient.SendBufferSize default value is 8192 bytes
			var writeTask = SaveSerializableEntityToDestinationAsync (message, channel, cancellationToken);
			var readTask = TransferSourceToTransactionAsync (transactionHandler, channel, cancellationToken);
			await Task.WhenAll (writeTask, readTask).ConfigureAwait (false);
		}

		private static async Task SaveSerializableEntityToDestinationAsync (IBinarySerializable entity, IBinaryDestination destination, CancellationToken cancellationToken)
		{
			try
			{
				await entity.SaveAsync (destination, cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// завершаем запись даже если она отменена или прервалась с исключением, иначе позднее чтение может оcтаться навечно заблокированным
				destination.SetComplete ();
			}
		}

		private static async Task TransferSourceToTransactionAsync (IMailTransferTransactionHandler transaction, IBufferedSource source, CancellationToken cancellationToken)
		{
			try
			{
				await transaction.TransferDataAndFinishAsync (source, -1, cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// забираем остатки из источника даже если передача их в транзакцию отменена или прервалась с исключением,
				// иначе позднее запись может оcтаться навечно заблокированной
				await source.SkipToEndAsync (default).ConfigureAwait (false);
			}
		}
	}
}
