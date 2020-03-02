using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Пользовательский формат лексического токена структурированной строки.
	/// </summary>
	public abstract class StructuredStringTokenCustomFormat : StructuredStringTokenFormat
	{
		/// <summary>
		/// Создаёт новый экземпляр класса StructuredStringTokenCustomFormat с указанным символом-маркером начала токена.
		/// </summary>
		/// <param name="startMarker"></param>
		public StructuredStringTokenCustomFormat (char startMarker)
		{
			this.StartMarker = startMarker;
		}

		/// <summary>
		/// Символ-маркер начала токена.
		/// </summary>
		public char StartMarker { get; }

		/// <summary>
		/// Получает следующий лексический токен в пользовательском формате начиная с указанной позиции в указанной строке.
		/// </summary>
		/// <param name="source">Структурированная строка, состоящая из лексических токенов.</param>
		/// <param name="position">
		/// Позиция в source, начиная с которой будет получен токен.
		/// После получения токена, position будет указывать на позицию, следующую за найденным токеном.
		/// </param>
		/// <returns>Лексический токен в пользовательском формате из указанной позиции в source.</returns>
		public abstract StructuredStringToken ParseToken (ReadOnlySpan<char> source, int position);
	}
}
