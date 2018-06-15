using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Элемент структурированного значения.
	/// </summary>
	public struct StructuredValueElement
	{
		private static readonly StructuredValueElement _invalid = new StructuredValueElement (StructuredValueElementType.Unspecified, -1, -1);

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueElement на основе указанного типа и кодированного значения.
		/// </summary>
		/// <param name="type">Тип тип, определяющий способ кодирования элемента.</param>
		/// <param name="startPosition">Позиция в source.</param>
		/// <param name="length">Количество элементов в source.</param>
		public StructuredValueElement (StructuredValueElementType type, int startPosition, int length)
		{
			this.ElementType = type;
			this.StartPosition = startPosition;
			this.Length = length;
		}

		/// <summary>
		/// Получает особый элемент-метку, который не может быть получен в результате обычного разбора.
		/// </summary>
		public static StructuredValueElement Invalid => _invalid;

		/// <summary>
		/// Получает тип, определяющий способ кодирования элемента.
		/// </summary>
		public StructuredValueElementType ElementType { get; }

		/// <summary>
		/// Получает начальную позицию элемента.
		/// </summary>
		public int StartPosition { get; }

		/// <summary>
		/// Получает количество байт элемента.
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Получает признак валидности элемента.
		/// </summary>
		public bool IsValid => (this.ElementType != _invalid.ElementType) || (this.StartPosition != _invalid.StartPosition) || (this.Length != _invalid.Length);

		/// <summary>
		/// Декодирует значение элемента в соответствии с его типом.
		/// </summary>
		/// <param name="source">Кодированное в соответствии с типом значение элемента.</param>
		/// <returns>Декодировенное значение элемента.</returns>
		public string DecodeElement (ReadOnlySpan<byte> source)
		{
			var src = source.Slice (this.StartPosition, this.Length);
			if ((this.ElementType != StructuredValueElementType.SquareBracketedValue) &&
				(this.ElementType != StructuredValueElementType.QuotedValue) &&
				(this.ElementType != StructuredValueElementType.Value) &&
				(this.ElementType != StructuredValueElementType.Separator))
			{
				throw new InvalidOperationException (FormattableString.Invariant (
					$"Element of type '{this.ElementType}' is complex and can not be decoded to discrete value."));
			}

			string valueStr = ((this.ElementType == StructuredValueElementType.SquareBracketedValue) || (this.ElementType == StructuredValueElementType.QuotedValue)) ?
				UnquoteString (src) :
				AsciiCharSet.GetString (src);
			if (this.ElementType == StructuredValueElementType.Separator)
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
