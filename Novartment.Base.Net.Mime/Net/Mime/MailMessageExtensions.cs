using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Методы расширения для IMailMessage.
	/// </summary>
	public static class MailMessageExtensions
	{
		/// <summary>
		/// Загружает сообщение из указанного источника данных.
		/// </summary>
		/// <param name="message">Сообщение, содержимое которого будет загружено из указанного источника данных.</param>
		/// <param name="source">Источник данных из которого будет загружено сообщение.</param>
		/// <param name="bodyFactory">Фабрика, позволяющая создавать тело сущности с указанными параметрами.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public static Task LoadAsync (
			this MailMessage message,
			IBufferedSource source,
			Func<EssentialContentProperties, IEntityBody> bodyFactory,
			CancellationToken cancellationToken)
		{
			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}

			Contract.EndContractBlock ();

			return message.LoadAsync (source, bodyFactory, Entity.DefaultType, Entity.DefaultSubtype, cancellationToken);
		}

		/// <summary>
		/// Создаёт уведомление о доставке указанного сообщения.
		/// </summary>
		/// <param name="message">Сообщение, для которого нужно создать уведомление о доставке.</param>
		/// <param name="explanation">Описание того, что призошло при доставке сообщения.</param>
		/// <param name="postmaster">Отправитель, от имени которого будет отправлено уведомление.</param>
		/// <returns>
		/// Созданное сообщение, параметры которого установлены в соответствии с правилами создания уведомления о доставке.
		/// </returns>
		// TODO: добавить параметры чтобы окончательно формировать уведомление
		public static MailMessage CreateDeliveryStatusNotification (
			this MailMessage message,
			string explanation,
			Mailbox postmaster)
		{
			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}

			Contract.EndContractBlock ();

			// RFC 3464:
			// 2. Format of a Delivery Status Notification (DSN)
			// A DSN is a MIME message with a top-level content-type of multipart/report.
			// When a multipart/report content is used to transmit a DSN:
			// (a) The report-type parameter of the multipart/report content is "delivery-status".
			// (b) The first component of the multipart/report contains a human-readable explanation of the DSN, as described in [REPORT].
			// (c) The second component of the multipart/report is of content-type message/delivery-status, described in section 2.1 of this document.
			// (d) If the original message or a portion of the message is to be returned to the sender, it appears as the third component of the multipart/report.
			var newMsg = MailMessage.CreateReport (MessageMediaSubtypeNames.DeliveryStatus);
			newMsg.Subject = "Delivery Status Notification";
			newMsg.GenerateId ();

			newMsg.From.Add (postmaster);

			// RFC 5321 part 4.4:
			// The reverse-path address (as copied into the Return-path) MUST be used as the target of any mail containing delivery error messages.
			if (message.ReturnPath != null)
			{
				newMsg.RecipientTo.Add (message.ReturnPath);
			}
			else
			{
				if (message.Sender != null)
				{
					newMsg.RecipientTo.Add (message.Sender);
				}
				else
				{
					newMsg.RecipientTo.AddRange (message.From);
				}
			}

			var textExplanation = newMsg.AddTextPart (explanation);

			var deliveryStatus = newMsg.AddDeliveryStatusPart ();

			// TODO: добавить заполение свойств deliveryStatus

			// создаём пустое письмо, в которое копируем только главные поля
			var returnedMessage = MailMessage.CreateSimpleText ();
			returnedMessage.OriginationDate = message.OriginationDate;
			returnedMessage.From.AddRange (message.From);
			returnedMessage.RecipientTo.AddRange (message.RecipientTo);
			returnedMessage.RecipientCC.AddRange (message.RecipientCC);
			returnedMessage.RecipientBcc.AddRange (message.RecipientBcc);
			returnedMessage.Sender = message.Sender;
			returnedMessage.Subject = message.Subject;
			returnedMessage.MessageId = message.MessageId;
			newMsg.AddMessagePart (returnedMessage, true);

			// TODO: добавить какой то механизм чтобы при отправке этого письма не был указан адрес возврата в SMTP-команде MAIL FROM:<>
			return newMsg;
		}

		/// <summary>
		/// Создаёт уведомление об изменении дислокации указанного сообщения.
		/// </summary>
		/// <param name="message">Сообщение, для которого нужно создать уведомление об изменении его дислокации.</param>
		/// <param name="recipient">Получатель, который является инициаторм изменения дислокации сообщения.</param>
		/// <param name="reportingUserAgentName">Агент пользователя, который произвёл изменения дислокации сообщения.
		/// For Internet Mail user agents, it is recommended that message field contain both
		/// the DNS name of the particular instance of the MUA that generated the MDN, and the name of the product.</param>
		/// <param name="dispositionType">Действие, произведённое с сообщением.</param>
		/// <param name="includeFullSourceMessage">Признак вставки в уведомление полной копии исходного сообщения.
		/// Если false, то будет вставлена копия только заголовка исходного сообщения.</param>
		/// <returns>
		/// Созданное сообщение, параметры которого установлены в соответствии с правилами создания уведомления об изменении дислокации.
		/// </returns>
		public static MailMessage CreateDispositionNotification (
			this MailMessage message,
			Mailbox recipient,
			string reportingUserAgentName,
			MessageDispositionChangedAction dispositionType,
			bool includeFullSourceMessage)
		{
			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}

			if (recipient == null)
			{
				throw new ArgumentNullException (nameof (recipient));
			}

			if (reportingUserAgentName == null)
			{
				throw new ArgumentNullException (nameof (reportingUserAgentName));
			}

			if (dispositionType == MessageDispositionChangedAction.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (dispositionType));
			}

			Contract.EndContractBlock ();

			if (message.DispositionNotificationTo.Count < 1)
			{
				throw new InvalidOperationException ("Unable to create Disposition Notification when it not requested.");
			}

			var newMsg = MailMessage.CreateReport (MessageMediaSubtypeNames.DispositionNotification);
			newMsg.Subject = "Disposition notification";
			newMsg.GenerateId ();

			// The MDN MUST be addressedto the address(es) from the Disposition-Notification-To header
			// from the original newMsg for which the MDN is being generated.
			newMsg.RecipientTo.AddRange (message.DispositionNotificationTo);

			// The From field of the newMsg header of the MDN MUST contain the address of the person
			// for whom the newMsg disposition notification is being issued.
			newMsg.From.Add (recipient);

			var template = ((dispositionType == MessageDispositionChangedAction.AutomaticallyDisplayed) || (dispositionType == MessageDispositionChangedAction.ManuallyDisplayed)) ?
				Resources.DispositionNotificationDisplayedMessage :
				Resources.DispositionNotificationDeletedMessage;
			var text = string.Format (CultureInfo.InvariantCulture, template, message.OriginationDate, message.RecipientTo[0], message.Subject);
			newMsg.AddTextPart (text, null, TextMediaSubtypeNames.Plain, ContentTransferEncoding.QuotedPrintable);

			var part2 = newMsg.AddDispositionNotificationPart (recipient.Address, dispositionType);
			((DispositionNotificationEntityBody)part2.Body).ReportingUserAgentName = reportingUserAgentName;
			if (message.MessageId != null)
			{
				((DispositionNotificationEntityBody)part2.Body).OriginalMessageId = message.MessageId;
			}

			newMsg.AddMessagePart (message, includeFullSourceMessage);

			// очищаем запрос на уведомление, потому что уведомление можно посылать только один раз
			message.DispositionNotificationTo.Clear ();
			message.DispositionNotificationOptions.Clear ();
			return newMsg;
		}

		/// <summary>
		/// Создаёт ответ на указанное сообщение.
		/// </summary>
		/// <param name="message">Сообщение, для которого нужно создать ответ.</param>
		/// <returns>
		/// Созданное сообщение, параметры которого установлены в соответствии с правилами создания ответа на сообщение.
		/// </returns>
		public static MailMessage CreateReply (this MailMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}

			Contract.EndContractBlock ();

			var newMsg = MailMessage.CreateComposite ();
			newMsg.RecipientTo.AddRange (message.ReplyTo);
			if (newMsg.RecipientTo.Count < 1)
			{
				newMsg.RecipientTo.AddRange (message.From);
			}

			newMsg.References.AddRange (message.References);
			if (message.MessageId != null)
			{
				newMsg.InReplyTo.Add (message.MessageId);
				newMsg.References.Add (message.MessageId);
			}

			// When used in a message, the field body MAY start with the string "Re: "
			// (an abbreviation of the Latin "in re", meaning "in the matter of")
			// followed by the contents of the "Subject:" field body of the original message.
			// If message is done, only one instance of the literal string "Re: " ought to be used
			// since use of other strings or more than one instance can lead to undesirable consequences.
			var isReply = message.Subject.StartsWith ("Re:", StringComparison.OrdinalIgnoreCase);
			newMsg.Subject = isReply ?
				message.Subject :
				("Re: " + message.Subject);

			return newMsg;
		}
	}
}
