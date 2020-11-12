using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Формат лексического токена структурированной строки,
	/// представляющий символ-разделитель (любой символ, недопустимый для значений).
	/// </summary>
	public sealed class StructuredStringTokenSeparatorFormat : StructuredStringTokenFormat
	{
		/// <summary>
		/// Декодирует значение лексического токена, помещая его декодированное значение в указанный буфер.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение лексического токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public override int DecodeToken (ReadOnlySpan<char> source, Span<char> buffer)
		{
			buffer[0] = source[0];
			return 1;
		}
	}
}
