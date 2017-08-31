using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpDeliveryProtocolSession :
		IDisposable
	{
		private static readonly string[] _SupportedExtensions =
		{
			"PIPELINING", // RFC 2920
			"8BITMIME", // RFC 6152
			"BINARYMIME", "CHUNKING", // RFC 3030
		};

		private readonly ISmtpCommandTransport _transport;
		private readonly IPHostEndPoint _remoteEndPoint;
		private readonly Func<MailDeliverySourceData, IMailDataTransferTransaction> _currentTransactionFactory;
		private readonly string _hostFqdn;
		private readonly SmtpServerSecurityParameters _securityParameters;
		private readonly ILogger _logger;

		private bool _clientIdentified = false;
		private IMailDataTransferTransaction _currentTransaction = null;
		private int _currentTransactionAcceptedRecipients = 0;
		private ContentTransferEncoding _currentTransactionRequestedEncoding = ContentTransferEncoding.SevenBit;
		private SmtpCommand.ExpectedInputType _expectedInput = SmtpCommand.ExpectedInputType.Command;
		private JobAgency<IBufferedSource, int> _chunkingAgency = null;
		private AggregatingBufferedSource _chunksBufferedSource = null;
		private Task _chunkingDataTransferTask = null;
		private int _completed = 0;
		private object _authenticatedUser = null;

		internal SmtpDeliveryProtocolSession (
			ISmtpCommandTransport transport,
			IPHostEndPoint remoteEndPoint,
			Func<MailDeliverySourceData, IMailDataTransferTransaction> transactionFactory,
			string hostFqdn,
			SmtpServerSecurityParameters securityParameters,
			ILogger logger)
		{
			_transport = transport;
			_remoteEndPoint = remoteEndPoint;
			_currentTransactionFactory = transactionFactory;
			_hostFqdn = hostFqdn;
			_securityParameters = securityParameters;
			_logger = logger;
		}

		internal IMailDataTransferTransaction CurrentTransaction => _currentTransaction;

		public void Dispose ()
		{
			Interlocked.Exchange (ref _currentTransaction, null)?.Dispose ();
			var oldValue = Interlocked.Exchange (ref _completed, 1);
			if (oldValue == 0)
			{
				// RFC 5321 part 3.8:
				// An SMTP server that is forcibly shut down via external means
				// SHOULD attempt to send a line containing a 421 response code to the SMTP client before exiting.
				// посылка прощального ответа необязательна, поэтому игнорируем исключения если связи уже нет
				try
				{
					_transport.SendReplyAsync (SmtpReply.ServiceNotAvailable, false, CancellationToken.None).GetAwaiter ().GetResult ();
				}
				catch (IOException)
				{
				}
				catch (ObjectDisposedException)
				{
				}
				catch (InvalidOperationException)
				{
				}
				catch (SocketException)
				{
				}
			}
		}

		internal Task StartAsync (CancellationToken cancellationToken)
		{
			// посылаем приветствие в виде названия и версии сервера
			// TODO: сделать конфигурируемым имя, используемое в качестве приветствия
			var assembly = this.GetType ().GetTypeInfo ().Assembly.GetName ();
			var reply = SmtpReply.CreateServiceReady (assembly.Name, assembly.Version);
			return _transport.SendReplyAsync (reply, false, cancellationToken);
		}

		internal async Task<bool> ReceiveCommandSendReplyAsync (CancellationToken cancellationToken)
		{
			var command = await _transport.ReceiveCommandAsync (_expectedInput, cancellationToken).ConfigureAwait (false);

			SmtpReply reply;
			bool canBeGrouped = false;
			try
			{
				var data = await ProcessCommandAsync (command, cancellationToken).ConfigureAwait (false);
				reply = data.Reply;
				canBeGrouped = data.CanBeGrouped;
			}
			catch (InvalidCredentialException)
			{
				_logger?.LogWarning ("User authenticator failed to check supplied credentials.");
				reply = SmtpReply.AuthenticationCredentialsInvalid;
			}
			catch (UnacceptableSmtpMailboxException excpt)
			{
				// 553  Requested action not taken: mailbox name not allowed
				_logger?.LogWarning ("Not acceptable mailbox " + excpt.Mailbox.ToAngleString ());
				reply = SmtpReply.MailboxNotAllowed;
			}
			catch (BadSequenceOfSmtpCommandsException)
			{
				// 503  Bad sequence of commands
				reply = SmtpReply.BadSequenceOfCommands;
			}
			catch (NoValidRecipientsException)
			{
				// 554 No valid recipients
				_logger?.LogWarning ("No accepted recipients");
				reply = SmtpReply.NoValidRecipients;
			}
			catch (Exception excpt) when (!(
				(excpt is OperationCanceledException) ||
				(excpt is UnrecoverableProtocolException)))
			{ // 451  Requested action aborted: local error in processing
				_logger?.LogError ("Exception processing command. " + excpt.Message);
				reply = SmtpReply.LocalError;
			}

			var continueProcesssing = command.CommandType != SmtpCommandType.Quit;
			if (!continueProcesssing)
			{
				var oldValue = Interlocked.Exchange (ref _completed, 1);
				if (oldValue != 0)
				{
					// ответ о прекращении общения уже послан
					return false;
				}
			}

			await _transport.SendReplyAsync (reply, canBeGrouped, cancellationToken).ConfigureAwait (false);

			if (reply == SmtpReply.ReadyToStartTls)
			{
				await _transport
					.StartTlsServerAsync (_securityParameters.ServerCertificate, _securityParameters.ClientCertificateRequired)
					.ConfigureAwait (false);

				// RFC 3207 part 4.2:
				// Upon completion of the TLS handshake, the SMTP protocol is reset to the initial state
				// (the state in SMTP after a server issues a 220 service ready greeting).
				// The server MUST discard any knowledge obtained from the client, such as the argument to the EHLO command,
				// which was not obtained from the TLS negotiation itself.
				ResetTransaction ();
				_clientIdentified = false;
				_remoteEndPoint.HostName = null;
				return true;
			}

			if (reply.Code == 334)
			{
				// server challenge
			}

			return continueProcesssing;
		}

		// Обрабатывает команду, вызывая нужные методы session и возвращая ответ, который нужно отправить клиенту.
		// Не обрабатывает никаких исключений, их нужно ловить снаружи.
		private Task<SmtpReplyWithGroupingMark> ProcessCommandAsync (SmtpCommand command, CancellationToken cancellationToken)
		{
			if (command is SmtpUnknownCommand)
			{
				_logger?.LogWarning ("Unknown command. " + ((SmtpUnknownCommand)command).Message);
				return Task.FromResult (SmtpReply.NotImplemented.DisallowGrouping ());
			}

			if (command is SmtpTooLongCommand)
			{
				_logger?.LogWarning ("Line with command too long.");
				return Task.FromResult (SmtpReply.LineTooLong.DisallowGrouping ());
			}

			if (command is SmtpInvalidSyntaxCommand)
			{
				_logger?.LogWarning ("Invalid syntax in command. " + ((SmtpInvalidSyntaxCommand)command).Message);
				if (command.CommandType == SmtpCommandType.Bdat)
				{
					ResetTransaction ();

					// клиент будет слать данные не дожидаясь ответа об ошибке, а мы не знаем сколько будет этих данных
					throw new UnrecoverableProtocolException (((SmtpInvalidSyntaxCommand)command).Message);
				}

				return Task.FromResult (SmtpReply.SyntaxErrorInParameter.DisallowGrouping ());
			}

			// TODO: SHOULD issue response text that indicates, either implicitly or explicitly, what command the response matches.
			switch (command.CommandType)
			{
				case SmtpCommandType.Helo:
					return Task.FromResult (ProcessCommandHelo ((SmtpHeloCommand)command));
				case SmtpCommandType.Ehlo:
					return Task.FromResult (ProcessCommandEhlo ((SmtpEhloCommand)command));
				case SmtpCommandType.MailFrom:
					return ProcessCommandMailFrom ((SmtpMailFromCommand)command, cancellationToken);
				case SmtpCommandType.RcptTo:
					return ProcessCommandRcptTo ((SmtpRcptToCommand)command, cancellationToken);
				case SmtpCommandType.Data:
					return Task.FromResult (ProcessCommandData ());
				case SmtpCommandType.Bdat:
					return ProcessCommandBdat ((SmtpBdatCommand)command, cancellationToken);
				case SmtpCommandType.Rset:
					return Task.FromResult (ProcessCommandRset ());
				case SmtpCommandType.Vrfy:
					return Task.FromResult (ProcessCommandVrfy ());
				case SmtpCommandType.Noop:
					return Task.FromResult (ProcessCommandNoop ());
				case SmtpCommandType.Quit:
					return Task.FromResult (ProcessCommandQuit ());
				case SmtpCommandType.ActualData:
					return ProcessCommandActualData ((SmtpActualDataCommand)command, cancellationToken);
				case SmtpCommandType.StartTls:
					return Task.FromResult (ProcessCommandStartTls ());
				case SmtpCommandType.Auth:
					return ProcessCommandAuth ((SmtpAuthCommand)command);
				case SmtpCommandType.SaslResponse:
					return ProcessCommandSaslResponse ((SmtpSaslResponseCommand)command);
				default:
					return Task.FromResult (SmtpReply.NotImplemented.DisallowGrouping ());
			}
		}

		private SmtpReplyWithGroupingMark ProcessCommandRset ()
		{
			ResetTransaction ();

			// RFC 2920:
			// RSET, MAIL FROM, RCPT TO can all appear anywhere in a pipelined command group
			return SmtpReply.OK.AllowGrouping ();
		}

		private SmtpReplyWithGroupingMark ProcessCommandVrfy ()
		{
			return SmtpReply.CannotVerifyUser.DisallowGrouping ();
		}

		private SmtpReplyWithGroupingMark ProcessCommandNoop ()
		{
			return SmtpReply.OK.DisallowGrouping ();
		}

		private SmtpReplyWithGroupingMark ProcessCommandQuit ()
		{
			return SmtpReply.Disconnect.DisallowGrouping ();
		}

		private SmtpReplyWithGroupingMark ProcessCommandHelo (SmtpHeloCommand command)
		{
			_clientIdentified = true;
			_remoteEndPoint.HostName = command.ClientIdentification;
			ResetTransaction ();
			return SmtpReply.OK.DisallowGrouping ();
		}

		private SmtpReplyWithGroupingMark ProcessCommandEhlo (SmtpEhloCommand command)
		{
			_clientIdentified = true;
			_remoteEndPoint.HostName = command.ClientIdentification;
			ResetTransaction ();
			var supportedExtensions = new ArrayList<string> (_SupportedExtensions);

			// RFC 3207 part 4.2:
			// A server MUST NOT return the STARTTLS extension in response to an EHLO command received after a TLS handshake has completed.
			if ((_securityParameters.ServerCertificate != null) && !_transport.TlsEstablished)
			{
				supportedExtensions.Add ("STARTTLS");
			}

			// разрешаем аутентификацию только если соединение зашифровано и предоставлен аутентификатор
			if (_transport.TlsEstablished && (_securityParameters.ClientAuthenticator != null))
			{
				supportedExtensions.Add ("AUTH PLAIN");
			}

			return SmtpReply.CreateHelloResponse (_hostFqdn, supportedExtensions).DisallowGrouping ();
		}

		private SmtpReplyWithGroupingMark ProcessCommandStartTls ()
		{
			return ((_securityParameters.ServerCertificate == null) ?
						SmtpReply.UnableToInitializeSecurity :
						_transport.TlsEstablished ? // TLS уже запущен
							SmtpReply.TlsNotAvailable :
							SmtpReply.ReadyToStartTls)
					.DisallowGrouping ();
		}

		private async Task<SmtpReplyWithGroupingMark> ProcessCommandAuth (SmtpAuthCommand authCommand)
		{
			if (_authenticatedUser != null)
			{
				// уже аутентифицировались, повтор невозможен
				// After a successful AUTH command completes, a server MUST reject any further AUTH commands with a 503 reply.
				throw new BadSequenceOfSmtpCommandsException (); // транзакция не начата
			}

			if (_securityParameters.ClientAuthenticator == null)
			{
				return SmtpReply.NotImplemented.DisallowGrouping ();
			}

			// список см. http://www.iana.org/assignments/sasl-mechanisms/sasl-mechanisms.xhtml
			if (!"PLAIN".Equals (authCommand.Mechanism, StringComparison.OrdinalIgnoreCase))
			{
				return SmtpReply.UnrecognizedAuthenticationType.DisallowGrouping ();
			}

			if (!_transport.TlsEstablished)
			{
				// TLS не запущен
				// RFC 4954 part 4:
				// A server implementation MUST implement a configuration in which
				// it does NOT permit any plaintext password mechanisms,
				// unless either the STARTTLS [SMTP-TLS] command has been negotiated ...
				return SmtpReply.EncryptionRequiredForAuthentication.AllowGrouping ();
			}

			// RFC 4954 part 4:
			// When the AUTH command is used together with the [PIPELINING] extension,
			// it MUST be the last command in a pipelined group of commands.
			// The only exception to this rule is when the AUTH command contains an initial response
			// for a SASL mechanism that allows the client to send data first,
			// the SASL mechanism is known to complete in one round-trip,
			// and a security layer is not negotiated by the client.
			// Two examples of such SASL mechanisms are PLAIN [PLAIN] and EXTERNAL [SASL].
			if (authCommand.InitialResponse == null)
			{
				_expectedInput = SmtpCommand.ExpectedInputType.AuthenticationResponse;
				return SmtpReply.CreateSaslChallenge (null).DisallowGrouping ();
			}
			else
			{
				_authenticatedUser = await AuthenticateUserAsync (authCommand.InitialResponse).ConfigureAwait (false);
				if (_authenticatedUser == null)
				{
					throw new InvalidCredentialException ();
				}

				return SmtpReply.AuthenticationSucceeded.AllowGrouping ();
			}
		}

		private async Task<SmtpReplyWithGroupingMark> ProcessCommandSaslResponse (SmtpSaslResponseCommand authResponseCommand)
		{
			_expectedInput = SmtpCommand.ExpectedInputType.Command;
			if (authResponseCommand.IsCancelRequest)
			{
				return SmtpReply.SyntaxErrorInParameter.DisallowGrouping ();
			}

			_authenticatedUser = await AuthenticateUserAsync (authResponseCommand.Response).ConfigureAwait (false);
			if (_authenticatedUser == null)
			{
				throw new InvalidCredentialException ();
			}

			return SmtpReply.AuthenticationSucceeded.DisallowGrouping ();
		}

		private Task<SmtpReplyWithGroupingMark> ProcessCommandMailFrom (SmtpMailFromCommand mailFromCommand, CancellationToken cancellationToken)
		{
			if ((!_clientIdentified) || (_currentTransaction != null))
			{
				// не поздоровались или транзакция уже начата
				throw new BadSequenceOfSmtpCommandsException ();
			}

			if ((_securityParameters.ServerCertificate != null) && !_transport.TlsEstablished)
			{
				// требуется шифрование, а соединение не TLS
				return Task.FromResult (SmtpReply.MustUseStartTlsFirst.AllowGrouping ());
			}

			if ((_securityParameters.ClientAuthenticator != null) && (_authenticatedUser == null))
			{
				// требуется аутентификация, а клиент не аутентифицировался
				return Task.FromResult (SmtpReply.AuthenticationRequired.AllowGrouping ());
			}

			ResetTransaction ();
			var newTransaction = _currentTransactionFactory.Invoke (new MailDeliverySourceData (
				_remoteEndPoint,
				_transport.RemoteCertificate,
				_authenticatedUser));
			Task task;
			try
			{
				// тут может возникнуть UnacceptableSmtpMailboxException или другое исключение
				task = newTransaction.StartAsync (mailFromCommand.ReturnPath, cancellationToken);

				return ProcessCommandMailFromFinalizer ();
			}
			catch
			{
				newTransaction.Dispose ();
				throw;
			}

			async Task<SmtpReplyWithGroupingMark> ProcessCommandMailFromFinalizer ()
			{
				try
				{
					await task.ConfigureAwait (false);
				}
				catch
				{
					newTransaction.Dispose ();
					throw;
				}

				_currentTransaction = newTransaction;
				_currentTransactionRequestedEncoding = mailFromCommand.RequestedContentTransferEncoding;

				// RFC 2920:
				// RSET, MAIL FROM, RCPT TO can all appear anywhere in a pipelined command group
				return SmtpReply.OK.AllowGrouping ();
			}
		}

		private async Task<SmtpReplyWithGroupingMark> ProcessCommandRcptTo (SmtpRcptToCommand rcptToCommand, CancellationToken cancellationToken)
		{
			if ((!_clientIdentified) || (_currentTransaction == null) || (_chunkingAgency != null))
			{
				// не поздоровались / уже начат режим передачи (порциями) / транзакция не начата
				throw new BadSequenceOfSmtpCommandsException ();
			}

			var result = await _currentTransaction.TryAddRecipientAsync (rcptToCommand.Recipient, cancellationToken).ConfigureAwait (false);
			switch (result)
			{
				case RecipientAcceptanceState.Success:
					_currentTransactionAcceptedRecipients++;
					return SmtpReply.OK.AllowGrouping ();
				case RecipientAcceptanceState.FailureTooManyRecipients:
					return SmtpReply.TooManyRecipients.AllowGrouping ();
				case RecipientAcceptanceState.FailureMailboxTemporarilyUnavailable:
					return SmtpReply.MailboxTemporarilyUnavailable.AllowGrouping ();
				default:
					return SmtpReply.MailboxUnavailable.AllowGrouping ();
			}
		}

		private SmtpReplyWithGroupingMark ProcessCommandData ()
		{
			// RFC 3030 part 2:
			// DATA and BDAT commands cannot be used in the same transaction.
			// If a DATA statement is issued after a BDAT for the current transaction,
			// a 503 "Bad sequence of commands" MUST be issued

			// RFC 3030 part 3:
			// BINARYMIME cannot be used with the DATA command.
			// If a DATA command is issued after a MAIL command containing the body-value of "BINARYMIME",
			// a 503 "Bad sequence of commands" response MUST be sent.
			if ((!_clientIdentified) ||
				(_currentTransaction == null) ||
				(_chunkingAgency != null) ||
				(_currentTransactionRequestedEncoding == ContentTransferEncoding.Binary))
			{
				// не поздоровались / транзакция не начата / уже начат другой режим передачи (порциями) / режим BINARYMIME не может использоваться
				throw new BadSequenceOfSmtpCommandsException ();
			}

			if (_currentTransactionAcceptedRecipients <= 0)
			{
				ResetTransaction ();
				throw new NoValidRecipientsException (); // не указаны получатели
			}

			_expectedInput = SmtpCommand.ExpectedInputType.Data;
			return SmtpReply.DataStart.DisallowGrouping ();
		}

		private async Task<SmtpReplyWithGroupingMark> ProcessCommandActualData (SmtpActualDataCommand actualDataCommand, CancellationToken cancellationToken)
		{
			try
			{
				try
				{
					await _currentTransaction.TransferDataAndFinishAsync (actualDataCommand.Source, -1, cancellationToken).ConfigureAwait (false);
				}
				catch
				{
					// мы дожны забрать все данные В ЛЮБОМ СЛУЧАЕ, иначе нарушится диалог
					// поэтому перехватываем ВСЕ исключения кроме отмены,
					// забираем все данные и только потом вновь бросаем исключение
					await actualDataCommand.Source.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
					throw;
				}

				// пропускаем остатки данных и разделитель
				var partSkipped = await actualDataCommand.Source.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
				if (!partSkipped)
				{
					throw new NotEnoughDataException ("Incoming data exhausted before DATA end-marker.", 5L);
				}
			}
			finally
			{
				ResetTransaction ();
			}

			// RFC 2920:
			// The actual transfer of message content is explicitly allowed to be the first "command" in a group.
			return SmtpReply.OK.AllowGrouping ();
		}

		private Task<SmtpReplyWithGroupingMark> ProcessCommandBdat (SmtpBdatCommand bdatCommand, CancellationToken cancellationToken)
		{
			if (!_clientIdentified || (_currentTransaction == null))
			{
				// не поздоровались или не начали транзакцию
				return bdatCommand.Source
					.SkipToEndAsync (cancellationToken) // пропускаем все данные (предотвратить их передачу невозможно)
					.ContinueWith<SmtpReplyWithGroupingMark> (
						notUsed => { throw new BadSequenceOfSmtpCommandsException (); },
						CancellationToken.None,
						TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously,
						TaskScheduler.Default);
			}

			if (_currentTransactionAcceptedRecipients <= 0)
			{
				// не указаны получатели
				ResetTransaction ();
				return bdatCommand.Source
					.SkipToEndAsync (cancellationToken) // пропускаем все данные (предотвратить их передачу невозможно)
					.ContinueWith<SmtpReplyWithGroupingMark> (
						notUsed => { throw new NoValidRecipientsException (); },
						CancellationToken.None,
						TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously,
						TaskScheduler.Default);
			}

			if (_chunkingAgency == null)
			{
				// порция - первая
				if (bdatCommand.IsLast)
				{
					// единственная порция
					return ProcessCommandBdatSingleChunk (bdatCommand, cancellationToken);
				}

				// порция не последняя, поэтому создаём поставщика порций
				_chunkingAgency = new JobAgency<IBufferedSource, int> ();
				_chunksBufferedSource = new AggregatingBufferedSource (
					new byte[bdatCommand.Source.Buffer.Length],
					_chunkingAgency);
				try
				{
					_chunkingDataTransferTask = _currentTransaction.TransferDataAndFinishAsync (
						_chunksBufferedSource,
						bdatCommand.Size,
						cancellationToken);
				}
				catch
				{
					ResetTransaction ();
					throw;
				}
			}

			return ProcessCommandBdatNextChunk (bdatCommand, cancellationToken);
		}

		private async Task<SmtpReplyWithGroupingMark> ProcessCommandBdatSingleChunk (SmtpBdatCommand bdatCommand, CancellationToken cancellationToken)
		{
			try
			{
				await _currentTransaction.TransferDataAndFinishAsync (bdatCommand.Source, bdatCommand.Size, cancellationToken).ConfigureAwait (false);
				return SmtpReply.OK.AllowGrouping ();
			}
			finally
			{
				ResetTransaction ();
			}
		}

		private async Task<SmtpReplyWithGroupingMark> ProcessCommandBdatNextChunk (SmtpBdatCommand bdatCommand, CancellationToken cancellationToken)
		{
			if (!bdatCommand.Source.IsExhausted || (bdatCommand.Source.Count > 0))
			{
				// если порция не пустая, то ожидаем пока поставщик порций обработает её
				var chunkCompletionTask = OfferChunkCheckExhaustedAsync (bdatCommand.Source, cancellationToken);

				// тут надо ждать обе задачи - поставку и потребление,
				// потому что успешно завершена будет поставка,
				// а в случае отмены будет отменено потребление
				var tcs = new TaskCompletionSource<int> ();
				var finalizer1 = new TwoTaskFinalizer (tcs, _chunkingDataTransferTask);
				var finalizer2 = new TwoTaskFinalizer (tcs, chunkCompletionTask);
				var notUsed1 = chunkCompletionTask.ContinueWith (
					finalizer1.TaskContinuation,
					CancellationToken.None,
					TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
				var notUsed2 = _chunkingDataTransferTask.ContinueWith (
					finalizer2.TaskContinuation,
					CancellationToken.None,
					TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
				try
				{
					await tcs.Task.ConfigureAwait (false);
				}
				catch
				{
					ResetTransaction ();
					throw;
				}
			}

			if (bdatCommand.IsLast)
			{
				// порция последняя, поэтому ожидаем и обработку последней порции, и общее потребление
				try
				{
					await Task.WhenAll (
						_chunkingAgency.PutMarker (),
						_chunkingDataTransferTask)
						.ConfigureAwait (false);
				}
				finally
				{
					ResetTransaction ();
				}
			}

			return SmtpReply.OK.AllowGrouping ();
		}

		private Task<object> AuthenticateUserAsync (byte[] userPasswordData)
		{
			if (_securityParameters.ClientAuthenticator == null)
			{
				return Task.FromResult<object> (null);
			}

			// message   = [authzid] UTF8NUL authcid UTF8NUL passwd
			// authcid   = 1*SAFE ; MUST accept up to 255 octets
			// authzid   = 1*SAFE ; MUST accept up to 255 octets
			// passwd    = 1*SAFE ; MUST accept up to 255 octets
			var idx1 = Array.IndexOf<byte> (userPasswordData, 0);
			if ((idx1 < 0) || (idx1 >= (userPasswordData.Length - 1)))
			{
				return Task.FromResult<object> (null);
			}

			var idx2 = Array.IndexOf<byte> (userPasswordData, 0, idx1 + 1);
			if (idx2 < 0)
			{
				return Task.FromResult<object> (null);
			}

			/*var authorizationIdentity = (idx1 > 0) ?
				Encoding.UTF8.GetString (userPasswordData, 0, idx1) :
				null;*/
			var authenticationIdentity = Encoding.UTF8.GetString (userPasswordData, idx1 + 1, idx2 - idx1 - 1);
			var password = Encoding.UTF8.GetString (userPasswordData, idx2 + 1, userPasswordData.Length - idx2 - 1);
			return _securityParameters.ClientAuthenticator.Invoke (authenticationIdentity, password);
		}

		private async Task OfferChunkCheckExhaustedAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			// мы дожны забрать все BDAT-данные В ЛЮБОМ СЛУЧАЕ, иначе нарушится диалог
			// поэтому перехватываем ВСЕ исключения кроме отмены,
			// забираем все данные и только потом вновь бросаем исключение.
			try
			{
				await _chunkingAgency.OfferJob (source).ConfigureAwait (false);
			}
			catch
			{
				await source.SkipToEndAsync (cancellationToken).ConfigureAwait (false);
				ResetTransaction ();
				throw;
			}

			await source.SkipToEndAsync (cancellationToken).ConfigureAwait (false);
		}

		private void ResetTransaction ()
		{
			Interlocked.Exchange (ref _currentTransaction, null)?.Dispose ();
			_currentTransactionAcceptedRecipients = 0;
			_currentTransactionRequestedEncoding = ContentTransferEncoding.SevenBit;
			_chunkingAgency = null;
			_chunksBufferedSource = null;
			_expectedInput = SmtpCommand.ExpectedInputType.Command;
		}

		private class TwoTaskFinalizer
		{
			private readonly TaskCompletionSource<int> _tcs;
			private readonly Task _otherTask;

			internal TwoTaskFinalizer (TaskCompletionSource<int> tcs, Task otherTask)
			{
				_tcs = tcs;
				_otherTask = otherTask;
			}

			internal void TaskContinuation (Task prevTask)
			{
				if ((prevTask.Status == TaskStatus.Canceled) ||
					(_otherTask.Status == TaskStatus.Canceled))
				{
					_tcs.TrySetCanceled ();
					return;
				}

				if ((prevTask.Status == TaskStatus.Faulted) && (_otherTask.Status == TaskStatus.Faulted))
				{
					_tcs.TrySetException (new Exception[] { prevTask.Exception.InnerException, _otherTask.Exception.InnerException });
					return;
				}

				if (prevTask.Status == TaskStatus.Faulted)
				{
					_tcs.TrySetException (prevTask.Exception.InnerException);
					return;
				}

				if (_otherTask.Status == TaskStatus.Faulted)
				{
					_tcs.TrySetException (_otherTask.Exception.InnerException);
					return;
				}

				_tcs.TrySetResult (0);
			}
		}
	}
}
