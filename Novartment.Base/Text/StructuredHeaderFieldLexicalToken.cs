using System;
using System.Buffers;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	internal enum IngoreElementType
	{
		Unspecified,
		QuotedValue,
		EscapedChar,
	}

	/// <summary>
	/// Отдельный лексический токен в составе строки типа RFC 5322 'Structured Header Field Body'.
	/// Содержит тип, позицию и количество знаков, относящиеся к отдельному токену.
	/// </summary>
	public struct StructuredHeaderFieldLexicalToken
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueElement с указанным типом, позицией и количеством знаков.
		/// </summary>
		/// <param name="type">Тип токена.</param>
		/// <param name="position">Позиция токена.</param>
		/// <param name="length">Количество знаков токена.</param>
		public StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType type, int position, int length)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			if ((length < 0) || ((type == StructuredHeaderFieldLexicalTokenType.Separator) && (length != 1)))
			{
				throw new ArgumentOutOfRangeException (nameof (length));
			}

			Contract.EndContractBlock ();

			this.TokenType = type;
			this.Position = position;
			this.Length = length;
		}

		/// <summary>
		/// Получает тип токена.
		/// </summary>
		public StructuredHeaderFieldLexicalTokenType TokenType { get; }

		/// <summary>
		/// Получает начальную позицию токена.
		/// </summary>
		public int Position { get; }

		/// <summary>
		/// Получает количество знаков токена.
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Получает признак валидности токена.
		/// </summary>
		public bool IsValid => this.TokenType != StructuredHeaderFieldLexicalTokenType.Unspecified;

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 822 'atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredHeaderFieldLexicalToken ParseDotAtom (ReadOnlySpan<char> source, ref int position)
		{
			return Parse (source, ref position, AsciiCharClasses.Atom, true, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 822 'dot-atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredHeaderFieldLexicalToken ParseAtom (ReadOnlySpan<char> source, ref int position)
		{
			return Parse (source, ref position, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 2045 'token' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredHeaderFieldLexicalToken ParseToken (ReadOnlySpan<char> source, ref int position)
		{
			return Parse (source, ref position, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
		}

		/// <summary>
		/// Получает лексический токен из указанной позиции в строке типа RFC 5322 'Structured Header Field Body'.
		/// После получения позиция будет указывать на следующий токен.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body'.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен токен.</param>
		/// <param name="valueCharClass">Класс символов, допустимых для токенов типа StructuredHeaderFieldLexicalTokenType.Value.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри токенов типа StructuredHeaderFieldLexicalTokenType.Value.</param>
		/// <param name="typeToSkip">Тип токена, который будет пропускаться.</param>
		/// <returns>Лексический токен из указанной позиции в source.</returns>
		public static StructuredHeaderFieldLexicalToken Parse (
			ReadOnlySpan<char> source,
			ref int position,
			AsciiCharClasses valueCharClass,
			bool allowDotInsideValue,
			StructuredHeaderFieldLexicalTokenType typeToSkip = StructuredHeaderFieldLexicalTokenType.Unspecified)
		{
			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			if ((AsciiCharClasses.WhiteSpace & valueCharClass) != 0)
			{
				throw new ArgumentOutOfRangeException (
					nameof (valueCharClass),
					"Specified valueCharClass includes white space characters. White spaces supposed to delimit elements and can not be in their values.");
			}

			// проверяем символы, которые разделяют отдельные элементы или определяют их тип (пробелы и скобки)
			// чтобы использовать их только когда они не входят в состав символов, допустимых для значения
			var valueMayContainQuotes = (AsciiCharSet.Classes['"'] & (short)valueCharClass) != 0;
			var valueMayContainRoundBrackets = ((AsciiCharSet.Classes['('] & (short)valueCharClass) != 0) ||
				((AsciiCharSet.Classes[')'] & (short)valueCharClass) != 0);
			var valueMayContainAngleBrackets = ((AsciiCharSet.Classes['<'] & (short)valueCharClass) != 0) ||
				((AsciiCharSet.Classes['>'] & (short)valueCharClass) != 0);
			var valueMayContainSquareBrackets = ((AsciiCharSet.Classes['['] & (short)valueCharClass) != 0) ||
				((AsciiCharSet.Classes[']'] & (short)valueCharClass) != 0);

			while (position < source.Length)
			{
				switch (source[position])
				{
					case ' ':
					case '\t':
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
						// are semantically interpreted as a single space character.
						while ((position < source.Length) && ((source[position] == ' ') || (source[position] == '\t')))
						{
							position++;
						}

						break;
					case '"':
						var startPos1 = position;
						position = SkipDelimitedElement (
							source: source,
							pos: position,
							startMarker: '\"',
							endMarker: '\"',
							ignoreElement: IngoreElementType.EscapedChar,
							allowNesting: false);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.QuotedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.QuotedValue, startPos1 + 1, position - startPos1 - 2);
						}

						break;
					case '(':
						/*
						RFC 5322 part 3.2.2:
						Strings of characters enclosed in parentheses are considered comments
						so long as they do not appear within a "quoted-string", as defined in section 3.2.4.
						Comments may nest.
						*/
						var startPos2 = position;
						position = SkipDelimitedElement (
							source: source,
							pos: position,
							startMarker: '(',
							endMarker: ')',
							ignoreElement: IngoreElementType.EscapedChar,
							allowNesting: true);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.RoundBracketedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, startPos2 + 1, position - startPos2 - 2);
						}

						break;
					case '<':
						var startPos3 = position;
						position = SkipDelimitedElement (
							source: source,
							pos: position,
							startMarker: '<',
							endMarker: '>',
							ignoreElement: IngoreElementType.QuotedValue,
							allowNesting: false);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.AngleBracketedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.AngleBracketedValue, startPos3 + 1, position - startPos3 - 2);
						}

						break;
					case '[':
						var startPos4 = position;
						position = SkipDelimitedElement (
							source: source,
							pos: position,
							startMarker: '[',
							endMarker: ']',
							ignoreElement: IngoreElementType.EscapedChar,
							allowNesting: false);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.SquareBracketedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, startPos4 + 1, position - startPos4 - 2);
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
							// допустимые для значения символы не встретились, значит это разделитель
							position++;
							if (typeToSkip != StructuredHeaderFieldLexicalTokenType.Separator)
							{
								return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Separator, valuePos, 1);
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

							if (typeToSkip != StructuredHeaderFieldLexicalTokenType.Value)
							{
								return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, valuePos, position - valuePos);
							}
						}

						break;
				}
			}

			return default;
		}

		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body', в которой выделен токен.</param>
		/// <returns>Декодировенное значение токена.</returns>
		public string Decode (ReadOnlySpan<char> source)
		{
			var src = source.Slice (this.Position, this.Length);
			if ((this.TokenType != StructuredHeaderFieldLexicalTokenType.SquareBracketedValue) &&
				(this.TokenType != StructuredHeaderFieldLexicalTokenType.QuotedValue) &&
				(this.TokenType != StructuredHeaderFieldLexicalTokenType.Value) &&
				(this.TokenType != StructuredHeaderFieldLexicalTokenType.Separator))
			{
				throw new InvalidOperationException (FormattableString.Invariant (
					$"Element of type '{this.TokenType}' is complex and can not be decoded to discrete value."));
			}

			var valueSpan = ((this.TokenType == StructuredHeaderFieldLexicalTokenType.SquareBracketedValue) || (this.TokenType == StructuredHeaderFieldLexicalTokenType.QuotedValue)) ?
				UnquoteString (src) :
				src;
#if NETCOREAPP2_1
			var valueStr = new string (valueSpan);
#else
			var valueStr = new string (valueSpan.ToArray ());
#endif
			if (this.TokenType == StructuredHeaderFieldLexicalTokenType.Separator)
			{
				return valueStr;
			}

			var isWordEncoded = (valueSpan.Length > 8) &&
				(valueSpan[0] == '=') &&
				(valueSpan[1] == '?') &&
				(valueSpan[valueSpan.Length - 2] == '?') &&
				(valueSpan[valueSpan.Length - 1] == '=');

			return isWordEncoded ? Rfc2047EncodedWord.Parse (valueSpan) : valueStr;
		}

