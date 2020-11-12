using System;
using Microsoft.Extensions.Logging;

namespace Novartment.Base
{
	/// <summary>
	/// Event log record.
	/// </summary>
	public sealed class SimpleEventRecord
	{
		/// <summary>
		/// Initializes a new instance of the SimpleEventLog class with a specified level of detail and a message.
		/// Инициализирует новый экземпляр SimpleEventRecord на основе указанных параметров.
		/// </summary>
		/// <param name="verbosity">The level of information detail corresponding to the event.</param>
		/// <param name="message">The message of the event.</param>
		internal SimpleEventRecord (LogLevel verbosity, string message)
		{
			this.Verbosity = verbosity;
			this.Time = DateTime.Now;
			this.Message = message;
		}

		/// <summary>Gets the level of information detail corresponding to the event.</summary>
		public LogLevel Verbosity { get; }

		/// <summary>Gets the time of the event.</summary>
		public DateTime Time { get; }

		/// <summary>Gets the message of the event.</summary>
		public string Message { get; }

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of the value of this instance.</returns>
		public override string ToString ()
		{
			return FormattableString.Invariant ($"{this.Time:yyy-MM-dd HH:mm:ss}\t{this.Message}");
		}
	}
}
