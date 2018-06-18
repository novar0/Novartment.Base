﻿using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы для создания и обработки коллекций StructuredValueElement.
	/// </summary>
	public static class StructuredValueParser
	{
		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 822 'atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredValueElement GetNextElementDotAtom (ReadOnlySpan<byte> source, ref int position)
		{
			return GetNextElement (source, ref position, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 822 'dot-atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredValueElement GetNextElementAtom (ReadOnlySpan<byte> source, ref int position)
		{
			return GetNextElement (source, ref position, AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 2045 'token' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredValueElement GetNextElementToken (ReadOnlySpan<byte> source, ref int position)
		{
			return GetNextElement (source, ref position, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <param name="valueCharClass">Класс символов, допустимых для отдельных элементов значения.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри элементов значения.</param>
		/// <param name="typeToSkip">Тип элементов значения, которые будут пропущены и не попадут в создаваемую коллекцию.</param>
		/// <returns>Элемент структурированного значения.</returns>
		/// <remarks>
		/// Все элементы в строковом представлении должны быть соответственно закодированы чтобы все знаки строки были из ASCII-набора.
		/// В создаваемую коллекцию элементы будут помещены в исходном виде, без декодирования.
		/// </remarks>
		public static StructuredValueElement GetNextElement (
			ReadOnlySpan<byte> source,
			ref int position,
			AsciiCharClasses valueCharClass,
			bool allowDotInsideValue,
			StructuredValueElementType typeToSkip = StructuredValueElementType.Unspecified)
		{
			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			while (position < source.Length)
			{
				switch (source[position])
				{
					case (byte)' ':
					case (byte)'\t':
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
						// are semantically interpreted as a single space character.
						while (position < source.Length)
						{
							var octet = source[position];
							if ((octet >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[octet] & (short)AsciiCharClasses.WhiteSpace) == 0))
							{
								break;
							}

							position++;
						}

						break;
					case (byte)'"':
						var startPos1 = position;
						position = EnsureDelimitedElement (source, position, StructuredValueParserDelimitingElement.QuotedString);
						if (typeToSkip != StructuredValueElementType.QuotedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.QuotedValue, startPos1 + 1, position - startPos1 - 2);
						}

						break;
					case (byte)'(':
						var startPos2 = position;
						position = EnsureDelimitedElement (source, position, StructuredValueParserDelimitingElement.CommentDelimitingData);
						if (typeToSkip != StructuredValueElementType.RoundBracketedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.RoundBracketedValue, startPos2 + 1, position - startPos2 - 2);
						}

						break;
					case (byte)'<':
						var startPos3 = position;
						position = EnsureDelimitedElement (source, position, StructuredValueParserDelimitingElement.AngleAddr);
						if (typeToSkip != StructuredValueElementType.AngleBracketedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.AngleBracketedValue, startPos3 + 1, position - startPos3 - 2);
						}

						break;
					case (byte)'[':
						var startPos4 = position;
						position = EnsureDelimitedElement (source, position, StructuredValueParserDelimitingElement.DomainLiteral);
						if (typeToSkip != StructuredValueElementType.SquareBracketedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, startPos4 + 1, position - startPos4 - 2);
						}

						break;
					default:
						var valuePos = position;
						while (position < source.Length)
						{
							var octet = source[position];
							if ((octet >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[octet] & (short)valueCharClass) == 0))
							{
								break;
							}

							position++;
						}

						if (position <= valuePos)
						{
							position++;
							if (typeToSkip != StructuredValueElementType.Separator)
							{
								return new StructuredValueElement (StructuredValueElementType.Separator, valuePos, 1);
							}

							position++;
						}
						else
						{
							// continue if dot followed by atom
							while (((position + 1) < source.Length) &&
								allowDotInsideValue &&
								(source[position] == '.') &&
								AsciiCharSet.IsCharOfClass ((char)source[position + 1], valueCharClass))
							{
								position++;
								while (position < source.Length)
								{
									var octet = source[position];
									if ((octet >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[octet] & (short)valueCharClass) == 0))
									{
										break;
									}

									position++;
								}
							}

							if (typeToSkip != StructuredValueElementType.Value)
							{
								return new StructuredValueElement (StructuredValueElementType.Value, valuePos, position - valuePos);
							}
						}

						break;
				}
			}

			return StructuredValueElement.Invalid;
		}

		/// <summary>
		/// Выделяет в указанном диапазоне байтов поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		private static int EnsureDelimitedElement (ReadOnlySpan<byte> source, int pos, StructuredValueParserDelimitingElement delimitedElement)
		{
			// первый символ уже проверен, пропускаем
			pos++;

			int nestingLevel = 0;
			var ignoreElement = delimitedElement.IgnoreElement;
			while (nestingLevel >= 0)
			{
				if (pos >= source.Length)
				{
					throw new FormatException (FormattableString.Invariant ($"Ending end marker 0x'{delimitedElement.EndMarker:x}' not found in source."));
				}

				var octet = source[pos];
				switch (ignoreElement)
				{
					case IngoreElementType.QuotedValue when octet == '\"':
						pos = EnsureDelimitedElement (source, pos, StructuredValueParserDelimitingElement.QuotedString);
						break;
					case IngoreElementType.EscapedChar when octet == (byte)'\\':
						pos += 2;
						if (pos > source.Length)
						{
							throw new FormatException ("Unexpected end of fixed-length element.");
						}

						break;
					case IngoreElementType.Unspecified:
					default:
						var isStartNested = delimitedElement.AllowNesting && (octet == delimitedElement.StarMarker);
						if (isStartNested)
						{
							pos++;
							nestingLevel++;
						}
						else
						{
							pos++;
							if (octet == delimitedElement.EndMarker)
							{
								nestingLevel--;
							}
						}

						break;
				}
			}

			return pos;
		}
	}
}