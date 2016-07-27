using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Методы расширения к массиву байтов.
	/// </summary>
	public static class ByteArrayExtensions
	{
		/// <summary>
		/// Вычисляет позицию в массиве байтов первого найденного соответствия образцу.
		/// </summary>
		/// <param name="source">Исходный массив, в котором производится поиск.</param>
		/// <param name="pattern">Массив-образец, который ищется.</param>
		/// <returns>Позиция в исходном массиве, где был найден образец, или -1 если образец не найден.</returns>
		public static int IndexOf (this byte[] source, byte[] pattern)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			if (pattern == null)
			{
				throw new ArgumentNullException (nameof (pattern));
			}
			if (pattern.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (pattern));
			}
			Contract.EndContractBlock ();

			if (source.Length < pattern.Length)
			{
				return -1;
			}

			return IndexOf (source, pattern, 0, source.Length);
		}

		/// <summary>
		/// Вычисляет позицию в указанном сегменте массива байтов первого найденного соответствия образцу.
		/// </summary>
		/// <param name="source">Исходный массив, в котором производится поиск.</param>
		/// <param name="pattern">Массив-образец, который ищется.</param>
		/// <param name="startIndex">Начальная для поиска позиция в исходном массиве.</param>
		/// <param name="count">Количество элементов в исходном массиве, которыми будет ограничен поиск.</param>
		/// <returns>Позиция в исходном массиве, где был найден образец, или -1 если образец не найден.</returns>
		public static int IndexOf (this byte[] source, byte[] pattern, int startIndex, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			if (pattern == null)
			{
				throw new ArgumentNullException (nameof (pattern));
			}
			if (pattern.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (pattern));
			}
			if ((startIndex < 0) || (startIndex > source.Length) || ((startIndex == source.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (startIndex));
			}
			if ((count < 0) || ((startIndex + count) > source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}
			Contract.EndContractBlock ();

			if (count >= pattern.Length)
			{
				var patternLength = pattern.Length;
				var maxPatternPosition = startIndex + count - pattern.Length;
				if (maxPatternPosition < 0)
				{
					return -1;
				}
				for (var i = startIndex; i <= maxPatternPosition; i++)
				{
					var j = 0;
					while ((j < patternLength) && (source[i + j] == pattern[j]))
					{
						j++;
					}
					if (j >= patternLength)
					{
						return i;
					}
				}
			}
			return -1;
		}
	}
}
