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
