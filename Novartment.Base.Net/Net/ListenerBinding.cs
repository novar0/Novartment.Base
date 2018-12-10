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

		internal ProcessState State { get; private set; } = ProcessState.InProgress;

		internal ICollection<ConnectedClient> ConnectedClients => _connections.Values;

		internal Task AcceptingConnectionsTask => _acceptingConnectionsTask;

#pragma warning disable CA1063 // Implement IDisposable Correctly
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			foreach (var client in _connections.Values)
			{
				client?.Dispose ();
			}

			SafeMethods.TryDispose (_listener);
			this.State = ProcessState.Disposed;
		}

		internal void Start ()
		{
			if (!_acceptingConnectionsTask.IsCompleted)
			{
				throw new InvalidOperationException ($"Listener {_listener.LocalEndpoint} already started.");
			}

			_acceptConnectionsLoopCancellation = new CancellationTokenSource ();
			_listener.Start ();
			_acceptingConnectionsTask = AcceptConnectionsLoop ();
		}

		internal void Cancel ()
		{
			_listener.Stop ();
			_acceptConnectionsLoopCancellation?.Cancel ();
			this.State = ProcessState.Canceling;
		}

		private async Task AcceptConnectionsLoop ()
		{
			_logger?.LogInformation (FormattableString.Invariant (
				$"Listener {_listener.LocalEndpoint} starting to accept connections."));
			Task protocolTask;
			ITcpConnection connection;
			try
			{
				while (!_acceptConnectionsLoopCancellation.IsCancellationRequested)
				{
					connection = await _listener.AcceptTcpClientAsync (_acceptConnectionsLoopCancellation.Token).ConfigureAwait (false);
					if (_acceptConnectionsLoopCancellation.IsCancellationRequested)
					{
						break;
					}

					_logger?.LogTrace (FormattableString.Invariant (
						$"Listener {_listener.LocalEndpoint} accepted connection from {connection.RemoteEndPoint}. Other clients connected: {_connections.Count}."));

					// на каждое подключением создаём источник отмены, потому что каждое может отменяться независимо от других по тайм-ауту
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

					if (protocolTask.IsCompleted)
					{
						CloseConnection (connection);
					}
					else
					{
						var client = new ConnectedClient (connection, protocolTask, cts);
						_connections[connection.RemoteEndPoint] = client;
						var notUsed = protocolTask.ContinueWith (
							AcceptedConnectionFinalizer,
							connection,
							CancellationToken.None,
							TaskContinuationOptions.ExecuteSynchronously,
							TaskScheduler.Default);
					}
				}
			}
			finally
			{
				Interlocked.Exchange (ref _acceptConnectionsLoopCancellation, null)?.Dispose ();
				_logger?.LogInformation ($"Listener {_listener.LocalEndpoint} stopping to accept connections. Сlients currently connected: {_connections.Count}.");
			}

			void AcceptedConnectionFinalizer (Task prevTask, object cntnData)
			{
				var cntn = (ITcpConnection)cntnData;
				_connections.TryRemove (cntn.RemoteEndPoint, out ConnectedClient connectionProtocolTask);
				CloseConnection (cntn);
			}
		}

		private void CloseConnection (ITcpConnection connection)
		{
			connection.Dispose ();
			_logger?.LogTrace (FormattableString.Invariant (
				$"Listener {_listener.LocalEndpoint} disconnected client {connection.RemoteEndPoint}. Connected clients left: {_connections.Count}."));
		}
	}
}
