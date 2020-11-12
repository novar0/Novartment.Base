using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Транспорт, доставляющий SmtpCommand-ы и ответы на них через TCP-подключение
	/// с возможностью организации безопасной доставки.
	/// </summary>
	internal class TcpConnectionSmtpCommandTransport :
		ISmtpCommandTransport
	{
		// RFC 5321 part 4.5.3.1.4: The maximum total length of a command line including the command word and the <CRLF> is 512 octets.
		// RFC 5321 part 4.5.3.1.6: The maximum total length of a text line including the <CRLF> is 1000 octets
		private const int MaximumCommandLength = 1000;

		private readonly char[] _commandBuf = new char[MaximumCommandLength];
		private readonly ILogger _logger;
		private IBufferedSource _reader;
		private IBinaryDestination _writer;
		private ITcpConnection _connection;
		private string _pendingReplies = null;

		internal TcpConnectionSmtpCommandTransport (ITcpConnection connection, ILogger logger = null)
		{
			if (connection == null)
			{
				throw new ArgumentNullException (nameof (connection));
			}

			// нужно минимум 5 байтов для команды (QUIT + CRLF) или ответа (три цифры + CRLF)
			if (connection.Reader.BufferMemory.Length < 5)
			{
				throw new InvalidOperationException ("Too small connection.Reader.Buffer.Length, required minimum 5 bytes.");
			}

			_logger = logger;
			SetConnection (connection);
		}

		public bool TlsEstablished => _connection is ITlsConnection;

		public X509Certificate RemoteCertificate => (_connection as ITlsConnection)?.RemoteCertificate;

		public async Task StartTlsClientAsync (X509CertificateCollection clientCertificates)
		{
			// RFC 3207 part 4:
			// If, after having issued the STARTTLS command,
			// the client finds out that some failure prevents it from actually starting a TLS handshake,
			// then it SHOULD abort the connection.
			if (_connection is not ITlsCapableConnection tlsCapableConnection)
			{
				throw new UnrecoverableProtocolException("Connection is not TLS-capable.");
			}

			_logger?.LogTrace ("Starting TLS as client...");
			var newConnection = await tlsCapableConnection.StartTlsClientAsync (clientCertificates, default)
				.ConfigureAwait (false);
			if ((_logger != null) && _logger.IsEnabled (LogLevel.Information))
			{
				_logger?.LogInformation (FormattableString.Invariant (
					$@"Started TLS client: Protocol={newConnection.TlsProtocol}, Cipher={newConnection.CipherAlgorithm}/{newConnection.CipherStrength}, Hash={GetHashAlgorithmName (newConnection.HashAlgorithm)}/{newConnection.HashStrength}, KeyExchange={GetExchangeAlgorithmName (newConnection.KeyExchangeAlgorithm)}/{newConnection.KeyExchangeStrength}"));
			}

			SetConnection (newConnection);
		}

		public async Task StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired)
		{
			if (_connection is not ITlsCapableConnection tlsCapableConnection)
			{
				throw new InvalidOperationException("Connection is not TLS-capable.");
			}

			_logger?.LogTrace ("Starting TLS as server...");
			var newConnection = await tlsCapableConnection.StartTlsServerAsync (serverCertificate, clientCertificateRequired, default)
				.ConfigureAwait (false);
			if ((_logger != null) && _logger.IsEnabled (LogLevel.Information))
			{
				_logger?.LogInformation (FormattableString.Invariant (
					$@"Started TLS server: Protocol={newConnection.TlsProtocol}, Cipher={newConnection.CipherAlgorithm}/{newConnection.CipherStrength}, Hash={GetHashAlgorithmName (newConnection.HashAlgorithm)}/{newConnection.HashStrength}, KeyExchange={GetExchangeAlgorithmName (newConnection.KeyExchangeAlgorithm)}/{newConnection.KeyExchangeStrength}"));
			}

			SetConnection (newConnection);
		}

		public ValueTask<SmtpCommand> ReceiveCommandAsync (SmtpCommand.ExpectedInputType expectedInputType, CancellationToken cancellationToken = default)
		{
			if (_reader.Count > 0)
			{
				var cmd = GetCommandFromReaderBuffer (expectedInputType);
				return new ValueTask<SmtpCommand> (cmd);
			}

			return ReceiveCommandAsyncStateMachine ();

			async ValueTask<SmtpCommand> ReceiveCommandAsyncStateMachine ()
			{
				await _reader.LoadAsync (cancellationToken).ConfigureAwait (false);

				if (_reader.Count < 1)
				{
					// данные кончились, означает подключение разорвано
					throw new IOException ("Connection closed waiting for command.");
				}

				return GetCommandFromReaderBuffer (expectedInputType);
			}
		}

		public async ValueTask<SmtpReply> ReceiveReplyAsync (CancellationToken cancellationToken = default)
		{
			if (_reader.Count > 0)
			{
				return SmtpReply.Parse (_reader, _logger);
			}

			await _reader.LoadAsync (cancellationToken).ConfigureAwait (false);
			if (_reader.Count < 1)
			{
				throw new InvalidOperationException ("Connection unexpectedly closed.");
			}

			return SmtpReply.Parse (_reader, _logger);
		}

		public ValueTask SendReplyAsync (SmtpReply reply, bool canBeGrouped, CancellationToken cancellationToken = default)
		{
			var replyText = reply.ToString ();

			// RFC 2920 part 3.2:
			// A server SMTP implementation that offers the pipelining extension:
			// ... (2) SHOULD elect to store responses to grouped ... commands in an internal buffer so they can sent as a unit.
			// ... (7) MUST send all pending responses immediately whenever the local TCP input buffer is emptied.
			_pendingReplies = (_pendingReplies == null) ?
				replyText :
				_pendingReplies + replyText;
			if (!canBeGrouped || (_reader.Count < 1))
			{
				var text = _pendingReplies;
				_pendingReplies = null;
				return SendTextAsync (text, cancellationToken);
			}

			return default;
		}

		public ValueTask SendCommandAsync (SmtpCommand command, CancellationToken cancellationToken = default)
		{
			var commandText = command.ToString ();
			return SendTextAsync (commandText, cancellationToken);
		}

		public Task SendBinaryAsync (IBufferedSource source, CancellationToken cancellationToken = default)
		{
			return BufferedSourceExtensions.WriteToAsync (source, _writer, cancellationToken);
		}

		private static string GetHashAlgorithmName (HashAlgorithmType hashAlgorithmType)
		{
			return ((int)hashAlgorithmType) switch
			{
				32780 => "SHA256",
				32781 => "SHA384",
				32782 => "SHA512",
				_ => hashAlgorithmType.ToString (),
			};
		}

		private static string GetExchangeAlgorithmName (ExchangeAlgorithmType exchangeAlgorithmType)
		{
			return ((int)exchangeAlgorithmType) switch
			{
				44550 => "ECDH_Ephemeral",
				_ => exchangeAlgorithmType.ToString (),
			};
		}

		private SmtpCommand GetCommandFromReaderBuffer (SmtpCommand.ExpectedInputType expectedInputType)
		{
			if (expectedInputType == SmtpCommand.ExpectedInputType.Data)
			{
				// TODO: сделать первые два байта разделителя (0x0d, 0x0a) частью данных
				var cmd = new SmtpActualDataCommand ();
				cmd.SetSource (_reader, true);
				return cmd;
			}

			var sourceData = _reader.BufferMemory.Span.Slice (_reader.Offset, _reader.Count);

			var inPos = 0;
			var outPos = 0;
			var isInvalidCharsFound = false;
			var isSizeLimitExceeded = false;

			// независимо от корректности команды мы должны посчитать сколько пропустить байт (то есть найти CRLF)
			while (true)
			{
				if (inPos > (sourceData.Length - 2))
				{
					// вся строка просканирована, CRLF не найден
					_reader.Skip (sourceData.Length);
					return new SmtpInvalidSyntaxCommand (SmtpCommandType.Unknown, "Ending CRLF not found in command.");
				}

				var octet = sourceData[inPos];
				if ((octet == 0x0d) && (sourceData[inPos + 1] == 0x0a))
				{
					// CRLF найден
					break;
				}

				if (!isInvalidCharsFound && !isSizeLimitExceeded)
				{
					// некорректные символы не встречены, предел по длине не достигнут, продолжаем наполнять _commandBuf
					if (octet > AsciiCharSet.MaxCharValue)
					{
						// RFC 5321 part 2.4: Commands and replies are composed of characters from the ASCII character set
						isInvalidCharsFound = true;
					}
					else
					{
						if (outPos > MaximumCommandLength)
						{
							// достигли предела по длине _commandBuf
							isSizeLimitExceeded = true;
						}
						else
						{
							_commandBuf[outPos++] = (char)octet;
						}
					}
				}

				inPos++;
			}

			_reader.Skip (inPos + 2);

			if (isInvalidCharsFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Unknown, "Invalid non-ASCII chars found in command.");
			}

			if (isSizeLimitExceeded)
			{
				return new SmtpTooLongCommand ();
			}

			if ((_logger != null) && _logger.IsEnabled (LogLevel.Trace))
			{
				var text = new string (_commandBuf, 0, outPos);
				_logger?.LogTrace ($"{_connection.RemoteEndPoint} <<< {text}");
			}

			var command = SmtpCommand.Parse (_commandBuf.AsSpan (0, outPos), expectedInputType);

			if (command is SmtpBdatCommand bdatCommand)
			{
				bdatCommand.SetSource (_reader);
			}

			if (command is SmtpActualDataCommand dataCommand)
			{
				dataCommand.SetSource (_reader, true);
			}

			return command;
		}

		private void SetConnection (ITcpConnection connection)
		{
			_reader = connection.Reader;
			_writer = connection.Writer;
			_connection = connection;
		}

		private ValueTask SendTextAsync (string text, CancellationToken cancellationToken)
		{
			if ((_logger != null) && _logger.IsEnabled (LogLevel.Trace))
			{
#if NETSTANDARD2_0
				var safeText = text.Replace ("\r\n", "||");
#else
				var safeText = text.Replace ("\r\n", "||", StringComparison.Ordinal);
#endif
				_logger?.LogTrace ($"{_connection.RemoteEndPoint} >>> {safeText}");
			}

			var size = text.Length;
			var buf = new byte[size];
			AsciiCharSet.GetBytes (text.AsSpan (), buf);
			_pendingReplies = null;
			return _writer.WriteAsync (buf.AsMemory (0, size), cancellationToken);
		}
	}
}
