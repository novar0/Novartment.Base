using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Пользовательский формат лексического токена структурированной строки.
	/// </summary>
	public abstract class StructuredStringTokenCustomFormat : StructuredStringTokenFormat
	{
		/// <summary>
		/// Создаёт новый экземпляр класса StructuredStringTokenCustomFormat с указанным символом-маркером начала лексического токена.
		/// </summary>
		/// <param name="startMarker"></param>
		public StructuredStringTokenCustomFormat (char startMarker)
		{
			this.StartMarker = startMarker;
		}

		/// <summary>
		/// Символ-маркер начала лексического токена.
		/// </summary>
		public char StartMarker { get; }

		/// <summary>
		/// Получает длину лексического токена в пользовательском формате, находящегося в указанной строке.
		/// </summary>
		/// <param name="source">Лексический токен в исходной строке.</param>
		/// <returns>Длина токена в пользовательском формате.</returns>
		/// <exception cref="System.FormatException">Лексический токен в пользовательском формате не найден в указанной строке.</exception>
		public abstract int FindTokenLength (ReadOnlySpan<char> source);
	}
}
