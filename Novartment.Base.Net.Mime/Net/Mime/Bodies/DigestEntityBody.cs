namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности, содержащей дайджест (коллецию интернет-сообщений)
	/// согласно RFC 2046 часть 5.1.5.
	/// </summary>
	public class DigestEntityBody : CompositeEntityBody
	{
		// TODO: вместо наследованного свойства Parts надо сделать свойство IAjustableList<IMailMessage> Messages.

		/// <summary>
		/// Инициализирует новый экземпляр класса DigestEntityBody.
		/// </summary>
		/// <param name="boundary">Разграничитель частей сущности согласно требованиям RFC 1341 часть 7.2.1,
		/// либо null для автоматического генерирования разграничителя.</param>
		public DigestEntityBody (string boundary = null)
			: base (ContentMediaType.Message, MessageMediaSubtypeNames.Rfc822, boundary)
		{
			// RFC 2046 часть 5.1.5:
			// in a digest, the default Content-Type value for a body part is changed from "text/plain" to "message/rfc822".
		}
	}
}
