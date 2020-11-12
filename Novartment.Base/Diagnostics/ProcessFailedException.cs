using System;

namespace Novartment.Base
{
	/// <summary>
	/// Error starting the application process.
	/// </summary>
	public sealed class ProcessFailedException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the ProcessFailedException class with a specified exit code.
		/// </summary>
		/// <param name="exitCode">The value that the associated process specified when it terminated.</param>
		public ProcessFailedException (int exitCode)
			: this (FormattableString.Invariant ($"Process exit with error {exitCode}."), exitCode)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ProcessFailedException with a specified exit code and a error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="exitCode">The value that the associated process specified when it terminated.</param>
		public ProcessFailedException (string message, int exitCode)
			: base (message)
		{
			this.ExitCode = exitCode;
		}

		/// <summary>
		/// Initializes a new instance of the ProcessFailedException with a specified exit code, a error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="exitCode">The value that the associated process specified when it terminated.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public ProcessFailedException (string message, int exitCode, Exception innerException)
			: base (message, innerException)
		{
			this.ExitCode = exitCode;
		}

		/// <summary>
		/// Gets the value that the associated process specified when it terminated.
		/// </summary>
		public int ExitCode { get; }
	}
}
