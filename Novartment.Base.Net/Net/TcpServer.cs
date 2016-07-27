using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Tasks;
using Novartment.Base.Collections;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Сервер, асинхронно принимающий и обрабатывающий множество TCP-подключений на множестве конечных точек.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class TcpServer :
		IDisposable
	{
		private readonly Func<IPEndPoint, ITcpListener> _listenerFactory;
		private readonly ILogWriter _logger;
		private readonly ConcurrentBag<ListenerBinding> _bindings = new ConcurrentBag<ListenerBinding> ();
		private readonly Timer _watchdogTimer;

		private int _timerCallbackRunnig = 0;
		private TimeSpan _connectionTotalTimeout = TimeSpan.FromMinutes (10.0);
		private TimeSpan _connectionIdleTimeout = TimeSpan.FromMinutes (1.0);

		/// <summary>
		/// Gets or sets total timeout of connections. Minimum one millisecond or Timeout.InfiniteTimeSpan.
		/// </summary>
		public TimeSpan ConnectionTimeout
		{
			get { return _connectionTotalTimeout; }
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
			get { return _connectionIdleTimeout; }
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
		/// Инициализирует новый экземпляр TcpServer, создающий объекты-прослушиватели подключений используя указанную фабрику
		/// и пишущий информацию о событиях в указанный журнал.
		/// </summary>
		/// <param name="listenerFactory">Фабрика для создания объектов-прослушивателей на указанной конечной точке.</param>
		/// <param name="logger">Журнал для записи событий. Укажите null если запись не нужна.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public TcpServer (Func<IPEndPoint, ITcpListener> listenerFactory, ILogWriter logger = null)
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
		/// Запускает фоновое прослушивание на указанной конечной точке.
		/// Для каждого подключения будет вызван указанный обработчик.
		/// </summary>
		/// <param name="endPoint">Конечная точки, на которой будет производиться прослушивание подключений.</param>
		/// <param name="protocol">Обработчик для входящих подключений.</param>
		public void AddListenEndpoint (
			IPEndPoint endPoint,
			ITcpConnectionProtocol protocol)
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
			binding.Start ();
			_bindings.Add (binding);
		}

		/// <summary>
		/// Asynchronously stops server optionaly aborting active connections.
		/// </summary>
		/// <param name="abortActiveConnections">Specify True to abort active connections.</param>
		/// <returns>Task representing stoping server.</returns>
		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Info(System.String)",
			Justification = "String is not exposed to the end user and will not be localized."),
		SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Trace(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public Task StopAsync (bool abortActiveConnections)
		{
			_logger?.Info (FormattableString.Invariant ($"Stopping listeners: {_bindings.Count}"));

			// для каждого из прослушивателей и клиентов собираем задачу означающую завершение
			var tasksToWait = new ArrayList<Task> ();
			var listeners = 0;
			var clients = 0;

			ListenerBinding binding;
			while (_bindings.TryTake (out binding))
			{
				binding.Cancel ();
				tasksToWait.Add (binding.AcceptingConnectionsTask);
				listeners++;

				// останавливаем все сессии
				foreach (var client in binding.ConnectedClients)
				{
					if (abortActiveConnections)
					{
						_logger?.Trace (FormattableString.Invariant ($"Aborting client {client.EndPoint}"));
					}
					var task = client.EndProcessing (abortActiveConnections);
					tasksToWait.Add (task);
					clients++;
				}
			}

			_logger?.Trace (FormattableString.Invariant ($"Waiting completion of {listeners} listeners and {clients} connections."));

			var globalWaitTask = Task.WhenAll (tasksToWait);
			// игнорируем исключения
			return globalWaitTask.ContinueWith (t => { },
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion,
				TaskScheduler.Default);
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

		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Warn(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		private void WatchdogTimerCallback (object state)
		{
			// защищаемся от реитерации
			var oldValue = Interlocked.CompareExchange (ref _timerCallbackRunnig, 1, 0);
			if (oldValue != 0)
			{
				return;
			}
			var connectionTimeout = _connectionTotalTimeout;
			var idleTimeout = _connectionIdleTimeout;

			foreach (var binding in _bindings)
			{
				foreach (var client in binding.ConnectedClients)
				{
					if ((connectionTimeout != Timeout.InfiniteTimeSpan) && (client.Duration >= connectionTimeout))
					{
						_logger?.Warn (FormattableString.Invariant (
							$"Disconnecting client {client.EndPoint} because of connection time ({client.Duration}) exceeds limit ({connectionTimeout})."));
						client.EndProcessing (true);
					}
					else
					{
						if ((connectionTimeout != Timeout.InfiniteTimeSpan) && (client.IdleDuration >= idleTimeout))
						{
							_logger?.Warn (FormattableString.Invariant (
								$"Disconnecting client {client.EndPoint} because of idle time ({client.IdleDuration}) exceeds limit ({idleTimeout})."));
							client.EndProcessing (true);
						}
					}
				}
			}
			_timerCallbackRunnig = 0;
		}

		/// <summary>
		/// Освобождает все используемые ресурсы.
		/// </summary>
		[SuppressMessage ("Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type."),
		SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			_watchdogTimer.Dispose ();

			// освобождаем все прослушиватели и соединения
			ListenerBinding binding;
			while (_bindings.TryTake (out binding))
			{
				binding?.Dispose ();
			}
		}
	}
}
