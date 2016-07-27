using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	internal class ListenerBinding :
		IDisposable
	{
		private readonly ITcpListener _listener;
		private readonly ITcpConnectionProtocol _protocol;
		private readonly ILogWriter _logger;
		private readonly ConcurrentDictionary<IPEndPoint, ConnectedClient> _connections = new ConcurrentDictionary<IPEndPoint, ConnectedClient> ();
		private CancellationTokenSource _acceptConnectionsLoopCancellation = null;
		private Task _acceptingConnectionsTask = Task.CompletedTask;

		internal ICollection<ConnectedClient> ConnectedClients => _connections.Values;

		internal Task AcceptingConnectionsTask => _acceptingConnectionsTask;

		internal ListenerBinding (ITcpListener listener, ITcpConnectionProtocol protocol, ILogWriter logger)
		{
			_listener = listener;
			_protocol = protocol;
			_logger = logger;
		}

		internal void Start ()
		{
			if (!_acceptingConnectionsTask.IsCompleted)
			{
				throw new InvalidOperationException ($"Accepting connections on {_listener.LocalEndpoint} already started.");
			}

			_acceptConnectionsLoopCancellation = new CancellationTokenSource ();
			_listener.Start ();
			_acceptingConnectionsTask = AcceptConnectionsLoop ();
		}

		internal void Cancel ()
		{
			_listener.Stop ();
			_acceptConnectionsLoopCancellation?.Cancel ();
		}

		private async Task AcceptConnectionsLoop ()
		{
			_logger?.Info ("Started accepting connections to " + _listener.LocalEndpoint);
			try
			{
				ITcpConnection connection;
				while (!_acceptConnectionsLoopCancellation.IsCancellationRequested)
				{
					connection = await _listener.AcceptTcpClientAsync (_acceptConnectionsLoopCancellation.Token).ConfigureAwait (false);
					_logger?.Trace (FormattableString.Invariant (
						$"Client connected {connection.RemoteEndPoint} -> {connection.LocalEndPoint} "));
					if (_acceptConnectionsLoopCancellation.IsCancellationRequested)
					{
						break;
					}
					// на каждое подключением создаём источник отмены, потому что каждое может отменятся независимо от других по тайм-ауту
					var cts = new CancellationTokenSource ();
					Task startingTask;
					try
					{
						startingTask = _protocol.StartAsync (connection, cts.Token);
					}
					catch
					{
						connection.Dispose ();
						throw;
					}
					var finishingTask = AcceptedConnectionFinalizer (startingTask, connection);
					var client = new ConnectedClient (connection, finishingTask, cts);
					_connections[connection.RemoteEndPoint] = client;
				}
			}
			finally
			{
				Interlocked.Exchange (ref _acceptConnectionsLoopCancellation, null)?.Dispose (); // отменять уже нечего
				_logger?.Info ("Stopped accepting connections to " + _listener.LocalEndpoint);
			}
		}

		private async Task AcceptedConnectionFinalizer (Task protocolProcessing, ITcpConnection connection)
		{
			try
			{
				await protocolProcessing.ConfigureAwait (false);
			}
			finally
			{
				ConnectedClient connectionProtocolTask;
				_connections.TryRemove (connection.RemoteEndPoint, out connectionProtocolTask);
				connection.Dispose ();
			}
		}

		public void Dispose ()
		{
			foreach (var client in _connections.Values)
			{
				client?.Dispose ();
			}
			SafeMethods.TryDispose (_listener);
		}
	}
}
