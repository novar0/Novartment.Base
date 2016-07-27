using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	internal static class HeaderFieldNameHelper
	{
		private static Dictionary<HeaderFieldName, string> _headerTypes = new Dictionary<HeaderFieldName, string> ()
		{
			[HeaderFieldName.Date] = HeaderFieldNames.Date,
			[HeaderFieldName.From] = HeaderFieldNames.From,
			[HeaderFieldName.Sender] = HeaderFieldNames.Sender,
			[HeaderFieldName.ReplyTo] = HeaderFieldNames.ReplyTo,
			[HeaderFieldName.To] = HeaderFieldNames.To,
			[HeaderFieldName.CC] = HeaderFieldNames.CC,
			[HeaderFieldName.Bcc] = HeaderFieldNames.Bcc,
			[HeaderFieldName.MessageId] = HeaderFieldNames.MessageId,
			[HeaderFieldName.InReplyTo] = HeaderFieldNames.InReplyTo,
			[HeaderFieldName.References] = HeaderFieldNames.References,
			[HeaderFieldName.Subject] = HeaderFieldNames.Subject,
			[HeaderFieldName.Comments] = HeaderFieldNames.Comments,
			[HeaderFieldName.Keywords] = HeaderFieldNames.Keywords,
			[HeaderFieldName.ResentDate] = HeaderFieldNames.ResentDate,
			[HeaderFieldName.ResentFrom] = HeaderFieldNames.ResentFrom,
			[HeaderFieldName.ResentSender] = HeaderFieldNames.ResentSender,
			[HeaderFieldName.ResentTo] = HeaderFieldNames.ResentTo,
			[HeaderFieldName.ResentCC] = HeaderFieldNames.ResentCC,
			[HeaderFieldName.ResentBcc] = HeaderFieldNames.ResentBcc,
			[HeaderFieldName.ResentMessageId] = HeaderFieldNames.ResentMessageId,
			[HeaderFieldName.ReturnPath] = HeaderFieldNames.ReturnPath,
			[HeaderFieldName.Received] = HeaderFieldNames.Received,
			[HeaderFieldName.DispositionNotificationTo] = HeaderFieldNames.DispositionNotificationTo,
			[HeaderFieldName.DispositionNotificationOptions] = HeaderFieldNames.DispositionNotificationOptions,
			[HeaderFieldName.AcceptLanguage] = HeaderFieldNames.AcceptLanguage,
			[HeaderFieldName.OriginalMessageId] = HeaderFieldNames.OriginalMessageId,
			[HeaderFieldName.PicsLabel] = HeaderFieldNames.PicsLabel,
			[HeaderFieldName.Encoding] = HeaderFieldNames.Encoding,
			[HeaderFieldName.MessageContext] = HeaderFieldNames.MessageContext,
			[HeaderFieldName.DLExpansionHistory] = HeaderFieldNames.DLExpansionHistory,
			[HeaderFieldName.AlternateRecipient] = HeaderFieldNames.AlternateRecipient,
			[HeaderFieldName.OriginalEncodedInformationTypes] = HeaderFieldNames.OriginalEncodedInformationTypes,
			[HeaderFieldName.GenerateDeliveryReport] = HeaderFieldNames.GenerateDeliveryReport,
			[HeaderFieldName.PreventNonDeliveryReport] = HeaderFieldNames.PreventNonDeliveryReport,
			[HeaderFieldName.Supersedes] = HeaderFieldNames.Supersedes,
			[HeaderFieldName.ExpiryDate] = HeaderFieldNames.ExpiryDate,
			[HeaderFieldName.Expires] = HeaderFieldNames.Expires,
			[HeaderFieldName.ReplyBy] = HeaderFieldNames.ReplyBy,
			[HeaderFieldName.Importance] = HeaderFieldNames.Importance,
			[HeaderFieldName.IncompleteCopy] = HeaderFieldNames.IncompleteCopy,
			[HeaderFieldName.Priority] = HeaderFieldNames.Priority,
			[HeaderFieldName.Sensitivity] = HeaderFieldNames.Sensitivity,
			[HeaderFieldName.Language] = HeaderFieldNames.Language,
			[HeaderFieldName.Conversion] = HeaderFieldNames.Conversion,
			[HeaderFieldName.ConversionWithLoss] = HeaderFieldNames.ConversionWithLoss,
			[HeaderFieldName.MessageType] = HeaderFieldNames.MessageType,
			[HeaderFieldName.AutoSubmitted] = HeaderFieldNames.Autosubmitted,
			[HeaderFieldName.AutoForwarded] = HeaderFieldNames.Autoforwarded,
			[HeaderFieldName.DiscloseRecipients] = HeaderFieldNames.DiscloseRecipients,
			[HeaderFieldName.DeferredDelivery] = HeaderFieldNames.DeferredDelivery,
			[HeaderFieldName.LatestDeliveryTime] = HeaderFieldNames.LatestDeliveryTime,
			[HeaderFieldName.OriginatorReturnAddress] = HeaderFieldNames.OriginatorReturnAddress,
			[HeaderFieldName.ListArchive] = HeaderFieldNames.ListArchive,
			[HeaderFieldName.ListHelp] = HeaderFieldNames.ListHelp,
			[HeaderFieldName.ListOwner] = HeaderFieldNames.ListOwner,
			[HeaderFieldName.ListPost] = HeaderFieldNames.ListPost,
			[HeaderFieldName.ListSubscribe] = HeaderFieldNames.ListSubscribe,
			[HeaderFieldName.ListUnsubscribe] = HeaderFieldNames.ListUnsubscribe,
			[HeaderFieldName.ListId] = HeaderFieldNames.ListId,
			[HeaderFieldName.DiscardedX400IpmsExtensions] = HeaderFieldNames.DiscardedX400IpmsExtensions,
			[HeaderFieldName.DiscardedX400MtsExtensions] = HeaderFieldNames.DiscardedX400MtsExtensions,
			[HeaderFieldName.X400ContentIdentifier] = HeaderFieldNames.X400ContentIdentifier,
			[HeaderFieldName.X400ContentReturn] = HeaderFieldNames.X400ContentReturn,
			[HeaderFieldName.X400ContentType] = HeaderFieldNames.X400ContentType,
			[HeaderFieldName.X400MtsIdentifier] = HeaderFieldNames.X400MtsIdentifier,
			[HeaderFieldName.X400Originator] = HeaderFieldNames.X400Originator,
			[HeaderFieldName.X400Received] = HeaderFieldNames.X400Received,
			[HeaderFieldName.X400Recipients] = HeaderFieldNames.X400Recipients,
			[HeaderFieldName.X400Trace] = HeaderFieldNames.X400Trace,
			[HeaderFieldName.MimeVersion] = HeaderFieldNames.MimeVersion,
			[HeaderFieldName.ContentType] = HeaderFieldNames.ContentType,
			[HeaderFieldName.ContentTransferEncoding] = HeaderFieldNames.ContentTransferEncoding,
			[HeaderFieldName.ContentId] = HeaderFieldNames.ContentId,
			[HeaderFieldName.ContentDescription] = HeaderFieldNames.ContentDescription,
			[HeaderFieldName.ContentDisposition] = HeaderFieldNames.ContentDisposition,
			[HeaderFieldName.ContentBase] = HeaderFieldNames.ContentBase,
			[HeaderFieldName.ContentLocation] = HeaderFieldNames.ContentLocation,
			[HeaderFieldName.ContentFeatures] = HeaderFieldNames.ContentFeatures,
			[HeaderFieldName.ContentLanguage] = HeaderFieldNames.ContentLanguage,
			[HeaderFieldName.ContentAlternative] = HeaderFieldNames.ContentAlternative,
			[HeaderFieldName.ContentMD5] = HeaderFieldNames.ContentMD5,
			[HeaderFieldName.ContentDuration] = HeaderFieldNames.ContentDuration,
			[HeaderFieldName.OriginalEnvelopeId] = HeaderFieldNames.OriginalEnvelopeId,
			[HeaderFieldName.MailTransferAgent] = HeaderFieldNames.MailTransferAgent,
			[HeaderFieldName.DsnGateway] = HeaderFieldNames.DsnGateway,
			[HeaderFieldName.ReceivedFromMailTransferAgent] = HeaderFieldNames.ReceivedFromMailTransferAgent,
			[HeaderFieldName.ArrivalDate] = HeaderFieldNames.ArrivalDate,
			[HeaderFieldName.OriginalRecipient] = HeaderFieldNames.OriginalRecipient,
			[HeaderFieldName.FinalRecipient] = HeaderFieldNames.FinalRecipient,
			[HeaderFieldName.Action] = HeaderFieldNames.Action,
			[HeaderFieldName.Status] = HeaderFieldNames.Status,
			[HeaderFieldName.RemoteMailTransferAgent] = HeaderFieldNames.RemoteMailTransferAgent,
			[HeaderFieldName.DiagnosticCode] = HeaderFieldNames.DiagnosticCode,
			[HeaderFieldName.LastAttemptDate] = HeaderFieldNames.LastAttemptDate,
			[HeaderFieldName.FinalLogId] = HeaderFieldNames.FinalLogId,
			[HeaderFieldName.WillRetryUntil] = HeaderFieldNames.WillRetryUntil,
			[HeaderFieldName.ReportingUA] = HeaderFieldNames.ReportingUA,
			[HeaderFieldName.MdnGateway] = HeaderFieldNames.MdnGateway,
			[HeaderFieldName.Disposition] = HeaderFieldNames.Disposition,
			[HeaderFieldName.Failure] = HeaderFieldNames.Failure,
			[HeaderFieldName.Error] = HeaderFieldNames.Error,
			[HeaderFieldName.Warning] = HeaderFieldNames.Warning
		};
		private static Dictionary<string, HeaderFieldName> _headerTypeNames = new Dictionary<string, HeaderFieldName> (StringComparer.OrdinalIgnoreCase)
		{
			[HeaderFieldNames.Date] = HeaderFieldName.Date,
			[HeaderFieldNames.From] = HeaderFieldName.From,
			[HeaderFieldNames.Sender] = HeaderFieldName.Sender,
			[HeaderFieldNames.ReplyTo] = HeaderFieldName.ReplyTo,
			[HeaderFieldNames.To] = HeaderFieldName.To,
			[HeaderFieldNames.CC] = HeaderFieldName.CC,
			[HeaderFieldNames.Bcc] = HeaderFieldName.Bcc,
			[HeaderFieldNames.MessageId] = HeaderFieldName.MessageId,
			[HeaderFieldNames.InReplyTo] = HeaderFieldName.InReplyTo,
			[HeaderFieldNames.References] = HeaderFieldName.References,
			[HeaderFieldNames.Subject] = HeaderFieldName.Subject,
			[HeaderFieldNames.Comments] = HeaderFieldName.Comments,
			[HeaderFieldNames.Keywords] = HeaderFieldName.Keywords,
			[HeaderFieldNames.ResentDate] = HeaderFieldName.ResentDate,
			[HeaderFieldNames.ResentFrom] = HeaderFieldName.ResentFrom,
			[HeaderFieldNames.ResentSender] = HeaderFieldName.ResentSender,
			[HeaderFieldNames.ResentTo] = HeaderFieldName.ResentTo,
			[HeaderFieldNames.ResentCC] = HeaderFieldName.ResentCC,
			[HeaderFieldNames.ResentBcc] = HeaderFieldName.ResentBcc,
			[HeaderFieldNames.ResentMessageId] = HeaderFieldName.ResentMessageId,
			[HeaderFieldNames.ReturnPath] = HeaderFieldName.ReturnPath,
			[HeaderFieldNames.Received] = HeaderFieldName.Received,
			[HeaderFieldNames.DispositionNotificationTo] = HeaderFieldName.DispositionNotificationTo,
			[HeaderFieldNames.DispositionNotificationOptions] = HeaderFieldName.DispositionNotificationOptions,
			[HeaderFieldNames.AcceptLanguage] = HeaderFieldName.AcceptLanguage,
			[HeaderFieldNames.OriginalMessageId] = HeaderFieldName.OriginalMessageId,
			[HeaderFieldNames.PicsLabel] = HeaderFieldName.PicsLabel,
			[HeaderFieldNames.Encoding] = HeaderFieldName.Encoding,
			[HeaderFieldNames.MessageContext] = HeaderFieldName.MessageContext,
			[HeaderFieldNames.DLExpansionHistory] = HeaderFieldName.DLExpansionHistory,
			[HeaderFieldNames.AlternateRecipient] = HeaderFieldName.AlternateRecipient,
			[HeaderFieldNames.OriginalEncodedInformationTypes] = HeaderFieldName.OriginalEncodedInformationTypes,
			[HeaderFieldNames.GenerateDeliveryReport] = HeaderFieldName.GenerateDeliveryReport,
			[HeaderFieldNames.PreventNonDeliveryReport] = HeaderFieldName.PreventNonDeliveryReport,
			[HeaderFieldNames.Supersedes] = HeaderFieldName.Supersedes,
			[HeaderFieldNames.ExpiryDate] = HeaderFieldName.ExpiryDate,
			[HeaderFieldNames.Expires] = HeaderFieldName.Expires,
			[HeaderFieldNames.ReplyBy] = HeaderFieldName.ReplyBy,
			[HeaderFieldNames.Importance] = HeaderFieldName.Importance,
			[HeaderFieldNames.IncompleteCopy] = HeaderFieldName.IncompleteCopy,
			[HeaderFieldNames.Priority] = HeaderFieldName.Priority,
			[HeaderFieldNames.Sensitivity] = HeaderFieldName.Sensitivity,
			[HeaderFieldNames.Language] = HeaderFieldName.Language,
			[HeaderFieldNames.Conversion] = HeaderFieldName.Conversion,
			[HeaderFieldNames.ConversionWithLoss] = HeaderFieldName.ConversionWithLoss,
			[HeaderFieldNames.MessageType] = HeaderFieldName.MessageType,
			[HeaderFieldNames.Autosubmitted] = HeaderFieldName.AutoSubmitted,
			[HeaderFieldNames.Autoforwarded] = HeaderFieldName.AutoForwarded,
			[HeaderFieldNames.DiscloseRecipients] = HeaderFieldName.DiscloseRecipients,
			[HeaderFieldNames.DeferredDelivery] = HeaderFieldName.DeferredDelivery,
			[HeaderFieldNames.LatestDeliveryTime] = HeaderFieldName.LatestDeliveryTime,
			[HeaderFieldNames.OriginatorReturnAddress] = HeaderFieldName.OriginatorReturnAddress,
			[HeaderFieldNames.ListArchive] = HeaderFieldName.ListArchive,
			[HeaderFieldNames.ListHelp] = HeaderFieldName.ListHelp,
			[HeaderFieldNames.ListOwner] = HeaderFieldName.ListOwner,
			[HeaderFieldNames.ListPost] = HeaderFieldName.ListPost,
			[HeaderFieldNames.ListSubscribe] = HeaderFieldName.ListSubscribe,
			[HeaderFieldNames.ListUnsubscribe] = HeaderFieldName.ListUnsubscribe,
			[HeaderFieldNames.ListId] = HeaderFieldName.ListId,
			[HeaderFieldNames.DiscardedX400IpmsExtensions] = HeaderFieldName.DiscardedX400IpmsExtensions,
			[HeaderFieldNames.DiscardedX400MtsExtensions] = HeaderFieldName.DiscardedX400MtsExtensions,
			[HeaderFieldNames.X400ContentIdentifier] = HeaderFieldName.X400ContentIdentifier,
			[HeaderFieldNames.X400ContentReturn] = HeaderFieldName.X400ContentReturn,
			[HeaderFieldNames.X400ContentType] = HeaderFieldName.X400ContentType,
			[HeaderFieldNames.X400MtsIdentifier] = HeaderFieldName.X400MtsIdentifier,
			[HeaderFieldNames.X400Originator] = HeaderFieldName.X400Originator,
			[HeaderFieldNames.X400Received] = HeaderFieldName.X400Received,
			[HeaderFieldNames.X400Recipients] = HeaderFieldName.X400Recipients,
			[HeaderFieldNames.X400Trace] = HeaderFieldName.X400Trace,
			[HeaderFieldNames.MimeVersion] = HeaderFieldName.MimeVersion,
			[HeaderFieldNames.ContentType] = HeaderFieldName.ContentType,
			[HeaderFieldNames.ContentTransferEncoding] = HeaderFieldName.ContentTransferEncoding,
			[HeaderFieldNames.ContentId] = HeaderFieldName.ContentId,
			[HeaderFieldNames.ContentDescription] = HeaderFieldName.ContentDescription,
			[HeaderFieldNames.ContentDisposition] = HeaderFieldName.ContentDisposition,
			[HeaderFieldNames.ContentBase] = HeaderFieldName.ContentBase,
			[HeaderFieldNames.ContentLocation] = HeaderFieldName.ContentLocation,
			[HeaderFieldNames.ContentFeatures] = HeaderFieldName.ContentFeatures,
			[HeaderFieldNames.ContentLanguage] = HeaderFieldName.ContentLanguage,
			[HeaderFieldNames.ContentAlternative] = HeaderFieldName.ContentAlternative,
			[HeaderFieldNames.ContentMD5] = HeaderFieldName.ContentMD5,
			[HeaderFieldNames.ContentDuration] = HeaderFieldName.ContentDuration,
			[HeaderFieldNames.OriginalEnvelopeId] = HeaderFieldName.OriginalEnvelopeId,
			[HeaderFieldNames.MailTransferAgent] = HeaderFieldName.MailTransferAgent,
			[HeaderFieldNames.DsnGateway] = HeaderFieldName.DsnGateway,
			[HeaderFieldNames.ReceivedFromMailTransferAgent] = HeaderFieldName.ReceivedFromMailTransferAgent,
			[HeaderFieldNames.ArrivalDate] = HeaderFieldName.ArrivalDate,
			[HeaderFieldNames.OriginalRecipient] = HeaderFieldName.OriginalRecipient,
			[HeaderFieldNames.FinalRecipient] = HeaderFieldName.FinalRecipient,
			[HeaderFieldNames.Action] = HeaderFieldName.Action,
			[HeaderFieldNames.Status] = HeaderFieldName.Status,
			[HeaderFieldNames.RemoteMailTransferAgent] = HeaderFieldName.RemoteMailTransferAgent,
			[HeaderFieldNames.DiagnosticCode] = HeaderFieldName.DiagnosticCode,
			[HeaderFieldNames.LastAttemptDate] = HeaderFieldName.LastAttemptDate,
			[HeaderFieldNames.FinalLogId] = HeaderFieldName.FinalLogId,
			[HeaderFieldNames.WillRetryUntil] = HeaderFieldName.WillRetryUntil,
			[HeaderFieldNames.ReportingUA] = HeaderFieldName.ReportingUA,
			[HeaderFieldNames.MdnGateway] = HeaderFieldName.MdnGateway,
			[HeaderFieldNames.Disposition] = HeaderFieldName.Disposition,
			[HeaderFieldNames.Failure] = HeaderFieldName.Failure,
			[HeaderFieldNames.Error] = HeaderFieldName.Error,
			[HeaderFieldNames.Warning] = HeaderFieldName.Warning
		};

		/// <summary>
		/// Gets string name of HeaderType enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of HeaderType enumeration value.</returns>
		internal static string GetName (this HeaderFieldName value)
		{
			string name;
			var isFound = _headerTypes.TryGetValue (value, out name);
			if (!isFound)
			{
				throw new NotSupportedException ("Unsupported type of header '" + value + "'.");
			}
			return name;
		}

		/// <summary>
		/// Parses string representation of HeaderType enumeration value.
		/// </summary>
		/// <param name="source">String representation of HeaderType enumeration value.</param>
		/// <param name="result">When this method returns, contains the HeaderType value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out HeaderFieldName result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			var isFound = _headerTypeNames.TryGetValue (source, out result);
			if (!isFound)
			{
				result = HeaderFieldName.Unspecified;
			}
			return isFound;
		}
	}

	/// <summary>
	/// MIME header field names.
	/// </summary>
	internal static class HeaderFieldNames
	{
		#region RFC 4021 part 2.1. Permanent Mail Header Field Registrations

		/// <summary>Message date and time. "Date" header field in RFC 2822.</summary>
		internal static readonly string Date = "Date";

		/// <summary>Mailbox of message author. "From" header field in RFC 2822.</summary>
		internal static readonly string From = "From";

		/// <summary> Mailbox of message sender. "Sender" header field in RFC 2822.</summary>
		internal static readonly string Sender = "Sender";

		/// <summary>Mailbox for replies to message. "Reply-To" header field in RFC 2822.</summary>
		internal static readonly string ReplyTo = "Reply-To";

		/// <summary>Primary recipient mailbox. "To" header field in RFC 2822.</summary>
		internal static readonly string To = "To";

		/// <summary>Carbon-copy recipient mailbox. "CC" header field in RFC 2822.</summary>
		internal static readonly string CC = "CC";

		/// <summary>Blind-carbon-copy recipient mailbox. "Bcc" header field in RFC 2822.</summary>
		internal static readonly string Bcc = "Bcc";

		/// <summary>Message identifier. "Message-ID" header field in RFC 2822.</summary>
		internal static readonly string MessageId = "Message-ID";

		/// <summary>Identify replied-to message(s). "In-Reply-To" header field in RFC 2822.</summary>
		internal static readonly string InReplyTo = "In-Reply-To";

		/// <summary>Related message identifier(s). "References" header field in RFC 2822.</summary>
		internal static readonly string References = "References";

		/// <summary>Topic of message. "Subject" header field in RFC 2822.</summary>
		internal static readonly string Subject = "Subject";

		/// <summary>Additional comments about the message. "Comments" header field in RFC 2822.</summary>
		internal static readonly string Comments = "Comments";

		/// <summary>Message key words and/or phrases. "Keywords" header field in RFC 2822.</summary>
		internal static readonly string Keywords = "Keywords";

		/// <summary>Date and time message is resent. "Resent-Date" header field in RFC 2822.</summary>
		internal static readonly string ResentDate = "Resent-Date";

		/// <summary>Mailbox of person for whom message is resent. "Resent-From" header field in RFC 2822.</summary>
		internal static readonly string ResentFrom = "Resent-From";

		/// <summary>Mailbox of person who actually resends the message. "Resent-Sender" header field in RFC 2822.</summary>
		internal static readonly string ResentSender = "Resent-Sender";

		/// <summary>Mailbox to which message is resent. "Resent-To" header field in RFC 2822.</summary>
		internal static readonly string ResentTo = "Resent-To";

		/// <summary>Mailbox(es) to which message is cc'ed on resend. "Resent-CC" header field in RFC 2822.</summary>
		internal static readonly string ResentCC = "Resent-CC";

		/// <summary>Mailbox(es) to which message is bcc'ed on resend. "Resent-Bcc" header field in RFC 2822.</summary>
		internal static readonly string ResentBcc = "Resent-Bcc";

		/// <summary>Message identifier for resent message. "Resent-Message-ID" header field in RFC 2822.</summary>
		internal static readonly string ResentMessageId = "Resent-Message-ID";

		/// <summary>Message return path. "Return-Path" header field in RFC 2822.</summary>
		internal static readonly string ReturnPath = "Return-Path";

		/// <summary>Mail transfer trace information. "Received" header field in RFC 2822.</summary>
		internal static readonly string Received = "Received";

		/// <summary>Mailbox for sending disposition notification. "Disposition-Notification-To" header field in RFC 3798.</summary>
		internal static readonly string DispositionNotificationTo = "Disposition-Notification-To";

		/// <summary>Disposition notification options. "Disposition-Notification-Options" header field in RFC 3798.</summary>
		internal static readonly string DispositionNotificationOptions = "Disposition-Notification-Options";

		/// <summary>Language(s) for auto-responses. "Accept-Language" header field in RFC 3282.</summary>
		internal static readonly string AcceptLanguage = "Accept-Language";

		/// <summary>Original message identifier. "Original-Message-ID" header field in RFC 3297.</summary>
		internal static readonly string OriginalMessageId = "Original-Message-ID";

		/// <summary>PICS rating label. "PICS-Label" header field in http://www.w3.org/TR/REC-PICS-labels/..</summary>
		internal static readonly string PicsLabel = "PICS-Label";

		/// <summary>Message encoding and other information. "Encoding" header field in RFC 1505.</summary>
		internal static readonly string Encoding = "Encoding";

		/// <summary>Type or context of message. "Message-Context" header field in RFC 3458.</summary>
		internal static readonly string MessageContext = "Message-Context";

		/// <summary>Trace of distribution lists passed. "DL-Expansion-History" header field in RFC 2156.</summary>
		internal static readonly string DLExpansionHistory = "DL-Expansion-History";

		/// <summary>Controls forwarding to alternate recipients. "Alternate-Recipient" header field in RFC 2156.</summary>
		internal static readonly string AlternateRecipient = "Alternate-Recipient";

		/// <summary>Body part types in message. "Original-Encoded-Information-Types" header field in RFC 2156.</summary>
		internal static readonly string OriginalEncodedInformationTypes = "Original-Encoded-Information-Types";

		/// <summary>Request delivery report generation. "Generate-Delivery-Report" header field in RFC 2156.</summary>
		internal static readonly string GenerateDeliveryReport = "Generate-Delivery-Report";

		/// <summary>Non-delivery report required? "Prevent-NonDelivery-Report" header field in RFC 2156.</summary>
		internal static readonly string PreventNonDeliveryReport = "Prevent-NonDelivery-Report";

		/// <summary>Reference message to be replaced. "Supersedes" header field in RFC 2156.</summary>
		internal static readonly string Supersedes = "Supersedes";

		/// <summary>Message delivery time. "Delivery-Date" header field in RFC 2156.</summary>
		internal static readonly string ExpiryDate = "Expiry-Date";

		/// <summary>Message expiry time. "Expires" header field in RFC 2156.</summary>
		internal static readonly string Expires = "Expires";

		/// <summary>Time by which a reply is requested. "Reply-By" header field in RFC 2156.</summary>
		internal static readonly string ReplyBy = "Reply-By";

		/// <summary>Message importance. "Importance" header field in RFC 2156.</summary>
		internal static readonly string Importance = "Importance";

		/// <summary>Body parts are missing. "Incomplete-Copy" header field in RFC 2156.</summary>
		internal static readonly string IncompleteCopy = "Incomplete-Copy";

		/// <summary>Message priority. "Priority" header field in RFC 2156.</summary>
		internal static readonly string Priority = "Priority";

		/// <summary>Message content sensitivity. "Sensitivity" header field in RFC 2156.</summary>
		internal static readonly string Sensitivity = "Sensitivity";

		/// <summary>X.400 message content language. "Language" header field in RFC 2156.</summary>
		internal static readonly string Language = "Language";

		/// <summary>Conversion allowed? "Conversion" header field in RFC 2156.</summary>
		internal static readonly string Conversion = "Conversion";

		/// <summary>Lossy conversion allowed? "Conversion-With-Loss" header field in RFC 2156.</summary>
		internal static readonly string ConversionWithLoss = "Conversion-With-Loss";

		/// <summary>Message type: delivery report? "Message-Type" header field in RFC 2156.</summary>
		internal static readonly string MessageType = "Message-Type";

		/// <summary>Automatically submitted indicator. "Autosubmitted" header field in RFC 2156.</summary>
		internal static readonly string Autosubmitted = "Autosubmitted";

		/// <summary>Automatically forwarded indicator. "Autoforwarded" header field in RFC 2156.</summary>
		internal static readonly string Autoforwarded = "Autoforwarded";

		/// <summary>Disclose names of other recipients? "Disclose-Recipients" header field in RFC 2156.</summary>
		internal static readonly string DiscloseRecipients = "Disclose-Recipients";

		/// <summary>Deferred delivery information. "Deferred-Delivery" header field in RFC 2156.</summary>
		internal static readonly string DeferredDelivery = "Deferred-Delivery";

		/// <summary>Latest delivery time requested. "Latest-Delivery-Time" header field in RFC 2156.</summary>
		internal static readonly string LatestDeliveryTime = "Latest-Delivery-Time";

		/// <summary>Originator return address. "Originator-Return-Address" header field in RFC 2156.</summary>
		internal static readonly string OriginatorReturnAddress = "Originator-Return-Address";

		/// <summary>URL of mailing list archive. "List-Archive" header field in RFC 2369.</summary>
		internal static readonly string ListArchive = "List-Archive";

		/// <summary>URL for mailing list information. "List-Help" header field in RFC 2369.</summary>
		internal static readonly string ListHelp = "List-Help";

		/// <summary>URL for mailing list owner's mailbox. "List-Owner" header field in RFC 2369.</summary>
		internal static readonly string ListOwner = "List-Owner";

		/// <summary> URL for mailing list posting. "List-Post" header field in RFC 2369.</summary>
		internal static readonly string ListPost = "List-Post";

		/// <summary>URL for mailing list subscription. "List-Subscribe" header field in RFC 2369.</summary>
		internal static readonly string ListSubscribe = "List-Subscribe";

		/// <summary>URL for mailing list unsubscription. "List-Unsubscribe" header field in RFC 2369.</summary>
		internal static readonly string ListUnsubscribe = "List-Unsubscribe";

		/// <summary>Mailing list identifier. "List-ID" header field in RFC 2919.</summary>
		internal static readonly string ListId = "List-ID";

		/// <summary>X.400 IPM extensions discarded. "Discarded-X400-IPMS-Extensions" header field in RFC 2156.</summary>
		internal static readonly string DiscardedX400IpmsExtensions = "Discarded-X400-IPMS-Extensions";

		/// <summary>X.400 MTS extensions discarded. "Discarded-X400-MTS-Extensions" header field in RFC 2156.</summary>
		internal static readonly string DiscardedX400MtsExtensions = "Discarded-X400-MTS-Extensions";

		/// <summary>Message content identifier. "X400-Content-Identifier" header field in RFC 2156.</summary>
		internal static readonly string X400ContentIdentifier = "X400-Content-Identifier";

		/// <summary>Return content on non-delivery? "X400-Content-Return" header field in RFC 2156.</summary>
		internal static readonly string X400ContentReturn = "X400-Content-Return";

		/// <summary>X400 content type. "X400-Content-Type" header field in RFC 2156.</summary>
		internal static readonly string X400ContentType = "X400-Content-Type";

		/// <summary>X400 MTS-Identifier. "X400-MTS-Identifier" header field in RFC 2156.</summary>
		internal static readonly string X400MtsIdentifier = "X400-MTS-Identifier";

		/// <summary>X400 Originator. "X400-Originator" header field in RFC 2156.</summary>
		internal static readonly string X400Originator = "X400-Originator";

		/// <summary>X400 Received. "X400-Received" header field in RFC 2156.</summary>
		internal static readonly string X400Received = "X400-Received";

		/// <summary>X400 Recipients. "X400-Recipients" header field in RFC 2156.</summary>
		internal static readonly string X400Recipients = "X400-Recipients";

		/// <summary>X400 Trace. "X400-Trace" header field in RFC 2156.</summary>
		internal static readonly string X400Trace = "X400-Trace";

		#endregion

		#region RFC 4021 part 2.2. Permanent MIME Header Field Registrations

		/// <summary>"MIME-Version". MIME version number. Определено в RFC 2045 part 4.</summary>
		internal static readonly string MimeVersion = "MIME-Version";

		/// <summary>"Content-Type". MIME content type. Определено в RFC 2045 part 5.</summary>
		internal static readonly string ContentType = "Content-Type";

		/// <summary>"Content-Transfer-Encoding". Content transfer encoding applied. Определено в RFC 2045 part 6.</summary>
		internal static readonly string ContentTransferEncoding = "Content-Transfer-Encoding";

		/// <summary>"Content-ID". Identify content body part. Определено в RFC 2045 part 7.</summary>
		internal static readonly string ContentId = "Content-ID";

		/// <summary>"Content-Description". Description of message body part. Определено в RFC 2045 part 8.</summary>
		internal static readonly string ContentDescription = "Content-Description";

		/// <summary>"Content-Disposition". Intended content disposition and file name. Определено в RFC 2183.</summary>
		internal static readonly string ContentDisposition = "Content-Disposition";

		/// <summary>"Content-Base". Base to be used for resolving relative URIs within this content part. Определено в RFC 2110 part 4.2.</summary>
		internal static readonly string ContentBase = "Content-Base";

		/// <summary>"Content-Location". URI for retrieving a body part. Определено в RFC 2557 part 4.2.</summary>
		internal static readonly string ContentLocation = "Content-Location";

		/// <summary>"Content-features". Indicates content features of a MIME body part. Определено в RFC 2912 part 3.</summary>
		internal static readonly string ContentFeatures = "Content-features";

		/// <summary>"Content-Language". Language of message content. Определено в RFC 3282.</summary>
		internal static readonly string ContentLanguage = "Content-Language";

		/// <summary>"Content-alternative". Alternative content available. Определено в RFC 3297 part 4.</summary>
		internal static readonly string ContentAlternative = "Content-alternative";

		/// <summary>"Content-MD5". MD5 checksum of content. Определено в RFC 1864.</summary>
		internal static readonly string ContentMD5 = "Content-MD5";

		/// <summary>"Content-Duration". Time duration of content. Определено в RFC 2424.</summary>
		internal static readonly string ContentDuration = "Content-Duration";

		#endregion

		#region RFC 3464 part 2.2. Delivery Status Notifications Per-Message DSN Fields

		/// <summary>"Original-Envelope-Id". Identifier that uniquely identifies the transaction during which the message was submitted.</summary>
		internal static readonly string OriginalEnvelopeId = "Original-Envelope-Id";

		/// <summary>"Reporting-MTA". MTA that attempted to perform the delivery, relay, or gateway operation described in the DSN.</summary>
		internal static readonly string MailTransferAgent = "Reporting-MTA";

		/// <summary>"DSN-Gateway". Name of the gateway or MTA that translated a foreign (non-Internet) delivery status notification into this DSN.</summary>
		internal static readonly string DsnGateway = "DSN-Gateway";

		/// <summary>"Received-From-MTA". Name of the MTA from which the message was received.</summary>
		internal static readonly string ReceivedFromMailTransferAgent = "Received-From-MTA";

		/// <summary>"Arrival-Date". Date and time at which the message arrived at the Reporting MTA.</summary>
		internal static readonly string ArrivalDate = "Arrival-Date";

		#endregion

		#region RFC 3464 part 2.3. Delivery Status Notifications Per-Recipient DSN fields

		/// <summary>"Original-Recipient". Original recipient address as specified by the sender of the message.</summary>
		internal static readonly string OriginalRecipient = "Original-Recipient";

		/// <summary>"Final-Recipient". Recipient for which this set of fields applies.</summary>
		internal static readonly string FinalRecipient = "Final-Recipient";

		/// <summary>"Action". Action performed by the Reporting-MTA as a result of its attempt to deliver the message to this recipient address.</summary>
		internal static readonly string Action = "Action";

		/// <summary>"Status". Transport-independent status code that indicates the delivery status of the message to that recipient.</summary>
		internal static readonly string Status = "Status";

		/// <summary>"Remote-MTA". Printable ASCII representation of the name of the "remote" MTA that reported delivery status to the "reporting" MTA.</summary>
		internal static readonly string RemoteMailTransferAgent = "Remote-MTA";

		/// <summary>"Diagnostic-Code". Actual diagnostic code issued by the mail transport.</summary>
		internal static readonly string DiagnosticCode = "Diagnostic-Code";

		/// <summary>"Last-Attempt-Date". Date and time of the last attempt to relay, gateway, or deliver the message (whether successful or unsuccessful) by the Reporting MTA.</summary>
		internal static readonly string LastAttemptDate = "Last-Attempt-Date";

		/// <summary>"Final-Log-ID". Final-log-id of the message that was used by the final-mta.</summary>
		internal static readonly string FinalLogId = "Final-Log-ID";

		/// <summary>"Will-Retry-Until". Date after which the Reporting MTA expects to abandon all attempts to deliver the message to that recipient.</summary>
		internal static readonly string WillRetryUntil = "Will-Retry-Until";

		#endregion

		#region RFC 3798 part 3.2. Message/disposition-notification Fields

		/// <summary>"Reporting-UA". MUA that performed the disposition described in the MDN.</summary>
		internal static readonly string ReportingUA = "Reporting-UA";

		/// <summary>"MDN-Gateway". Name of the gateway or MTA that translated a foreign (non-Internet) message disposition notification into this MDN.</summary>
		internal static readonly string MdnGateway = "MDNGateway";

		/// <summary>"Disposition". Action performed by the Reporting-MUA on behalf of the user.</summary>
		internal static readonly string Disposition = "Disposition";

		/// <summary>"Failure". Additional information when the "failure" disposition modifier appear.</summary>
		internal static readonly string Failure = "Failure";

		/// <summary>"Error". Additional information when the "error" disposition modifier appear.</summary>
		internal static readonly string Error = "Error";

		/// <summary>"Warning". Additional information when the "warning" disposition modifier appear.</summary>
		internal static readonly string Warning = "Warning";

		#endregion
	}
}
