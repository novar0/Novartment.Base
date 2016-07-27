using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности, содержащей отчет о событиях, связанных с сообщением согласно RFC 6522.
	/// </summary>
	// TODO: вместо наследованного свойства Parts надо сделать свойства IEntity Description, Entity Status, IMailMessage ReturnedMessage.
	public class ReportEntityBody : CompositeEntityBody,
		ICompositeEntityBody
	{
		/*
		The multipart/report media type contains either two or three sub-parts, in the following order:

		1.  (REQUIRED) The first body part contains a human-readable message.
		The purpose of this message is to provide an easily understood description of the condition(s) that caused the report to be generated,
		for a human reader who might not have a user agent capable of interpreting the second section of the multipart/report.
		The text in the first section can use any IANA-registered MIME media type, charset, or language.
		Where a description of the error is desired in several languages or several media, a multipart/alternative construct MAY be used.
		This body part MAY also be used to send detailed information that cannot be easily formatted into the second body part.

		2.  (REQUIRED) A machine-parsable body part containing an account of the reported message handling event.
		The purpose of this body part is to provide a machine-readable description of the condition(s) that caused the report to be generated,
		along with details not present in the first body part that might be useful to human experts.
		An initial body part, message/delivery-status, is defined in RFC 3464.

		3.  (OPTIONAL) A body part containing the returned message or a portion thereof.
		This information could be useful to aid human experts in diagnosing problems.
		(Although it might also be useful to allow the sender to identify the message about which the report was issued,
		it is hoped that the envelope-id and original-recipient-address returned in the message/report body part will replace the traditional use
		of the returned content for this purpose.)
		*/

		//RFC 3464 part 2 "Format of a Delivery Status Notification":
		//The report-type parameter of the multipart/report content is "delivery-status".
		//
		//RFC 3798 part 3 "Format of a Message Disposition Notification":
		//The report-type parameter of the multipart/report content is "disposition-notification".

		/// <summary>Получает тип отчёта, определяющий медиа подтип статусной части (второй вложенной сущности). Определено в RFC 6522.</summary>
		public string ReportType { get; }

		/// <summary>
		/// Инициализирует новый экземпляр класса ReportEntityBody.
		/// </summary>
		/// <param name="reportType">Тип отчёта, определяющий медиа подтип статусной части (второй вложенной сущности).</param>
		/// <param name="boundary">Разграничитель частей сущности согласно требованиям RFC 1341 часть 7.2.1,
		/// либо null для автоматического генерирования разграничителя.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public ReportEntityBody (
			string reportType,
			string boundary = null) :
			base (boundary)
		{
			this.ReportType = reportType;
		}
	}
}
