using System;
using System.Diagnostics.Contracts;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.IO
{
	/// <summary>
	/// A signaler for sending signals between components that uses named channels.
	/// </summary>
	public sealed class NamedPipeSignaler :
		ISignaler
	{
		private readonly string _pipeName;
		private int _started = 0;

		/// <summary>
		/// Initializes a new instance of the FolderIterator class that uses the specified channel name.
		/// </summary>
		/// <param name="pipeName">The channel name. Must be unique among the communicating components.</param>
		public NamedPipeSignaler (string pipeName)
		{
			var isNullOrWhiteSpace = string.IsNullOrWhiteSpace (pipeName);
			if (isNullOrWhiteSpace)
			{
				throw new ArgumentNullException (nameof (pipeName));
			}

			Contract.EndContractBlock ();

			_pipeName = pipeName;
		}

		/// <summary>Starts waiting for the signal to be received.</summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the process of waiting for a signal.</returns>
		public async Task WaitForSignalAsync (CancellationToken cancellationToken = default)
		{
			var oldValue = Interlocked.CompareExchange (ref _started, 1, 0);
			if (oldValue != 0)
			{
				throw new InvalidOperationException ("Listening already started, second instance not supported.");
			}

			try
			{
				using var pipeServer = new NamedPipeServerStream (_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				await pipeServer.WaitForConnectionAsync (cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				_started = 0;
			}
		}

		/// <summary>Send signals.</summary>
		/// <param name="millisecondsTimeout">The maximum time (in milliseconds) allowed for sending a signal.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public async Task SendSignalAsync (int millisecondsTimeout, CancellationToken cancellationToken = default)
		{
			using var client = new NamedPipeClientStream (".", _pipeName, PipeDirection.Out);
			// это послужит сигналом серверу, ждущему подключения
			await client.ConnectAsync (millisecondsTimeout, cancellationToken).ConfigureAwait (false);
		}
	}
}
