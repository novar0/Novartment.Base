using System;
using System.Text;
using System.Threading;
using static System.Linq.Enumerable;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime.Test
{
	public class DispositionNotificationEntityBodyTests
	{
		private static readonly string Template1 =
"Reporting-UA: joes-pc.cs.example.com; Foomail 97.1\r\n" +
"Original-Recipient: rfc822;Joe_Recipient@example.com\r\n" +
"Final-Recipient: rfc822;Joe_Recipient@example.com\r\n" +
"Original-Message-ID: <199509192301.23456@example.org>\r\n" +
"Disposition: manual-action/MDN-sent-manually; displayed\r\n\r\n";

		[Fact, Trait ("Category", "Mime")]
		public void Load ()
		{
			var body = new DispositionNotificationEntityBody ();
			body.LoadAsync (new ArrayBufferedSource (Encoding.ASCII.GetBytes (Template1)), null, CancellationToken.None).Wait ();

			Assert.Equal ("joes-pc.cs.example.com", body.ReportingUserAgentName);
			Assert.Equal ("Foomail 97.1", body.ReportingUserAgentProduct);
			Assert.Equal (NotificationFieldValueKind.Mailbox, body.OriginalRecipient.Kind);
			Assert.Equal ("Joe_Recipient@example.com", body.OriginalRecipient.Value);
			Assert.Equal (NotificationFieldValueKind.Mailbox, body.FinalRecipient.Kind);
			Assert.Equal ("Joe_Recipient@example.com", body.FinalRecipient.Value);
			Assert.Equal ("199509192301.23456", body.OriginalMessageId.LocalPart);
			Assert.Equal ("example.org", body.OriginalMessageId.Domain);
			Assert.Equal (MessageDispositionChangedAction.ManuallyDisplayed, body.Disposition);
			Assert.Equal (0, body.DispositionModifiers.Count);
			Assert.Null (body.Gateway);
			Assert.Equal (0, body.FailureInfo.Count);
			Assert.Equal (0, body.ErrorInfo.Count);
			Assert.Equal (0, body.WarningInfo.Count);
		}

		[Fact, Trait ("Category", "Mime")]
		public void Save ()
		{
			var body = new DispositionNotificationEntityBody ();
			body.ReportingUserAgentName = "joes-pc.cs.example.com";
			body.ReportingUserAgentProduct = "Foomail 97.1";
			body.OriginalRecipient = new NotificationFieldValue (NotificationFieldValueKind.Mailbox, "Joe_Recipient@example.com");
			body.FinalRecipient = new NotificationFieldValue (NotificationFieldValueKind.Mailbox, "Joe_Recipient@example.com");
			body.OriginalMessageId = new AddrSpec ("199509192301.23456", "example.org");
			body.Disposition = MessageDispositionChangedAction.ManuallyDisplayed;

			var bytes = new BinaryDestinationMock (8192);
			body.SaveAsync (bytes, CancellationToken.None).Wait ();
			var text = Encoding.UTF8.GetString (bytes.Buffer, 0, bytes.Count);
			var lines = text.Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (7, lines.Length);
			Assert.Equal ("", lines[5]);
			Assert.Equal ("", lines[6]);

			var lines2 = lines.Take (5).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();

			int idx = 0;
			Assert.Equal ("Disposition: manual-action/MDN-sent-automatically; displayed", lines2[idx++], true);
			Assert.Equal ("Final-Recipient: rfc822; Joe_Recipient@example.com", lines2[idx++], true);
			Assert.Equal ("Original-Message-ID: <199509192301.23456@example.org>", lines2[idx++], true);
			Assert.Equal ("Original-Recipient: rfc822; Joe_Recipient@example.com", lines2[idx++], true);
			Assert.Equal ("Reporting-UA: joes-pc.cs.example.com; Foomail 97.1", lines2[idx++], true);
		}
	}
}
