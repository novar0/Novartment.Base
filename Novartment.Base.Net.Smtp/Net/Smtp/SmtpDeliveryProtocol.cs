using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Протокол получения почты по стандарту SMTP.
	/// </summary>
	/// <remarks>Соответствует роли 'Delivery' из RFC 5321 часть 2.3.10.</remarks>
	public class SmtpDeliveryProtocol :
		ITcpConnectionProtocol
	{
		private readonly Func<MailDeliverySourceData, IMailDataTransferTransaction> _transactionFactory;
		private readonly SmtpServerSecurityParameters _securityParameters;
		private readonly ILogger _logger;

		/// <summary>
		/// Инициализирует новый экземпляр SmtpDeliveryProtocol
		/// создающий транзакции по передачи почты с помощью указанной фабрики и
		/// записывающий происходящие события в указанный журнал.
		/// </summary>
		/// <param name="transactionFactory">Функция, создающая транзакции по передаче почты для указанного источника доставки.</param>
		/// <param name="securityParameters">Параметры безопасности,
		/// устнавливающие использование шифрования и аутентификации при выполнении транзакций.</param>
		/// <param name="logger">Журнал для записи событий. Укажите null если запись не нужна.</param>
		public SmtpDeliveryProtocol (
			Func<MailDeliverySourceData, IMailDataTransferTransaction> transactionFactory,
			SmtpServerSecurityParameters securityParameters,
			ILogger<SmtpDeliveryProtocol> logger = null)
		{
			if (transactionFactory == null)
			{
				throw new ArgumentNullException (nameof (transactionFactory));
			}

			if (securityParameters == null)
			{
				throw new ArgumentNullException (nameof (securityParameters));
			}

			if ((securityParameters.ServerCertificate == null) &&
				(securityParameters.ClientCertificateRequired || (securityParameters.ClientAuthenticator != null)))
			{// проверять клиента можно только при наличии серверного сертификата
				throw new ArgumentOutOfRangeException (nameof (securityParameters));
			}

			Contract.EndContractBlock ();

			_transactionFactory = transactionFactory;
			_securityParameters = securityParameters;
			_logger = logger;
		}

		/// <summary>
		/// Запускает обработку указанного подключения.
		/// </summary>
		/// <param name="connection">TCP-подключение.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая обработку подключения.</returns>
		/// <exception cref="Novartment.Base.Net.UnrecoverableProtocolException">
		/// Происходит когда в протоколе возникло неустранимое противоречие, делающее его дальнейшую работу невозможным.
		/// Настоятельно рекомендуется закрыть соединение.
		/// </exception>
		public Task StartAsync (ITcpConnection connection, CancellationToken cancellationToken)
		{
			if (connection == null)
			{
				throw new ArgumentNullException (nameof (connection));
			}

			if (connection.LocalEndPoint?.HostName == null)
			{
				throw new ArgumentOutOfRangeException (nameof (connection), "Not valid connection.LocalEndPoint.HostName.");
			}

			// для команды нужно минимум 6 байт
			if (connection.Reader.Buffer.Length < 6)
			{
				throw new ArgumentOutOfRangeException (
					nameof (connection),
					FormattableString.Invariant (
						$"Aborting protocol because of insufficient connection.Reader.Buffer.Length ({connection.Reader.Buffer.Length}). Required minimum 6 bytes."));
			}

			Contract.EndContractBlock ();

			return StartAsyncStateMachine ();

			async Task StartAsyncStateMachine ()
			{
				var transport = new TcpConnectionSmtpCommandTransport (connection, _logger);
				using (var session = new SmtpDeliveryProtocolSession (
					transport,
					connection.RemoteEndPoint,
					_transactionFactory,
					connection.LocalEndPoint.HostName,
					_securityParameters,
					_logger))
				{
					if (cancellationToken.CanBeCanceled)
					{
						cancellationToken.ThrowIfCancellationRequested ();

						// на отмену регистрируем посылку прощального ответа
						cancellationToken.Register (session.Dispose, false);
					}

					await session.StartAsync (cancellationToken).ConfigureAwait (false);

					// запускаем цикл обработки команд
					while (true)
					{
						try
						{
							var continueProcesssing = await session.ReceiveCommandSendReplyAsync (cancellationToken).ConfigureAwait (false);
							if (!continueProcesssing)
							{
								break;
							}
						}
						catch (OperationCanceledException)
						{
							_logger?.LogWarning ($"Canceling protocol with {connection.RemoteEndPoint}.");
							throw;
						}
						catch (Exception excpt)
						{
							// Отдельно отслеживаем запрос отмены при ObjectDisposedException.
							// Такая комбинация означает отмену операции с объектом, не поддерживающим отмену отдельных операций.
							if (cancellationToken.IsCancellationRequested &&
								((excpt is ObjectDisposedException) ||
								((excpt is IOException) && (excpt.InnerException is ObjectDisposedException))))
							{
								_logger?.LogWarning ($"Canceling protocol with {connection.RemoteEndPoint}.");
								cancellationToken.ThrowIfCancellationRequested ();
							}

							_logger?.LogWarning (
								$"Aborting protocol with {connection.RemoteEndPoint}. {ExceptionDescriptionProvider.GetDescription (excpt)}");
							throw;
						}
					}
				}
			}
		}
	}
}
