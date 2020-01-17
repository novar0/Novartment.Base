using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net.Test
{
	internal class TcpListenerMock :
		ITcpListener
	{
		private readonly AutoResetEvent _stopedEvent = new AutoResetEvent (false);
		private readonly IPEndPoint _endpoint;
		private readonly AsyncQueue<IPEndPoint> _incomingConnections = new AsyncQueue<IPEndPoint> ();
		private bool _isStarted = false;

		internal TcpListenerMock (IPEndPoint endpoint)
		{
			_endpoint = endpoint;
		}

		public IPEndPoint LocalEndpoint => _endpoint;

		internal bool IsStarted => _isStarted;

		internal EventWaitHandle StopedEvent => _stopedEvent;

		public void Start ()
		{
			_isStarted = true;
		}

		public void Stop ()
		{
			_stopedEvent.Set ();
			_isStarted = false;
		}

		public async Task<ITcpConnection> AcceptTcpClientAsync (CancellationToken cancellationToken = default)
		{
			if (!_isStarted)
			{
				throw new InvalidOperationException ();
			}

			var remoteEndpoint = await _incomingConnections.Dequeue (cancellationToken).ConfigureAwait (false);
			return new TcpConnectionMock (
					new IPHostEndPoint (_endpoint),
					new IPHostEndPoint (remoteEndpoint),
					default);
		}

		internal void SimulateIncomingConnection (IPEndPoint remoteEndpoint)
		{
			_incomingConnections.Enqueue (remoteEndpoint);
		}

		internal class AsyncQueue<TItem>
		{
			private readonly Queue<TItem> _producers = new Queue<TItem> ();
			private readonly Queue<TaskCompletionSource<TItem>> _consumers = new Queue<TaskCompletionSource<TItem>> ();

			internal void Enqueue (TItem item)
			{
				TaskCompletionSource<TItem> consumer;
				lock (_producers)
				{
					do
					{
						if (_consumers.Count < 1)
						{
							_producers.Enqueue (item);
							return;
						}

						consumer = _consumers.Dequeue ();
					}
					while (consumer.Task.IsCompleted);
				}

				consumer.TrySetResult (item);
			}

			internal Task<TItem> Dequeue (CancellationToken cancellationToken)
			{
				lock (_producers)
				{
					if (_producers.Count > 0)
					{
						return Task.FromResult (_producers.Dequeue ());
					}

					if (cancellationToken.IsCancellationRequested)
					{
						return Task.FromCanceled<TItem> (cancellationToken);
					}

					var consumer = new TaskCompletionSource<TItem> ();
					if (cancellationToken.CanBeCanceled)
					{
						cancellationToken.Register (() => consumer.TrySetCanceled (), false);
					}

					_consumers.Enqueue (consumer);
					return consumer.Task;
				}
			}
		}
	}
}
