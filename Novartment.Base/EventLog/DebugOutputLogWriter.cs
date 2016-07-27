using System;
using SysDebug = System.Diagnostics.Debug;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Provides logging to system debugging output.
	/// </summary>
	public class DebugOutputLogWriter :
		ILogWriter
	{
		private readonly string _prefix;

		/// <summary>
		/// Initializes new instance of DebugOutputLogWriter with specified message prefix.
		/// </summary>
		/// <param name="prefix">Prefix for all messages.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public DebugOutputLogWriter (string prefix = null)
		{
			_prefix = prefix;
		}

		/// <summary>
		/// Occurs when logger configuration changes.
		/// </summary>
		public event EventHandler<EventArgs> LoggerReconfigured { add { } remove { } }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsTraceEnabled => true;

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsDebugEnabled => true;

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsInfoEnabled => true;

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsWarnEnabled => true;

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsErrorEnabled => true;

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
		public bool IsFatalEnabled => true;

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level.
		/// Most detailed information. Expect these to be written to logs only.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Trace (string message)
		{
			SysDebug.WriteLine (_prefix + message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level.
		/// Detailed information on the flow through the system. Expect these to be written to logs only.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Debug (string message)
		{
			SysDebug.WriteLine (_prefix + message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level.
		/// Interesting runtime events (startup/shutdown). Expect these to be immediately visible on a console,
		/// so be conservative and keep to a minimum.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Info (string message)
		{
			SysDebug.WriteLine (_prefix + message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level.
		/// Use of deprecated APIs, poor use of API, 'almost' errors, other runtime situations that are undesirable or unexpected,
		/// but not necessarily "wrong". Expect these to be immediately visible on a status console.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Warn (string message)
		{
			SysDebug.WriteLine (_prefix + message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level.
		/// Other runtime errors or unexpected conditions. Expect these to be immediately visible on a status console.
		/// </summary>
		/// <param name="message">Log message.</param>
		[SuppressMessage ("Microsoft.Naming",
			"CA1716:IdentifiersShouldNotMatchKeywords",
			MessageId = "Error",
			Justification = "No other name could be applied. NLog historically have method 'Error()'.")]
		public void Error (string message)
		{
			SysDebug.WriteLine (_prefix + message);
		}

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level.
		/// Severe errors that cause premature termination. Expect these to be immediately visible on a status console.
		/// </summary>
		/// <param name="message">Log message.</param>
		public void Fatal (string message)
		{
			SysDebug.WriteLine (_prefix + message);
		}
	}
}
