using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using Novartment.Base.Text;
using Novartment.Base.Collections;

namespace Novartment.Base
{
	/// <summary>
	/// Строковое представление всех свойств исключения и окружения в котором оно произошло.
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

		/// <summary>Тип исключения.</summary>
		public string Name => _name;
		/// <summary>Сообщение исключения.</summary>
		public string Message => _message;
		/// <summary>Дополнительная информация исключения.</summary>
		public string Details => _details;
		/// <summary>Трассировка стэка исключения.</summary>
		public string Trace => _trace;
		/// <summary>Коллекция описаний вложенных исключений.</summary>
		public ICollection<ExceptionDescription> InnerExceptions => _innerExceptions;

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="name">Тип исключения.</param>
		/// <param name="message">Сообщение исключения.</param>
		/// <param name="details">Дополнительная информация исключения.</param>
		/// <param name="trace">Трассировка стэка исключения.</param>
		/// <param name="innerExceptions">Коллекция описаний вложенных исключений.</param>
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

		/// <summary>Получает одно-строковое краткое представление подробностей об исключении.</summary>
		/// <returns>Одна строка подробностей об исключении.</returns>
		public override string ToString ()
		{
			return ToString (false, null);
		}

		/// <summary>Получает строковое представление подробностей об исключении.</summary>
		/// <param name="detailed">Укажите true чтобы получить детальное многостроковое представлени,
		/// или false чтобы получить однострочное краткое представление.</param>
		/// <param name="tracePatternToHide">Строка-образец, который в трассировке стэка будет заменён на многоточие.</param>
		/// <returns>Строка подробностей об исключении.</returns>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public virtual string ToString (bool detailed, string tracePatternToHide = null)
		{
			string result;

			if (detailed)
			{
				var strList = new ArrayList<string> ();
				strList.Add (_name);
				strList.Add ("Message: " + _message);
				if (_details != null)
				{
					strList.Add ("Details: " + _details);
				}

				if (_trace != null)
				{
					var trace = (tracePatternToHide == null) ?
						_trace :
						_trace.Replace (tracePatternToHide, "...", StringComparison.OrdinalIgnoreCase);
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
