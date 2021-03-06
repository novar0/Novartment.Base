using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Формат лексического токена-значения структурированной строки.
	/// </summary>
	public sealed class StructuredStringTokenValueFormat : StructuredStringTokenFormat
	{
		/// <summary>
		/// Декодирует значение лексического токена, помещая его декодированное значение в указанный буфер.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение лексического токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public override int DecodeToken (ReadOnlySpan<char> source, Span<char> buffer)
		{
			source.CopyTo (buffer);
			return source.Length;
		}
	}
}
