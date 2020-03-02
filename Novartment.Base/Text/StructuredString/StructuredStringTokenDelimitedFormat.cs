using System;

namespace Novartment.Base.Text
{
	public class StructuredStringTokenDelimitedFormat : StructuredStringTokenCustomFormat
	{
		private char _endMarker;
		private StructuredStringIngoreTokenType _ignoreToken;
		private bool _allowNesting;

		public char EndMarker => _endMarker;

		public StructuredStringIngoreTokenType IgnoreToken => _ignoreToken;

		public bool AllowNesting => _allowNesting;

		public StructuredStringTokenDelimitedFormat (char startMarker, char endMarker, StructuredStringIngoreTokenType ignoreToken, bool allowNesting)
			: base (startMarker)
		{
			_endMarker = endMarker;
			_ignoreToken = ignoreToken;
			_allowNesting = allowNesting;
		}


		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			// по умолчанию берём исходную строку отрезая начальный и конечный символы-маркеры
			// унаследованные классы могут переопределить это поведение
			var src = source.Slice (token.Position + 1, token.Length - 2);
			src.CopyTo (buffer);
			return src.Length;
		}

		public override StructuredStringToken ParseToken (ReadOnlySpan<char> source, int position)
		{
			var startPos = position;
			position = SkipDelimitedToken (source, position, this.StartMarker, _endMarker, _ignoreToken, _allowNesting);
			return new StructuredStringToken (this, startPos, position - startPos);
		}

		/// <summary>
		/// Выделяет в указанной строке поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		/// <returns>Позиция, следующая за endMarker.</returns>
		private static int SkipDelimitedToken (ReadOnlySpan<char> source, int pos, char startMarker, char endMarker, StructuredStringIngoreTokenType ignoreToken, bool allowNesting)
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
					case StructuredStringIngoreTokenType.QuotedValue when octet == '\"':
						pos = SkipDelimitedToken (
							source: source,
							pos: pos,
							startMarker: '\"',
							endMarker: '\"',
							ignoreToken: StructuredStringIngoreTokenType.EscapedChar,
							allowNesting: false);
						break;
					case StructuredStringIngoreTokenType.EscapedChar when octet == '\\':
						pos += 2;
						if (pos > source.Length)
						{
							throw new FormatException ("Unexpected end of fixed-length token.");
						}

						break;
					case StructuredStringIngoreTokenType.Unspecified:
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
