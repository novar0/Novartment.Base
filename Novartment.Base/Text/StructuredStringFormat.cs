using System;
using System.Collections.Generic;

namespace Novartment.Base.Text
{
	public class StructuredStringFormat
	{
		public AsciiCharClasses WhiteSpaceClasses { get; }

		public AsciiCharClasses ValueClasses { get; }

		public bool AllowDotInsideValue { get; }

		public IReadOnlyCollection<StructuredStringTokenFormat> CustomTokenFormats { get; }

		/// <param name="whiteSpaceClasses">Класс символов, которые игнорируются между токенами.</param>
		/// <param name="valueClasses">Класс символов, допустимых для токенов типа Value.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри токенов типа Value.</param>
		/// <param name="customTokenFormats">Дополнительные форматы для распознавания.</param>
		public StructuredStringFormat (
			AsciiCharClasses whiteSpaceClasses,
			AsciiCharClasses valueClasses,
			bool allowDotInsideValue,
			IReadOnlyCollection<StructuredStringTokenFormat> customTokenFormats = null)
		{
			this.WhiteSpaceClasses = whiteSpaceClasses;
			this.ValueClasses = valueClasses;
			this.AllowDotInsideValue = allowDotInsideValue;
			this.CustomTokenFormats = customTokenFormats;
		}
	}
}
