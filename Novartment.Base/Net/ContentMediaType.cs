
namespace Novartment.Base.Net
{
	/// <summary>
	/// Медиатип согласно <url>http://www.iana.org/assignments/media-types</url>.
	/// </summary>
	public enum ContentMediaType
	{
		/// <summary>Тип не указан.</summary>
		Unspecified = 0,

		/// <summary>
		/// Содержимое можно представить как текст.
		/// Определён в RFC 2046 часть 4.1 ("text").
		/// </summary>
		Text = 1,

		/// <summary>
		/// Содержимое можно представить как изображение.
		/// Определён в RFC 2046 часть 4.2 ("image").
		/// </summary>
		Image = 2,

		/// <summary>
		/// Содержимое можно представить как аудио.
		/// Определён в RFC 2046 часть 4.3 ("audio").
		/// </summary>
		Audio = 3,

		/// <summary>
		/// Содержимое можно представить как видео.
		/// Определён в RFC 2046 часть 4.4 ("video").
		/// </summary>
		Video = 4,

		/// <summary>
		/// Содержимое в формате какого либо специфического приложения.
		/// Определён в RFC 2046 часть 4.5 ("application").
		/// </summary>
		Application = 5,

		/// <summary>
		/// Множественное содержимое.
		/// Определён в RFC 2046 часть 5.1 ("multipart").
		/// </summary>
		Multipart = 6,

		/// <summary>
		/// Содержимое в виде интернет-сообщения.
		/// Определён в RFC 2046 часть 5.2 ("message").
		/// </summary>
		Message = 7
	}
}
