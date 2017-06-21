using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Разновидность трэка матрёшка-файла.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Matroska",
		Justification = "'Matroska' represents standard term.")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1028:EnumStorageShouldBeInt32",
		Justification = "Enumeration defined externally in MKV specification.")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1027:MarkEnumsWithFlags",
		Justification = "Enumeration defined externally in MKV specification.")]
	public enum MatroskaTrackType : byte
	{
		/// <summary>Не указано.</summary>
		Unspecified = 0,

		/// <summary>Видео.</summary>
		Video = 1,

		/// <summary>Аудио.</summary>
		Audio = 2,

		/// <summary>Комплексный.</summary>
		Complex = 3,

		/// <summary>Логотип.</summary>
		Logo = 0x10,

		/// <summary>Субтитры</summary>
		Subtitle = 0x11,

		/// <summary>Кнопки.</summary>
		Buttons = 0x12,

		/// <summary>Управление.</summary>
		Control = 0x20,
	}
}
