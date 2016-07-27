using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Novartment.Base.Collections;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpOriginatorProtocolSession :
		IDisposable
	{
		private readonly ISmtpCommandReplyConnectionSenderReceiver _sender;
		private readonly AvlHashTreeSet<string> _serverSupportedExtensions = new AvlHashTreeSet<string> (StringComparer.OrdinalIgnoreCase);
		private readonly string _hostFqdn;
		private int _completed = 0;

		internal IReadOnlyFiniteSet<string> ServerSupportedExtensions => _serverSupportedExtensions;

		internal SmtpOriginatorProtocolSession (ISmtpCommandReplyConnectionSenderReceiver sender, string hostFqdn)
		{
			_sender = sender;
			_hostFqdn = hostFqdn ?? "anonym";
		}

		internal async Task ReceiveGreetingAndStartAsync (CancellationToken cancellationToken)
		{
			// ждём приветствие сервера
			var greeting = await ProcessCommandAsync (SmtpCommand.NoCommand, cancellationToken).ConfigureAwait (false);
			if (!greeting.IsPositive)
			{
				throw new InvalidOperationException (string.Join ("\r\n", greeting.Text));
			}
			await StartAsync (cancellationToken).ConfigureAwait (false);
		}

		private async Task StartAsync (CancellationToken cancellationToken)
		{
			// посылаем приветственную команду EHLO
			_serverSupportedExtensions.Clear ();
			var reply = await ProcessCommandAsync (new SmtpEhloCommand (_hostFqdn), cancellationToken).ConfigureAwait (false);
			if (reply.IsPositive)
			{
				foreach (var extension in reply.Text)
				{
					var isOK = "OK".Equals (extension, StringComparison.OrdinalIgnoreCase);
					if (!isOK)
					{
						_serverSupportedExtensions.Add (extension);
					}
				}
			}
			else
			{
				// RFC 5321 part 4.1.4:
				// If the EHLO command is not acceptable to the SMTP server,
				// 501, 500, 502, or 550 failure replies MUST be returned as appropriate.
				var needRepeatGreetWithHelo = (reply.Code == 500) || (reply.Code == 501) || (reply.Code == 502) || (reply.Code == 550);
				if (!needRepeatGreetWithHelo)
				{
					throw new InvalidOperationException (string.Join ("\r\n", reply.Text));
				}
				// если сервер не понял EHLO, посылаем HELO
				reply = await ProcessCommandAsync (new SmtpHeloCommand (_hostFqdn), cancellationToken).ConfigureAwait (false);
				if (!reply.IsPositive)
				{
					throw new InvalidOperationException (string.Join ("\r\n", reply.Text));
				}
			}
		}

		internal async Task RestartWithTlsAsync (X509CertificateCollection clientCertificates, CancellationToken cancellationToken)
		{
			var isTlsSupported = _serverSupportedExtensions.Contains ("STARTTLS");
			if (!isTlsSupported)
			{
				throw new InvalidOperationException ("Remote host not supports TLS.");
			}
			var reply = await ProcessCommandAsync (SmtpCommand.StartTls, cancellationToken).ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw new InvalidOperationException ("Remote host does not agreed to start TLS.");
			}
			try
			{
				await _sender.StartTlsClientAsync (clientCertificates).ConfigureAwait (false);
			}
			catch (Exception excpt)
			{
				// RFC 3207 part 4:
				// If, after having issued the STARTTLS command,
				// the client finds out that some failure prevents it from actually starting a TLS handshake,
				// then it SHOULD abort the connection.
				throw new UnrecoverableProtocolException ("Remote host does not agreed to start TLS.", excpt);
			}

			// RFC 3207 part 4.2:
			// Upon completion of the TLS handshake, the SMTP protocol is reset to the initial state
			// (the state in SMTP after a server issues a 220 service ready greeting).
			// ...
			// The client MUST discard any knowledge obtained from the server,
			// such as the list of SMTP service extensions, which was not obtained from the TLS negotiation itself.
			// The client SHOULD send an EHLO command as the first command after a successful TLS negotiation.
			await StartAsync (cancellationToken).ConfigureAwait (false);
		}

		internal async Task AuthenticateAsync (NetworkCredential credential, CancellationToken cancellationToken)
		{
			var isServerSupportsAuthPlain = IsServerSupportsAuthPlain ();
			if (!isServerSupportsAuthPlain)
			{
				throw new InvalidOperationException ("Remote host not supports PLAIN authentification.");
			}
			// TODO: брать имя/пароль у IMailTransactionOriginator
			string login = credential.UserName;
			string password = credential.Password;
			var buf = new byte[1000];
			var idx = 0;
			buf[idx++] = 0;
			idx += Encoding.UTF8.GetBytes (login, 0, login.Length, buf, idx);
			buf[idx++] = 0;
			idx += Encoding.UTF8.GetBytes (password, 0, password.Length, buf, idx);
			var bufExact = new byte[idx];
			Array.Copy (buf, 0, bufExact, 0, idx);
			var cmd = new SmtpAuthCommand ("PLAIN", bufExact);
			var reply = await ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw new SecurityException ("Authentification failed.");
			}
		}

		internal bool IsServerSupportsAuthPlain ()
		{
			foreach (var extension in _serverSupportedExtensions)
			{
				var isExtensionAuth = extension.StartsWith ("AUTH ", StringComparison.OrdinalIgnoreCase);
				if (isExtensionAuth)
				{
					var idx = 5;
					var startIdx = idx;
					while (idx < extension.Length)
					{
						if (extension[idx] == ' ')
						{
							var mechanism = extension.Substring (startIdx, idx - startIdx);
							var isMechanismPlain = "PLAIN".Equals (mechanism, StringComparison.OrdinalIgnoreCase);
							if (isMechanismPlain)
							{
								return true;
							}
							startIdx = idx + 1;
						}
						idx++;
					}
					var mechanism2 = extension.Substring (startIdx, idx - startIdx);
					var isMechanism2Plain = "PLAIN".Equals (mechanism2, StringComparison.OrdinalIgnoreCase);
					if (isMechanism2Plain)
					{
						return true;
					}
				}
			}
			return false;
		}

		internal async Task FinishAsync (CancellationToken cancellationToken)
		{
			// посылаем прощальную команду QUIT
			var reply = await ProcessCommandAsync (SmtpCommand.Quit, cancellationToken).ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw new InvalidOperationException (string.Join ("\r\n", reply.Text));
			}
		}

		internal async Task<SmtpReply> ProcessCommandAsync (SmtpCommand command, CancellationToken cancellationToken)
		{
			switch (command.CommandType)
			{
				case SmtpCommandType.Quit:
					var oldValue = Interlocked.Exchange (ref _completed, 1);
					if (oldValue != 0)
					{
						// игнорируем команду QUIT если работа уже завершена
						return SmtpReply.OK;
					}
					await _sender.SendCommandAsync (command, cancellationToken).ConfigureAwait (false);
					break;
				case SmtpCommandType.NoCommand:
					// NoCommand означает ничего не посылать, только получить ответ
					break;
				case SmtpCommandType.Bdat:
					await _sender.SendCommandAsync (command, cancellationToken).ConfigureAwait (false);
					var bdatCmd = (SmtpBdatCommand)command;
					try
					{
						await _sender.SendBinaryAsync (bdatCmd.Source, cancellationToken).ConfigureAwait (false);
					}
					catch (NotEnoughDataException excpt)
					{
						// в указанном источнике оказалось данных меньше чем указанный размер
						throw new UnrecoverableProtocolException (
							FormattableString.Invariant ($"Source provided less data ({bdatCmd.Size - bdatCmd.Source.UnusedSize}) then specified ({bdatCmd.Size}). Appended {bdatCmd.Source.UnusedSize} bytes."),
							excpt);
					}
					break;
				case SmtpCommandType.ActualData:
					var actualDataCmd = (SmtpActualDataCommand)command;
					await _sender.SendBinaryAsync (actualDataCmd.Source, cancellationToken).ConfigureAwait (false);
					var isPartSkipped = await actualDataCmd.Source.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
					if (isPartSkipped)
					{
						// в данных встретился маркер конца данных, данные не дочитаны до конца
						throw new UnrecoverableProtocolException ("Unexpected end-mark 'CRLF.CRLF' found in data. Data transfer incomplete.");
					}
					var endMarker = new byte[] { 0x0d, 0x0a, (byte)'.', 0x0d, 0x0a };
					var endMarkerSrc = new ArrayBufferedSource (endMarker);
					await _sender.SendBinaryAsync (endMarkerSrc, cancellationToken).ConfigureAwait (false);
					break;
				default:
					await _sender.SendCommandAsync (command, cancellationToken).ConfigureAwait (false);
					break;
			}

			// получаем ответ
			var reply = await _sender.ReceiveReplyAsync (cancellationToken).ConfigureAwait (false);

			if (reply.Code == 421) // 421 Service not available, closing transmission channel
			{
				_completed = 1; // соединение будет закрыто сервером, дальше слать команды бесполезно
			}

			return reply;
		}

		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Trace(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public void Dispose ()
		{
			var oldValue = Interlocked.Exchange (ref _completed, 1);
			if (oldValue == 0)
			{
				try
				{
					_sender.SendCommandAsync (SmtpCommand.Quit, CancellationToken.None).Wait ();
				}
				catch (ObjectDisposedException) { }
				catch (InvalidOperationException) { }
				catch (IOException) { }
				catch (SocketException) { }
			}
		}
	}
}
