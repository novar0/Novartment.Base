using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Информация о присоединённом файле.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Matroska",
		Justification = "'Matroska' represents standard term.")]
	public class MatroskaAttachedFileInfo
	{
		/// <summary>
		/// Получает имя файла.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает медиа-тип файла.
		/// </summary>
		public string ContentType { get; }

		/// <summary>
		/// Инициализирует новый экземпляр класса MatroskaAttachedFileInfo с указанным именем и медиа-типом.
		/// </summary>
		public MatroskaAttachedFileInfo (string name, string contentType)
		{
			this.Name = name;
			this.ContentType = contentType;
		}
	}
}
