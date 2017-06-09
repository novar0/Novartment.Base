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
		/// <remarks>Осуществляет поиск по алгоритму КМП (Knuth–Morris–Pratt algorithm).
		/// Время работы линейно зависит от (source.Length + pattern.Length).</remarks>
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
		/// <remarks>Осуществляет поиск по алгоритму КМП (Knuth–Morris–Pratt algorithm).
		/// Время работы линейно зависит от (source.Length + pattern.Length).</remarks>
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

			var data = KmpBuildTable (pattern);

			int m = startIndex;
			int i = 0;
			var endIndex = startIndex + count;
			while ((m + i) < endIndex)
			{
				if (pattern[i] == source[m + i])
				{
					if (i == (pattern.Length - 1))
					{
						return m;
					}

					i++;
				}
				else
				{
					m = m + i - data[i];
					if (data[i] > -1)
					{
						i = data[i];
					}
					else
					{
						i = 0;
					}
				}
			}

			return -1;  // not found
		}

		private static int[] KmpBuildTable (byte[] pattern)
		{
			var result = new int[pattern.Length];
			int pos = 2;
			int cnd = 0;
			result[0] = -1;
			result[1] = 0;
			while (pos < pattern.Length)
			{
				if (pattern[pos - 1] == pattern[cnd])
				{
					cnd++;
					result[pos] = cnd;
					pos++;
				}
				else
				{
					if (cnd > 0)
					{
						cnd = result[cnd];
					}
					else
					{
						result[pos] = 0;
						pos++;
					}
				}
			}

			return result;
		}
	}
}
