﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	internal class ConnectedClient :
		IDisposable
	{
		private readonly ITcpConnection _connection = null;
		private readonly Task _processingTask = null;
		private readonly CancellationTokenSource _cancellationSource;
		private TimeSpan _cancelDurationStart;

		internal ConnectedClient (ITcpConnection connection, Task processingTask, CancellationTokenSource cancellationSource)
		{
			_connection = connection;
			_processingTask = processingTask;
			_cancellationSource = cancellationSource;
		}

		internal ProcessState State { get; private set; } = ProcessState.InProgress;

		internal IPHostEndPoint EndPoint => _connection.RemoteEndPoint;

		internal TimeSpan Duration => _connection.Duration;

		internal TimeSpan IdleDuration => _connection.IdleDuration;

		internal TimeSpan CancelDurationStart => _cancelDurationStart;

#pragma warning disable CA1063 // Implement IDisposable Correctly
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			this.State = ProcessState.Disposed;
			_connection.Dispose ();
		}

		internal Task EndProcessing (bool abortConnection)
		{
			if (abortConnection)
			{
				this.State = ProcessState.Canceling;
				_cancelDurationStart = _connection.Duration;
				_cancellationSource.Cancel ();
			}

			return _processingTask;
		}
	}
}
