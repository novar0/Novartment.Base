namespace Novartment.Base.Net
{
	/// <summary>
	/// Имя поля заголовка.
	/// </summary>
	public enum HeaderFieldName
	{
		/// <summary>Не указано.</summary>
		Unspecified = 0,

		/// <summary>Message date and time. "Date" header field in RFC 2822.</summary>
		Date = 1,

		/// <summary>Mailbox of message author. "From" header field in RFC 2822.</summary>
		From = 2,

		/// <summary> Mailbox of message sender. "Sender" header field in RFC 2822.</summary>
		Sender = 3,

		/// <summary>Mailbox for replies to message. "Reply-To" header field in RFC 2822.</summary>
		ReplyTo = 4,

		/// <summary>Primary recipient mailbox. "To" header field in RFC 2822.</summary>
		To = 5,

		/// <summary>Carbon-copy recipient mailbox. "CC" header field in RFC 2822.</summary>
		CC = 6,

		/// <summary>Blind-carbon-copy recipient mailbox. "Bcc" header field in RFC 2822.</summary>
		Bcc = 7,

		/// <summary>Message identifier. "Message-ID" header field in RFC 2822.</summary>
		MessageId = 8,

		/// <summary>Identify replied-to message(s). "In-Reply-To" header field in RFC 2822.</summary>
		InReplyTo = 9,

		/// <summary>Related message identifier(s). "References" header field in RFC 2822.</summary>
		References = 10,

		/// <summary>Topic of message. "Subject" header field in RFC 2822.</summary>
		Subject = 11,

		/// <summary>Additional comments about the message. "Comments" header field in RFC 2822.</summary>
		Comments = 12,

		/// <summary>Message key words and/or phrases. "Keywords" header field in RFC 2822.</summary>
		Keywords = 13,

		/// <summary>Date and time message is resent. "Resent-Date" header field in RFC 2822.</summary>
		ResentDate = 14,

		/// <summary>Mailbox of person for whom message is resent. "Resent-From" header field in RFC 2822.</summary>
		ResentFrom = 15,

		/// <summary>Mailbox of person who actually resends the message. "Resent-Sender" header field in RFC 2822.</summary>
		ResentSender = 16,

		/// <summary>Mailbox to which message is resent. "Resent-To" header field in RFC 2822.</summary>
		ResentTo = 17,

		/// <summary>Mailbox(es) to which message is cc'ed on resend. "Resent-CC" header field in RFC 2822.</summary>
		ResentCC = 18,

		/// <summary>Mailbox(es) to which message is bcc'ed on resend. "Resent-Bcc" header field in RFC 2822.</summary>
		ResentBcc = 19,

		/// <summary>Message identifier for resent message. "Resent-Message-ID" header field in RFC 2822.</summary>
		ResentMessageId = 20,

		/// <summary>Message return path. "Return-Path" header field in RFC 2822.</summary>
		ReturnPath = 21,

		/// <summary>Mail transfer trace information. "Received" header field in RFC 2822.</summary>
		Received = 22,

		/// <summary>Mailbox for sending disposition notification. "Disposition-Notification-To" header field in RFC 3798.</summary>
		DispositionNotificationTo = 23,

		/// <summary>Disposition notification options. "Disposition-Notification-Options" header field in RFC 3798.</summary>
		DispositionNotificationOptions = 24,

		/// <summary>Language(s) for auto-responses. "Accept-Language" header field in RFC 3282.</summary>
		AcceptLanguage = 25,

		/// <summary>Original message identifier. "Original-Message-ID" header field in RFC 3297.</summary>
		OriginalMessageId = 26,

		/// <summary>PICS rating label. "PICS-Label" header field in http://www.w3.org/TR/REC-PICS-labels/..</summary>
		PicsLabel = 27,

		/// <summary>Message encoding and other information. "Encoding" header field in RFC 1505.</summary>
		Encoding = 28,

		/// <summary>Type or context of message. "Message-Context" header field in RFC 3458.</summary>
		MessageContext = 29,

		/// <summary>Trace of distribution lists passed. "DL-Expansion-History" header field in RFC 2156.</summary>
		DLExpansionHistory = 30,

		/// <summary>Controls forwarding to alternate recipients. "Alternate-Recipient" header field in RFC 2156.</summary>
		AlternateRecipient = 31,

		/// <summary>Body part types in message. "Original-Encoded-Information-Types" header field in RFC 2156.</summary>
		OriginalEncodedInformationTypes = 32,

		/// <summary>Request delivery report generation. "Generate-Delivery-Report" header field in RFC 2156.</summary>
		GenerateDeliveryReport = 33,

		/// <summary>Non-delivery report required? "Prevent-NonDelivery-Report" header field in RFC 2156.</summary>
		PreventNonDeliveryReport = 34,

		/// <summary>Reference message to be replaced. "Supersedes" header field in RFC 2156.</summary>
		Supersedes = 35,

		/// <summary>Message delivery time. "Delivery-Date" header field in RFC 2156.</summary>
		ExpiryDate = 36,

		/// <summary>Message expiry time. "Expires" header field in RFC 2156.</summary>
		Expires = 37,

		/// <summary>Time by which a reply is requested. "Reply-By" header field in RFC 2156.</summary>
		ReplyBy = 38,

		/// <summary>Message importance. "Importance" header field in RFC 2156.</summary>
		Importance = 39,

		/// <summary>Body parts are missing. "Incomplete-Copy" header field in RFC 2156.</summary>
		IncompleteCopy = 40,

		/// <summary>Message priority. "Priority" header field in RFC 2156.</summary>
		Priority = 41,

		/// <summary>Message content sensitivity. "Sensitivity" header field in RFC 2156.</summary>
		Sensitivity = 42,

		/// <summary>X.400 message content language. "Language" header field in RFC 2156.</summary>
		Language = 43,

		/// <summary>Conversion allowed? "Conversion" header field in RFC 2156.</summary>
		Conversion = 44,

		/// <summary>Lossy conversion allowed? "Conversion-With-Loss" header field in RFC 2156.</summary>
		ConversionWithLoss = 45,

		/// <summary>Message type: delivery report? "Message-Type" header field in RFC 2156.</summary>
		MessageType = 46,

		/// <summary>Automatically submitted indicator. "Autosubmitted" header field in RFC 2156.</summary>
		AutoSubmitted = 47,

		/// <summary>Automatically forwarded indicator. "Autoforwarded" header field in RFC 2156.</summary>
		AutoForwarded = 48,

		/// <summary>Disclose names of other recipients? "Disclose-Recipients" header field in RFC 2156.</summary>
		DiscloseRecipients = 49,

		/// <summary>Deferred delivery information. "Deferred-Delivery" header field in RFC 2156.</summary>
		DeferredDelivery = 50,

		/// <summary>Latest delivery time requested. "Latest-Delivery-Time" header field in RFC 2156.</summary>
		LatestDeliveryTime = 51,

		/// <summary>Originator return address. "Originator-Return-Address" header field in RFC 2156.</summary>
		OriginatorReturnAddress = 52,

		/// <summary>URL of mailing list archive. "List-Archive" header field in RFC 2369.</summary>
		ListArchive = 53,

		/// <summary>URL for mailing list information. "List-Help" header field in RFC 2369.</summary>
		ListHelp = 54,

		/// <summary>URL for mailing list owner's mailbox. "List-Owner" header field in RFC 2369.</summary>
		ListOwner = 55,

		/// <summary> URL for mailing list posting. "List-Post" header field in RFC 2369.</summary>
		ListPost = 56,

		/// <summary>URL for mailing list subscription. "List-Subscribe" header field in RFC 2369.</summary>
		ListSubscribe = 57,

		/// <summary>URL for mailing list unsubscription. "List-Unsubscribe" header field in RFC 2369.</summary>
		ListUnsubscribe = 58,

		/// <summary>Mailing list identifier. "List-ID" header field in RFC 2919.</summary>
		ListId = 59,

		/// <summary>X.400 IPM extensions discarded. "Discarded-X400-IPMS-Extensions" header field in RFC 2156.</summary>
		DiscardedX400IpmsExtensions = 60,

		/// <summary>X.400 MTS extensions discarded. "Discarded-X400-MTS-Extensions" header field in RFC 2156.</summary>
		DiscardedX400MtsExtensions = 61,

		/// <summary>Message content identifier. "X400-Content-Identifier" header field in RFC 2156.</summary>
		X400ContentIdentifier = 62,

		/// <summary>Return content on non-delivery? "X400-Content-Return" header field in RFC 2156.</summary>
		X400ContentReturn = 63,

		/// <summary>X400 content type. "X400-Content-Type" header field in RFC 2156.</summary>
		X400ContentType = 64,

		/// <summary>X400 MTS-Identifier. "X400-MTS-Identifier" header field in RFC 2156.</summary>
		X400MtsIdentifier = 65,

		/// <summary>X400 Originator. "X400-Originator" header field in RFC 2156.</summary>
		X400Originator = 66,

		/// <summary>X400 Received. "X400-Received" header field in RFC 2156.</summary>
		X400Received = 67,

		/// <summary>X400 Recipients. "X400-Recipients" header field in RFC 2156.</summary>
		X400Recipients = 68,

		/// <summary>X400 Trace. "X400-Trace" header field in RFC 2156.</summary>
		X400Trace = 69,

		/// <summary>"MIME-Version". MIME version number. Определено в RFC 2045 part 4.</summary>
		MimeVersion = 70,

		/// <summary>"Content-Type". MIME content type. Определено в RFC 2045 part 5.</summary>
		ContentType = 71,

		/// <summary>"Content-Transfer-Encoding". Content transfer encoding applied. Определено в RFC 2045 part 6.</summary>
		ContentTransferEncoding = 72,

		/// <summary>"Content-ID". Identify content body part. Определено в RFC 2045 part 7.</summary>
		ContentId = 73,

		/// <summary>"Content-Description". Description of message body part. Определено в RFC 2045 part 8.</summary>
		ContentDescription = 74,

		/// <summary>"Content-Disposition". Intended content disposition and file name. Определено в RFC 2183.</summary>
		ContentDisposition = 75,

		/// <summary>"Content-Base". Base to be used for resolving relative URIs within this content part. Определено в RFC 2110 part 4.2.</summary>
		ContentBase = 76,

		/// <summary>"Content-Location". URI for retrieving a body part. Определено в RFC 2557 part 4.2.</summary>
		ContentLocation = 77,

		/// <summary>"Content-features". Indicates content features of a MIME body part. Определено в RFC 2912 part 3.</summary>
		ContentFeatures = 78,

		/// <summary>"Content-Language". Language of message content. Определено в RFC 3282.</summary>
		ContentLanguage = 79,

		/// <summary>"Content-alternative". Alternative content available. Определено в RFC 3297 part 4.</summary>
		ContentAlternative = 80,

		/// <summary>"Content-MD5". MD5 checksum of content. Определено в RFC 1864.</summary>
		ContentMD5 = 81,

		/// <summary>"Content-Duration". Time duration of content. Определено в RFC 2424.</summary>
		ContentDuration = 82,

		/// <summary>"Original-Envelope-Id". Identifier that uniquely identifies the transaction during which the message was submitted.</summary>
		OriginalEnvelopeId = 83,

		/// <summary>"Reporting-MTA". MTA that attempted to perform the delivery, relay, or gateway operation described in the DSN.</summary>
		MailTransferAgent = 84,

		/// <summary>"DSN-Gateway". Name of the gateway or MTA that translated a foreign (non-Internet) delivery status notification into this DSN.</summary>
		DsnGateway = 85,

		/// <summary>"Received-From-MTA". Name of the MTA from which the message was received.</summary>
		ReceivedFromMailTransferAgent = 86,

		/// <summary>"Arrival-Date". Date and time at which the message arrived at the Reporting MTA.</summary>
		ArrivalDate = 87,

		/// <summary>"Original-Recipient". Original recipient address as specified by the sender of the message.</summary>
		OriginalRecipient = 88,

		/// <summary>"Final-Recipient". Recipient for which this set of fields applies.</summary>
		FinalRecipient = 89,

		/// <summary>"Action". Action performed by the Reporting-MTA as a result of its attempt to deliver the message to this recipient address.</summary>
		Action = 90,

		/// <summary>"Status". Transport-independent status code that indicates the delivery status of the message to that recipient.</summary>
		Status = 91,

		/// <summary>"Remote-MTA". Printable ASCII representation of the name of the "remote" MTA that reported delivery status to the "reporting" MTA.</summary>
		RemoteMailTransferAgent = 92,

		/// <summary>"Diagnostic-Code". Actual diagnostic code issued by the mail transport.</summary>
		DiagnosticCode = 93,

		/// <summary>"Last-Attempt-Date". Date and time of the last attempt to relay, gateway, or deliver the message (whether successful or unsuccessful) by the Reporting MTA.</summary>
		LastAttemptDate = 94,

		/// <summary>"Final-Log-ID". Final-log-id of the message that was used by the final-mta.</summary>
		FinalLogId = 95,

		/// <summary>"Will-Retry-Until". Date after which the Reporting MTA expects to abandon all attempts to deliver the message to that recipient.</summary>
		WillRetryUntil = 96,

		/// <summary>"Reporting-UA". MUA that performed the disposition described in the MDN.</summary>
		ReportingUA = 97,

		/// <summary>"MDN-Gateway". Name of the gateway or MTA that translated a foreign (non-Internet) message disposition notification into this MDN.</summary>
		MdnGateway = 98,

		/// <summary>"Disposition". Action performed by the Reporting-MUA on behalf of the user.</summary>
		Disposition = 99,

		/// <summary>"Failure". Additional information when the "failure" disposition modifier appear.</summary>
		Failure = 100,

		/// <summary>"Error". Additional information when the "error" disposition modifier appear.</summary>
		Error = 101,

		/// <summary>"Warning". Additional information when the "warning" disposition modifier appear.</summary>
		Warning = 102,

		/// <summary>Нестандартное дополнительное имя. Для использования в расширениях протоколов.</summary>
		Extension = 0x7ffffff,
	}
}
