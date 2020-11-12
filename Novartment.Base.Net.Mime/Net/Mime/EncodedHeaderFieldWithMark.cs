namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Обёртка для поля заголовка, позволяющая ставить на нём отметку.
	/// </summary>
	public sealed class EncodedHeaderFieldWithMark :
		IMarkHolder
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldWithMark с указанным полем заголовка.
		/// </summary>
		/// <param name="field">Поле заголовка.</param>
		public EncodedHeaderFieldWithMark (EncodedHeaderField field)
		{
			this.Field = field;
		}

		/// <summary>
		/// Получает поле заголовка.
		/// </summary>
		public EncodedHeaderField Field { get; }

		/// <summary>
		/// Получает наличие отметки объекта.
		/// </summary>
		public bool IsMarked { get; set; }
	}
}
