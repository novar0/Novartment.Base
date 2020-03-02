using System;
using System.Collections.Generic;
using Novartment.Base.Collections;

namespace Novartment.Base.Text
{
	public class StructuredStringFormat
	{
		private readonly StructuredStringCustomTokenFormat[] _customTokenFormats;
		private readonly char[] _startMarkers;

		public AsciiCharClasses WhiteSpaceClasses { get; }

		public AsciiCharClasses ValueClasses { get; }

		public bool AllowDotInsideValue { get; }


		/// <param name="whiteSpaceClasses">Класс символов, которые игнорируются между токенами.</param>
		/// <param name="valueClasses">Класс символов, допустимых для токенов типа Value.</param>
		/// <param name="allowDotInsideValue">Признак допустимости символа 'точка' внутри токенов типа Value.</param>
		/// <param name="customTokenFormats">Дополнительные форматы для распознавания.</param>
		public StructuredStringFormat (
			AsciiCharClasses whiteSpaceClasses,
			AsciiCharClasses valueClasses,
			bool allowDotInsideValue,
			IReadOnlyCollection<StructuredStringCustomTokenFormat> customTokenFormats = null)
		{
			this.WhiteSpaceClasses = whiteSpaceClasses;
			this.ValueClasses = valueClasses;
			this.AllowDotInsideValue = allowDotInsideValue;
			if (customTokenFormats != null)
			{
				_customTokenFormats = customTokenFormats.DuplicateToArray ();
				_startMarkers = new char[_customTokenFormats.Length];
				for (var i = 0; i < _startMarkers.Length; i++)
				{
					_startMarkers[i] = _customTokenFormats[i].StartMarker;
				}
			}
		}

		public StructuredStringToken ParseCustomFormats (ReadOnlySpan<char> source, int position)
		{
			var startMarkers = _startMarkers;
			if (startMarkers != null)
			{
				var octet = source[position];
				for (var i = 0; i < startMarkers.Length; i++)
				{
					if (startMarkers[i] == octet)
					{
						var customFormatToken = _customTokenFormats[i].ParseToken (source, position);
						return customFormatToken;
					}
				}
			}

			return default;
		}
	}
}
