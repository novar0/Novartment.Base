using System;

namespace Novartment.Base.Text
{
	public enum IngoreTokenType
	{
		Unspecified,
		QuotedValue,
		EscapedChar,
	}

	public abstract class StructuredStringTokenFormat
	{
		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="token">Токен для декодирования.</param>
		/// <param name="source">Исходная строка.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public virtual int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			var src = source.Slice (token.Position, token.Length);
			src.CopyTo (buffer);
			return src.Length;
		}
	}

	public class StructuredStringSeparatorTokenFormat : StructuredStringTokenFormat
	{
		internal StructuredStringSeparatorTokenFormat ()
		{
		}

		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			buffer[0] = source[token.Position];
			return 1;
		}
	}

	public class StructuredStringValueTokenFormat : StructuredStringTokenFormat
	{
		internal StructuredStringValueTokenFormat ()
		{
		}
	}

	public abstract class StructuredStringCustomTokenFormat : StructuredStringTokenFormat
	{
		public StructuredStringCustomTokenFormat (char startMarker)
		{
			this.StartMarker = startMarker;
		}

		public char StartMarker { get; }

		public abstract StructuredStringToken ParseToken (ReadOnlySpan<char> source, int position);
	}

	public class StructuredStringTokenDelimitedFormat : StructuredStringCustomTokenFormat
	{
		public StructuredStringTokenDelimitedFormat (char startMarker, char endMarker, IngoreTokenType ignoreToken, bool allowNesting)
			: base (startMarker)
		{
			this.EndMarker = endMarker;
			this.IgnoreToken = ignoreToken;
			this.AllowNesting = allowNesting;
		}

		public char EndMarker { get; }

		public IngoreTokenType IgnoreToken { get; }

		public bool AllowNesting { get; }

		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="token">Токен для декодирования.</param>
		/// <param name="source">Исходная строка.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			var src = source.Slice (token.Position + 1, token.Length - 2);
			src.CopyTo (buffer);
			return src.Length;
		}

		public override StructuredStringToken ParseToken (ReadOnlySpan<char> source, int position)
		{
			var startPos = position;
			position = SkipDelimitedToken (
				source: source,
				pos: position,
				startMarker: this.StartMarker,
				endMarker: this.EndMarker,
				ignoreToken: this.IgnoreToken,
				allowNesting: this.AllowNesting);
			return new StructuredStringToken (this, startPos, position - startPos);
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
