using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Collections;
using Novartment.Base.Text.CharSpanExtensions;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы для создания и обработки коллекций StructuredValueElement.
	/// </summary>
	public static class StructuredValueElementCollection
	{
		private static readonly DelimitedElement _CommentDelimitingData = DelimitedElement.CreateBracketed (
			'(',
			')',
			DelimitedElement.OneEscapedChar,
			true);

		private static readonly DelimitedElement _QuotedStringDelimitingData = DelimitedElement.CreateBracketed (
			'\"',
			'\"',
			DelimitedElement.OneEscapedChar,
			false);

		private static readonly DelimitedElement _AngleAddrDelimitingData = DelimitedElement.CreateBracketed (
			'<',
			'>',
			DelimitedElement.CreateBracketed (
				'\"',
				'\"',
				DelimitedElement.OneEscapedChar,
				false),
			false);

		private static readonly DelimitedElement _DomainLiteralDelimitingData = DelimitedElement.CreateBracketed (
			'[',
			']',
			DelimitedElement.OneEscapedChar,
			false);

		/// <summary>
		/// Воссоздаёт строку, закодированную в виде последовательности элементов.
		/// </summary>
		/// <param name="elements">Список элементов составляющих строку.</param>
		/// <param name="count">Количество элементов списка, которые составляют строку.</param>
		/// <returns>Строка, декодированная из последовательности элементов.</returns>
		/// <remarks>
		/// Все элементы будут декодированы, а не просто склеены в единую строку.
		/// Результирующая строка может содержать произвольные знаки, а не только ASCII-набор.
		/// </remarks>
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
		/// Создаёт коллекцию элементов структурированного значения из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="valueCharClass">Класс символов, допустимых для отдельных элементов значения.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри элементов значения.</param>
		/// <param name="typeToSkip">Тип элементов значения, которые будут пропущены и не попадут в создаваемую коллекцию.</param>
		/// <returns>Коллекция элементов структурированного значения.</returns>
		/// <remarks>
		/// Все элементы в строковом представлении должны быть соответственно закодированы чтобы все знаки строки были из ASCII-набора.
		/// В создаваемую коллекцию элементы будут помещены в исходном виде, без декодирования.
		/// </remarks>
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

			return Parse (source.AsSpan (), valueCharClass, allowDotInsideValue, typeToSkip);
		}

		/// <summary>
		/// Создаёт коллекцию элементов структурированного значения из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="valueCharClass">Класс символов, допустимых для отдельных элементов значения.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри элементов значения.</param>
		/// <param name="typeToSkip">Тип элементов значения, которые будут пропущены и не попадут в создаваемую коллекцию.</param>
		/// <returns>Коллекция элементов структурированного значения.</returns>
		/// <remarks>
		/// Все элементы в строковом представлении должны быть соответственно закодированы чтобы все знаки строки были из ASCII-набора.
		/// В создаваемую коллекцию элементы будут помещены в исходном виде, без декодирования.
		/// </remarks>
		public static IReadOnlyList<StructuredValueElement> Parse (
			ReadOnlySpan<char> source,
			AsciiCharClasses valueCharClass,
			bool allowDotInsideValue,
			StructuredValueElementType typeToSkip = StructuredValueElementType.Unspecified)
		{
			// TODO: заменить вызовы CodePointReader на более простые, учитывая что исходная строка состоит только из ASCII
			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			var elements = new ArrayList<StructuredValueElement> ();
			while (source.Length > 0)
			{
				switch (source[0])
				{
					case ' ':
					case '\t':
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
						// are semantically interpreted as a single space character.
						var whiteSpace = source.GetSubstringOfClassChars (AsciiCharSet.Classes, (short)AsciiCharClasses.WhiteSpace);
						source = source.Slice (whiteSpace.Length);
						break;
					case '"':
						var quotedValue = source.EnsureDelimitedElement (_QuotedStringDelimitingData);
						source = source.Slice (quotedValue.Length);
						if (typeToSkip != StructuredValueElementType.QuotedValue)
						{
#if NETCOREAPP2_1
							var str = new string (quotedValue.Slice (1, quotedValue.Length - 2));
#else
							var str = new string (quotedValue.Slice (1, quotedValue.Length - 2).ToArray ());
#endif
							elements.Add (new StructuredValueElement (StructuredValueElementType.QuotedValue, str));
						}

						break;
					case '(':
						var roundBracketedValue = source.EnsureDelimitedElement (_CommentDelimitingData);
						source = source.Slice (roundBracketedValue.Length);
						if (typeToSkip != StructuredValueElementType.RoundBracketedValue)
						{
#if NETCOREAPP2_1
							var str = new string (roundBracketedValue.Slice (1, roundBracketedValue.Length - 2));
#else
							var str = new string (roundBracketedValue.Slice (1, roundBracketedValue.Length - 2).ToArray ());
#endif
							elements.Add (new StructuredValueElement (StructuredValueElementType.RoundBracketedValue, str));
						}

						break;
					case '<':
						var angleBracketedValue = source.EnsureDelimitedElement (_AngleAddrDelimitingData);
						source = source.Slice (angleBracketedValue.Length);
						if (typeToSkip != StructuredValueElementType.AngleBracketedValue)
						{
#if NETCOREAPP2_1
							var str = new string (angleBracketedValue.Slice (1, angleBracketedValue.Length - 2));
#else
							var str = new string (angleBracketedValue.Slice (1, angleBracketedValue.Length - 2).ToArray ());
#endif
							elements.Add (new StructuredValueElement (StructuredValueElementType.AngleBracketedValue, str));
						}

						break;
					case '[':
						var squareBracketedValue = source.EnsureDelimitedElement (_DomainLiteralDelimitingData);
						source = source.Slice (squareBracketedValue.Length);
						if (typeToSkip != StructuredValueElementType.SquareBracketedValue)
						{
#if NETCOREAPP2_1
							var str = new string (squareBracketedValue.Slice (1, squareBracketedValue.Length - 2));
#else
							var str = new string (squareBracketedValue.Slice (1, squareBracketedValue.Length - 2).ToArray ());
#endif
							elements.Add (new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, str));
						}

						break;
					default:
						var value = source.GetSubstringOfClassChars (AsciiCharSet.Classes, (short)valueCharClass);
						if (value.Length < 1)
						{
							if (typeToSkip != StructuredValueElementType.Separator)
							{
								elements.Add (new StructuredValueElement (source[0]));
							}

							source = source.Slice (1);
						}
						else
						{
							var valueSize = value.Length;
							var subParser = source.Slice (valueSize);

							// continue if dot followed by atom
							while ((subParser.Length > 1) &&
								allowDotInsideValue &&
								(subParser[0] == '.') &&
								AsciiCharSet.IsCharOfClass ((char)subParser[1], valueCharClass))
							{
								value = subParser.Slice (1).GetSubstringOfClassChars (AsciiCharSet.Classes, (short)valueCharClass);
								valueSize += 1 + value.Length;
								subParser = subParser.Slice (1 + value.Length);
							}

							if (typeToSkip != StructuredValueElementType.Value)
							{
#if NETCOREAPP2_1
								var str = new string (source.Slice (0, valueSize));
#else
								var str = new string (source.Slice (0, valueSize).ToArray ());
#endif
								elements.Add (new StructuredValueElement (StructuredValueElementType.Value, str));
							}

							source = source.Slice (valueSize);
						}

						break;
				}
			}

			return elements;
		}
	}
}
