using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Novartment.Base.Collections;

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
		private Task _processingTask = Task.CompletedTask;

		internal ListenerBinding (ITcpListener listener, ITcpConnectionProtocol protocol, ILogger logger)
		{
			_listener = listener;
			_protocol = protocol;
			_logger = logger;
		}

		internal ProcessState State { get; private set; } = ProcessState.InProgress;

		internal ICollection<ConnectedClient> ConnectedClients => _connections.Values;

#pragma warning disable CA1063 // Implement IDisposable Correctly
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			SafeMethods.TryDispose (_listener);
			foreach (var client in _connections.Values)
			{
				client?.Dispose ();
			}

			this.State = ProcessState.Disposed;
		}

		internal Task Start ()
		{
			if (!_processingTask.IsCompleted)
			{
				throw new InvalidOperationException ($"Listener {_listener.LocalEndpoint} already started.");
			}

			_logger?.LogInformation (FormattableString.Invariant (
				$"Listener {_listener.LocalEndpoint} starting to accept connections."));
			_acceptConnectionsLoopCancellation = new CancellationTokenSource ();
			_listener.Start ();
			_processingTask = AcceptConnectionsLoop ();
			return _processingTask;
		}

		internal void Stop (bool abortActiveConnections)
		{
			_logger?.LogInformation (FormattableString.Invariant ($"Listener {_listener.LocalEndpoint} stopping to accept connections."));
			_listener.Stop ();
			_acceptConnectionsLoopCancellation?.Cancel ();
			this.State = ProcessState.Canceling;
			if (abortActiveConnections)
			{
				foreach (var client in _connections.Values)
				{
					_logger?.LogTrace (FormattableString.Invariant ($"Aborting client {client.EndPoint}."));
					client.AbortProcessing ();
				}
			}
		}

		private async Task AcceptConnectionsLoop ()
		{
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
						$"Listener {_listener.LocalEndpoint} accepted connection from {connection.RemoteEndPoint}. Starting protocol {_protocol.GetType ().FullName}. Other clients connected: {_connections.Count}."));

					// на каждое подключением создаём источник отмены, потому что каждое может отменяться независимо от других по тайм-ауту
					var cts = new CancellationTokenSource ();
					Task protocolTask = null;
					try
					{
						protocolTask = _protocol
							.StartAsync (connection, cts.Token)
							.ContinueWith (
								AcceptedConnectionFinalizer,
								connection,
								CancellationToken.None,
								TaskContinuationOptions.ExecuteSynchronously,
								TaskScheduler.Default);
					}
					catch (Exception excpt)
					{
						_logger?.LogError (FormattableString.Invariant (
							$"Protocol starting faulted with {connection.RemoteEndPoint}. {ExceptionDescriptionProvider.CreateDescription (excpt).GetShortInfo ()}"));
					}

					if ((protocolTask == null) || protocolTask.IsCompleted)
					{
						CloseConnection (connection);
					}
					else
					{
						var client = new ConnectedClient (connection, protocolTask, cts);
						_connections[connection.RemoteEndPoint] = client;
					}
				}
			}
			catch (TaskCanceledException)
			{
				// отмена - это просто выход из цикла принятия коннектов
			}
			finally
			{
				Interlocked.Exchange (ref _acceptConnectionsLoopCancellation, null)?.Dispose ();
			}

			_logger?.LogTrace ($"Listener {_listener.LocalEndpoint} stopped to accept connections. Сlients currently connected: {_connections.Count}.");
			var tasksToWait = new ArrayList<Task> (_connections.Count);
			foreach (var client in _connections.Values)
			{
				if (!client.ProcessingTask.IsCompleted)
				{
					tasksToWait.Add (client.ProcessingTask);
				}
			}

			if (tasksToWait.Count > 0)
			{
				_logger?.LogTrace (FormattableString.Invariant ($"Listener {_listener.LocalEndpoint} waiting completion of {tasksToWait.Count} connections."));
				await Task.WhenAll (tasksToWait).ConfigureAwait (false);
			}

			void AcceptedConnectionFinalizer (Task prevTask, object cntnData)
			{
				var cntn = (ITcpConnection)cntnData;
				_connections.TryRemove (cntn.RemoteEndPoint, out ConnectedClient connectionProtocolTask);
				if (prevTask.IsCanceled)
				{
					_logger?.LogDebug (FormattableString.Invariant (
						$"Protocol canceled with {cntn.RemoteEndPoint}."));
				}
				else
				{
					if (prevTask.IsFaulted)
					{
						_logger?.LogError (FormattableString.Invariant (
							$"Protocol faulted with {cntn.RemoteEndPoint}. {ExceptionDescriptionProvider.CreateDescription (prevTask.Exception).GetShortInfo ()}"));
					}
				}

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
