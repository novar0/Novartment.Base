﻿using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// A base class for timers.
	/// </summary>
	/// <typeparam name="TState">Type of the state object, passed to the callback.</typeparam>
	public abstract class BaseTimer<TState> :
		ITimer
	{
		private readonly Action<TState> _callback;
		private readonly TState _state;

		/// <summary>
		/// Initializes a new instance of the BaseTimer class
		/// that calls the specified callback with the specified parameter when triggered.
		/// </summary>
		/// <param name="callback">The callback, that will be called when the timer is triggered.</param>
		/// <param name="state">The state object, that will be passed to callback when the timer is triggered.</param>
		protected BaseTimer (Action<TState> callback, TState state)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}

			Contract.EndContractBlock ();

			_callback = callback;
			_state = state;
		}

		/// <summary>
		/// Gets or sets the trigger interval.
		/// </summary>
		public abstract TimeSpan Interval { get; set; }

		/// <summary>
		/// Gets state of the time. True means that the timer is started, and false means that the timer is stopped.
		/// </summary>
		public abstract bool Enabled { get; }

		/// <summary>
		/// Starts the timer.
		/// </summary>
		public abstract void Start ();

#pragma warning disable CA1716 // Identifiers should not match keywords
		/// <summary>
		/// Stops the timer.
		/// </summary>
		public abstract void Stop ();
#pragma warning restore CA1716 // Identifiers should not match keywords

#pragma warning disable CA1063 // Implement IDisposable Correctly
		/// <summary>
		/// Performs freeing and releasing resources.
		/// </summary>
		public abstract void Dispose ();
#pragma warning restore CA1063 // Implement IDisposable Correctly

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Review unused parameters
		/// <summary>
		/// Invokes the timer callback.
		/// Can be used for a TimerCallback type delegate.
		/// </summary>
		/// <param name="notUsed">Not used. Required to generate a signature similar to some other timers.</param>
		protected void DoCallback (object notUsed)
#pragma warning restore CA1801 // Review unused parameters
#pragma warning restore IDE0060 // Remove unused parameter
		{
			_callback.Invoke (_state);
		}

		/// <summary>
		/// Invokes the timer callback.
		/// Can be used for a EventHandler type delegate.
		/// </summary>
		/// <param name="notUsed1">Not used. Required to generate a signature similar to some other timers.</param>
		/// <param name="notUsed2">Not used. Required to generate a signature similar to some other timers.</param>
		protected void DoCallback (object notUsed1, EventArgs notUsed2)
		{
			_callback.Invoke (_state);
		}

		/// <summary>
		/// Invokes the timer callback.
		/// Can be used for a Action type delegate.
		/// </summary>
		protected void DoCallback ()
		{
			_callback.Invoke (_state);
		}
	}
}
