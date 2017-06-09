using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Reflection;
using static System.Linq.Enumerable;

namespace Novartment.Base
{
	/// <summary>
	/// Поставщик информации об исключениях.
	/// </summary>
	/// <remarks>
	/// Понимает следующие типы исключений:
	/// System.AggregateException,
	/// System.Reflection.ReflectionTypeLoadException,
	/// System.ArgumentException,
	/// System.Globalization.CultureNotFoundException,
	/// System.ObjectDisposedException,
	/// System.IO.FileNotFoundException,
	/// System.BadImageFormatException,
	/// System.TypeInitializationException,
	/// System.TypeLoadException.
	/// Novartment.Base.CustomErrorException
	/// </remarks>
	public static class ExceptionDescriptionProvider
	{
		/// <summary>
		/// Предоставляет список дочерних (вложенных) исключений.
		/// </summary>
		/// <param name="exception">Исключение, для которого нужно получить список дочерних (вложенных) исключений.</param>
		/// <returns>Список дочерних (вложенных) исключений</returns>
		public static IReadOnlyList<Exception> GetInnerExceptions (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			var customErrorException = exception as CustomErrorException;
			if (customErrorException != null)
			{
				return customErrorException.InnerExceptions ?? ReadOnlyList.Empty<Exception> ();
			}

			IReadOnlyList<Exception> innerExcpts = null;

			var aggregateException = exception as AggregateException;
			if ((aggregateException != null) && (aggregateException.InnerExceptions != null))
			{
				innerExcpts = aggregateException.InnerExceptions.AsReadOnlyList ();
			}

			var reflectionTypeLoadException = exception as ReflectionTypeLoadException;
			if ((reflectionTypeLoadException != null) && (reflectionTypeLoadException.LoaderExceptions != null))
			{
				innerExcpts = reflectionTypeLoadException.LoaderExceptions.AsReadOnlyList ();
			}

			if ((innerExcpts == null) && (exception.InnerException != null))
			{
				innerExcpts = ReadOnlyList.Repeat (exception.InnerException, 1);
			}

			return innerExcpts ?? ReadOnlyList.Empty<Exception> ();
		}

		/// <summary>
		/// Предоставляет название исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого нужно получить название.</param>
		/// <returns>Название исключения.</returns>
		public static string GetName (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			var customErrorException = exception as CustomErrorException;
			return (customErrorException != null) ?
				customErrorException.Name :
				ReflectionService.GetFormattedFullName (exception.GetType ());
		}

		/// <summary>
		/// Предоставляет трассировку исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого нужно получить трассировку.</param>
		/// <returns>Трассировка исключения.</returns>
		public static string GetTrace (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			var customErrorException = exception as CustomErrorException;
			return (customErrorException != null) ?
				customErrorException.Trace :
				exception.StackTrace;
		}

		/// <summary>
		/// Предоставляет сообщение исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого нужно получить сообщение.</param>
		/// <returns>Сообщение исключения.</returns>
		public static string GetMessage (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			var customErrorException = exception as CustomErrorException;
			if (customErrorException != null)
			{
				return customErrorException.Message;
			}

			string message = null;

			// для AggregateException сообщение совершенно неинформативно, заменяем его на сообщение первого вложенного исключения
			var aggregateException = exception as AggregateException;
			if ((aggregateException != null) && (aggregateException.InnerExceptions != null))
			{
				if (aggregateException.InnerExceptions.Count > 0)
				{
					message = GetMessage (aggregateException.InnerExceptions[0]);
				}
			}

			return message ?? exception.Message;
		}

		/// <summary>
		/// Предоставляет подробности исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого нужно получить подробности.</param>
		/// <returns>Подробности исключения.</returns>
		public static string GetDetails (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			var customErrorException = exception as CustomErrorException;
			if (customErrorException != null)
			{
				return customErrorException.Details;
			}

			string details = null;

			var argumentException = exception as ArgumentException;
			if (argumentException != null)
			{
				details = "ParamName = " + argumentException.ParamName;

				var cultureNotFoundException = argumentException as CultureNotFoundException;
				if (cultureNotFoundException != null)
				{
					details += ", InvalidCultureName = " + cultureNotFoundException.InvalidCultureName;
				}
			}

			var objectDisposedException = exception as ObjectDisposedException;
			if (objectDisposedException != null)
			{
				details = "ObjectName = " + objectDisposedException.ObjectName;
			}

			var fileNotFoundException = exception as FileNotFoundException;
			if (fileNotFoundException != null)
			{
				details = "FileName = " + fileNotFoundException.FileName;
			}

			var badImageFormatException = exception as BadImageFormatException;
			if (badImageFormatException != null)
			{
				details = "FileName = " + badImageFormatException.FileName;
			}

			var typeInitializationException = exception as TypeInitializationException;
			if (typeInitializationException != null)
			{
				details = "TypeName = " + typeInitializationException.TypeName;
			}

			var typeLoadException = exception as TypeLoadException;
			if (typeLoadException != null)
			{
				details = "TypeName = " + typeLoadException.TypeName;
			}

			return details;
		}

		/// <summary>
		/// Получает краткое однострочное описание исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого нужно получить краткое описание.</param>
		/// <returns>Краткое однострочное описание исключения.</returns>
		public static string GetDescription (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			return string.Join (
				"; ",
				CreateDescription (exception)
					.EnumerateHierarchy (true)
					.Select (item => item.ToString (false, null)));
		}

		/// <summary>
		/// Получает полное подробное описание исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого создаётся подробное описание.</param>
		/// <param name="tracePatternToHide">Строка-образец, которая в трассировке будет заменена на многоточие.</param>
		/// <returns>Последовательность строк, составляющих описание исключения.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public static IReadOnlyList<string> GetFullInfo (Exception exception, string tracePatternToHide = null)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			return CreateDescription (exception)
				.EnumerateHierarchy (false)
				.Select (item => item.ToString (true, tracePatternToHide))
				.ToArray ().AsReadOnlyList ();
		}

		/// <summary>
		/// Получает объект-описание исключения, включая все дочерние и вложенные исключения.
		/// </summary>
		/// <param name="exception">Исключение, для которого создаётся объект-описание.</param>
		/// <returns>
		/// Объект-описание исключения, содержащий строковое представление всех свойств исключения
		/// и окружения в котором оно произошло.
		/// </returns>
		public static ExceptionDescription CreateDescription (Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}

			Contract.EndContractBlock ();

			var innerExceptions = ExceptionDescriptionProvider.GetInnerExceptions (exception);
			var innerDescriptions = (innerExceptions != null) ?
				CollectionExtensions.DuplicateToArray (
					innerExceptions.Select (item => CreateDescription (item))) :
				Array.Empty<ExceptionDescription> ();
			return new ExceptionDescription (
				ExceptionDescriptionProvider.GetName (exception),
				ExceptionDescriptionProvider.GetMessage (exception),
				ExceptionDescriptionProvider.GetDetails (exception),
				ExceptionDescriptionProvider.GetTrace (exception),
				innerDescriptions);
		}
	}
}
