using System;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime.Test
{
	public class DeliveryStatusEntityBodyTests
	{
		private static readonly string Template1 =
			"Reporting-MTA: dns;itc-serv01.chmk.mechelgroup.ru\r\n" +
			"Received-From-MTA: dns;ChelMKMail.mailinator.com\r\n" +
			"Arrival-Date: Sun, 13 May 2012 12:48:25 +0600\r\n" +
			"\r\n" +
			"Final-Recipient: rfc822;quality@itc-serv01.chmk.mechelgroup.ru\r\n" +
			"Action: failed\r\n" +
			"Status: 5.0.0\r\n";

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load ()
		{
			var body = new DeliveryStatusEntityBody ();
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (Template1));
			body.LoadAsync (src, null).Wait ();

			Assert.Equal (NotificationFieldValueKind.Host, body.MailTransferAgent.Kind);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", body.MailTransferAgent.Value);
			Assert.Equal (NotificationFieldValueKind.Host, body.ReceivedFromMailTransferAgent.Kind);
			Assert.Equal ("ChelMKMail.mailinator.com", body.ReceivedFromMailTransferAgent.Value);
			Assert.Equal (new DateTimeOffset (new DateTime (634725101050000000L), TimeSpan.FromHours (6)), body.ArrivalDate);

			Assert.Equal (1, body.Recipients.Count);
			Assert.Equal (NotificationFieldValueKind.Mailbox, body.Recipients[0].FinalRecipient.Kind);
			Assert.Equal ("quality@itc-serv01.chmk.mechelgroup.ru", body.Recipients[0].FinalRecipient.Value);
			Assert.Equal (DeliveryAttemptResult.Failed, body.Recipients[0].Action);
			Assert.Equal ("5.0.0", body.Recipients[0].Status);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Save ()
		{
			var body = new DeliveryStatusEntityBody ()
			{
				MailTransferAgent = new NotificationFieldValue (NotificationFieldValueKind.Host, "itc-serv01.chmk.mechelgroup.ru"),
				ReceivedFromMailTransferAgent = new NotificationFieldValue (NotificationFieldValueKind.Host, "ChelMKMail.mailinator.com"),
				ArrivalDate = new DateTimeOffset (634725101050000000L, new TimeSpan (-3, 0, 0)),
			};
			var recipient = new RecipientDeliveryStatus (
				new NotificationFieldValue (NotificationFieldValueKind.Mailbox, "quality@itc-serv01.chmk.mechelgroup.ru"),
				DeliveryAttemptResult.Failed,
				"5.0.0");
			body.Recipients.Add (recipient);

			var bytes = new BinaryDestinationMock (8192);
			body.SaveAsync (bytes).Wait ();
			var text = Encoding.UTF8.GetString (bytes.Buffer.Slice (0, bytes.Count));
			var lines = text.Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (9, lines.Length);
			Assert.Equal (string.Empty, lines[3]);
			Assert.Equal (string.Empty, lines[7]);
			Assert.Equal (string.Empty, lines[8]);

			var lines2 = lines.Take (3).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();
			int idx = 0;
			Assert.Equal ("Arrival-Date: 13 May 2012 12:48:25 -0300", lines2[idx++]);
			Assert.Equal ("Received-From-MTA: dns; ChelMKMail.mailinator.com", lines2[idx++]);
			Assert.Equal ("Reporting-MTA: dns; itc-serv01.chmk.mechelgroup.ru", lines2[idx++]);

			var lines3 = lines.Skip (4).Take (3).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();
			idx = 0;
			Assert.Equal ("Action: failed", lines3[idx++]);
			Assert.Equal ("Final-Recipient: rfc822; quality@itc-serv01.chmk.mechelgroup.ru", lines3[idx++]);
			Assert.Equal ("Status: 5.0.0", lines3[idx++]);
		}
	}
}
