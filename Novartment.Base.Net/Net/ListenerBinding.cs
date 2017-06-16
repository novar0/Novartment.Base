using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Novartment.Base.Net
{
	internal class ListenerBinding :
		IDisposable
	{
		private readonly ITcpListener _listener;
		private readonly ITcpConnectionProtocol _protocol;
		private readonly ILogger _logger;
		private readonly ConcurrentDictionary<IPEndPoint, ConnectedClient> _connections = new ConcurrentDictionary<IPEndPoint, ConnectedClient> ();
		private CancellationTokenSource _acceptConnectionsLoopCancellation = null;
		private Task _acceptingConnectionsTask = Task.CompletedTask;

		internal ListenerBinding (ITcpListener listener, ITcpConnectionProtocol protocol, ILogger logger)
		{
			_listener = listener;
			_protocol = protocol;
			_logger = logger;
		}

		internal ICollection<ConnectedClient> ConnectedClients => _connections.Values;

		internal Task AcceptingConnectionsTask => _acceptingConnectionsTask;

		public void Dispose ()
		{
			foreach (var client in _connections.Values)
			{
				client?.Dispose ();
			}

			SafeMethods.TryDispose (_listener);
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
			_logger?.LogInformation ("Started accepting connections to " + _listener.LocalEndpoint);
			Task protocolTask;
			ITcpConnection connection;
			try
			{
				while (!_acceptConnectionsLoopCancellation.IsCancellationRequested)
				{
					connection = await _listener.AcceptTcpClientAsync (_acceptConnectionsLoopCancellation.Token).ConfigureAwait (false);
					_logger?.LogTrace (FormattableString.Invariant (
						$"Client connected {connection.RemoteEndPoint} -> {connection.LocalEndPoint} "));
					if (_acceptConnectionsLoopCancellation.IsCancellationRequested)
					{
						break;
					}

					// на каждое подключением создаём источник отмены, потому что каждое может отменятся независимо от других по тайм-ауту
					var cts = new CancellationTokenSource ();
					try
					{
						protocolTask = _protocol.StartAsync (connection, cts.Token);
					}
					catch
					{
						connection.Dispose ();
						throw;
					}

					var finishingTask = AcceptedConnectionFinalizer ();
					var client = new ConnectedClient (connection, finishingTask, cts);
					_connections[connection.RemoteEndPoint] = client;
				}
			}
			finally
			{
				Interlocked.Exchange (ref _acceptConnectionsLoopCancellation, null)?.Dispose (); // отменять уже нечего
				_logger?.LogInformation ("Stopped accepting connections to " + _listener.LocalEndpoint);
			}

			async Task AcceptedConnectionFinalizer ()
			{
				try
				{
					await protocolTask.ConfigureAwait (false);
				}
				finally
				{
					_connections.TryRemove (connection.RemoteEndPoint, out ConnectedClient connectionProtocolTask);
					connection.Dispose ();
					_logger?.LogTrace (FormattableString.Invariant (
						$"Client disconnected {connection.RemoteEndPoint} -> {connection.LocalEndPoint} "));
				}
			}
		}
	}
}
