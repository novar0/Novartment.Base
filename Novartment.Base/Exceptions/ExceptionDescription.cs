using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Novartment.Base.Collections;

namespace Novartment.Base
{
	/// <summary>
	/// A textual representation of all the properties of the exception.
	/// </summary>
	[DataContract]
	public class ExceptionDescription
	{
		[DataMember (Name = "Name")]
		private string _name;
		[DataMember (Name = "Message")]
		private string _message;
		[DataMember (Name = "Details")]
		private string _details;
		[DataMember (Name = "Trace")]
		private string _trace;
		[DataMember (Name = "InnerExceptions")]
		private ICollection<ExceptionDescription> _innerExceptions;

		/// <summary>
		/// Initializes a new instance of the ExceptionDescription class with all specified properies.
		/// </summary>
		/// <param name="name">The name of the exception.</param>
		/// <param name="message">The message of the exception.</param>
		/// <param name="details">The details of the exception.</param>
		/// <param name="trace">The stack trace of the exception.</param>
		/// <param name="innerExceptions">The collection of the exceptions that is the cause of the current exception.</param>
		public ExceptionDescription (
			string name,
			string message,
			string details,
			string trace,
			ICollection<ExceptionDescription> innerExceptions)
		{
			_name = name;
			_message = message;
			_details = details;
			_trace = trace;
			_innerExceptions = innerExceptions;
		}

		/// <summary>Gets the name of the exception.</summary>
		public string Name => _name;

		/// <summary>Gets a message that describes the exception.</summary>
		public string Message => _message;

		/// <summary>Gets the details of the exception.</summary>
		public string Details => _details;

		/// <summary>Get the stack trace of the exception.</summary>
		public string Trace => _trace;

		/// <summary>Gets the list of the exceptions that is the cause of the current exception.</summary>
		public ICollection<ExceptionDescription> InnerExceptions => _innerExceptions;

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
		public virtual string ToString (bool detailed, string tracePatternToHide = null)
		{
			string result;

			if (detailed)
			{
				var strList = new ArrayList<string>
				{
					_name,
					"Message: " + _message,
				};
				if (_details != null)
				{
					strList.Add ("Details: " + _details);
				}

				if (_trace != null)
				{
#if NETSTANDARD2_0
					var trace = (tracePatternToHide == null) ? _trace : _trace.Replace (tracePatternToHide, "...");
#else
					var trace = (tracePatternToHide == null) ? _trace : _trace.Replace (tracePatternToHide, "...", StringComparison.Ordinal);
#endif
					var isNewLinePresent = trace.StartsWith (Environment.NewLine, StringComparison.OrdinalIgnoreCase);
					if (isNewLinePresent)
					{
						strList.Add ("Stack:" + trace);
					}
					else
					{
						strList.Add ("Stack:\r\n" + trace);
					}
				}

				result = string.Join (Environment.NewLine, strList);
			}
			else
			{
				var idxOfLastDot = _name.LastIndexOf ('.');
				var shortName = ((idxOfLastDot >= 0) && (idxOfLastDot < (_name.Length - 1))) ?
					_name.Substring (idxOfLastDot + 1) :
					_name;
				var sb = new StringBuilder (FormattableString.Invariant ($"{shortName}: {_message} ({_details})"));
				for (var idx = 0; idx < sb.Length; idx++)
				{
					if (sb[idx] < ' ')
					{
						sb[idx] = ' ';
					}
				}

				result = sb.ToString ();
			}

			return result;
		}
	}
}
