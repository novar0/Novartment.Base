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
			[HeaderFieldName.Warning] = HeaderFieldNames.Warning,
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
			[HeaderFieldNames.Warning] = HeaderFieldName.Warning,
		};

		/// <summary>
		/// Gets string name of HeaderType enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of HeaderType enumeration value.</returns>
		internal static string GetName (this HeaderFieldName value)
		{
			var isFound = _headerTypes.TryGetValue (value, out string name);
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
}
