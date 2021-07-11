using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;

namespace Novartment.Base.Smtp.Test
{
	internal enum TransactionBehavior
	{
		Normal,
		FailStarting,
		FailAddingRecipient,
		FailProcessData,
		SlowStarting,
		SlowAddingRecipient,
		SlowProcessData,
	}

	public sealed class SmtDataTransferTransactionMock :
		IMailTransferTransactionHandler
	{
		private readonly List<AddrSpec> _recipients = new ();
		private readonly AddrSpec _forbiddenReversePath;
		private readonly AddrSpec _forbiddenRecipient;
		private readonly TransactionBehavior _transactionBehavior;
		private readonly AutoResetEvent _slowOperationInProgressEvent = new (false);
		private AddrSpec _returnPath;
		private string _readedData = null;
		private bool _completed = false;
		private bool _disposed = false;

		internal SmtDataTransferTransactionMock (
			AddrSpec forbiddenReversePath,
			AddrSpec forbiddenRecipient,
			TransactionBehavior transactionBehavior)
		{
			_forbiddenReversePath = forbiddenReversePath;
			_forbiddenRecipient = forbiddenRecipient;
			_transactionBehavior = transactionBehavior;
		}

		public AddrSpec ReversePath => _returnPath;

		public List<AddrSpec> Recipients => _recipients;

		public string ReadedData => _readedData;

		public bool Completed => _completed;

		public bool Disposed => _disposed;

		internal WaitHandle SlowOperationInProgressEvent => _slowOperationInProgressEvent;

		public void Dispose ()
		{
			_disposed = true;
			_slowOperationInProgressEvent.Dispose ();
			GC.SuppressFinalize (this);
		}

		public async Task StartAsync (AddrSpec returnPath, CancellationToken cancellationToken = default)
		{
			if (_disposed || _completed)
			{
				throw new InvalidOperationException ();
			}

			if (_transactionBehavior == TransactionBehavior.FailStarting)
			{
				throw new OverflowException ();
			}

			if ((returnPath == _forbiddenReversePath) && (_forbiddenReversePath != null))
			{
				throw new UnacceptableSmtpMailboxException (returnPath);
			}

			_returnPath = returnPath;
			if (_transactionBehavior == TransactionBehavior.SlowStarting)
			{
				_slowOperationInProgressEvent.Set ();
				for (int i = 0; i < 100; i++)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					await Task.Delay (20, cancellationToken).ConfigureAwait (false);
				}

				throw new InvalidOperationException ();
			}
		}

		public async Task<RecipientAcceptanceState> TryAddRecipientAsync (AddrSpec recipient, CancellationToken cancellationToken = default)
		{
			if (_disposed || _completed)
			{
				throw new InvalidOperationException ();
			}

			if (_transactionBehavior == TransactionBehavior.FailAddingRecipient)
			{
				throw new OverflowException ();
			}

			if (recipient == _forbiddenRecipient)
			{
				return RecipientAcceptanceState.FailureMailboxUnavailable;
			}

			_recipients.Add (recipient);
			if (_transactionBehavior == TransactionBehavior.SlowAddingRecipient)
			{
				_slowOperationInProgressEvent.Set ();
				for (int i = 0; i < 100; i++)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					await Task.Delay (20, cancellationToken).ConfigureAwait (false);
				}

				throw new InvalidOperationException ();
			}

			return RecipientAcceptanceState.Success;
		}

		public async Task TransferDataAndFinishAsync (IBufferedSource source, long exactSize, CancellationToken cancellationToken = default)
		{
			if (_disposed || _completed || (_readedData != null))
			{
				throw new InvalidOperationException ();
			}

			if (_transactionBehavior == TransactionBehavior.FailProcessData)
			{
				throw new OverflowException ();
			}

			if (_transactionBehavior == TransactionBehavior.SlowProcessData)
			{
				_slowOperationInProgressEvent.Set ();
				for (int i = 0; i < 100; i++)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					await Task.Delay (20, cancellationToken).ConfigureAwait (false);
				}

				throw new InvalidOperationException ();
			}
			else
			{
				_readedData = await source.ReadAllTextAsync (Encoding.ASCII, cancellationToken).ConfigureAwait (false);
			}

			_completed = true;
		}
	}
}
