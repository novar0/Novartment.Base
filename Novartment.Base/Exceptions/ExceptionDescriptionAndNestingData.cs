using System;
using System.Collections.Generic;

namespace Novartment.Base
{
	/// <summary>
	/// A textual representation of all the properties of the exception,
	/// supplemented by data on its position in the hierarchy of exceptions.
	/// </summary>
	public sealed class ExceptionDescriptionAndNestingData : ExceptionDescription
	{
		/// <summary>
		/// Initializes a new instance of the ExceptionDescriptionAndNestingData class with all specified properies.
		/// </summary>
		/// <param name="name">The name of the exception.</param>
		/// <param name="message">The message of the exception.</param>
		/// <param name="details">The details of the exception.</param>
		/// <param name="trace">The stack trace of the exception.</param>
		/// <param name="innerExceptions">The collection of the exceptions that is the cause of the current exception.</param>
		/// <param name="nestingLevel">The level of the exception in the hierarchy, starting from zero.</param>
		/// <param name="numberInLevel">The number of the exception within the same hierarchy, starting at zero.</param>
		/// <param name="totalInLevel">The total number of exceptions within the same hierarchy level.</param>
		public ExceptionDescriptionAndNestingData (
			string name,
			string message,
			string details,
			string trace,
			ICollection<ExceptionDescription> innerExceptions,
			int nestingLevel,
			int numberInLevel,
			int totalInLevel)
			: base (name, message, details, trace, innerExceptions)
		{
			this.NestingLevel = nestingLevel;
			this.NumberInLevel = numberInLevel;
			this.TotalInLevel = totalInLevel;
		}

		/// <summary>Gets the level of this exception in the hierarchy, starting from zero.</summary>
		public int NestingLevel { get; }

		/// <summary>Gets the number of this exception within the same hierarchy, starting at zero.</summary>
		public int NumberInLevel { get; }

		/// <summary>Gets the total number of exceptions within the same hierarchy level.</summary>
		public int TotalInLevel { get; }

		/// <summary>
		/// Creates and returns a single-line string representation of the current exception.
		/// </summary>
		/// <returns>The single-line string representation of the current exception.</returns>
		public override string ToString ()
		{
			return ToString (false, null);
		}

		/// <summary>
		/// Creates and returns a string  of the current exception
		/// according to the specified level of detail and, optionally, hides the specified string pattern.
		/// </summary>
		/// <param name="detailed">
		/// A value indicating whether to create a detailed multi-line or a simple single-line representation.
		/// </param>
		/// <param name="tracePatternToHide">
		/// A string pattern that will be replaced by an ellipsis in the stack trace.
		/// Specify null-reference if not needed.
		/// </param>
		/// <returns>The string representation of the current exception.</returns>
		public override string ToString (bool detailed, string tracePatternToHide = null)
		{
			var nestingInfo = ((this.NestingLevel > 0) || ((this.InnerExceptions != null) && (this.InnerExceptions.Count > 0))) ?
				FormattableString.Invariant ($"Level {this.NestingLevel} ") :
				string.Empty;
			if (this.TotalInLevel > 1)
			{
				nestingInfo += FormattableString.Invariant ($"#{this.NumberInLevel} ");
			}

			return nestingInfo + base.ToString (detailed, tracePatternToHide);
		}
	}
}
