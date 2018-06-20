using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

		/// <summary>
		/// Получает UTF-32 код знака в строке в указанной позиции, обновляя позицию чтобы указывала на следующий знак.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="position">Позиция в source для получения знака, будет обновлена чтобы указывала на следующий знак.</param>
		/// <returns>UTF-32 код знака в строке в указанной позиции. Позиция обновлена чтобы указывала на следующий знак.</returns>
		public static int GetCodePoint (this string source, ref int position)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return GetCodePoint (source.AsSpan (), ref position);
		}

		/// <summary>
		/// Получает UTF-32 код знака в строке в указанной позиции, обновляя позицию чтобы указывала на следующий знак.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="position">Позиция в source для получения знака, будет обновлена чтобы указывала на следующий знак.</param>
		/// <returns>UTF-32 код знака в строке в указанной позиции. Позиция обновлена чтобы указывала на следующий знак.</returns>
		public static int GetCodePoint (this ReadOnlySpan<char> source, ref int position)
		{
			if ((position < 0) || (position >= source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			Contract.EndContractBlock ();

			var highSurrogate = source[position++];
			if ((position >= source.Length) || (highSurrogate < 0xd800) || (highSurrogate > 0xdbff))
			{
				return highSurrogate;
			}

			var lowSurrogate = source[position];
			if ((lowSurrogate < 0xdc00) || (lowSurrogate > 0xdfff))
			{
				return highSurrogate;
			}

			position++;
			return ((highSurrogate - '\ud800') * 0x400) + (lowSurrogate - '\udc00') + 0x10000;
		}
	}
}
