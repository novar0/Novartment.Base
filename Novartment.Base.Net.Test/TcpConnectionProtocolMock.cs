using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net.Test
{
	internal class TcpConnectionProtocolMock : ITcpConnectionProtocol, IDisposable
	{
		private readonly AutoResetEvent _startedEvent = new AutoResetEvent (false);
		private readonly List<ITcpConnection> _connections = new List<ITcpConnection> ();
		private readonly Dictionary<IPEndPoint, TaskCompletionSource<int>> _stopSignalers = new Dictionary<IPEndPoint, TaskCompletionSource<int>> ();

		internal TcpConnectionProtocolMock ()
		{
		}

		internal IReadOnlyList<ITcpConnection> Connections => _connections;

		internal EventWaitHandle StartedEvent => _startedEvent;

		public Task StartAsync (ITcpConnection connection, CancellationToken cancellationToken = default)
		{
			_connections.Add (connection);
			var stopSignaler = new TaskCompletionSource<int> ();
			_stopSignalers[connection.RemoteEndPoint] = stopSignaler;
			cancellationToken.Register (Cancel, connection.RemoteEndPoint, false);
			_startedEvent.Set ();
			return stopSignaler.Task;
		}

		public void Dispose ()
		{
			_startedEvent.Dispose ();
		}

		internal void FinishHandlingConnection (IPEndPoint remoteEndPoint)
		{
			_stopSignalers[remoteEndPoint].TrySetResult (0);
		}

		private void Cancel (object remoteEndPointObject)
		{
			var remoteEndPoint = (IPEndPoint)remoteEndPointObject;
			_stopSignalers[remoteEndPoint].TrySetCanceled ();
		}
	}
}
