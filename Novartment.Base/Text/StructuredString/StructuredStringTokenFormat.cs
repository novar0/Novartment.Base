using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Формат лексического токена структурированной строки.
	/// </summary>
	public abstract class StructuredStringTokenFormat
	{
		/// <summary>
		/// Декодирует значение лексического токена, помещая его декодированное значение в указанный буфер.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение лексического токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public abstract int DecodeToken (ReadOnlySpan<char> source, Span<char> buffer);
	}
}
