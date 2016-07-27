using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net.Test
{
	internal class TcpConnectionProtocolMock : ITcpConnectionProtocol
	{
		private List<ITcpConnection> _connections = new List<ITcpConnection> ();
		private Dictionary<IPEndPoint, TaskCompletionSource<int>> _stopSignalers = new Dictionary<IPEndPoint, TaskCompletionSource<int>> ();
		private readonly AutoResetEvent _startedEvent = new AutoResetEvent (false);

		internal IReadOnlyList<ITcpConnection> Connections => _connections;
		internal EventWaitHandle StartedEvent => _startedEvent;

		internal TcpConnectionProtocolMock ()
		{
		}

		public Task StartAsync (ITcpConnection connection, CancellationToken cancellationToken)
		{
			_connections.Add (connection);
			var stopSignaler = new TaskCompletionSource<int> ();
			_stopSignalers[connection.RemoteEndPoint] = stopSignaler;
			cancellationToken.Register (() => stopSignaler.TrySetCanceled (), false);
			_startedEvent.Set ();
			return stopSignaler.Task;
		}

		internal void FinishHandlingConnection (IPEndPoint remoteEndPoint)
		{
			_stopSignalers[remoteEndPoint].TrySetResult (0);
		}
	}
}
