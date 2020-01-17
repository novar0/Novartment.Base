using System;

namespace Novartment.Base
{
	/// <summary>
	/// The exception that is thrown when a application requires elevation of privilege to continue.
	/// </summary>
	public class ElevationRequiredException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the ElevationRequiredException class.
		/// </summary>
		public ElevationRequiredException ()
			: base ("Requested action requires elevated privilegies.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the ElevationRequiredException with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public ElevationRequiredException (string message)
			: base (message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ElevationRequiredException with a specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public ElevationRequiredException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
