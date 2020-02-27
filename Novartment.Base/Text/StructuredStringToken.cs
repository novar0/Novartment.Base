using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// An individual lexical token as part of the structured string.
	/// Contains the type, position, and number of characters of an individual token.
	/// </summary>
	[DebuggerDisplay ("{Format}: {Position}...{Position+Length}")]
	public readonly ref struct StructuredStringToken
	{
		private static readonly StructuredStringTokenFormat _formatSeparator = new StructuredStringTokenFormatSeparator ();
		private static readonly StructuredStringTokenFormat _formatValue = new StructuredStringTokenFormatValue ();

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredHeaderFieldLexicalToken с указанным типом, позицией и количеством знаков.
		/// </summary>
		/// <param name="format">Тип токена.</param>
		/// <param name="position">Позиция токена.</param>
		/// <param name="length">Количество знаков токена.</param>
		public StructuredStringToken (StructuredStringTokenFormat format, int position, int length)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			if ((length < 0) || ((format is StructuredStringTokenFormatSeparator) && (length != 1)))
			{
				throw new ArgumentOutOfRangeException (nameof (length));
			}

			Contract.EndContractBlock ();

			this.Format = format;
			this.Position = position;
			this.Length = length;
		}

		/// <summary>
		/// Получает тип токена.
		/// </summary>
		public readonly StructuredStringTokenFormat Format { get; }

		/// <summary>
		/// Получает начальную позицию токена.
		/// </summary>
		public readonly int Position { get; }

		/// <summary>
		/// Получает количество знаков токена.
		/// </summary>
		public readonly int Length { get; }

		/// <summary>
		/// Декодирует значение токена в соответствии с его форматом.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public int Decode (ReadOnlySpan<char> source, Span<char> buffer)
		{
			if (this.Format == null)
			{
				throw new InvalidOperationException ("Cant decode invalid token.");
			}

			return this.Format.DecodeToken (this, source, buffer);
		}

		/// <summary>
		/// Проверяет, является ли токен указанным символом-сепаратором.
		/// </summary>
		/// <param name="source">Исходная строка.</param>
		/// <param name="separator">Проверяемый символ-сепаратор.</param>
		/// <returns>True если токен является указанным символом-сепаратором.</returns>
		public bool IsSeparator (ReadOnlySpan<char> source, char separator)
		{
			return (this.Format is StructuredStringTokenFormatSeparator) && (source[this.Position] == separator);
		}

		/// <summary>
		/// Получает следующий лексический токен начиная с указанной позиции в указанной строке согласно указанному формату.
		/// Встроено распознавание трёх форматов:
		/// null - не удалось считать токен из-за окончания строки,
		/// StructuredStringTokenFormatSeparator - любой символ, недопустимый для значений,
		/// StructuredStringTokenFormatValue - значение.
		/// Дополнительные форматы для распознавания указываются в format.CustomTokenFormats.
		/// </summary>
		/// <param name="source">Структурированная строка, состоящая из лексических токенов.</param>
		/// <param name="position">
		/// Позиция в source, начиная с которой будет получен токен.
		/// После получения токена, position будет указывать на позицию, следующую за найденным токеном.
		/// </param>
		/// <returns>Лексический токен из указанной позиции в source.</returns>
		public static StructuredStringToken Parse (StructuredStringFormat format, ReadOnlySpan<char> source, ref int position)
		{
			var charClasses = AsciiCharSet.ValueClasses.Span;
			var whiteSpaceClasses = format.WhiteSpaceClasses;
			var valueClasses = format.ValueClasses;
			var allowDotInsideValue = format.AllowDotInsideValue;
			var customTokenFormats = format.CustomTokenFormats;
			while (position < source.Length)
			{
				char octet;
				// пропускаем пробельные символы
				while (true)
				{
					octet = source[position];
					if ((octet >= charClasses.Length) || ((charClasses[octet] & whiteSpaceClasses) == 0))
					{
						break;
					}

					position++;
					if (position >= source.Length)
					{
						return default;
					}
				}

				// TODO: оптимизировать этот цикл проверки, потому что он запускается на каждый символ значения
				// проверяем все форматы, ограниченные определёнными символами
				if (customTokenFormats != null)
				{
					foreach (var tokenFormat in customTokenFormats)
					{
						if (octet == tokenFormat.StartMarker)
						{
							var startPos = position;
							position = SkipDelimitedToken (
								source: source,
								pos: position,
								startMarker: tokenFormat.StartMarker,
								endMarker: tokenFormat.EndMarker,
								ignoreToken: tokenFormat.IgnoreToken,
								allowNesting: tokenFormat.AllowNesting);
							return new StructuredStringToken (tokenFormat, startPos, position - startPos);
						}
					}
				}

				var valuePos = position;
				while (position < source.Length)
				{
					octet = source[position];
					if ((octet >= charClasses.Length) || ((charClasses[octet] & valueClasses) == 0))
					{
						break;
					}
					position++;
				}

				if (position <= valuePos)
				{
					// допустимые для значения символы не встретились, значит это разделитель
					position++;
					return new StructuredStringToken (_formatSeparator, valuePos, 1);
				}
				else
				{
					// continue if dot followed by atom
					while (((position + 1) < source.Length) &&
						allowDotInsideValue &&
						(source[position] == '.') &&
						((source[position + 1] < charClasses.Length) && ((charClasses[source[position + 1]] & valueClasses) != 0)))
					{
						position++;
						while (position < source.Length)
						{
							octet = source[position];
							if ((octet >= charClasses.Length) || ((charClasses[octet] & valueClasses) == 0))
							{
								break;
							}

							position++;
						}
					}

					return new StructuredStringToken (_formatValue, valuePos, position - valuePos);
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
