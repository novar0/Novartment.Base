using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Collections;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы для создания и обработки коллекций StructuredValueElement.
	/// </summary>
	public static class StructuredValueElementCollection
	{
		private static readonly ByteSequenceDelimitedElement _CommentDelimitingData = ByteSequenceDelimitedElement.CreateMarkered (
			(byte)'(',
			(byte)')',
			ByteSequenceDelimitedElement.CreatePrefixedFixedLength ((byte)'\\', 2),
			true);

		private static readonly ByteSequenceDelimitedElement _QuotedStringDelimitingData = ByteSequenceDelimitedElement.CreateMarkered (
			(byte)'\"',
			(byte)'\"',
			ByteSequenceDelimitedElement.CreatePrefixedFixedLength ((byte)'\\', 2),
			false);

		private static readonly ByteSequenceDelimitedElement _AngleAddrDelimitingData = ByteSequenceDelimitedElement.CreateMarkered (
			(byte)'<',
			(byte)'>',
			ByteSequenceDelimitedElement.CreateMarkered (
				(byte)'\"',
				(byte)'\"',
				ByteSequenceDelimitedElement.CreatePrefixedFixedLength ((byte)'\\', 2),
				false),
			false);

		private static readonly ByteSequenceDelimitedElement _DomainLiteralDelimitingData = ByteSequenceDelimitedElement.CreateMarkered (
			(byte)'[',
			(byte)']',
			ByteSequenceDelimitedElement.CreatePrefixedFixedLength ((byte)'\\', 2),
			false);

		/// <summary>
		/// Воссоздаёт строку, закодированную в виде последовательности элементов.
		/// </summary>
		/// <param name="elements">Список элементов составляющих строку.</param>
		/// <param name="source">Исходное ASCII-строковое значение.</param>
		/// <param name="count">Количество элементов списка, которые составляют строку.</param>
		/// <returns>Строка, декодированная из последовательности элементов.</returns>
		/// <remarks>
		/// Все элементы будут декодированы, а не просто склеены в единую строку.
		/// Результирующая строка может содержать произвольные знаки, а не только ASCII-набор.
		/// </remarks>
		public static string Decode (this IReadOnlyList<StructuredValueElement> elements, ReadOnlySpan<byte> source, int count)
		{
			if (elements == null)
			{
				throw new ArgumentNullException (nameof (elements));
			}

			Contract.EndContractBlock ();

			var result = new StringBuilder ();
			var prevIsWordEncoded = false;
			for (var idx = 0; idx < count; idx++)
			{
				var token = elements[idx];
				if ((token.ElementType != StructuredValueElementType.Separator) &&
					(token.ElementType != StructuredValueElementType.Value) &&
					(token.ElementType != StructuredValueElementType.QuotedValue))
				{
					throw new FormatException (FormattableString.Invariant (
						$"Element of type '{token.ElementType}' is complex and can not be decoded into discrete value."));
				}

				var tokenSrc = source.Slice (token.StartPosition, token.Length);
				var tokenIsWordEncoded = (tokenSrc.Length > 8) &&
					(tokenSrc[0] == '=') &&
					(tokenSrc[1] == '?') &&
					(tokenSrc[tokenSrc.Length - 2] == '?') &&
					(tokenSrc[tokenSrc.Length - 1] == '=');
				var decodedValue = StructuredValueElementCollection.DecodeElement (tokenSrc, token.ElementType);

				// RFC 2047 часть 6.2:
				// When displaying a particular header field that contains multiple 'encoded-word's,
				// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
				if ((!prevIsWordEncoded || !tokenIsWordEncoded) && (result.Length > 0))
				{
					// RFC 5322 часть 3.2.2:
					// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
					// are semantically interpreted as a single space character.
					result.Append (' ');
				}

				prevIsWordEncoded = tokenIsWordEncoded;
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
			ReadOnlySpan<byte> source,
			AsciiCharClasses valueCharClass,
			bool allowDotInsideValue,
			StructuredValueElementType typeToSkip = StructuredValueElementType.Unspecified)
		{
			// TODO: заменить вызовы CodePointReader на более простые, учитывая что исходная строка состоит только из ASCII
			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			var elements = new ArrayList<StructuredValueElement> ();
			var pos = 0;
			while (pos < source.Length)
			{
				switch (source[pos])
				{
					case (byte)' ':
					case (byte)'\t':
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
						// are semantically interpreted as a single space character.
						var whiteSpace = source.Slice (pos).SliceElementsOfOneClass (AsciiCharSet.Classes, (short)AsciiCharClasses.WhiteSpace);
						pos += whiteSpace.Length;
						break;
					case (byte)'"':
						var quotedValue = source.Slice (pos).SliceDelimitedElement (_QuotedStringDelimitingData);
						if (typeToSkip != StructuredValueElementType.QuotedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.QuotedValue, pos + 1, quotedValue.Length - 2));
						}

						pos += quotedValue.Length;

						break;
					case (byte)'(':
						var roundBracketedValue = source.Slice (pos).SliceDelimitedElement (_CommentDelimitingData);
						if (typeToSkip != StructuredValueElementType.RoundBracketedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.RoundBracketedValue, pos + 1, roundBracketedValue.Length - 2));
						}

						pos += roundBracketedValue.Length;

						break;
					case (byte)'<':
						var angleBracketedValue = source.Slice (pos).SliceDelimitedElement (_AngleAddrDelimitingData);
						if (typeToSkip != StructuredValueElementType.AngleBracketedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.AngleBracketedValue, pos + 1, angleBracketedValue.Length - 2));
						}

						pos += angleBracketedValue.Length;

						break;
					case (byte)'[':
						var squareBracketedValue = source.Slice (pos).SliceDelimitedElement (_DomainLiteralDelimitingData);
						if (typeToSkip != StructuredValueElementType.SquareBracketedValue)
						{
							elements.Add (new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, pos + 1, squareBracketedValue.Length - 2));
						}

						pos += squareBracketedValue.Length;

						break;
					default:
						var value = source.Slice (pos).SliceElementsOfOneClass (AsciiCharSet.Classes, (short)valueCharClass);
						var valueSize = value.Length;
						if (valueSize < 1)
						{
							if (typeToSkip != StructuredValueElementType.Separator)
							{
								elements.Add (new StructuredValueElement (StructuredValueElementType.Separator, pos, 1));
							}

							pos++;
						}
						else
						{
							var valuePos = pos;
							pos += valueSize;

							// continue if dot followed by atom
							while (((pos + 1) < source.Length) &&
								allowDotInsideValue &&
								(source[pos] == '.') &&
								AsciiCharSet.IsCharOfClass ((char)source[pos + 1], valueCharClass))
							{
								value = source.Slice (pos + 1).SliceElementsOfOneClass (AsciiCharSet.Classes, (short)valueCharClass);
								pos += value.Length + 1;
							}

							if (typeToSkip != StructuredValueElementType.Value)
							{
								elements.Add (new StructuredValueElement (StructuredValueElementType.Value, valuePos, pos - valuePos));
							}
						}

						break;
				}
			}

			return elements;
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
