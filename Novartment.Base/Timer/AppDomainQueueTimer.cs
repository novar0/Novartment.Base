using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Novartment.Base
{
	// Реализация таймера на основе System.Timers.Timer не нужна потому что он основан на
	// System.Threading.Timer + возможность вызова через ISynchronizeInvoke,
	// а ISynchronizeInvoke используется только в Windows.Forms.

	/// <summary>
	/// A timer based on the System.Threading.Timer.
	/// Calls the specified callback with the specified parameter when triggered.
	/// </summary>
	/// <typeparam name="TState">Type of the state object, passed to the callback.</typeparam>
	public class AppDomainQueueTimer<TState> :
		BaseTimer<TState>
	{
		private readonly Timer _timer;
		private readonly object _integrityLocker = new object ();
		private TimeSpan _interval;
		private bool _enabled;

		/// <summary>
		/// Initializes a new instance of the AppDomainQueueTimer class
		/// that calls the specified callback with the specified parameter when triggered.
		/// </summary>
		/// <param name="callback">The callback, that will be called when the timer is triggered.</param>
		/// <param name="state">The state object, that will be passed to callback when the timer is triggered.</param>
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
		/// Gets or sets the trigger interval.
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
		/// Gets state of the time. True means that the timer is started, and false means that the timer is stopped.
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
		/// Starts the timer.
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
		/// Stops the timer.
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
		/// Performs freeing and releasing resources.
		/// </summary>
		public override void Dispose ()
		{
			lock (_integrityLocker)
			{
				_timer.Dispose ();
				_enabled = false;
			}
			GC.SuppressFinalize (this);
		}
	}
}
