using System;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Уведомление о статусе доставки сообщения одному адресату.
	/// Определено в RFC 3464.
	/// </summary>
	public sealed class RecipientDeliveryStatus
	{
		// TODO: добавить валидацию при установке свойств

		/// <summary>
		/// Инициализирует новый экземпляр класса RecipientDeliveryStatus для указанного адресата, действия и статуса.
		/// </summary>
		/// <param name="recipient">Адресат, к которому относятся остальные свойства.</param>
		/// <param name="action">Действие, предпринятое почтовым агентом в результате попытки доставки сообщения адресату.</param>
		/// <param name="status">Транспорт-независимый код соответствующий статусу доставки сообщения адресату.</param>
		public RecipientDeliveryStatus (NotificationFieldValue recipient, DeliveryAttemptResult action, string status)
		{
			if (action == DeliveryAttemptResult.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (action));
			}

			this.FinalRecipient = recipient ?? throw new ArgumentNullException (nameof (recipient));
			this.Status = status ?? throw new ArgumentNullException (nameof (status));
			this.Action = action;
		}

		/// <summary>Gets or sets original recipient address as specified by the sender of the message for which the DSN is being issued.</summary>
		public NotificationFieldValue OriginalRecipient { get; set; }

		/// <summary>Gets recipient for which this set of per-recipient fields applies.</summary>
		public NotificationFieldValue FinalRecipient { get; }

		/// <summary>Gets action performed by the Reporting-MTA as a result of its attempt to deliver the message to this recipient address.</summary>
		public DeliveryAttemptResult Action { get; }

		/// <summary>Gets transport-independent status code that indicates the delivery status of the message to that recipient.</summary>
		public string Status { get; }

		/// <summary>Gets or sets printable ASCII representation of the name of the "remote" MTA that reported delivery status to the "reporting" MTA.</summary>
		public NotificationFieldValue RemoteMailTransferAgent { get; set; }

		/// <summary>Gets or sets actual diagnostic code issued by the mail transport.</summary>
		public NotificationFieldValue DiagnosticCode { get; set; }

		/// <summary>Gets or sets date and time of the last attempt to relay, gateway, or deliver
		/// the message (whether successful or unsuccessful) by the Reporting MTA.</summary>
		public DateTimeOffset? LastAttemptDate { get; set; }

		/// <summary>Gets or sets final-log-id of the message that was used by the final-mta.</summary>
		public string FinalLogId { get; set; }

		/// <summary>Gets or sets date after which the Reporting MTA expects to abandon all attempts to deliver the message to that recipient.</summary>
		public DateTimeOffset? WillRetryUntil { get; set; }
	}
}
