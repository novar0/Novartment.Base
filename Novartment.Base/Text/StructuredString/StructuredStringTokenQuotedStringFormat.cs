using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Формат quoted-string лексического токена структурированной строки,
	/// согласно RFC 5322 part 3.2.4.
	/// </summary>
	public sealed class StructuredStringTokenQuotedStringFormat : StructuredStringTokenDelimitedFormat
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredStringTokenQuotedStringFormat.
		/// </summary>
		public StructuredStringTokenQuotedStringFormat ()
			: base ('\"', '\"', StructuredStringIngoreTokenType.EscapedChar, false)
		{
		}

		/// <summary>
		/// Декодирует значение лексического токена, помещая его декодированное значение в указанный буфер.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение лексического токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public override int DecodeToken (ReadOnlySpan<char> source, Span<char> buffer)
		{
			int dstIdx = 0;
			var endPos = source.Length - 1;
			for (var srcIdx = 1; srcIdx < endPos; srcIdx++)
			{
				var ch = source[srcIdx];
				if (ch == '\\')
				{
					srcIdx++;
					ch = source[srcIdx];
				}

				buffer[dstIdx++] = ch;
			}

			return dstIdx;
		}
	}
}
