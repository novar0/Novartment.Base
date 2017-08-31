namespace Novartment.Base.Media
{
	/// <summary>
	/// Информация о присоединённом файле.
	/// </summary>
	public class MatroskaAttachedFileInfo
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса MatroskaAttachedFileInfo с указанным именем и медиа-типом.
		/// </summary>
		/// <param name="name">Имя элемента.</param>
		/// <param name="contentType">Медиа-тип элемента.</param>
		public MatroskaAttachedFileInfo (string name, string contentType)
		{
			this.Name = name;
			this.ContentType = contentType;
		}

		/// <summary>
		/// Получает имя файла.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает медиа-тип файла.
		/// </summary>
		public string ContentType { get; }
	}
}
