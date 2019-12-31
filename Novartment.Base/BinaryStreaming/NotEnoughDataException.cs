using System;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// The exception that is thrown when a source could not provide the requested amount of data.
	/// </summary>
	public class NotEnoughDataException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the NotEnoughDataException class with a specified amount of missing data.
		/// </summary>
		/// <param name="shortage">The amount of missing data that led to the exception.</param>
		public NotEnoughDataException (long shortage)
			: this (FormattableString.Invariant ($"Source can not provide requested size of data. Shortage = {shortage}."), shortage)
		{
		}

		/// <summary>
		/// Initializes a new instance of the NotEnoughDataException with a specified error message and an amount of missing data.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="shortage">The amount of missing data that led to the exception.</param>
		public NotEnoughDataException (string message, long shortage)
			: base (message)
		{
			this.Shortage = shortage;
		}

		/// <summary>
		/// Initializes a new instance of the NotEnoughDataException with a specified error message and an amount of missing data.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="shortage">The amount of missing data that led to the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public NotEnoughDataException (string message, long shortage, Exception innerException)
			: base (message, innerException)
		{
			this.Shortage = shortage;
		}

		/// <summary>Gets the amount of missing data.</summary>
		public long Shortage { get; }
	}
}
