using System;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Декодер значения-фразы из его отдельных элементов.
	/// </summary>
	public class StructuredHeaderFieldDecoder
	{
		/*
		Фраза (phrase) это display-name в mailbox, элемент списка в keywords, описание в list-id.
		phrase =
		1) набор atom, encoded-word или quoted-string разделенных WSP или comment
		2) WSP между элементами являются пригодными для фолдинга
		3) не может быть пустым
		*/

		private readonly StringBuilder _result = new StringBuilder ();
		private bool _prevIsWordEncoded = false;

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueDecoder.
		/// </summary>
		public StructuredHeaderFieldDecoder ()
		{
		}

		/// <summary>
		/// Воссоздаёт строку, закодированную в виде последовательности элементов.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение.</param>
		/// <param name="element">Элемент для добавления в декодированный результат.</param>
		public void AddElement (ReadOnlySpan<char> source, StructuredHeaderFieldLexicalToken element)
		{
			if ((element.TokenType != StructuredHeaderFieldLexicalTokenType.Separator) &&
				(element.TokenType != StructuredHeaderFieldLexicalTokenType.Value) &&
				(element.TokenType != StructuredHeaderFieldLexicalTokenType.QuotedValue))
			{
				throw new FormatException (FormattableString.Invariant (
					$"Element of type '{element.TokenType}' is complex and can not be decoded into discrete value."));
			}

			var isWordEncoded = (element.Length > 8) &&
				(source[element.Position] == '=') &&
				(source[element.Position + 1] == '?') &&
				(source[element.Position + element.Length - 2] == '?') &&
				(source[element.Position + element.Length - 1] == '=');
			var decodedValue = element.Decode (source);

			// RFC 2047 часть 6.2:
			// When displaying a particular header field that contains multiple 'encoded-word's,
			// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
			if ((!_prevIsWordEncoded || !isWordEncoded) && (_result.Length > 0))
			{
				// RFC 5322 часть 3.2.2:
				// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
				// are semantically interpreted as a single space character.
				_result.Append (' ');
			}

			_prevIsWordEncoded = isWordEncoded;
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
	}
}
