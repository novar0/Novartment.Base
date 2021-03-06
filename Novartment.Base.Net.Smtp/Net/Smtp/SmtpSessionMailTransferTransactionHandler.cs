using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Обработчик транзакции по передаче почтового сообщения.
	/// Все вызовы транзакции преобразуются в SmtpCommand для указанной при создании SmtpOriginatorProtocolSession.
	/// </summary>
	internal sealed class SmtpSessionMailTransferTransactionHandler :
		IMailTransferTransactionHandler
	{
		private readonly SmtpOriginatorProtocolSession _session;
		private readonly ContentTransferEncoding _requiredEncodingSupport;
		private readonly ILogger _logger;
		private readonly ArrayList<AddrSpec> _acceptedRecipients = new (1);
		private string _startingReturnPath = null;
		private TransactionStatus _status = TransactionStatus.NotStarted;

		internal SmtpSessionMailTransferTransactionHandler (SmtpOriginatorProtocolSession session, ContentTransferEncoding requiredEncodingSupport, ILogger logger = null)
		{
			_session = session ?? throw new ArgumentNullException (nameof (session));
			_requiredEncodingSupport = requiredEncodingSupport;
			_logger = logger;
		}

		private enum TransactionStatus
		{
			NotStarted = 0,
			Started = 1,
			RecipientsSpecified = 2,
			Finished = 5,
		}

		public void Dispose ()
		{
			_acceptedRecipients.Clear ();
			_startingReturnPath = null;
			_status = TransactionStatus.Finished;
			_logger?.LogTrace ("Data transfer transaction disposed.");
		}

		public async Task StartAsync (AddrSpec returnPath, CancellationToken cancellationToken = default)
		{
			if (_status != TransactionStatus.NotStarted)
			{
				throw new InvalidOperationException ("Already started");
			}

			var cmd = new SmtpMailFromCommand (returnPath, _requiredEncodingSupport, null);
			var reply = await _session.ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw (reply.Code == 553) ?
					new UnacceptableSmtpMailboxException (returnPath) :
					new InvalidOperationException (string.Join ("\r\n", reply.Text));
			}

			_startingReturnPath = "<" + returnPath?.ToString () ?? string.Empty + ">";
			_status = TransactionStatus.Started;
		}

		public async Task<RecipientAcceptanceState> TryAddRecipientAsync (AddrSpec recipient, CancellationToken cancellationToken = default)
		{
			if (recipient == null)
			{
				throw new ArgumentNullException (nameof (recipient));
			}

			if ((_status != TransactionStatus.Started) &&
				(_status != TransactionStatus.RecipientsSpecified))
			{
				throw new InvalidOperationException ("Not started");
			}

			var cmd = new SmtpRcptToCommand (recipient);
			var reply = await _session.ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (reply.IsPositive)
			{
				_acceptedRecipients.Add (recipient);
				_status = TransactionStatus.RecipientsSpecified;
				return RecipientAcceptanceState.Success;
			}

			// RFC 5321 part 4.5.3.1.10:
			// ... incorrectly listed the error where an SMTP server exhausts its implementation limit
			// on the number of RCPT commands ("too many recipients") as having reply code 552.
			// The correct reply code for this condition is 452.
			// Clients SHOULD treat a 552 code in this case as a temporary, rather than permanent, ...
			if ((reply.Code == 452) || (reply.Code == 552))
			{
				return RecipientAcceptanceState.FailureTooManyRecipients;
			}

			return reply.IsTransientNegative ?
				RecipientAcceptanceState.FailureMailboxTemporarilyUnavailable :
				RecipientAcceptanceState.FailureMailboxUnavailable;
		}

		public Task TransferDataAndFinishAsync (IBufferedSource data, long exactSize, CancellationToken cancellationToken = default)
		{
			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			if (_status == TransactionStatus.Started)
			{
				throw new NoValidRecipientsException ();
			}

			if (_status != TransactionStatus.RecipientsSpecified)
			{
				throw new InvalidOperationException ("Not started");
			}

			if ((exactSize < 0) && (_requiredEncodingSupport == ContentTransferEncoding.Binary))
			{
				throw new InvalidOperationException ("Requested binary encoding, but BINARYMIME requires exactSize to be known.");
			}

			if ((_logger != null) && _logger.IsEnabled (LogLevel.Information))
			{
				_logger?.LogInformation ("Starting transfering mail data from " + _startingReturnPath + " to " + string.Join (",", _acceptedRecipients));
			}

			_status = TransactionStatus.Finished; // заранее на случай исключений
			var isServerSupportsChunking = _session.ServerSupportedExtensions.Contains ("CHUNKING");
			return ((exactSize >= 0) && isServerSupportsChunking) ?
				TransferDataWithChunking (data, exactSize, cancellationToken) :
				TransferDataWithoutChunking (data, cancellationToken);
		}

		private async Task TransferDataWithChunking (IBufferedSource data, long exactSize, CancellationToken cancellationToken)
		{
			var cmd = new SmtpBdatCommand (exactSize, true);
			cmd.SetSource (data);
			var reply = await _session.ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw new InvalidOperationException (string.Join ("\r\n", reply.Text));
			}

			_logger?.LogTrace ("Mail data transfer completed.");
		}

		private async Task TransferDataWithoutChunking (IBufferedSource data, CancellationToken cancellationToken)
		{
			var result = await _session.ProcessCommandAsync (SmtpCommand.CachedCmdData, cancellationToken).ConfigureAwait (false);
			if (!result.IsPositiveIntermediate)
			{
				throw new InvalidOperationException (string.Join ("\r\n", result.Text));
			}

			var cmd = new SmtpActualDataCommand ();
			cmd.SetSource (data, false);
			result = await _session.ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (!result.IsPositive)
			{
				throw new InvalidOperationException (string.Join ("\r\n", result.Text));
			}

			_logger?.LogTrace ("Mail data transfer completed.");
		}
	}
}
