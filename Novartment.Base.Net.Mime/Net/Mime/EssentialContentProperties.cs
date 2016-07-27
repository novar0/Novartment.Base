using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Основные параметры содержимого.
	/// </summary>
	public class EssentialContentProperties
	{
		/// <summary>
		/// Медиатип содержимого.
		/// </summary>
		public ContentMediaType Type { get; set; } = ContentMediaType.Unspecified;

		/// <summary>
		/// Медиа подтип содержимого.
		/// </summary>
		public string Subtype { get; set; } = null;

		/// <summary>
		/// Кодировка передачи содержимого.
		/// </summary>
		public ContentTransferEncoding TransferEncoding { get; set; } = ContentTransferEncoding.Unspecified;

		/// <summary>
		/// Коллекция дополнительных параметров содержимого, обычно представленных в поле заголовка "Content-Type".
		/// </summary>
		public IAdjustableList<HeaderFieldParameter> Parameters { get; } = new ArrayList<HeaderFieldParameter> ();

		/// <summary>
		/// Инициализирует новый экземпляр класса EssentialContentProperties.
		/// </summary>
		public EssentialContentProperties ()
		{
		}
	}
}
