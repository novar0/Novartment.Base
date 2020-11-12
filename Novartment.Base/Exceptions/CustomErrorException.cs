using System;
using System.Collections.Generic;

namespace Novartment.Base
{
	/// <summary>
	/// A wrapper for passing information about exceptions of various kinds
	/// (not represented by the System.Exception type) where the System.Exception type is expected.
	/// </summary>
	public sealed class CustomErrorException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the CustomErrorException class with a specified name.
		/// </summary>
		/// <param name="name">The name of the exception.</param>
		public CustomErrorException (string name)
			: base ()
		{
			this.Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the CustomErrorException class with a specified name and a message.
		/// </summary>
		/// <param name="name">The name of the exception.</param>
		/// <param name="message">The message of the exception.</param>
		public CustomErrorException (string name, string message)
			: base (message)
		{
			this.Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the CustomErrorException class with a specified name, a message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="name">The name of the exception.</param>
		/// <param name="message">The message of the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public CustomErrorException (
			string name,
			string message,
			Exception innerException)
			: base (message)
		{
			this.Name = name;
			this.InnerExceptions = new Exception[] { innerException };
		}

		/// <summary>
		/// Initializes a new instance of the CustomErrorException class with all specified properies.
		/// </summary>
		/// <param name="name">The name of the exception.</param>
		/// <param name="message">The message of the exception.</param>
		/// <param name="details">The details of the exception.</param>
		/// <param name="trace">The stack trace of the exception.</param>
		/// <param name="innerExceptions">The list of the exceptions that is the cause of the current exception.</param>
		public CustomErrorException (
			string name,
			string message,
			string details,
			string trace,
			IReadOnlyList<Exception> innerExceptions)
			: base (message)
		{
			this.Name = name;
			this.Details = details;
			this.Trace = trace;
			this.InnerExceptions = innerExceptions;
		}

		/// <summary>Gets the name of the exception.</summary>
		public string Name { get; }

		/// <summary>Gets the details of the exception.</summary>
		public string Details { get; }

		/// <summary>Get the stack trace of the exception.</summary>
		public string Trace { get; }

		/// <summary>Gets the list of the exceptions that is the cause of the current exception.</summary>
		public IReadOnlyList<Exception> InnerExceptions { get; }
	}
}
