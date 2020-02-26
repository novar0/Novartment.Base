using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

namespace Novartment.Base.Text
{
	public enum IngoreTokenType
	{
		Unspecified,
		QuotedValue,
		EscapedChar,
	}

	public readonly struct DelimitedTokenFormat
	{
		public DelimitedTokenFormat (char startMarker, char endMarker, IngoreTokenType ignoreToken, bool allowNesting)
		{
			this.StartMarker = startMarker;
			this.EndMarker = endMarker;
			this.IgnoreToken = ignoreToken;
			this.AllowNesting = allowNesting;
		}

		public char StartMarker { get; }
		public char EndMarker { get; }
		public IngoreTokenType IgnoreToken { get; }
		public bool AllowNesting { get; }
	}


	public class StructuredStringParser
	{
		private readonly AsciiCharClasses _whiteSpaceClasses;
		private readonly AsciiCharClasses _valueClasses;
		private readonly bool _allowDotInsideValue;
		private readonly IReadOnlyList<DelimitedTokenFormat> _delimitedTokenFormats;

		public static readonly IReadOnlyList<DelimitedTokenFormat> StructuredHeaderFieldBodyFormats = new ArrayList<DelimitedTokenFormat> ()
		{
			new DelimitedTokenFormat ('\"', '\"', IngoreTokenType.EscapedChar, false),
			new DelimitedTokenFormat ('(', ')', IngoreTokenType.EscapedChar, true),
			new DelimitedTokenFormat ('<', '>', IngoreTokenType.QuotedValue, false),
			new DelimitedTokenFormat ('[', ']', IngoreTokenType.EscapedChar, false),
		};

		public StructuredStringParser ()
		{
			_whiteSpaceClasses = AsciiCharClasses.WhiteSpace;
			_valueClasses = AsciiCharClasses.Visible;
			_allowDotInsideValue = false;
			_delimitedTokenFormats = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="whiteSpaceClasses"></param>
		/// <param name="valueClasses">Класс символов, допустимых для токенов типа StructuredHeaderFieldLexicalTokenType.Value.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри токенов типа StructuredHeaderFieldLexicalTokenType.Value.</param>
		/// <param name="delimitedTokenFormats"></param>
		public StructuredStringParser (
			AsciiCharClasses whiteSpaceClasses,
			AsciiCharClasses valueClasses,
			bool allowDotInsideValue,
			IReadOnlyList<DelimitedTokenFormat> delimitedTokenFormats = null)
		{
			_whiteSpaceClasses = whiteSpaceClasses;
			_valueClasses = valueClasses;
			_allowDotInsideValue = allowDotInsideValue;
			_delimitedTokenFormats = delimitedTokenFormats;
		}

		/// <summary>
		/// Получает следующий лексический токен начиная с указанной позиции в указанной строке.
		/// Пробельные символы между токенами игнорируются.
		/// Токеном считается непрерывная последовательность символов указнного класса,
		/// либо произвольная последовательность символов в кавычках или скобках (круглых, треугольных или квадратных).
		/// Любой другой символ будет считаться токеном-разделителем.
		/// Указанный в typeToSkip тип токенов будет пропущен.
		/// </summary>
		/// <param name="source">
		/// Строка типа RFC 5322 'Structured Header Field Body'.
		/// После получения токена, position будет указывать на позицию, следующую за найденным токеном.
		/// </param>
		/// <param name="position">Позиция в source, начиная с которой будет получен токен.</param>
		/// <returns>Лексический токен из указанной позиции в source.</returns>
		public StructuredStringToken Parse (ReadOnlySpan<char> source, ref int position)
		{
			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			if ((AsciiCharClasses.WhiteSpace & _valueClasses) != 0)
			{
				throw new ArgumentOutOfRangeException (
					nameof (_valueClasses),
					"Specified valueCharClass includes white space characters. White spaces supposed to delimit tokens and can not be in their values.");
			}

			var charClasses = AsciiCharSet.ValueClasses.Span;
			while (position < source.Length)
			{
				char octet;
				// пропускаем пробельные символы
				while (true)
				{
					octet = source[position];
					if ((octet >= charClasses.Length) || ((charClasses[octet] & _whiteSpaceClasses) == 0))
					{
						break;
					}

					position++;
					if (position >= source.Length)
					{
						return default;
					}
				}

				// проверяем все форматы, ограниченные определёнными символами
				if (_delimitedTokenFormats != null)
				{
					for (var idx = 0; idx < _delimitedTokenFormats.Count; idx++)
					{
						var format = _delimitedTokenFormats[idx];
						if (octet == format.StartMarker)
						{
							var startPos = position;
							position = SkipDelimitedToken (
								source: source,
								pos: position,
								startMarker: format.StartMarker,
								endMarker: format.EndMarker,
								ignoreToken: format.IgnoreToken,
								allowNesting: format.AllowNesting);
							return new StructuredStringToken (StructuredStringTokenType.DelimitedValue, startPos, position - startPos);
						}
					}
				}

				var valuePos = position;
				while (position < source.Length)
				{
					octet = source[position];
					if ((octet >= charClasses.Length) || ((charClasses[octet] & _valueClasses) == 0))
					{
						break;
					}
					position++;
				}

				if (position <= valuePos)
				{
					// допустимые для значения символы не встретились, значит это разделитель
					position++;
					return new StructuredStringToken (StructuredStringTokenType.Separator, valuePos, 1);
				}
				else
				{
					// continue if dot followed by atom
					while (((position + 1) < source.Length) &&
						_allowDotInsideValue &&
						(source[position] == '.') &&
						((source[position + 1] < charClasses.Length) && ((charClasses[source[position + 1]] & _valueClasses) != 0)))
					{
						position++;
						while (position < source.Length)
						{
							octet = source[position];
							if ((octet >= charClasses.Length) || ((charClasses[octet] & _valueClasses) == 0))
							{
								break;
							}

							position++;
						}
					}

					return new StructuredStringToken (StructuredStringTokenType.Value, valuePos, position - valuePos);
				}

			}

			return default;
		}

		/// <summary>
		/// Выделяет в указанной строке поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		/// <returns>Позиция, следующая за endMarker.</returns>
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
