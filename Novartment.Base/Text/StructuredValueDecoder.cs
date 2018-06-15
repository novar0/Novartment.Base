using System;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Декодер структурированных значений.
	/// </summary>
	public class StructuredValueDecoder
	{
		private readonly StringBuilder _result = new StringBuilder ();
		private bool _prevIsWordEncoded = false;

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueDecoder.
		/// </summary>
		public StructuredValueDecoder ()
		{
		}

		/// <summary>
		/// Декодирует значение элемента в соответствии с его типом.
		/// </summary>
		/// <param name="source">Кодированное в соответствии с типом значение элемента.</param>
		/// <param name="type">Тип, определяющий способ кодирования элемента.</param>
		/// <returns>Декодировенное значение элемента.</returns>
		public static string DecodeElement (ReadOnlySpan<byte> source, StructuredValueElementType type)
		{
			if ((type != StructuredValueElementType.SquareBracketedValue) &&
				(type != StructuredValueElementType.QuotedValue) &&
				(type != StructuredValueElementType.Value) &&
				(type != StructuredValueElementType.Separator))
			{
				throw new InvalidOperationException (FormattableString.Invariant (
					$"Element of type '{type}' is complex and can not be decoded to discrete value."));
			}

			string valueStr = ((type == StructuredValueElementType.SquareBracketedValue) || (type == StructuredValueElementType.QuotedValue)) ?
				UnquoteString (source) :
				AsciiCharSet.GetString (source);
			if (type == StructuredValueElementType.Separator)
			{
				return valueStr;
			}

			var isWordEncoded = (valueStr.Length > 8) &&
				(valueStr[0] == '=') &&
				(valueStr[1] == '?') &&
				(valueStr[valueStr.Length - 2] == '?') &&
				(valueStr[valueStr.Length - 1] == '=');

			return isWordEncoded ? Rfc2047EncodedWord.Parse (valueStr) : valueStr;
		}

		/// <summary>
		/// Воссоздаёт строку, закодированную в виде последовательности элементов.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение.</param>
		/// <param name="element">Элемент для добавления в декодированный результат.</param>
		public void AddElement (ReadOnlySpan<byte> source, StructuredValueElement element)
		{
			if ((element.ElementType != StructuredValueElementType.Separator) &&
				(element.ElementType != StructuredValueElementType.Value) &&
				(element.ElementType != StructuredValueElementType.QuotedValue))
			{
				throw new FormatException (FormattableString.Invariant (
					$"Element of type '{element.ElementType}' is complex and can not be decoded into discrete value."));
			}

			var tokenSrc = source.Slice (element.StartPosition, element.Length);
			var tokenIsWordEncoded = (tokenSrc.Length > 8) &&
				(tokenSrc[0] == '=') &&
				(tokenSrc[1] == '?') &&
				(tokenSrc[tokenSrc.Length - 2] == '?') &&
				(tokenSrc[tokenSrc.Length - 1] == '=');
			var decodedValue = DecodeElement (tokenSrc, element.ElementType);

			// RFC 2047 часть 6.2:
			// When displaying a particular header field that contains multiple 'encoded-word's,
			// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
			if ((!_prevIsWordEncoded || !tokenIsWordEncoded) && (_result.Length > 0))
			{
				// RFC 5322 часть 3.2.2:
				// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
				// are semantically interpreted as a single space character.
				_result.Append (' ');
			}

			_prevIsWordEncoded = tokenIsWordEncoded;
			_result.Append (decodedValue);
		}

		/// <summary>
		/// Воссоздаёт исходную строку из всех добавленных элементов.
		/// </summary>
		/// <returns>
		/// Результирующая строка из всех добавленных элементов.
		/// Добавляемые элементы будут декодированы, а не просто склеены в единую строку.
		/// Результирующая строка может содержать произвольные знаки, а не только ASCII-набор.
		/// </returns>
		public string GetResult ()
		{
			return _result.ToString ();
		}

		private static string UnquoteString (ReadOnlySpan<byte> value)
		{
			int idx = 0;
			var result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == '\\')
				{
					i++;
				}

				result[idx++] = (char)value[i];
			}

			return new string (result, 0, idx);
		}
	}
}
