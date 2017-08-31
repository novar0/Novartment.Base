using System;
using System.Collections.Generic;
using Novartment.Base.Collections.Linq;

namespace Novartment.Base
{
#pragma warning disable CA1032 // Implement standard exception constructors
	/// <summary>
	/// Класс-обёртка для передачи информации об исключениях различного рода
	/// (не представленных типом System.Exception) туда, где ожидается тип System.Exception.
	/// </summary>
	public class CustomErrorException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
	{
		/// <summary>
		/// Инициализирует новый экземпляр CustomErrorException на основе предоставленных данных.
		/// </summary>
		/// <param name="name">Название исключения.</param>
		public CustomErrorException(string name)
			: base()
		{
			this.Name = name;
		}

		/// <summary>
		/// Инициализирует новый экземпляр CustomErrorException на основе предоставленных данных.
		/// </summary>
		/// <param name="name">Название исключения.</param>
		/// <param name="message">Сообщение исключения.</param>
		public CustomErrorException (string name, string message)
			: base (message)
		{
			this.Name = name;
		}

		/// <summary>
		/// Инициализирует новый экземпляр CustomErrorException на основе предоставленных данных.
		/// </summary>
		/// <param name="name">Название исключения.</param>
		/// <param name="message">Сообщение исключения.</param>
		/// <param name="innerException">Дочернее (вложенное) исключение.</param>
		public CustomErrorException (
			string name,
			string message,
			Exception innerException)
			: base (message)
		{
			this.Name = name;
			this.InnerExceptions = ReadOnlyList.Repeat (innerException, 1);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CustomErrorException на основе предоставленных данных.
		/// </summary>
		/// <param name="name">Название исключения.</param>
		/// <param name="message">Сообщение исключения.</param>
		/// <param name="details">Подробности исключения.</param>
		/// <param name="trace">Трассировка исключения.</param>
		/// <param name="innerExceptions">Список дочерних (вложенных) исключений.</param>
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

		/// <summary>Получает название исключения.</summary>
		public string Name { get; }

		/// <summary>Получает подробности исключения.</summary>
		public string Details { get; }

		/// <summary>Получает трассировку исключения.</summary>
		public string Trace { get; }

		/// <summary>Получает список дочерних (вложенных) исключений.</summary>
		public IReadOnlyList<Exception> InnerExceptions { get; }
	}
}
