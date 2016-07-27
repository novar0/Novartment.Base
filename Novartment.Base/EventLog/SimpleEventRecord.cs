using System;
using System.Globalization;

namespace Novartment.Base
{
	/// <summary>
	/// Запись журнала событий.
	/// </summary>
	public class SimpleEventRecord
	{
		/// <summary>
		/// Инициализирует новый экземпляр SimpleEventRecord на основе указанных параметров.
		/// </summary>
		/// <param name="verbosity">Уровень детализации информации, для которого показывать событие.</param>
		/// <param name="message">Сообщение события.</param>
		internal SimpleEventRecord (LogLevel verbosity, string message)
		{
			this.Verbosity = verbosity;
			this.Time = DateTime.Now;
			this.Message = message;
		}

		/// <summary>Получает уровень детализации информации, для которого показывать событие.</summary>
		public LogLevel Verbosity { get; }

		/// <summary>Получает время события.</summary>
		public DateTime Time { get; }

		/// <summary>Получает сообщение события.</summary>
		public string Message { get; }

		/// <summary>
		/// Преобразовывает значение объекта в эквивалентное ему строковое представление.
		/// </summary>
		/// <returns>Строковое представление значения объекта.</returns>
		public override string ToString ()
		{
			return FormattableString.Invariant ($"{this.Time:yyy-MM-dd HH:mm:ss}\t{this.Message}");
		}
	}
}
