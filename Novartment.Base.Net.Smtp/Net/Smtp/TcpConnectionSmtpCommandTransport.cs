using System;
using System.Diagnostics.CodeAnalysis;
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
	/// с возможностью орагнизации безопасной доставки.
	/// </summary>
	internal class TcpConnectionSmtpCommandTransport :
		ISmtpCommandTransport
	{
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
			if (connection.Reader.Buffer.Length < 5)
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
			var tlsCapableConnection = _connection as ITlsCapableConnection;
			if (tlsCapableConnection == null)
			{
				throw new UnrecoverableProtocolException ("Connection is not TLS-capable.");
			}

			_logger?.LogTrace ("Starting TLS as client...");
			var newConnection = await tlsCapableConnection.StartTlsClientAsync (clientCertificates, CancellationToken.None)
				.ConfigureAwait (false);
			_logger?.LogInformation (FormattableString.Invariant (
				$@"Started TLS client: Protocol={newConnection.TlsProtocol}, Cipher={newConnection.CipherAlgorithm}/{newConnection.CipherStrength}, Hash={GetHashAlgorithmName (newConnection.HashAlgorithm)}/{newConnection.HashStrength}, KeyExchange={GetExchangeAlgorithmName (newConnection.KeyExchangeAlgorithm)}/{newConnection.KeyExchangeStrength}"));
			SetConnection (newConnection);
		}

		public async Task StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired)
		{
			var tlsCapableConnection = _connection as ITlsCapableConnection;
			if (tlsCapableConnection == null)
			{
				throw new InvalidOperationException ("Connection is not TLS-capable.");
			}

			_logger?.LogTrace ("Starting TLS as server...");
			var newConnection = await tlsCapableConnection.StartTlsServerAsync (serverCertificate, clientCertificateRequired, CancellationToken.None)
				.ConfigureAwait (false);
			_logger?.LogInformation (FormattableString.Invariant (
				$@"Started TLS server: Protocol={newConnection.TlsProtocol}, Cipher={newConnection.CipherAlgorithm}/{newConnection.CipherStrength}, Hash={GetHashAlgorithmName (newConnection.HashAlgorithm)}/{newConnection.HashStrength}, KeyExchange={GetExchangeAlgorithmName (newConnection.KeyExchangeAlgorithm)}/{newConnection.KeyExchangeStrength}"));
			SetConnection (newConnection);
		}

		public async Task<SmtpCommand> ReceiveCommandAsync (SmtpCommand.ExpectedInputType expectedInputType, CancellationToken cancellationToken)
		{
			if (_reader.Count < 1)
			{
				await _reader.FillBufferAsync (cancellationToken).ConfigureAwait (false);

				if (_reader.Count < 1)
				{
					// данные кончились, означает подключение разорвано
					throw new IOException ("Connection closed waiting for command.");
				}
			}

			return ParseReceivedCommand (expectedInputType);
		}

		public async Task<SmtpReply> ReceiveReplyAsync (CancellationToken cancellationToken)
		{
			if (_reader.Count > 0)
			{
				return SmtpReply.Parse (_reader, _logger);
			}

			await _reader.FillBufferAsync (cancellationToken).ConfigureAwait (false);
			if (_reader.Count < 1)
			{
				throw new InvalidOperationException ("Connection unexpectedly closed.");
			}

			return SmtpReply.Parse (_reader, _logger);
		}

		public Task SendReplyAsync (SmtpReply reply, bool canBeGrouped, CancellationToken cancellationToken)
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

			return Task.CompletedTask;
		}

		public Task SendCommandAsync (SmtpCommand command, CancellationToken cancellationToken)
		{
			var commandText = command.ToString ();
			return SendTextAsync (commandText, cancellationToken);
		}

		public Task SendBinaryAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			return source.WriteToAsync (_writer, cancellationToken);
		}

		private static string GetHashAlgorithmName (HashAlgorithmType hashAlgorithmType)
		{
			switch ((int)hashAlgorithmType)
			{
				case 32780:
					return "SHA256";
				case 32781:
					return "SHA384";
				case 32782:
					return "SHA512";
				default:
					return hashAlgorithmType.ToString ();
			}
		}

		private static string GetExchangeAlgorithmName (ExchangeAlgorithmType exchangeAlgorithmType)
		{
			switch ((int)exchangeAlgorithmType)
			{
				case 44550:
					return "ECDH_Ephemeral";
				default:
					return exchangeAlgorithmType.ToString ();
			}
		}

		private void SetConnection (ITcpConnection connection)
		{
			_reader = connection.Reader;
			_writer = connection.Writer;
			_connection = connection;
		}

		private SmtpCommand ParseReceivedCommand (SmtpCommand.ExpectedInputType expectedInputType)
		{
			SmtpCommand command;
			try
			{
				command = SmtpCommand.Parse (_reader, expectedInputType, _logger);
			}
			catch (FormatException excpt)
			{
				// исключение произойдёт ТОЛЬКО если не распознано первое слово (собственно команда)
				command = new SmtpUnknownCommand (excpt.Message);
			}

			return command;
		}

		private Task SendTextAsync (string text, CancellationToken cancellationToken)
		{
			var size = text.Length;
			var buf = new byte[size];
			AsciiCharSet.GetBytes (text, 0, size, buf, 0);

			var isCRLF = (size > 1) && (buf[size - 2] == 0x0d) && (buf[size - 1] == 0x0a);
			_logger?.LogTrace (">>> " + (isCRLF ? text.Substring (0, size - 2) : text));

			_pendingReplies = null;
			return _writer.WriteAsync (buf, 0, size, cancellationToken);
		}
	}
}
