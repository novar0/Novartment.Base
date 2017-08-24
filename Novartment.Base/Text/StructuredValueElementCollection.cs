using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Collections;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы для создания и обработки коллекций StructuredValueElement.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public static class StructuredValueElementCollection
	{
		private static readonly DelimitedElement _CommentDelimitingData = new DelimitedElement ('(', ')', new DelimitedElement ('\\', 2), true);
		private static readonly DelimitedElement _QuotedStringDelimitingData = new DelimitedElement ('\"', '\"', new DelimitedElement ('\\', 2), false);
		private static readonly DelimitedElement _AngleAddrDelimitingData = new DelimitedElement ('<', '>', new DelimitedElement ('\"', '\"', new DelimitedElement ('\\', 2), false), false);
		private static readonly DelimitedElement _DomainLiteralDelimitingData = new DelimitedElement ('[', ']', new DelimitedElement ('\\', 2), false);

		/// <summary>
		/// Декодирует последовательность элементов как единую строку.
		/// </summary>
		/// <param name="elements">Список элементов составляющих строку.</param>
		/// <param name="count">Количество элементов списка, которые составляют строку.</param>
		/// <returns>Строка, декодированная из последовательности элементов.</returns>
		public static string Decode (this IReadOnlyList<StructuredValueElement> elements, int count)
		{
			if (elements == null)
			{
				throw new ArgumentNullException (nameof (elements));
			}

			Contract.EndContractBlock ();

			var result = new StringBuilder ();
			var prevIsWordEncoded = false;
			for (var i = 0; i < count; i++)
			{
				var token = elements[i];
				if ((token.ElementType != StructuredValueElementType.Separator) &&
					(token.ElementType != StructuredValueElementType.Value) &&
					(token.ElementType != StructuredValueElementType.QuotedValue))
				{
					throw new FormatException (FormattableString.Invariant (
						$"Element of type '{token.ElementType}' is complex and can not be decoded into discrete value."));
				}

				var decodedValue = token.Decode ();

				// RFC 2047 часть 6.2:
				// When displaying a particular header field that contains multiple 'encoded-word's,
				// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
				if ((!prevIsWordEncoded || !token.IsWordEncoded) && (result.Length > 0))
				{
					// RFC 5322 часть 3.2.2:
					// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
					// are semantically interpreted as a single space character.
					result.Append (' ');
				}

				prevIsWordEncoded = token.IsWordEncoded;
				result.Append (decodedValue);
			}

			return result.ToString ();
		}

		/// <summary>
		/// Создаёт коллекцию элементов структурированного значения из его исходного строкового представления.
		/// </summary>
		/// <param name="source">Исходное строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="valueCharClass">Класс символов, допустимых для отдельных элементов значения.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри элементов значения.</param>
		/// <param name="typeToSkip">Тип элементов значения, которые будут пропущены и не попадут в создаваемую коллекцию.</param>
		/// <returns>Коллекция элементов структурированного значения.</returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public static IReadOnlyList<StructuredValueElement> Parse (
			string source,
			AsciiCharClasses valueCharClass,
			bool allowDotInsideValue,
			StructuredValueElementType typeToSkip = StructuredValueElementType.Unspecified)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			var parser = new StructuredStringReader (source);
			var elements = new ArrayList<StructuredValueElement> ();
			while (!parser.IsExhausted)
			{
				var start = parser.Position;
				int end;
				switch (parser.NextChar)
				{
					case ' ':
					case '\t':
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
						// are semantically interpreted as a single space character.
						parser.SkipClassChars (AsciiCharSet.Classes, (short)AsciiCharClasses.WhiteSpace);
						break;
					case '"':
						end = parser.SkipDelimited (_QuotedStringDelimitingData);
						if (typeToSkip != StructuredValueElementType.QuotedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.QuotedValue, parser.Source.Substring (start + 1, end - start - 2)));
						}

						break;
					case '(':
						end = parser.SkipDelimited (_CommentDelimitingData);
						if (typeToSkip != StructuredValueElementType.RoundBracketedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.RoundBracketedValue, parser.Source.Substring (start + 1, end - start - 2)));
						}

						break;
					case '<':
						end = parser.SkipDelimited (_AngleAddrDelimitingData);
						if (typeToSkip != StructuredValueElementType.AngleBracketedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.AngleBracketedValue, parser.Source.Substring (start + 1, end - start - 2)));
						}

						break;
					case '[':
						end = parser.SkipDelimited (_DomainLiteralDelimitingData);
						if (typeToSkip != StructuredValueElementType.SquareBracketedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, parser.Source.Substring (start + 1, end - start - 2)));
						}

						break;
					default:
						end = parser.SkipClassChars (AsciiCharSet.Classes, (short)valueCharClass);
						if (end <= start)
						{
							var nextChar = parser.SkipChar ();
							if (typeToSkip != StructuredValueElementType.Separator)
							{
								elements.Add (new StructuredValueElement ((char)nextChar));
							}
						}
						else
						{
							// continue if dot followed by atom
							while (!parser.IsExhausted &&
								allowDotInsideValue &&
								(parser.NextChar == '.') &&
								(parser.NextNextChar >= 0) && (parser.NextNextChar < AsciiCharSet.MaxCharValue) &&
								AsciiCharSet.IsCharOfClass ((char)parser.NextNextChar, valueCharClass))
							{
								parser.SkipChar (); // '.'
								end = parser.SkipClassChars (AsciiCharSet.Classes, (short)valueCharClass);
							}

							if (typeToSkip != StructuredValueElementType.Value)
							{
								elements.Add (new StructuredValueElement (StructuredValueElementType.Value, parser.Source.Substring (start, end - start)));
							}
						}

						break;
				}
			}

			return elements;
		}
	}
}
