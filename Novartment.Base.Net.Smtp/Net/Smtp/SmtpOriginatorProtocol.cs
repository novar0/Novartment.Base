using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Фабрика для создания транзакций, поддерживающих указанную кодировку передачи содержимого.
	/// </summary>
	/// <param name="contentTransferEncoding">Кодировка передачи содержимого, которую должна использовать создаваемая транзакция.</param>
	/// <returns>Вновь созданная транзакция для передачи содержимого в указанной кодировке.</returns>
	public delegate IMailDataTransferTransaction TransactionFactory (ContentTransferEncoding contentTransferEncoding);

	/// <summary>
	/// Протокол внесения/отправки почты по стандарту SMTP.
	/// </summary>
	/// <remarks>Соответствует роли 'Originator' из RFC 5321 часть 2.3.10.</remarks>
	public class SmtpOriginatorProtocol :
		ITcpConnectionProtocol
	{
		private readonly Func<TransactionFactory, CancellationToken, Task> _transactionOriginator;
		private readonly SmtpClientSecurityParameters _securityParameters;
		private readonly ILogWriter _logger;

		/// <summary>
		/// Инициализирует новый экземпляр SmtpOriginatorProtocol
		/// при старте вызывающий указанный исполнитель транзакций и
		/// записывающий происходящие события в указанный журнал.
		/// </summary>
		/// <param name="transactionOriginator">Исполнитель транзакций, который будет запущен после установки SMTP-подключения.</param>
		/// <param name="securityParameters">Параметры безопасности,
		/// устнавливающие использование шифрования и аутентификации при выполнении транзакций.</param>
		/// <param name="logger">Журнал для записи событий. Укажите null если запись не нужна.</param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public SmtpOriginatorProtocol (
			Func<TransactionFactory, CancellationToken, Task> transactionOriginator,
			SmtpClientSecurityParameters securityParameters,
			ILogWriter logger = null)
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

			return StartAsyncStateMachine (connection, credential, cancellationToken);
		}

		private async Task StartAsyncStateMachine (
			ITcpConnection connection,
			NetworkCredential credential,
			CancellationToken cancellationToken)
		{
			var sender = new TcpConnectionSmtpCommandTransport (connection, _logger);
			using (var session = new SmtpOriginatorProtocolSession (sender, connection.LocalEndPoint.HostName))
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
						requiredEncoding => new SmtpOriginatorDataTransferTransaction (
							session,
							requiredEncoding,
							_logger),
						cancellationToken).ConfigureAwait (false);

					await session.FinishAsync (CancellationToken.None).ConfigureAwait (false);
				}
				catch (OperationCanceledException)
				{
					_logger?.Warn (string.Format (
						"Canceling protocol with {0}.",
						connection.RemoteEndPoint));
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
						_logger?.Warn (string.Format (
							"Canceling protocol with {0}.",
							connection.RemoteEndPoint));
						cancellationToken.ThrowIfCancellationRequested ();
					}

					_logger?.Warn (string.Format (
						"Aborting protocol with {0}. {1}",
						connection.RemoteEndPoint,
						ExceptionDescriptionProvider.GetDescription (excpt)));
					throw;
				}
			}
		}
	}
}
