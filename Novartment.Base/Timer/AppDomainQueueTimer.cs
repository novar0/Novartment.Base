using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Реализация таймера на основе System.Threading.Timer.
	/// При срабатывании вызывает указанный делегат с указанным параметром.
	/// </summary>
	/// <typeparam name="TState">Тип объекта, передаваемый в делегат срабатывания таймера.</typeparam>
	/// <remarks>Реализация таймера на основе System.Timers.Timer не нужна потому что он основан на
	/// System.Threading.Timer + возможность вызова через ISynchronizeInvoke,
	/// а ISynchronizeInvoke используется только в Windows.Forms.
	/// </remarks>
	public class AppDomainQueueTimer<TState> :
		BaseTimer<TState>
	{
		private readonly Timer _timer;
		private readonly object _integrityLocker = new object ();
		private TimeSpan _interval;
		private bool _enabled;

		/// <summary>
		/// Инициализирует новый экземпляр AppDomainQueueTimer, вызывающий при срабатывании указанный делегат с указанным параметром.
		/// </summary>
		/// <param name="callback">Делегат, вызываемый при срабатывании таймера.</param>
		/// <param name="state">Объект, передавамый в делегат при срабатывании таймера.</param>
		public AppDomainQueueTimer (Action<TState> callback, TState state)
			: base (callback, state)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}

			Contract.EndContractBlock ();

			_enabled = false;
			_interval = TimeSpan.FromMilliseconds (0.0);
			_timer = new Timer (base.DoCallback, state, _interval, _interval);
		}

		/// <summary>
		/// Получает или устанавливает интервал срабатывания таймера.
		/// </summary>
		public override TimeSpan Interval
		{
			get
			{
				lock (_integrityLocker)
				{
					return _interval;
				}
			}

			set
			{
				lock (_integrityLocker)
				{
					_interval = value;
					if (_enabled)
					{
						_timer.Change (_interval, _interval);
					}
				}
			}
		}

		/// <summary>
		/// Получает состояние таймера. True если таймер запущен, иначе false.
		/// </summary>
		public override bool Enabled
		{
			get
			{
				lock (_integrityLocker)
				{
					return _enabled;
				}
			}
		}

		/// <summary>
		/// Запускает таймер.
		/// </summary>
		public override void Start ()
		{
			lock (_integrityLocker)
			{
				_timer.Change (_interval, _interval);
				_enabled = true;
			}
		}

		/// <summary>
		/// Останавливает таймер.
		/// </summary>
		public override void Stop ()
		{
			lock (_integrityLocker)
			{
				_timer.Change (Timeout.Infinite, Timeout.Infinite);
				_enabled = false;
			}
		}

		/// <summary>
		/// Освобождает занимаемые объектом ресурсы.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type.")]
		[SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public override void Dispose ()
		{
			lock (_integrityLocker)
			{
				_timer.Dispose ();
				_enabled = false;
			}
		}
	}
}
