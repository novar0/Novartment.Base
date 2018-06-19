using System;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Обёртка для поля заголовка, позволяющая ставить на нём отметку.
	/// </summary>
	public class HeaderFieldWithMark :
		IMarkHolder
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldWithMark с указанным полем заголовка.
		/// </summary>
		/// <param name="field">Поле заголовка.</param>
		public HeaderFieldWithMark (HeaderField field)
		{
			this.Field = field;
		}

		/// <summary>
		/// Получает поле заголовка.
		/// </summary>
		public HeaderField Field { get; }

		/// <summary>
		/// Получает наличие отметки объекта.
		/// </summary>
		public bool IsMarked { get; set; }
	}
}
