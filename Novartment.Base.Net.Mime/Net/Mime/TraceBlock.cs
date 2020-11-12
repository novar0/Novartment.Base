using System;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Блок трассировки, добавляемый каждым узлом в процессе пересылки сообщения.
	/// </summary>
	public class TraceBlock
	{
		// According to RFC 5322:
		// trace-block = trace *optional-field / *(resent-date / resent-from / resent-sender / resent-to / resent-cc / resent-bcc / resent-msg-id
		// trace = [return] 1*received
		// return = "Return-Path:" path CarriageReturnLinefeed
		// path = angle-addr / ([CFWS] "&lt;" [CFWS] "&gt;" [CFWS])
		// received = "Received:" *received-token ";" date-time CarriageReturnLinefeed
		// received-token = word / angle-addr / addr-spec / domain

		/// <summary>
		/// Инициализирует новый экземпляр класса TraceBlock.
		/// </summary>
		public TraceBlock ()
		{
		}

		/// <summary>
		/// Gets or sets 'Received*' protocol-dependent (not MIME) parameters.
		/// For SMTP typically contains 'name value' pairs such as 'From ... By ... Id ...'.
		/// </summary>
		public string ReceivedParameters { get; set; }

		/// <summary>Gets or sets time received.</summary>
		public DateTimeOffset? ReceivedTime { get; set; }

		/// <summary>Gets or sets date and time message is resent. "Resent-Date" field defined in RFC 2822.</summary>
		public DateTimeOffset? ResentDate { get; set; }

		/// <summary>Gets mailbox of person who actually resends the message. "Resent-Sender" field defined in RFC 2822.</summary>
		public Mailbox ResentSender { get; set; }

		/// <summary>Gets or sets message identifier for resent message. "Resent-Message-ID" field defined in RFC 2822.</summary>
		public AddrSpec ResentMessageId { get; set; }

		/// <summary>Gets mailbox of person for whom message is resent. "Resent-From" field defined in RFC 2822.</summary>
		public IAdjustableList<Mailbox> ResentFrom { get; } = new ArrayList<Mailbox> ();

		/// <summary>Gets mailbox to which message is resent. "Resent-To" field defined in RFC 2822.</summary>
		public IAdjustableList<Mailbox> ResentTo { get; } = new ArrayList<Mailbox> ();

		/// <summary>Gets mailbox(es) to which message is cc'ed on resend. "Resent-CC" field defined in RFC 2822.</summary>
		public IAdjustableList<Mailbox> ResentCC { get; } = new ArrayList<Mailbox> ();

		/// <summary>Gets mailbox(es) to which message is bcc'ed on resend. "Resent-Bcc" field defined in RFC 2822.</summary>
		public IAdjustableList<Mailbox> ResentBcc { get; } = new ArrayList<Mailbox> ();

		/// <summary>
		/// Получает строковое представление объекта.
		/// </summary>
		/// <returns>Строковое представление объекта.</returns>
		public override string ToString ()
		{
			return (this.ReceivedTime.HasValue ? this.ReceivedTime.Value.LocalDateTime.ToString () : string.Empty) +
				" " +
				this.ReceivedParameters;
		}
	}
}