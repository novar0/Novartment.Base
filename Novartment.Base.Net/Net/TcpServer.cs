using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Novartment.Base.Collections;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Сервер, асинхронно принимающий и обрабатывающий множество TCP-подключений на множестве конечных точек.
	/// </summary>
	public sealed class TcpServer :
		IDisposable
	{
		private readonly Func<IPEndPoint, ITcpListener> _listenerFactory;
		private readonly ILogger _logger;
		private readonly ConcurrentDictionary<IPEndPoint, ListenerBinding> _bindings = new ConcurrentDictionary<IPEndPoint, ListenerBinding> ();
		private readonly Timer _watchdogTimer;

		private int _timerCallbackRunnig = 0;
		private TimeSpan _connectionTotalTimeout = TimeSpan.FromMinutes (10.0);
		private TimeSpan _connectionIdleTimeout = TimeSpan.FromMinutes (1.0);
		private TimeSpan _protocolCancelingTimeout = TimeSpan.FromMinutes (1.0);

		/// <summary>
		/// Инициализирует новый экземпляр TcpServer, создающий объекты-прослушиватели подключений используя указанную фабрику
		/// и пишущий информацию о событиях в указанный журнал.
		/// </summary>
		/// <param name="listenerFactory">Фабрика для создания объектов-прослушивателей на указанной конечной точке.</param>
		/// <param name="logger">Журнал для записи событий. Укажите null если запись не нужна.</param>
		public TcpServer (Func<IPEndPoint, ITcpListener> listenerFactory, ILogger<TcpServer> logger = null)
		{
			if (listenerFactory == null)
			{
				throw new ArgumentNullException (nameof (listenerFactory));
			}

			Contract.EndContractBlock ();

			_listenerFactory = listenerFactory;
			_logger = logger;
			var interval = CalculateWatchdogTimerInterval ();
			_watchdogTimer = new Timer (WatchdogTimerCallback, null, interval, interval);
		}

		/// <summary>
		/// Gets or sets total timeout of connections. Minimum one millisecond or Timeout.InfiniteTimeSpan.
		/// </summary>
		public TimeSpan ConnectionTimeout
		{
			get => _connectionTotalTimeout;

			set
			{
				if ((value != Timeout.InfiniteTimeSpan) && (value.TotalMilliseconds < 1.0))
				{
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				_connectionTotalTimeout = value;
				var interval = CalculateWatchdogTimerInterval ();
				_watchdogTimer.Change (interval, interval);
			}
		}

		/// <summary>
		/// Gets or sets timeout of idle connections. Minimum one millisecond or Timeout.InfiniteTimeSpan.
		/// </summary>
		public TimeSpan ConnectionIdleTimeout
		{
			get => _connectionIdleTimeout;

			set
			{
				if ((value != Timeout.InfiniteTimeSpan) && (value.TotalMilliseconds < 1.0))
				{
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				_connectionIdleTimeout = value;
				var interval = CalculateWatchdogTimerInterval ();
				_watchdogTimer.Change (interval, interval);
			}
		}

		/// <summary>
		/// Gets or sets timeout of canceling protocols. Minimum one millisecond or Timeout.InfiniteTimeSpan.
		/// </summary>
		public TimeSpan ProtocolCancelingTimeout
		{
			get => _protocolCancelingTimeout;

			set
			{
				if ((value != Timeout.InfiniteTimeSpan) && (value.TotalMilliseconds < 1.0))
				{
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				_protocolCancelingTimeout = value;
				var interval = CalculateWatchdogTimerInterval ();
				_watchdogTimer.Change (interval, interval);
			}
		}

		/// <summary>
		/// Запускает фоновое прослушивание на указанной конечной точке.
		/// Для каждого подключения будет вызван указанный обработчик.
		/// </summary>
		/// <param name="endPoint">Конечная точки, на которой будет производиться прослушивание подключений.</param>
		/// <param name="protocol">Обработчик для входящих подключений.</param>
		public void AddListenEndpoint (IPEndPoint endPoint, ITcpConnectionProtocol protocol)
		{
			if (endPoint == null)
			{
				throw new ArgumentNullException (nameof (endPoint));
			}

			if (protocol == null)
			{
				throw new ArgumentNullException (nameof (protocol));
			}

			Contract.EndContractBlock ();

			// TODO: добавить проверку чтобы не стартовала пока идёт остановка в методе Stop()
			ITcpListener listener = _listenerFactory.Invoke (endPoint);
			var binding = new ListenerBinding (listener, protocol, _logger);
			if (!_bindings.TryAdd (endPoint, binding))
			{
				throw new InvalidOperationException ($"Specified EndPoint {endPoint} already listening.");
			}

			binding.Start ();
		}

		/// <summary>
		/// Asynchronously stops server optionaly aborting active connections.
		/// </summary>
		/// <param name="abortActiveConnections">Specify True to abort active connections.</param>
		/// <returns>Task representing stoping server.</returns>
		public Task StopAsync (bool abortActiveConnections)
		{
			_logger?.LogInformation (FormattableString.Invariant ($"Stopping listeners: {_bindings.Count}"));

			// для каждого из прослушивателей и клиентов собираем задачу означающую завершение
			var tasksToWait = new ArrayList<Task> ();
			var listeners = 0;
			var clients = 0;

			var bindingsToDelete = new ArrayList<IPEndPoint> (_bindings.Count);

			foreach (var bindingEntry in _bindings)
			{
				var binding = bindingEntry.Value;

				binding.Cancel ();

				// освободим и удалим сразу те привязки, в которых нет активных клиентов
				// привязки, в которых есть активные клиенты, будут опрашиваться по таймеру и удалятся после отключения клиентов
				if (binding.ConnectedClients.Count < 1)
				{
					binding.Dispose ();
					bindingsToDelete.Add (bindingEntry.Key);
				}
				else
				{
					tasksToWait.Add (binding.AcceptingConnectionsTask);
					listeners++;

					// останавливаем все сессии
					foreach (var client in binding.ConnectedClients)
					{
						if (abortActiveConnections)
						{
							_logger?.LogTrace (FormattableString.Invariant ($"Aborting client {client.EndPoint}."));
						}

						var task = client.EndProcessing (abortActiveConnections);
						tasksToWait.Add (task);
						clients++;
					}
				}
			}

			foreach (var endPoint in bindingsToDelete)
			{
				_bindings.TryRemove (endPoint, out var notUsed);
			}

			if (tasksToWait.Count < 1)
			{
				return Task.CompletedTask;
			}

			_logger?.LogTrace (FormattableString.Invariant ($"Waiting completion of {listeners} listeners and {clients} connections."));

			var globalWaitTask = Task.WhenAll (tasksToWait);

			// игнорируем исключения: никому не интересны проблемы в процессе "умирания" прослушивателей и протоколов
			return globalWaitTask.ContinueWith (
				t => { },
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion,
				TaskScheduler.Default);
		}

		/// <summary>
		/// Освобождает все используемые ресурсы.
		/// </summary>
		public void Dispose ()
		{
			_watchdogTimer.Dispose ();

			// освобождаем все прослушиватели и соединения
			foreach (var binding in _bindings.Values)
			{
				binding?.Dispose ();
			}

			_bindings.Clear ();
		}

		private int CalculateWatchdogTimerInterval ()
		{
			if ((_connectionTotalTimeout == Timeout.InfiniteTimeSpan) && (_connectionIdleTimeout == Timeout.InfiniteTimeSpan))
			{
				return Timeout.Infinite;
			}

			double inverval;
			if (_connectionIdleTimeout == Timeout.InfiniteTimeSpan)
			{
				inverval = _connectionTotalTimeout.TotalMilliseconds;
			}
			else
			{
				if (_connectionTotalTimeout == Timeout.InfiniteTimeSpan)
				{
					inverval = _connectionIdleTimeout.TotalMilliseconds;
				}
				else
				{
					inverval = Math.Min (_connectionTotalTimeout.TotalMilliseconds, _connectionIdleTimeout.TotalMilliseconds);
				}
			}

			// точность соблюдения времени будет 1/20 тоесть 5%
			inverval /= 20.0;

			// не меньше 1мс и не больше 1с
			return Math.Min (1000, Math.Max (1, (int)inverval));
		}

#pragma warning disable CA1801 // Review unused parameters
		private void WatchdogTimerCallback (object state)
#pragma warning restore CA1801 // Review unused parameters
		{
			// защищаемся от реитерации
			var oldValue = Interlocked.CompareExchange (ref _timerCallbackRunnig, 1, 0);
			if ((oldValue != 0) || (_connectionTotalTimeout == Timeout.InfiniteTimeSpan))
			{
				return;
			}

			var bindingsToDelete = new ArrayList<IPEndPoint> (_bindings.Count);

			foreach (var bindingEntry in _bindings)
			{
				var binding = bindingEntry.Value;

				// посчитаем сколько у привязки живых клиентов чтобы позже удалить остановленные привязки, в которых их нет
				var aliveClients = 0;
				foreach (var client in binding.ConnectedClients)
				{
					switch (client.State)
					{
						case ProcessState.InProgress:
							if (client.Duration >= _connectionTotalTimeout)
							{
								_logger?.LogWarning (FormattableString.Invariant (
									$"Canceling protocol with client {client.EndPoint} because of connection time ({client.Duration}) exceeds limit ({_connectionTotalTimeout})."));
								client.EndProcessing (true);
							}
							else
							{
								if (client.IdleDuration >= _connectionIdleTimeout)
								{
									_logger?.LogWarning (FormattableString.Invariant (
										$"Canceling protocol with client {client.EndPoint} because of idle time exceeds limit {_connectionIdleTimeout}."));
									client.EndProcessing (true);
								}
							}

							aliveClients++;
							break;
						case ProcessState.Canceling:
							if ((client.Duration - client.CancelDurationStart) >= _protocolCancelingTimeout)
							{
								_logger?.LogWarning (FormattableString.Invariant (
									$"Disconnecting client {client.EndPoint} because of protocol not finished canceling in {_protocolCancelingTimeout}."));
								client.Dispose ();
							}
							else
							{
								aliveClients++;
							}

							break;
						case ProcessState.Disposed:
							break;
					}
				}

				if (((binding.State == ProcessState.Canceling) || (binding.State == ProcessState.Disposed)) && (aliveClients < 1))
				{
					binding.Dispose ();
					bindingsToDelete.Add (bindingEntry.Key);
				}
			}

			foreach (var endPoint in bindingsToDelete)
			{
				_bindings.TryRemove (endPoint, out var notUsed);
			}

			_timerCallbackRunnig = 0;
		}
	}
}
