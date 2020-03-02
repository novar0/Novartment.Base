using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Формат лексического токена структурированной строки.
	/// </summary>
	public abstract class StructuredStringTokenFormat
	{
		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="token">Токен для декодирования.</param>
		/// <param name="source">Исходная строка.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public abstract int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer);
	}
}
