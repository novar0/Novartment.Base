using System;
using System.Buffers;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Элемент структурированного значения.
	/// </summary>
	public struct StructuredValueElement
	{
		private static readonly StructuredValueElement _invalid = new StructuredValueElement (default (StructuredValueElementType), default (int), default (int));

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
		public string Decode (ReadOnlySpan<char> source)
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

			var valueSpan = ((this.ElementType == StructuredValueElementType.SquareBracketedValue) || (this.ElementType == StructuredValueElementType.QuotedValue)) ?
				UnquoteString (src) :
				src;
#if NETCOREAPP2_1
			var valueStr = new string (valueSpan);
#else
			var valueStr = new string (valueSpan.ToArray ());
#endif
			if (this.ElementType == StructuredValueElementType.Separator)
			{
				return valueStr;
			}

			var isWordEncoded = (valueSpan.Length > 8) &&
				(valueSpan[0] == '=') &&
				(valueSpan[1] == '?') &&
				(valueSpan[valueSpan.Length - 2] == '?') &&
				(valueSpan[valueSpan.Length - 1] == '=');

			return isWordEncoded ? Rfc2047EncodedWord.Parse (valueSpan) : valueStr;
		}

#if NETCOREAPP2_1

		/// <summary>
		/// Декодирует значение элемента в соответствии с его типом.
		/// </summary>
		/// <param name="source">Кодированное в соответствии с типом значение элемента.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение элемента.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public int Decode (ReadOnlySpan<char> source, Span<char> buffer)
		{
			switch (this.ElementType)
			{
				case StructuredValueElementType.SquareBracketedValue:
				case StructuredValueElementType.QuotedValue:
					char[] bufferTemp = null;
					try
					{
						bufferTemp = ArrayPool<char>.Shared.Rent (this.Length);
						var size = UnquoteString (source.Slice (this.StartPosition, this.Length), bufferTemp);
						var isWordEncodedQ = (size > 8) &&
							(bufferTemp[0] == '=') &&
							(bufferTemp[1] == '?') &&
							(bufferTemp[size - 2] == '?') &&
							(bufferTemp[size - 1] == '=');
						if (isWordEncodedQ)
						{
							return Rfc2047EncodedWord.Parse (bufferTemp.AsSpan (0, size), buffer);
						}
						else
						{
							bufferTemp.AsSpan (0, size).CopyTo (buffer);
							return size;
						}
					}
					finally
					{
						if (bufferTemp != null)
						{
							ArrayPool<char>.Shared.Return (bufferTemp);
						}
					}

				case StructuredValueElementType.Separator:
					source.Slice (this.StartPosition, this.Length).CopyTo (buffer);
					return this.Length;

				case StructuredValueElementType.Value:
					var src = source.Slice (this.StartPosition, this.Length);
					var isWordEncoded = (src.Length > 8) &&
						(src[0] == '=') &&
						(src[1] == '?') &&
						(src[src.Length - 2] == '?') &&
						(src[src.Length - 1] == '=');
					if (isWordEncoded)
					{
						return Rfc2047EncodedWord.Parse (src, buffer);
					}

					src.CopyTo (buffer);
					return src.Length;

				default:
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Element of type '{this.ElementType}' is complex and can not be decoded to discrete value."));
			}
		}

		private static int UnquoteString (ReadOnlySpan<char> value, Span<char> result)
		{
			int idx = 0;
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == '\\')
				{
					i++;
				}

				result[idx++] = (char)value[i];
			}

			return idx;
		}

#endif
		private static ReadOnlySpan<char> UnquoteString (ReadOnlySpan<char> value)
		{
			int idx = 0;
			Span<char> result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == '\\')
				{
					i++;
				}

				result[idx++] = (char)value[i];
			}

			return result.Slice (0, idx);
		}
	}
}
