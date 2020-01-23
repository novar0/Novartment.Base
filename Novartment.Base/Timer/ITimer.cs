using System;

namespace Novartment.Base
{
	/// <summary>
	/// A timer with a variable trigger interval
	/// that can be stopped and restarted.
	/// </summary>
	public interface ITimer :
		IDisposable
	{
		/// <summary>
		/// Gets or sets the trigger interval.
		/// </summary>
		TimeSpan Interval { get; set; }

		/// <summary>
		/// Gets state of the time. True means that the timer is started, and false means that the timer is stopped.
		/// </summary>
		bool Enabled { get; }

		/// <summary>
		/// Starts the timer.
		/// </summary>
		void Start ();

#pragma warning disable CA1716 // Identifiers should not match keywords
		/// <summary>
		/// Stops the timer.
		/// </summary>
		void Stop ();
#pragma warning restore CA1716 // Identifiers should not match keywords
	}
}
