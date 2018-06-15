using System;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы для создания и обработки коллекций StructuredValueElement.
	/// </summary>
	public static class StructuredValueParser
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
		/// Создаёт элемент структурированного значения типа RFC 822 'atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredValueElement GetNextElementDotAtom (ReadOnlySpan<byte> source, ref int position)
		{
			return GetNextElement (source, ref position, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 822 'dot-atom' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredValueElement GetNextElementAtom (ReadOnlySpan<byte> source, ref int position)
		{
			return GetNextElement (source, ref position, AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения типа RFC 2045 'token' из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <returns>Элемент структурированного значения.</returns>
		public static StructuredValueElement GetNextElementToken (ReadOnlySpan<byte> source, ref int position)
		{
			return GetNextElement (source, ref position, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);
		}

		/// <summary>
		/// Создаёт элемент структурированного значения из его исходного ASCII-строкового представления.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение, представляющее из себя отдельные элементы.</param>
		/// <param name="position">Позиция в source, начиная с которой будет получен элемент.</param>
		/// <param name="valueCharClass">Класс символов, допустимых для отдельных элементов значения.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри элементов значения.</param>
		/// <param name="typeToSkip">Тип элементов значения, которые будут пропущены и не попадут в создаваемую коллекцию.</param>
		/// <returns>Элемент структурированного значения.</returns>
		/// <remarks>
		/// Все элементы в строковом представлении должны быть соответственно закодированы чтобы все знаки строки были из ASCII-набора.
		/// В создаваемую коллекцию элементы будут помещены в исходном виде, без декодирования.
		/// </remarks>
		public static StructuredValueElement GetNextElement (
			ReadOnlySpan<byte> source,
			ref int position,
			AsciiCharClasses valueCharClass,
			bool allowDotInsideValue,
			StructuredValueElementType typeToSkip = StructuredValueElementType.Unspecified)
		{
			// TODO: добавить пропуск кодированных слов (внутри них могут быть символы "(<[ если оно в кодировке Q)
			while (position < source.Length)
			{
				switch (source[position])
				{
					case (byte)' ':
					case (byte)'\t':
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
						// are semantically interpreted as a single space character.
						var whiteSpace = source.Slice (position).SliceElementsOfOneClass (AsciiCharSet.Classes, (short)AsciiCharClasses.WhiteSpace);
						position += whiteSpace.Length;
						break;
					case (byte)'"':
						var quotedValue = source.Slice (position).SliceDelimitedElement (_QuotedStringDelimitingData);
						var pos1 = position + 1;
						position += quotedValue.Length;
						if (typeToSkip != StructuredValueElementType.QuotedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.QuotedValue, pos1, quotedValue.Length - 2);
						}

						break;
					case (byte)'(':
						var roundBracketedValue = source.Slice (position).SliceDelimitedElement (_CommentDelimitingData);
						var pos2 = position + 1;
						position += roundBracketedValue.Length;
						if (typeToSkip != StructuredValueElementType.RoundBracketedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.RoundBracketedValue, pos2, roundBracketedValue.Length - 2);
						}

						break;
					case (byte)'<':
						var angleBracketedValue = source.Slice (position).SliceDelimitedElement (_AngleAddrDelimitingData);
						var pos3 = position + 1;
						position += angleBracketedValue.Length;
						if (typeToSkip != StructuredValueElementType.AngleBracketedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.AngleBracketedValue, pos3, angleBracketedValue.Length - 2);
						}

						break;
					case (byte)'[':
						var squareBracketedValue = source.Slice (position).SliceDelimitedElement (_DomainLiteralDelimitingData);
						var pos4 = position + 1;
						position += squareBracketedValue.Length;
						if (typeToSkip != StructuredValueElementType.SquareBracketedValue)
						{
							return new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, pos4, squareBracketedValue.Length - 2);
						}

						break;
					default:
						var value = source.Slice (position).SliceElementsOfOneClass (AsciiCharSet.Classes, (short)valueCharClass);
						var valueSize = value.Length;
						if (valueSize < 1)
						{
							var pos5 = position;
							position++;
							if (typeToSkip != StructuredValueElementType.Separator)
							{
								return new StructuredValueElement (StructuredValueElementType.Separator, pos5, 1);
							}

							position++;
						}
						else
						{
							var valuePos = position;
							position += valueSize;

							// continue if dot followed by atom
							while (((position + 1) < source.Length) &&
								allowDotInsideValue &&
								(source[position] == '.') &&
								AsciiCharSet.IsCharOfClass ((char)source[position + 1], valueCharClass))
							{
								value = source.Slice (position + 1).SliceElementsOfOneClass (AsciiCharSet.Classes, (short)valueCharClass);
								position += value.Length + 1;
							}

							if (typeToSkip != StructuredValueElementType.Value)
							{
								return new StructuredValueElement (StructuredValueElementType.Value, valuePos, position - valuePos);
							}
						}

						break;
				}
			}

			return StructuredValueElement.Invalid;
		}
	}
}
