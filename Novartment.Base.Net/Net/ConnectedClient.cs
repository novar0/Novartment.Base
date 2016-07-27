using System;
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

		internal IPHostEndPoint EndPoint => _connection.RemoteEndPoint;

		internal TimeSpan Duration => _connection.Duration;

		internal TimeSpan IdleDuration => _connection.IdleDuration;

		internal ConnectedClient (ITcpConnection connection, Task processingTask, CancellationTokenSource cancellationSource)
		{
			_connection = connection;
			_processingTask = processingTask;
			_cancellationSource = cancellationSource;
		}

		internal Task EndProcessing (bool abortConnection)
		{
			if (abortConnection)
			{
				_cancellationSource.Cancel ();
			}
			return _processingTask;
		}

		public void Dispose ()
		{
			_connection.Dispose ();
		}
	}
}
