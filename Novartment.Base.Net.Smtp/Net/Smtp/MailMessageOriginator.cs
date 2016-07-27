using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Инициатор SMTP-транзакции по передаче одного почтового сообщения.
	/// </summary>
	public static class MailMessageOriginator
	{
		/// <summary>
		/// Асинхронно создаёт и выполняет транзакцию по передаче почтового сообщения.
		/// </summary>
		/// <param name="message">Сообщение для передачи.</param>
		/// <param name="transactionFactory">Фабрика для создания транзакций, поддерживающих указанную кодировку передачи содержимого.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая процесс выполнения транзакций.</returns>
		public static Task OriginateTransaction (
			this IMailMessage<AddrSpec> message,
			TransactionFactory transactionFactory,
			CancellationToken cancellationToken)
		{
			// TODO: предусмотреть отправку писем не подразумевающих ответ (уведомлений о доставке), то есть без указания returnPath
			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}
			if (transactionFactory == null)
			{
				throw new ArgumentNullException (nameof (transactionFactory));
			}
			Contract.EndContractBlock ();

			var recipients = new ArrayList<AddrSpec> ();
			foreach (var recipient in message.Recipients)
			{
				recipients.Add (recipient);
			}

			return (recipients.Count > 0) ?
				OriginateTransactionStateMachine (message, transactionFactory, recipients, cancellationToken) :
				Task.CompletedTask;
		}

		// Парралельно идут два асинхронных процесса:
		// 1. Запись данных в message.SaveAsync().
		// 3. Чтение этих же данных в transaction.TransferDataAndFinishAsync().
		// Посредником/медиатором служит канал BufferedChannel.
		private static async Task OriginateTransactionStateMachine (
			this IMailMessage<AddrSpec> message,
			TransactionFactory transactionFactory,
			ArrayList<AddrSpec> recipients,
			CancellationToken cancellationToken)
		{
			using (var transaction = transactionFactory.Invoke (message.TransferEncoding))
			{
				var returnPath = (message.Originators.Count > 0) ? message.Originators[0] : null;
				await transaction.StartAsync (returnPath, cancellationToken).ConfigureAwait (false);
				foreach (var recipient in recipients)
				{
					await transaction.TryAddRecipientAsync (recipient, cancellationToken).ConfigureAwait (false);
				}
				var channel = new BufferedChannel (new byte[8192]); // TcpClient.SendBufferSize default value is 8192 bytes
				var writeTask = WriteToChannelAsync (message, channel, cancellationToken);
				var readTask = ReadFromChannelAsync (transaction, channel, cancellationToken);
				await Task.WhenAll (writeTask, readTask).ConfigureAwait (false);
			}
		}

		private static async Task WriteToChannelAsync (IMailMessage<AddrSpec> source, BufferedChannel channel, CancellationToken cancellationToken)
		{
			try
			{
				await source.SaveAsync (channel, cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// завершаем запись даже если чтение отменено или прервалось с исключением, иначе чтение может оcтаться навечно заблокированным
				channel.SetComplete ();
			}
		}

		private static async Task ReadFromChannelAsync (IMailDataTransferTransaction destination, BufferedChannel channel, CancellationToken cancellationToken)
		{
			try
			{
				await destination.TransferDataAndFinishAsync (channel, -1, cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// забираем записанные остатки даже если запись отменена или прервалась с исключением, иначе запись может оcтаться навечно заблокированной
				await channel.SkipToEndAsync (CancellationToken.None).ConfigureAwait (false);
			}
		}
	}
}
