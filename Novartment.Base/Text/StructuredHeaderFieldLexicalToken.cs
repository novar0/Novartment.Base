using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Text
{
	internal enum IngoreTokenType
	{
		Unspecified,
		QuotedValue,
		EscapedChar,
	}

	/// <summary>
	/// Отдельный лексический токен в составе строки типа RFC 5322 'Structured Header Field Body'.
	/// Содержит тип, позицию и количество знаков, относящиеся к отдельному токену.
	/// </summary>
	[DebuggerDisplay ("{TokenType}: {Position}...{Position+Length}")]
	public readonly ref struct StructuredHeaderFieldLexicalToken
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredHeaderFieldLexicalToken с указанным типом, позицией и количеством знаков.
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
		public readonly StructuredHeaderFieldLexicalTokenType TokenType { get; }

		/// <summary>
		/// Получает начальную позицию токена.
		/// </summary>
		public readonly int Position { get; }

		/// <summary>
		/// Получает количество знаков токена.
		/// </summary>
		public readonly int Length { get; }

		/// <summary>
		/// Получает признак валидности токена.
		/// </summary>
		public readonly bool IsValid => this.TokenType != StructuredHeaderFieldLexicalTokenType.Unspecified;

		/// <summary>
		/// Создаёт токен структурированного значения типа RFC 822 'atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные токены.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен токен.</param>
		/// <returns>Токен структурированного значения.</returns>
		public static StructuredHeaderFieldLexicalToken ParseDotAtom (ReadOnlySpan<char> source, ref int position)
		{
			return Parse (source, ref position, AsciiCharClasses.Atom, true, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт токен структурированного значения типа RFC 822 'dot-atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные токены.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен токен.</param>
		/// <returns>Токен структурированного значения.</returns>
		public static StructuredHeaderFieldLexicalToken ParseAtom (ReadOnlySpan<char> source, ref int position)
		{
			return Parse (source, ref position, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт токен структурированного значения типа RFC 2045 'token' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные токены.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен токен.</param>
		/// <returns>Токен структурированного значения.</returns>
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
					"Specified valueCharClass includes white space characters. White spaces supposed to delimit tokens and can not be in their values.");
			}

			var charClasses = AsciiCharSet.ValueClasses.Span;
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
						position = SkipDelimitedToken (
							source: source,
							pos: position,
							startMarker: '\"',
							endMarker: '\"',
							ignoreToken: IngoreTokenType.EscapedChar,
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
						position = SkipDelimitedToken (
							source: source,
							pos: position,
							startMarker: '(',
							endMarker: ')',
							ignoreToken: IngoreTokenType.EscapedChar,
							allowNesting: true);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.RoundBracketedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, startPos2 + 1, position - startPos2 - 2);
						}

						break;
					case '<':
						var startPos3 = position;
						position = SkipDelimitedToken (
							source: source,
							pos: position,
							startMarker: '<',
							endMarker: '>',
							ignoreToken: IngoreTokenType.QuotedValue,
							allowNesting: false);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.AngleBracketedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.AngleBracketedValue, startPos3 + 1, position - startPos3 - 2);
						}

						break;
					case '[':
						var startPos4 = position;
						position = SkipDelimitedToken (
							source: source,
							pos: position,
							startMarker: '[',
							endMarker: ']',
							ignoreToken: IngoreTokenType.EscapedChar,
							allowNesting: false);
						if (typeToSkip != StructuredHeaderFieldLexicalTokenType.SquareBracketedValue)
						{
							return new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, startPos4 + 1, position - startPos4 - 2);
						}

						break;
					default:
						var valuePos = position;
						var asciiClasses = AsciiCharSet.ValueClasses.Span;
						while (position < source.Length)
						{
							var octet = source[position];
							if ((octet >= asciiClasses.Length) || ((asciiClasses[octet] & valueCharClass) == 0))
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
								((source[position + 1] < charClasses.Length) && ((charClasses[source[position + 1]] & valueCharClass) != 0)))
							{
								position++;
								while (position < source.Length)
								{
									var octet = source[position];
									if ((octet >= asciiClasses.Length) || ((asciiClasses[octet] & valueCharClass) == 0))
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
		/// Проверяет, является ли токен "encoded-word" согласно RFC 2047.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body', в которой выделен токен.</param>
		/// <returns>Признак того, что токен является "encoded-word" согласно RFC 2047.</returns>
		public bool IsWordEncoded (ReadOnlySpan<char> source)
		{
			var pos = this.Position;
			var len = this.Length;
			return (len > 8) &&
				(source[pos] == '=') &&
				(source[pos + 1] == '?') &&
				(source[pos + len - 2] == '?') &&
				(source[pos + len - 1] == '=');
		}

		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body', в которой выделен токен.</param>
		/// <returns>Декодировенное значение токена.</returns>
		public string Decode (ReadOnlySpan<char> source)
		{
			switch (this.TokenType)
			{
				case StructuredHeaderFieldLexicalTokenType.SquareBracketedValue:
				case StructuredHeaderFieldLexicalTokenType.QuotedValue:
					// unquote string
					int idx = 0;
					var result = new char[this.Length];
					var endPos = this.Position + this.Length;
					for (var i = this.Position; i < endPos; i++)
					{
						var ch = source[i];
						if (ch == '\\')
						{
							i++;
							ch = source[i];
						}

						result[idx++] = ch;
					}

					return new string (result, 0, idx);
				case StructuredHeaderFieldLexicalTokenType.Separator:
					return new string (source[this.Position], 1);

				case StructuredHeaderFieldLexicalTokenType.Value:
					var src = source.Slice (this.Position, this.Length);
					var isWordEncoded = (this.TokenType == StructuredHeaderFieldLexicalTokenType.Value) &&
						(src.Length > 8) &&
						(src[0] == '=') &&
						(src[1] == '?') &&
						(src[src.Length - 2] == '?') &&
						(src[src.Length - 1] == '=');

					if (isWordEncoded)
					{
						return Rfc2047EncodedWord.Parse (src);
					}
#if NETSTANDARD2_0
					return new string (src.ToArray ());
#else
					return new string (src);
#endif
				default:
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Token of type '{this.TokenType}' is complex and can not be decoded to discrete value."));
			}
		}

		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body', в которой выделен токен.</param>
		/// <param name="destination">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public int Decode (ReadOnlySpan<char> source, Span<char> destination)
		{
			switch (this.TokenType)
			{
				case StructuredHeaderFieldLexicalTokenType.SquareBracketedValue:
				case StructuredHeaderFieldLexicalTokenType.QuotedValue:
					int idx = 0;
					var endPos = this.Position + this.Length;
					for (var i = this.Position; i < endPos; i++)
					{
						var ch = source[i];
						if (ch == '\\')
						{
							i++;
							ch = source[i];
						}

						destination[idx++] = ch;
					}

					return idx;

				case StructuredHeaderFieldLexicalTokenType.Separator:
					destination[0] = source[this.Position];
					return 1;

				case StructuredHeaderFieldLexicalTokenType.Value:
					var src = source.Slice (this.Position, this.Length);
					var isWordEncoded = (src.Length > 8) &&
						(src[0] == '=') &&
						(src[1] == '?') &&
						(src[src.Length - 2] == '?') &&
						(src[src.Length - 1] == '=');
					if (!isWordEncoded)
					{
						src.CopyTo (destination);
						return src.Length;
					}

					// декодируем 'encoded-word'
					int resultSize;
					var encodedWordBuffer = ArrayPool<byte>.Shared.Rent (Rfc2047EncodedWord.MaxBinaryLenght);
					try
					{
						var size = Rfc2047EncodedWord.Parse (src, encodedWordBuffer, out Encoding encoding);
#if NETSTANDARD2_0
						var tempBuf = encoding.GetChars (encodedWordBuffer, 0, size);
						resultSize = tempBuf.Length;
						tempBuf.AsSpan ().CopyTo (destination);
#else
						resultSize = encoding.GetChars (encodedWordBuffer.AsSpan (0, size), destination);
#endif
					}
					finally
					{
						ArrayPool<byte>.Shared.Return (encodedWordBuffer);
					}

					return resultSize;

				default:
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Token of type '{this.TokenType}' is complex and can not be decoded to discrete value."));
			}
		}

		/// <summary>
		/// Выделяет в указанном диапазоне байтов поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		private static int SkipDelimitedToken (ReadOnlySpan<char> source, int pos, char startMarker, char endMarker, IngoreTokenType ignoreToken, bool allowNesting)
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
				switch (ignoreToken)
				{
					case IngoreTokenType.QuotedValue when octet == '\"':
						pos = SkipDelimitedToken (
							source: source,
							pos: pos,
							startMarker: '\"',
							endMarker: '\"',
							ignoreToken: IngoreTokenType.EscapedChar,
							allowNesting: false);
						break;
					case IngoreTokenType.EscapedChar when octet == '\\':
						pos += 2;
						if (pos > source.Length)
						{
							throw new FormatException ("Unexpected end of fixed-length token.");
						}

						break;
					case IngoreTokenType.Unspecified:
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
	}
}
