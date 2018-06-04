using System;
using System.Diagnostics.Contracts;
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
		public static Task PerformTransferTransaction (
			this IMailMessage<AddrSpec> message,
			TransactionHandlerFactory transactionHandlerFactory,
#pragma warning disable CA1801 // Review unused parameters
			CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
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

			Contract.EndContractBlock ();

			var recipients = new ArrayList<AddrSpec> ();
			foreach (var recipient in message.Recipients)
			{
				recipients.Add (recipient);
			}

			return (recipients.Count > 0) ?
				OriginateTransactionStateMachine () :
				Task.CompletedTask;

			// Парралельно идут два асинхронных процесса:
			// 1. Запись данных в message.SaveAsync().
			// 3. Чтение этих же данных в transaction.TransferDataAndFinishAsync().
			// Посредником/медиатором служит канал BufferedChannel.
			async Task OriginateTransactionStateMachine ()
			{
				using (var transactionHandler = transactionHandlerFactory.Invoke (message.TransferEncoding))
				{
					var returnPath = (message.Originators.Count > 0) ? message.Originators[0] : null;
					await transactionHandler.StartAsync (returnPath, cancellationToken).ConfigureAwait (false);
					foreach (var recipient in recipients)
					{
						await transactionHandler.TryAddRecipientAsync (recipient, cancellationToken).ConfigureAwait (false);
					}

					var channel = new BufferedChannel (new byte[8192]); // TcpClient.SendBufferSize default value is 8192 bytes
					var writeTask = WriteToChannelAsync (message, channel, cancellationToken);
					var readTask = ReadFromChannelAsync (transactionHandler, channel, cancellationToken);
					await Task.WhenAll (writeTask, readTask).ConfigureAwait (false);
				}
			}
		}

		private static async Task WriteToChannelAsync (IBinarySerializable source, BufferedChannel channel, CancellationToken cancellationToken)
		{
			try
			{
				await source.SaveAsync (channel, cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// завершаем запись даже если чтение отменено или прервалось с исключением, иначе запись может оcтаться навечно заблокированной
				channel.SetComplete ();
			}
		}

		private static async Task ReadFromChannelAsync (IMailTransferTransactionHandler destination, BufferedChannel channel, CancellationToken cancellationToken)
		{
			try
			{
				await destination.TransferDataAndFinishAsync (channel, -1, cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// забираем записанные остатки даже если чтение отменено или прервалось с исключением,
				// иначе чтение может оcтаться навечно заблокированной
				await channel.SkipToEndAsync (CancellationToken.None).ConfigureAwait (false);
			}
		}
	}
}
