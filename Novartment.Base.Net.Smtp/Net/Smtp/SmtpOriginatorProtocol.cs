using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Фабрика для создания обработчиков транзакций, поддерживающих указанную кодировку передачи содержимого.
	/// </summary>
	/// <param name="contentTransferEncoding">Кодировка передачи содержимого, которую должна использовать создаваемая транзакция.</param>
	/// <returns>Вновь созданная транзакция для передачи содержимого в указанной кодировке.</returns>
	public delegate IMailTransferTransactionHandler TransactionHandlerFactory (ContentTransferEncoding contentTransferEncoding);

	/// <summary>
	/// Протокол внесения/отправки почты по стандарту SMTP.
	/// </summary>
	/// <remarks>Соответствует роли 'Originator' из RFC 5321 часть 2.3.10.</remarks>
	public class SmtpOriginatorProtocol :
		ITcpConnectionProtocol
	{
		private readonly Func<TransactionHandlerFactory, CancellationToken, Task> _transactionOriginator;
		private readonly SmtpClientSecurityParameters _securityParameters;
		private readonly ILogger _logger;

		/// <summary>
		/// Инициализирует новый экземпляр SmtpOriginatorProtocol с указанными параметрами безопасности
		/// при старте вызывающий указанный инициатор транзакций и
		/// записывающий происходящие события в указанный журнал.
		/// </summary>
		/// <param name="transactionOriginator">
		/// Инициатор транзакций, который будет запущен после установки SMTP-подключения.
		/// Инициатору будут переданы фабрика обработчиков транзакций и токен отмены.
		/// При запуске инициатор должен создать обработчик и вызывать его методы для каждого передаваемого сообщения.
		/// </param>
		/// <param name="securityParameters">Параметры безопасности,
		/// устнавливающие использование шифрования и аутентификации при выполнении транзакций.</param>
		/// <param name="logger">Журнал для записи событий. Укажите null если запись не нужна.</param>
		public SmtpOriginatorProtocol (
			Func<TransactionHandlerFactory, CancellationToken, Task> transactionOriginator,
			SmtpClientSecurityParameters securityParameters,
			ILogger<SmtpOriginatorProtocol> logger = null)
		{
			if (transactionOriginator == null)
			{
				throw new ArgumentNullException (nameof (transactionOriginator));
			}

			if (securityParameters == null)
			{
				throw new ArgumentNullException (nameof (securityParameters));
			}

			if (((securityParameters.ClientCertificates != null) || (securityParameters.ClientCredentials != null)) &&
				!securityParameters.EncryptionRequired)
			{
				throw new ArgumentOutOfRangeException (nameof (securityParameters));
			}

			Contract.EndContractBlock ();

			_transactionOriginator = transactionOriginator;
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
		/// Происходит когда в протоколе возникла неустранимое противоречие, делающее его дальнейшую работу невозможным.
		/// Настоятельно рекомендуется закрыть соединение.
		/// </exception>
		public Task StartAsync (ITcpConnection connection, CancellationToken cancellationToken)
		{
			if (connection == null)
			{
				throw new ArgumentNullException (nameof (connection));
			}

			if (connection.LocalEndPoint.HostName == null)
			{
				throw new ArgumentOutOfRangeException (nameof (connection));
			}

			Contract.EndContractBlock ();

			var credential = _securityParameters.ClientCredentials?.GetCredential (
					connection.RemoteEndPoint.HostName,
					connection.RemoteEndPoint.Port,
					"PLAIN");

			return StartAsyncStateMachine ();

			async Task StartAsyncStateMachine ()
			{
				var transport = new TcpConnectionSmtpCommandTransport (connection, _logger);
				using (var session = new SmtpOriginatorProtocolSession (transport, connection.LocalEndPoint.HostName))
				{
					try
					{
						await session.ReceiveGreetingAndStartAsync (cancellationToken).ConfigureAwait (false);

						if (_securityParameters.EncryptionRequired && !(connection is ITlsConnection))
						{
							await session.RestartWithTlsAsync (_securityParameters.ClientCertificates, cancellationToken).ConfigureAwait (false);
						}

						if (credential != null)
						{
							await session.AuthenticateAsync (credential, cancellationToken).ConfigureAwait (false);
						}

						// отрабатывает инициатор транзакций
						await _transactionOriginator.Invoke (
							requiredEncoding => new SmtpSessionMailTransferTransactionHandler (
								session,
								requiredEncoding,
								_logger),
							cancellationToken).ConfigureAwait (false);

						await session.FinishAsync (CancellationToken.None).ConfigureAwait (false);
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