#if NETCOREAPP2_1

		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body', в которой выделен токен.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public int Decode (ReadOnlySpan<char> source, Span<char> buffer)
		{
			switch (this.TokenType)
			{
				case StructuredHeaderFieldLexicalTokenType.SquareBracketedValue:
				case StructuredHeaderFieldLexicalTokenType.QuotedValue:
					char[] bufferTemp = null;
					try
					{
						bufferTemp = ArrayPool<char>.Shared.Rent (this.Length);
						var size = UnquoteString (source.Slice (this.Position, this.Length), bufferTemp);
						var isWordEncodedQ = (size > 8) &&
							(bufferTemp[0] == '=') &&
							(bufferTemp[1] == '?') &&
							(bufferTemp[size - 2] == '?') &&
							(bufferTemp[size - 1] == '=');
						if (isWordEncodedQ)
						{
							return Rfc2047EncodedWord.Parse (bufferTemp.AsSpan (0, size), buffer);
						}
						else
						{
							bufferTemp.AsSpan (0, size).CopyTo (buffer);
							return size;
						}
					}
					finally
					{
						if (bufferTemp != null)
						{
							ArrayPool<char>.Shared.Return (bufferTemp);
						}
					}

				case StructuredHeaderFieldLexicalTokenType.Separator:
					source.Slice (this.Position, this.Length).CopyTo (buffer);
					return this.Length;

				case StructuredHeaderFieldLexicalTokenType.Value:
					var src = source.Slice (this.Position, this.Length);
					var isWordEncoded = (src.Length > 8) &&
						(src[0] == '=') &&
						(src[1] == '?') &&
						(src[src.Length - 2] == '?') &&
						(src[src.Length - 1] == '=');
					if (isWordEncoded)
					{
						return Rfc2047EncodedWord.Parse (src, buffer);
					}

					src.CopyTo (buffer);
					return src.Length;

				default:
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Element of type '{this.TokenType}' is complex and can not be decoded to discrete value."));
			}
		}

		private static int UnquoteString (ReadOnlySpan<char> value, Span<char> result)
		{
			int idx = 0;
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == '\\')
				{
					i++;
				}

				result[idx++] = (char)value[i];
			}

			return idx;
		}

