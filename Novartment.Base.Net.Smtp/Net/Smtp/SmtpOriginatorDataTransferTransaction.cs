using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpOriginatorDataTransferTransaction :
		IMailDataTransferTransaction
	{
		private enum TransactionStatus
		{
			NotStarted = 0,
			Started = 1,
			RecipientsSpecified = 2,
			Finished = 5
		}

		private readonly SmtpOriginatorProtocolSession _session;
		private readonly ContentTransferEncoding _requiredEncodingSupport;
		private readonly ILogWriter _logger;
		private readonly ArrayList<AddrSpec> _acceptedRecipients = new ArrayList<AddrSpec> (1);
		private string _startingReturnPath = null;
		private TransactionStatus _status = TransactionStatus.NotStarted;

		internal SmtpOriginatorDataTransferTransaction (
			SmtpOriginatorProtocolSession session,
			ContentTransferEncoding requiredEncodingSupport,
			ILogWriter logger = null)
		{
			if (session == null)
			{
				throw new ArgumentNullException (nameof (session));
			}
			Contract.EndContractBlock ();

			_session = session;
			_requiredEncodingSupport = requiredEncodingSupport;
			_logger = logger;
		}

		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Trace(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public void Dispose ()
		{
			_acceptedRecipients.Clear ();
			_startingReturnPath = null;
			_status = TransactionStatus.Finished;
			_logger?.Trace ("Data transfer transaction disposed.");
		}

		public Task StartAsync (AddrSpec returnPath, CancellationToken cancellationToken)
		{
			if (_status != TransactionStatus.NotStarted)
			{
				throw new InvalidOperationException ("Already started");
			}
			var cmd = new SmtpMailFromCommand (returnPath, _requiredEncodingSupport, null);
			var task = _session.ProcessCommandAsync (cmd, cancellationToken);
			return StartAsyncFinalizer (task, returnPath);
		}

		private async Task StartAsyncFinalizer (Task<SmtpReply> task, AddrSpec returnPath)
		{
			var reply = await task.ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw (reply.Code == 553) ?
					new UnacceptableSmtpMailboxException (returnPath) :
					new InvalidOperationException (string.Join ("\r\n", reply.Text));
			}
			_startingReturnPath = returnPath?.ToAngleString () ?? "<>";
			_status = TransactionStatus.Started;
		}

		public Task<RecipientAcceptanceState> TryAddRecipientAsync (AddrSpec recipient, CancellationToken cancellationToken)
		{
			if (recipient == null)
			{
				throw new ArgumentNullException (nameof (recipient));
			}
			Contract.EndContractBlock ();

			if ((_status != TransactionStatus.Started) &&
				(_status != TransactionStatus.RecipientsSpecified))
			{
				throw new InvalidOperationException ("Not started");
			}
			var cmd = new SmtpRcptToCommand (recipient);
			var task = _session.ProcessCommandAsync (cmd, cancellationToken);
			return TryAddRecipientAsyncFinalizer (task, recipient);
		}

		private async Task<RecipientAcceptanceState> TryAddRecipientAsyncFinalizer (Task<SmtpReply> task, AddrSpec recipient)
		{
			var reply = await task.ConfigureAwait (false);
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
			return (reply.IsTransientNegative) ?
				RecipientAcceptanceState.FailureMailboxTemporarilyUnavailable :
				RecipientAcceptanceState.FailureMailboxUnavailable;
		}

		public Task TransferDataAndFinishAsync (IBufferedSource data, long exactSize, CancellationToken cancellationToken)
		{
			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}
			Contract.EndContractBlock ();

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

			_logger?.Info ("Starting transfering mail data from " + _startingReturnPath + " to " + string.Join (",", _acceptedRecipients));
			_status = TransactionStatus.Finished; // заранее на случай исключений
			var isServerSupportsChunking = _session.ServerSupportedExtensions.Contains ("CHUNKING");
			return ((exactSize >= 0) && isServerSupportsChunking) ?
				TransferDataWithChunking (data, exactSize, cancellationToken) :
				TransferDataWithoutChunking (data, cancellationToken);
		}

		private async Task TransferDataWithChunking (IBufferedSource data, long exactSize, CancellationToken cancellationToken)
		{
			var cmd = new SmtpBdatCommand (data, exactSize, true);
			var reply = await _session.ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (!reply.IsPositive)
			{
				throw new InvalidOperationException (string.Join ("\r\n", reply.Text));
			}
			_logger?.Trace ("Mail data transfer completed.");
		}

		private async Task TransferDataWithoutChunking (IBufferedSource data, CancellationToken cancellationToken)
		{
			var result = await _session.ProcessCommandAsync (SmtpCommand.Data, cancellationToken).ConfigureAwait (false);
			if (!result.IsPositiveIntermediate)
			{
				throw new InvalidOperationException (string.Join ("\r\n", result.Text));
			}
			var cmd = new SmtpActualDataCommand (data, false);
			result = await _session.ProcessCommandAsync (cmd, cancellationToken).ConfigureAwait (false);
			if (!result.IsPositive)
			{
				throw new InvalidOperationException (string.Join ("\r\n", result.Text));
			}
			_logger?.Trace ("Mail data transfer completed.");
		}
	}
}
