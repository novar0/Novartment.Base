using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Novartment.Base.Text.CharSpanExtensions
{
	/// <summary>
	/// Последовательный считыватель элементов строки, позволяющий читать одиночные знаки,
	/// последовательности знаков определённого класса,
	/// либо последовательности ограниченные определёнными знаками.
	/// </summary>
	/// <remarks>
	/// Все члены, принимающие или возвращающие отдельные знаки, используют кодировку UTF-32.
	/// Суррогатная пара (char, char) считается одним знаком.
	/// </remarks>
	public static class UnicodeCodePointReader
	{
		/// <summary>
		/// Создаёт новую строку, пропуская первый знак указанной строки.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <returns>Новая строка, начинающая со второго знака исходной.</returns>
		public static ReadOnlySpan<char> SkipCodePoint (this ReadOnlySpan<char> source)
		{
			var pos = 0;
			GetCodePoint (source, ref pos);
			return source.Slice (pos);
		}

		/// <summary>Получает код первого знака (в UTF-32) в строке.
		/// Получает -1 если достигнут конец строки.</summary>
		/// <param name="source">Исходная строка.</param>
		/// <returns>Код первого знака (в UTF-32) в строке.</returns>
		public static int GetFirstCodePoint (this ReadOnlySpan<char> source)
		{
			var pos = 0;
			return GetCodePoint (source, ref pos);
		}

		/// <summary>Получает код второго знака (в UTF-32) в строке.
		/// Получает -1 если достигнут конец строки.</summary>
		/// <param name="source">Исходная строка.</param>
		/// <returns>Код второго знака (в UTF-32) в строке.</returns>
		public static int GetSecondCodePoint (this ReadOnlySpan<char> source)
		{
			var pos = 0;
			GetCodePoint (source, ref pos);
			return GetCodePoint (source, ref pos);
		}

		/// <summary>
		/// Проверяет что знак в первой позиции строки совпадает с указанным.
		/// Если символ не совпадает, то генерируется исключение.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="utf32CodePoint">Код знака (в UTF-32) для проверки.</param>
		/// <returns>Новая строка, начинающая после проверяемого знака.</returns>
		public static ReadOnlySpan<char> EnsureCodePoint (this ReadOnlySpan<char> source, int utf32CodePoint)
		{
			if (source.Length > 0)
			{
				var pos = 0;
				var currentCodePoint = GetCodePoint (source, ref pos);
				if (currentCodePoint == utf32CodePoint)
				{
					return source.Slice (pos);
				}

				throw new FormatException (FormattableString.Invariant ($"Expected code point U+'{utf32CodePoint:x}' but found U+'{currentCodePoint:x}'."));
			}

			throw new FormatException (FormattableString.Invariant ($"Expected code point U+'{utf32CodePoint:x}' but reached end of string."));
		}

		/// <summary>
		/// Проверяет, что строка начинается с указанного элемента.
		/// Если указанный элемент не найден, то генерируется исключение.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="delimitedElement">Пропускаемый элемент.</param>
		/// <returns>Новая строка, начинающая после проверяемого элемента.</returns>
		public static ReadOnlySpan<char> EnsureDelimitedElement (this ReadOnlySpan<char> source, DelimitedElement delimitedElement)
		{
			if (delimitedElement == null)
			{
				throw new ArgumentNullException (nameof (delimitedElement));
			}

			Contract.EndContractBlock ();

			if (delimitedElement.FixedLength > 0)
			{
				var currentCP = source.GetFirstCodePoint ();
				if (currentCP != delimitedElement.StartChar)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Expected code point U+'{delimitedElement.StartChar:x}' not found."));
				}

				if (delimitedElement.FixedLength >= source.Length)
				{
					throw new FormatException ("Unexpected end of fixed-length element.");
				}

				return source.Slice (0, delimitedElement.FixedLength);
			}

			var subSource = source.EnsureCodePoint (delimitedElement.StartChar);
			int nestingLevel = 0;
			var ignoreElement = delimitedElement.IgnoreElement;
			while (nestingLevel >= 0)
			{
				if (subSource.Length < 1)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Ending code point U+'{delimitedElement.EndChar:x}' not found in source."));
				}

				var currentCP = subSource.GetFirstCodePoint ();
				if ((ignoreElement != null) && (currentCP == ignoreElement.StartChar))
				{
					var ignoreChunk = EnsureDelimitedElement (subSource, ignoreElement);
					subSource = subSource.Slice (ignoreChunk.Length);
				}
				else
				{
					var isStartNested = delimitedElement.AllowNesting && (currentCP == delimitedElement.StartChar);
					if (isStartNested)
					{
						subSource = subSource.SkipCodePoint ();
						nestingLevel++;
					}
					else
					{
						subSource = subSource.SkipCodePoint ();
						if (currentCP == delimitedElement.EndChar)
						{
							nestingLevel--;
						}
					}
				}
			}

			return source.Slice (0, source.Length - subSource.Length);
		}

		/// <summary>
		/// Выделяет подстроку, состоящую из символов указанного типа.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Выделяемый тип символов.</param>
		/// <returns>Выделенная строка, состоящая из символов указанного типа.</returns>
		public static ReadOnlySpan<char> GetSubstringOfClassChars (this ReadOnlySpan<char> source, IReadOnlyList<short> charClassTable, short suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			/*if (source.Length < 1)
			{
				throw new FormatException ("Chars of specified class not found to the end of string.");
			}*/

			var currentPos = 0;
			while (currentPos < source.Length)
			{
				var pos = currentPos;
				var character = source.GetCodePoint (ref pos);

				if ((character >= charClassTable.Count) || ((charClassTable[character] & suitableClassMask) == 0))
				{
					break;
				}

				currentPos = pos;
			}

			return source.Slice (0, currentPos);
		}

		/// <summary>
		/// Выделяет подстроку, состоящую из любых символов, кроме символов указанного типа.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Недопустимый тип символов.</param>
		/// <returns>Выделенная строка, состоящая из любых символов, кроме символов указанного типа.</returns>
		public static ReadOnlySpan<char> GetSubstringOfNotClassChars (this ReadOnlySpan<char> source, IReadOnlyList<short> charClassTable, short suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			var currentPos = 0;
			while (currentPos < source.Length)
			{
				var pos = currentPos;
				var character = source.GetCodePoint (ref pos);

				if ((character < charClassTable.Count) && ((charClassTable[character] & suitableClassMask) != 0))
				{
					break;
				}

				currentPos = pos;
			}

			return source.Slice (0, currentPos);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static int GetCodePoint (this ReadOnlySpan<char> source, ref int index)
		{
			if (index >= source.Length)
			{
				return -1;
			}

			var highSurrogate = source[index++];
			if ((index >= source.Length) || (highSurrogate < 0xd800) || (highSurrogate > 0xdbff))
			{
				return highSurrogate;
			}

			var lowSurrogate = source[index];
			if ((lowSurrogate < 0xdc00) || (lowSurrogate > 0xdfff))
			{
				return highSurrogate;
			}

			index++;
			return ((highSurrogate - '\ud800') * 0x400) + (lowSurrogate - '\udc00') + 0x10000;
		}
	}
}
