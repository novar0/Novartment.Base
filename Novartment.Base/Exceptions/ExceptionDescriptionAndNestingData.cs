using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Описание исключения, дополненное данными о его положении в иерархии исключений.
	/// </summary>
	public class ExceptionDescriptionAndNestingData : ExceptionDescription
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса ExceptionDescriptionAndNestingData содержащий указанные данные.
		/// </summary>
		/// <param name="name">Название (тип) исключения.</param>
		/// <param name="message">Сообщение исключения.</param>
		/// <param name="details">Дополнительная информация исключения.</param>
		/// <param name="trace">Трассировка исключения.</param>
		/// <param name="innerExceptions">Коллекция описаний вложенных исключений.</param>
		/// <param name="nestingLevel">Уровень исключения в иерархии, начиная с нуля.</param>
		/// <param name="numberInLevel">Номер исключения в пределах одного уровня иерархии, начиная с нуля.</param>
		/// <param name="totalInLevel">Общее количество исключений в пределах одного уровня иерархии.</param>
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

		/// <summary>Получает уровень исключения в иерархии, начиная с нуля.</summary>
		public int NestingLevel { get; }

		/// <summary>Получает номер исключения в пределах одного уровня иерархии, начиная с нуля.</summary>
		public int NumberInLevel { get; }

		/// <summary>Получает общее количество исключений в пределах одного уровня иерархии.</summary>
		public int TotalInLevel { get; }

		/// <summary>
		/// Получает одно-строковое краткое представление подробностей об исключении.
		/// </summary>
		/// <returns>Одна строка подробностей об исключении.</returns>
		public override string ToString ()
		{
			return ToString (false, null);
		}

		/// <summary>
		/// Получает строковое представление подробностей об исключении с указанным уровнем детализации и скрытием указанного образца.
		/// </summary>
		/// <param name="detailed">Укажите true чтобы получить детальное многостроковое представлени, или false чтобы получить однострочное краткое представление.</param>
		/// <param name="tracePatternToHide">Строка-образец, который в трассировке стэка будет заменён на многоточие.</param>
		/// <returns>Строка подробностей об исключении.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public override string ToString (bool detailed, string tracePatternToHide = null)
		{
			var nestingInfo = ((this.NestingLevel > 0) || ((InnerExceptions != null) && (InnerExceptions.Count > 0))) ?
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
