using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Основные параметры содержимого.
	/// </summary>
	public class EssentialContentProperties
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса EssentialContentProperties.
		/// </summary>
		public EssentialContentProperties ()
		{
		}

		/// <summary>
		/// Медиатип содержимого.
		/// </summary>
		public ContentMediaType MediaType { get; set; } = ContentMediaType.Unspecified;

		/// <summary>
		/// Медиа подтип содержимого.
		/// </summary>
		public string MediaSubtype { get; set; } = null;

		/// <summary>
		/// Кодировка передачи содержимого.
		/// </summary>
		public ContentTransferEncoding TransferEncoding { get; set; } = ContentTransferEncoding.Unspecified;

		/// <summary>
		/// Коллекция дополнительных параметров содержимого, обычно представленных в поле заголовка "Content-Type".
		/// </summary>
		public IAdjustableList<HeaderFieldParameter> Parameters { get; } = new ArrayList<HeaderFieldParameter> ();
	}
}
