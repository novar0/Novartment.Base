using System;

namespace Novartment.Base.Net
{
	/// <summary>
	/// The exception that is thrown in case of a critical violation of the protocol in which it cannot continue.
	/// </summary>
	public class UnrecoverableProtocolException : InvalidOperationException
	{
		/// Initializes a new instance of the UnrecoverableProtocolException class.
		public UnrecoverableProtocolException ()
			: base ("Unrecoverable protocol exception.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the UnrecoverableProtocolException with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public UnrecoverableProtocolException (string message)
			: base (message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the UnrecoverableProtocolException with a specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public UnrecoverableProtocolException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
