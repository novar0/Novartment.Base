using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Provides logging interface.
	/// </summary>
	public interface ILogWriter
	{
		/// <summary>
		/// Occurs when logger configuration changes.
		/// </summary>
		event EventHandler<EventArgs> LoggerReconfigured;

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
		bool IsTraceEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
		bool IsDebugEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
		bool IsInfoEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
		bool IsWarnEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
		bool IsErrorEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
		/// </summary>
		/// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
		bool IsFatalEnabled { get; }

		/// <summary>
		/// Writes the diagnostic message at the <c>Trace</c> level.
		/// Most detailed information. Expect these to be written to logs only.
		/// </summary>
		/// <param name="message">Log message.</param>
		void Trace (string message);

		/// <summary>
		/// Writes the diagnostic message at the <c>Debug</c> level.
		/// Detailed information on the flow through the system. Expect these to be written to logs only.
		/// </summary>
		/// <param name="message">Log message.</param>
		void Debug (string message);

		/// <summary>
		/// Writes the diagnostic message at the <c>Info</c> level.
		/// Interesting runtime events (startup/shutdown). Expect these to be immediately visible on a console,
		/// so be conservative and keep to a minimum.
		/// </summary>
		/// <param name="message">Log message.</param>
		void Info (string message);

		/// <summary>
		/// Writes the diagnostic message at the <c>Warn</c> level.
		/// Use of deprecated APIs, poor use of API, 'almost' errors, other runtime situations that are undesirable or unexpected,
		/// but not necessarily "wrong". Expect these to be immediately visible on a status console.
		/// </summary>
		/// <param name="message">Log message.</param>
		void Warn (string message);

		/// <summary>
		/// Writes the diagnostic message at the <c>Error</c> level.
		/// Other runtime errors or unexpected conditions. Expect these to be immediately visible on a status console.
		/// </summary>
		/// <param name="message">Log message.</param>
		[SuppressMessage (
		"Microsoft.Naming",
			"CA1716:IdentifiersShouldNotMatchKeywords",
			MessageId = "Error",
			Justification = "No other name could be applied. NLog historically have method 'Error()'.")]
		void Error (string message);

		/// <summary>
		/// Writes the diagnostic message at the <c>Fatal</c> level.
		/// Severe errors that cause premature termination. Expect these to be immediately visible on a status console.
		/// </summary>
		/// <param name="message">Log message.</param>
		void Fatal (string message);
	}
}