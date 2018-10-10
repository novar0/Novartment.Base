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
			this.State = ProcessState.Canceling;
		}

		private async Task AcceptConnectionsLoop ()
		{
			_logger?.LogInformation (FormattableString.Invariant (
				$"Listener {_listener.LocalEndpoint}: started accepting connections."));
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
						$"Listener {_listener.LocalEndpoint}: new client connected {connection.RemoteEndPoint}. Clients connected: {_connections.Count}."));

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

					if (protocolTask.IsCompleted)
					{
						connection.Dispose ();
						_logger?.LogTrace (FormattableString.Invariant (
							$"Listener {_listener.LocalEndpoint}: client disconnected {connection.RemoteEndPoint}. Clients connected: {_connections.Count}."));
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
				Interlocked.Exchange (ref _acceptConnectionsLoopCancellation, null)?.Dispose (); // отменять уже нечего
				_logger?.LogInformation ($"Listener {_listener.LocalEndpoint}: stopped accepting connections. Clients connected: {_connections.Count}.");
			}

			void AcceptedConnectionFinalizer (Task prevTask, object cntnData)
			{
				var cntn = (ITcpConnection)cntnData;
				_connections.TryRemove (cntn.RemoteEndPoint, out ConnectedClient connectionProtocolTask);
				cntn.Dispose ();
				_logger?.LogTrace (FormattableString.Invariant (
					$"Listener {_listener.LocalEndpoint}: client disconnected {cntn.RemoteEndPoint}. Clients connected: {_connections.Count}."));
			}
		}
	}
}
