using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Формат лексического токена структурированной строки,
	/// определяемый маркерами начала, конца, типом игнорируемых токенов и возможностью вложенности.
	/// </summary>
	public class StructuredStringTokenDelimitedFormat : StructuredStringTokenCustomFormat
	{
		private char _endMarker;
		private StructuredStringIngoreTokenType _ignoreToken;
		private bool _allowNesting;

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredStringTokenDelimitedFormat с указанными маркерами,
		/// типом игнорируемых токенов и возможностью вложенности.
		/// </summary>
		/// <param name="startMarker"></param>
		/// <param name="endMarker"></param>
		/// <param name="ignoreToken"></param>
		/// <param name="allowNesting"></param>
		public StructuredStringTokenDelimitedFormat (char startMarker, char endMarker, StructuredStringIngoreTokenType ignoreToken, bool allowNesting)
			: base (startMarker)
		{
			_endMarker = endMarker;
			_ignoreToken = ignoreToken;
			_allowNesting = allowNesting;
		}

		/// <summary>
		/// Символ-маркер конца лексического токена.
		/// </summary>
		public char EndMarker => _endMarker;

		/// <summary>
		/// Получает тип токенов, которые игнорируются внутри токена этого типа.
		/// </summary>
		public StructuredStringIngoreTokenType IgnoreToken => _ignoreToken;

		/// <summary>
		/// Получает признак возмости вложения токенов этого вида друг в друга.
		/// </summary>
		public bool AllowNesting => _allowNesting;

		/// <summary>
		/// Декодирует значение лексического токена, помещая его декодированное значение в указанный буфер.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение лексического токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public override int DecodeToken (ReadOnlySpan<char> source, Span<char> buffer)
		{
			// по умолчанию берём исходную строку отрезая начальный и конечный символы-маркеры
			// наследники могут переопределить это поведение
			var src = source.Slice (1, source.Length - 2);
			src.CopyTo (buffer);
			return src.Length;
		}

		/// <summary>
		/// Получает длину лексического токена в пользовательском формате, находящегося в указанной строке.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <returns>Длина токена в пользовательском формате.</returns>
		/// <exception cref="System.FormatException">Лексический токен в пользовательском формате не найден в указанной строке.</exception>
		public override int FindTokenLength (ReadOnlySpan<char> source)
		{
			var length = SkipDelimitedToken (source, 0, this.StartMarker, _endMarker, _ignoreToken, _allowNesting);
			return length;
		}

		/// <summary>
		/// Выделяет в указанной строке поддиапазон, отвечающий указанному ограничению.
		/// Если диапазон не соответствует требованиям ограничителя, то генерируется исключение.
		/// </summary>
		/// <returns>Позиция, следующая за endMarker.</returns>
		private static int SkipDelimitedToken (
			ReadOnlySpan<char> source,
			int pos,
			char startMarker,
			char endMarker,
			StructuredStringIngoreTokenType ignoreToken,
			bool allowNesting)
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