#endif

		/// <summary>
		/// Выделяет в указанном диапазоне байтов поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		private static int SkipDelimitedElement (ReadOnlySpan<char> source, int pos, char startMarker, char endMarker, IngoreElementType ignoreElement, bool allowNesting)
		{
			// первый символ уже проверен, пропускаем
			pos++;

			int nestingLevel = 0;
			while (nestingLevel >= 0)
			{
				if (pos >= source.Length)
				{
					throw new FormatException (FormattableString.Invariant ($"Ending end marker 0x'{endMarker:x}' not found in source."));
				}

				var octet = source[pos];
				switch (ignoreElement)
				{
					case IngoreElementType.QuotedValue when octet == '\"':
						pos = SkipDelimitedElement (
							source: source,
							pos: pos,
							startMarker: '\"',
							endMarker: '\"',
							ignoreElement: IngoreElementType.EscapedChar,
							allowNesting: false);
						break;
					case IngoreElementType.EscapedChar when octet == '\\':
						pos += 2;
						if (pos > source.Length)
						{
							throw new FormatException ("Unexpected end of fixed-length element.");
						}

						break;
					case IngoreElementType.Unspecified:
					default:
						var isStartNested = allowNesting && (octet == startMarker);
						if (isStartNested)
						{
							pos++;
							nestingLevel++;
						}
						else
						{
							pos++;
							if (octet == endMarker)
							{
								nestingLevel--;
							}
						}

						break;
				}
			}

			return pos;
		}

		private static ReadOnlySpan<char> UnquoteString (ReadOnlySpan<char> value)
		{
			int idx = 0;
			Span<char> result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == '\\')
				{
					i++;
				}

				result[idx++] = (char)value[i];
			}

			return result.Slice (0, idx);
		}
	}
}
