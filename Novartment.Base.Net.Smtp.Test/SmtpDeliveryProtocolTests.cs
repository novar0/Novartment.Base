using System;
using System.Net;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public class SmtpDeliveryProtocolTests
	{
		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void CommandPipelining ()
		{
			var allowedRecipient1 = new AddrSpec ("ned", "innosoft.com");
			var allowedRecipient2 = new AddrSpec ("dan", "wikipedia.net");
			var allowedRecipient3 = new AddrSpec ("postmater", "github.com");
			var disallowedRecipient = new AddrSpec ("nsb", "thumper.bellcore.com");
			var inData =
				"EHLO dbc.mtview.ca.us\r\n" +

				// точка синхронизации, тут сервер должен послать все накопившиеся ответы
				"MAIL FROM:<mrose@dbc.mtview.ca.us>\r\n" +
				"RCPT TO:<" + allowedRecipient1 + ">\r\n" +
				"RCPT TO:<" + allowedRecipient2 + ">\r\n" +
				"RCPT TO:<" + allowedRecipient3 + ">\r\n" +
				"DATA\r\n" +

				// точка синхронизации, тут сервер должен послать все накопившиеся ответы
				"22 sample\r\n\r\ndata 22\r\n.\r\n" +
				"MAIL FROM:<mrose@dbc.mtview.ca.us>\r\n" +
				"RCPT TO:<" + disallowedRecipient + ">\r\n" +
				"RCPT TO:<" + disallowedRecipient + ">\r\n" +
				"BDAT 5\r\n12345" + // две BDAT команды не принимаются сервером (нет получателей) но должны быть корректно пропущены

				// точка синхронизации, тут сервер должен послать все накопившиеся ответы
				"BDAT 3 LAST\r\n678" +

				// точка синхронизации, тут сервер должен послать все накопившиеся ответы
				"RSET\r\n" +
				"MAIL FROM:<mrose@dbc.mtview.ca.us>\r\n" +
				"RCPT TO:<" + allowedRecipient1 + ">\r\n" +
				"DATA\r\n" +

				// точка синхронизации, тут сервер должен послать все накопившиеся ответы
				"QUIT\r\n.\r\n" + // тут слово QUIT является данными, а не командой
				"QUIT\r\n";

			var transactionFactory = new Func<MailDeliverySourceData, IMailTransferTransactionHandler> (srcAttribs =>
				new SmtDataTransferTransactionMock (null, disallowedRecipient, TransactionBehavior.Normal));
			var connection = new TcpConnectionMock (
				new IPEndPoint (IPAddress.Loopback, 25),
				new IPEndPoint (new IPAddress (1144955L), 32701),
				new MemoryBufferedSource (Encoding.ASCII.GetBytes (inData)));
			var protocol = new SmtpDeliveryProtocol (transactionFactory, SmtpServerSecurityParameters.NoSecurity, null);

			protocol.StartAsync (connection).Wait ();

			// анализируем ответы протокола
			var replies = connection.OutData.Queue.ToArray ();
			Assert.Equal (7, replies.Length);

			var group = replies[0].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (2, group.Length);
			Assert.StartsWith ("220 ", group[0], StringComparison.OrdinalIgnoreCase); // EHLO
			Assert.Equal (0, group[1].Length);

			// убеждаемся что EHLO-ответ содержит ключевое слово поддержки расширения PIPELINING
			group = replies[1].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			int i;
			bool keywordFound = false;
			for (i = 0; i < (group.Length - 2); i++)
			{
				Assert.StartsWith ("250-", group[i], StringComparison.OrdinalIgnoreCase);
				if (group[i].Substring (4) == "PIPELINING")
				{
					keywordFound = true;
				}
			}

			Assert.StartsWith ("250 ", group[i], StringComparison.OrdinalIgnoreCase);
			if (group[i].Substring (4) == "PIPELINING")
			{
				keywordFound = true;
			}

			Assert.True (keywordFound);
			Assert.Equal (0, group[i + 1].Length);

			group = replies[2].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (6, group.Length);
			Assert.StartsWith ("250 ", group[0], StringComparison.OrdinalIgnoreCase); // MAIL FROM:
			Assert.StartsWith ("250 ", group[1], StringComparison.OrdinalIgnoreCase); // RCPT TO:
			Assert.StartsWith ("250 ", group[2], StringComparison.OrdinalIgnoreCase); // RCPT TO:
			Assert.StartsWith ("250 ", group[3], StringComparison.OrdinalIgnoreCase); // RCPT TO:
			Assert.StartsWith ("354 ", group[4], StringComparison.OrdinalIgnoreCase); // DATA
			Assert.Equal (0, group[5].Length);

			group = replies[3].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (6, group.Length);
			Assert.StartsWith ("250 ", group[0], StringComparison.OrdinalIgnoreCase); // actual data
			Assert.StartsWith ("250 ", group[1], StringComparison.OrdinalIgnoreCase); // MAIL FROM:
			Assert.StartsWith ("550 ", group[2], StringComparison.OrdinalIgnoreCase); // RCPT TO:
			Assert.StartsWith ("550 ", group[3], StringComparison.OrdinalIgnoreCase); // RCPT TO:
			Assert.StartsWith ("554 ", group[4], StringComparison.OrdinalIgnoreCase); // BDAT ...
			Assert.Equal (0, group[5].Length);

			group = replies[4].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (2, group.Length);
			Assert.StartsWith ("503 ", group[0], StringComparison.OrdinalIgnoreCase); // BDAT ... LAST
			Assert.Equal (0, group[1].Length);

			group = replies[5].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (5, group.Length);
			Assert.StartsWith ("250 ", group[0], StringComparison.OrdinalIgnoreCase); // RSET
			Assert.StartsWith ("250 ", group[1], StringComparison.OrdinalIgnoreCase); // MAIL FROM:
			Assert.StartsWith ("250 ", group[2], StringComparison.OrdinalIgnoreCase); // RCPT TO:
			Assert.StartsWith ("354 ", group[3], StringComparison.OrdinalIgnoreCase); // DATA
			Assert.Equal (0, group[4].Length);

			group = replies[6].Split (new string[] { "\r\n" }, StringSplitOptions.None);
			Assert.Equal (3, group.Length);
			Assert.StartsWith ("250 ", group[0], StringComparison.OrdinalIgnoreCase); // actual data
			Assert.StartsWith ("221 ", group[1], StringComparison.OrdinalIgnoreCase); // QUIT
			Assert.Equal (0, group[2].Length);
		}
	}
}
