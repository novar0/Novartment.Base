using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;
using static System.Linq.Enumerable;

namespace Novartment.Base
{
	/// <summary>
	/// Методы расширения к ExceptionDescription.
	/// </summary>
	public static class ExceptionDescriptionExtensions
	{
		/// <summary>
		/// Creates and returns a simple single-line string representation of a exception description.
		/// </summary>
		/// <param name="exceptionDescription">The description of the exception for which a string representation is created.</param>
		/// <returns>The simple single-line string representation of a exception description.</returns>
		public static string GetShortInfo (this ExceptionDescription exceptionDescription)
		{
			if (exceptionDescription == null)
			{
				throw new ArgumentNullException (nameof (exceptionDescription));
			}

			Contract.EndContractBlock ();

			var elements = exceptionDescription
				.EnumerateHierarchy (true)
				.Select (item => item.ToString (false, null));
			return string.Join ("; ", elements);
		}

		/// <summary>
		/// Creates and returns a full multi-line string representation of a exception description.
		/// </summary>
		/// <param name="exceptionDescription">The description of the exception for which a string representation is created.</param>
		/// <param name="tracePatternToHide">
		/// A string pattern that will be replaced by an ellipsis in the stack trace.
		/// Specify null-reference if not needed.
		/// </param>
		/// <returns>The full multi-line string representation of a exception description.</returns>
		public static IReadOnlyList<string> GetFullInfo (this ExceptionDescription exceptionDescription, string tracePatternToHide = null)
		{
			if (exceptionDescription == null)
			{
				throw new ArgumentNullException (nameof (exceptionDescription));
			}

			Contract.EndContractBlock ();

			return exceptionDescription
				.EnumerateHierarchy (false)
				.Select (item => item.ToString (true, tracePatternToHide))
				.ToList ();
		}

		/// <summary>
		/// Retrieves the sequence of all elements from the hierarchy of the specified exception description.
		/// </summary>
		/// <param name="exceptionDescription">The description of the exception for which the sequence is created.</param>
		/// <param name="skipAggregate">
		/// A value indicating whether to exclude from hierarchy an exception of type AggregateException.
		/// </param>
		/// <returns>A sequence of objects that contains a description of an exception and information about its position in the hierarchy.</returns>
		public static IEnumerable<ExceptionDescriptionAndNestingData> EnumerateHierarchy (
			this ExceptionDescription exceptionDescription,
			bool skipAggregate)
		{
			if (exceptionDescription == null)
			{
				throw new ArgumentNullException (nameof (exceptionDescription));
			}

			Contract.EndContractBlock ();
			return new ExceptionDescriptionHierarchyEnumerator (exceptionDescription, skipAggregate);
		}

		internal sealed class ExceptionDescriptionHierarchyEnumerator :
			IEnumerable<ExceptionDescriptionAndNestingData>
		{
			private readonly ExceptionDescription _rootException;
			private readonly bool _skipAggregate;

			internal ExceptionDescriptionHierarchyEnumerator (ExceptionDescription exceptionInfo, bool skipAggregate)
			{
				_rootException = exceptionInfo;
				_skipAggregate = skipAggregate;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<ExceptionDescriptionAndNestingData> GetEnumerator ()
			{
				var stack = new ArrayList<ExceptionDescriptionAndNestingData>
				{
					new ExceptionDescriptionAndNestingData (
						_rootException.Name,
						_rootException.Message,
						_rootException.Details,
						_rootException.Trace,
						_rootException.InnerExceptions,
						0,
						0,
						1),
				};
				while (stack.TryTakeLast (out ExceptionDescriptionAndNestingData data))
				{
					var level = data.NestingLevel;

					// AggregateException не содержит никакой полезной информации кроме списка вложенных исключений
					if (!_skipAggregate || (data.Name != "System.AggregateException"))
					{
						level++;
						yield return data;
					}

					int numb = 0;
					if (data.InnerExceptions != null)
					{
						foreach (var innerException in data.InnerExceptions)
						{
							stack.Add (new ExceptionDescriptionAndNestingData (
								innerException.Name,
								innerException.Message,
								innerException.Details,
								innerException.Trace,
								innerException.InnerExceptions,
								level,
								numb++,
								data.InnerExceptions.Count));
						}
					}
				}
			}
		}
	}
}
