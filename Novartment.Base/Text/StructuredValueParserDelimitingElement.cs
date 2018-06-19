namespace Novartment.Base
{
	internal enum IngoreElementType : byte
	{
		Unspecified,
		QuotedValue,
		EscapedChar,
	}

	/// <summary>
	/// Набор параметров, определяющих способ выделения элемента байтовой последовательности.
	/// </summary>
	internal struct StructuredValueParserDelimitingElement
	{
		internal static readonly StructuredValueParserDelimitingElement CommentDelimitingData = new StructuredValueParserDelimitingElement (
			startMarker: '(',
			endMarker: ')',
			ignoreElement: IngoreElementType.EscapedChar,
			allowNesting: true);

		internal static readonly StructuredValueParserDelimitingElement QuotedString = new StructuredValueParserDelimitingElement (
			startMarker: '\"',
			endMarker: '\"',
			ignoreElement: IngoreElementType.EscapedChar,
			allowNesting: false);

		internal static readonly StructuredValueParserDelimitingElement AngleAddr = new StructuredValueParserDelimitingElement (
			startMarker: '<',
			endMarker: '>',
			ignoreElement: IngoreElementType.QuotedValue,
			allowNesting: false);

		internal static readonly StructuredValueParserDelimitingElement DomainLiteral = new StructuredValueParserDelimitingElement (
			startMarker: '[',
			endMarker: ']',
			ignoreElement: IngoreElementType.EscapedChar,
			allowNesting: false);

		internal StructuredValueParserDelimitingElement (char startMarker, char endMarker, IngoreElementType ignoreElement, bool allowNesting)
		{
			this.StarMarker = startMarker;
			this.EndMarker = endMarker;
			this.IgnoreElement = ignoreElement;
			this.AllowNesting = allowNesting;
		}

		/// <summary>Начальный байт элемента.</summary>
		internal char StarMarker { get; }

		/// <summary>Конечный байт элемента.</summary>
		internal char EndMarker { get; }

		/// <summary>Разрешены ли вложенные элементы.</summary>
		internal bool AllowNesting { get; }

		/// <summary>Элемент, который пропускается при поиске границ.</summary>
		internal IngoreElementType IgnoreElement { get; }
	}
}
