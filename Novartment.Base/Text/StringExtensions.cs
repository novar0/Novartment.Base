using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Collections;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы расширения к System.String.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Заменяет все найденные в строке образцы на заданное значение в соответствии с указанным правилом сравнения.
		/// </summary>
		/// <param name="source">Исходная строка, в которой осуществляется поиск.</param>
		/// <param name="oldValue">Образец, который будет заменяться на новое значение.</param>
		/// <param name="newValue">Новое значение, которое будет заменять образец.</param>
		/// <param name="comparisonType">Правило сравнения при поиске образца.</param>
		/// <returns>Новая строка, в которой все включения образца заменены на указанное значение.</returns>
		public static string Replace (this string source, string oldValue, string newValue, StringComparison comparisonType)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			if (oldValue == null)
			{
				throw new ArgumentNullException (nameof (oldValue));
			}
			if (oldValue.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (oldValue));
			}
			if (newValue == null)
			{
				throw new ArgumentNullException (nameof (newValue));
			}
			Contract.EndContractBlock ();

			var sourceLen = source.Length;
			var oldValueLen = oldValue.Length;
			if (oldValueLen > sourceLen)
			{
				return source;
			}

			int currentIdx = 0;
			int foundIdx;
			var strb = new StringBuilder ();
			do
			{
				foundIdx = source.IndexOf (oldValue, currentIdx, comparisonType);
				if (foundIdx >= 0)
				{
					var len = foundIdx - currentIdx;
					if (len > 0)
					{
						strb.Append (source.Substring (currentIdx, len));
					}
					strb.Append (newValue);
					currentIdx = foundIdx + oldValueLen;
				}
			} while (foundIdx >= 0);
			if (currentIdx < (sourceLen - 1))
			{
				strb.Append (source.Substring (currentIdx));
			}
			return strb.ToString ();
		}

		/// <summary>
		/// Дополняет все значения кроме последнего в последовательности строк знаком разделителя.
		/// </summary>
		/// <param name="values">Коллекция значений.</param>
		/// <param name="separator">Разделитель значений.</param>
		/// <returns>Последовательность строк в которой все значения кроме последнего дополнены знаком разделителя.</returns>
		public static IReadOnlyList<string> AppendSeparator (this IReadOnlyCollection<string> values, char separator)
		{
			if (values == null)
			{
				throw new ArgumentNullException (nameof (values));
			}
			Contract.EndContractBlock ();

			var list = new ArrayList<string> (values.Count);
			string lastValue = null;
			foreach (var part in values)
			{
				if (lastValue != null)
				{
					list.Add (lastValue + separator);
				}
				lastValue = part;
			}
			if (lastValue != null)
			{
				list.Add (lastValue);
			}
			return list;
		}
	}
}
