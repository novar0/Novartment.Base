using System;
using System.Collections.Generic;
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

			return source.Length < pattern.Length ? -1 : IndexOf (source, pattern, 0, source.Length);
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

		/// <summary>
		/// Выделяет в указанном диапазоне байтов поддиапазон, в котором все байты соответствую указанному классу согласно указанной таблице классов.
		/// </summary>
		/// <param name="source">Исходный диапазон байтов.</param>
		/// <param name="classTable">Таблица, в которой каждому возможному значению байта указан набор (маска) классов.</param>
		/// <param name="suitableClassMask">Набор классов (маска), которые определяют пригодность байтов.</param>
		/// <returns>Новый диапазон байтов, выделенный из начала исходного согласно указанному ограничению по классу.</returns>
		public static ReadOnlySpan<byte> SliceElementsOfOneClass (this ReadOnlySpan<byte> source, IReadOnlyList<short> classTable, short suitableClassMask)
		{
			if (classTable == null)
			{
				throw new ArgumentNullException (nameof (classTable));
			}

			Contract.EndContractBlock ();

			var pos = 0;
			while (pos < source.Length)
			{
				var octet = source[pos];

				if ((octet >= classTable.Count) || ((classTable[octet] & suitableClassMask) == 0))
				{
					break;
				}

				pos++;
			}

			return source.Slice (0, pos);
		}

		/// <summary>
		/// Выделяет в указанном диапазоне байтов поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		/// <param name="source">Исходный диапазон байтов.</param>
		/// <param name="delimitedElement">Ограничительный элемент.</param>
		/// <returns>Новый диапазон байтов, выделенный из начала исходного согласно указанному ограничителюs.</returns>
		public static ReadOnlySpan<byte> SliceDelimitedElement (this ReadOnlySpan<byte> source, ByteSequenceDelimitedElement delimitedElement)
		{
			if (delimitedElement == null)
			{
				throw new ArgumentNullException (nameof (delimitedElement));
			}

			Contract.EndContractBlock ();

			if (delimitedElement.FixedLength > 0)
			{
				if (source[0] != delimitedElement.StarMarker)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Expected start marker 0x'{delimitedElement.StarMarker:x}' not found."));
				}

				if (delimitedElement.FixedLength >= source.Length)
				{
					throw new FormatException ("Unexpected end of fixed-length element.");
				}

				return source.Slice (0, delimitedElement.FixedLength);
			}

			if (source.Length < 1)
			{
				throw new FormatException (FormattableString.Invariant ($"Expecting start marker 0x'{delimitedElement.StarMarker:x}' but reached end of source."));
			}

			if (source[0] != delimitedElement.StarMarker)
			{
				throw new FormatException (FormattableString.Invariant ($"Expecting start marker 0x'{delimitedElement.StarMarker:x}', but found 0x'{source[0]:x}'."));
			}

			var subSource = source.Slice (1);

			int nestingLevel = 0;
			var ignoreElement = delimitedElement.IgnoreElement;
			while (nestingLevel >= 0)
			{
				if (subSource.Length < 1)
				{
					throw new FormatException (FormattableString.Invariant ($"Ending end marker 0x'{delimitedElement.EndMarker:x}' not found in source."));
				}

				var octet = subSource[0];
				if ((ignoreElement != null) && (octet == ignoreElement.StarMarker))
				{
					var ignoreChunk = SliceDelimitedElement (subSource, ignoreElement);
					subSource = subSource.Slice (ignoreChunk.Length);
				}
				else
				{
					var isStartNested = delimitedElement.AllowNesting && (octet == delimitedElement.StarMarker);
					if (isStartNested)
					{
						subSource = subSource.Slice (1);
						nestingLevel++;
					}
					else
					{
						subSource = subSource.Slice (1);
						if (octet == delimitedElement.EndMarker)
						{
							nestingLevel--;
						}
					}
				}
			}

			return source.Slice (0, source.Length - subSource.Length);
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
