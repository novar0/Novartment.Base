using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Immutable;
using static System.Linq.Enumerable;

namespace Novartment.Base
{
	/// <summary>
	/// Методы расширения к ExceptionDescription.
	/// </summary>
	public static class ExceptionDescriptionExtensions
	{
		/// <summary>
		/// Получает краткое описание исключения.
		/// </summary>
		/// <param name="exceptionDescription">Объект-описание исключения, для которого создаётся описание.</param>
		/// <returns>Однострочное описание исключения.</returns>
		public static string GetDescription (this ExceptionDescription exceptionDescription)
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
		/// Получает подробное описание исключения в виде коллекции строк.
		/// </summary>
		/// <param name="exceptionDescription">Объект-описание исключения, для которого создаётся описание.</param>
		/// <param name="tracePatternToHide">Строка-образец, который в трассировке стэка будет заменён на многоточие.</param>
		/// <returns>Последовательность строк, составляющих описание исключения.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public static IReadOnlyList<string> GetFullInfo (this ExceptionDescription exceptionDescription, string tracePatternToHide = null)
		{
			if (exceptionDescription == null)
			{
				throw new ArgumentNullException (nameof (exceptionDescription));
			}

			Contract.EndContractBlock ();

			return new ReadOnlyArray<string> (exceptionDescription
				.EnumerateHierarchy (false)
				.Select (item => item.ToString (true, tracePatternToHide))
				.ToArray ());
		}

		/// <summary>
		/// Получает последовательность всех элементов из иерархии указанного описания исключения.
		/// </summary>
		/// <param name="exceptionDescription">Объект-описание исключения, для которого создаётся последовательность.</param>
		/// <param name="skipAggregate">Укажите true чтобы исключать из ирерахии исключения типа AggregateException.</param>
		/// <returns>Последовательность объектов, содержащий описание исключение и данные о его положении в иерархии.</returns>
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

		internal class ExceptionDescriptionHierarchyEnumerator : IEnumerable<ExceptionDescriptionAndNestingData>
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
				var stack = new ArrayList<ExceptionDescriptionAndNestingData> ();
				stack.Add (new ExceptionDescriptionAndNestingData (
					_rootException.Name,
					_rootException.Message,
					_rootException.Details,
					_rootException.Trace,
					_rootException.InnerExceptions,
					0,
					0,
					1));

				ExceptionDescriptionAndNestingData data;
				while (stack.TryTakeLast (out data))
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
