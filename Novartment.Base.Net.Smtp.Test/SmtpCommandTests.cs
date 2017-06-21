using System;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public class SmtpCommandTests
	{
		private readonly string _quitCommand = "QUIT\r\n";

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void Parse ()
		{
			// UnknownCommand
			var buf = new byte[1000];
			var source = new ArrayBufferedSource (buf);
			var cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);
			Assert.Equal (buf.Length, source.Offset);

			var cmdStr = "HELO\tlocalhost\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			var cmdBytes = new byte[] { 0xd0, 0x81, 0x20, 0x0d, 0x0a, 0x0d, 0x0a };
			source = new ArrayBufferedSource (cmdBytes);
			Assert.Throws<FormatException> (() => SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null));

			cmdStr = " QUIT\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "DATA2\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "MAIL FROM <someone@server.com>\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Data
			cmdStr = "DATA\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "Data\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// waiting Data
			cmdStr = "Hello!\r\nPlease send me some info.";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + "\r\n.\r\n" + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Data, null);
			Assert.Equal (SmtpCommandType.ActualData, cmd.CommandType);
			Assert.IsType<SmtpActualDataCommand> (cmd);
			var cmdActualData = (SmtpActualDataCommand)cmd;
			Assert.Equal (0, source.Offset);
			cmdActualData.Source.SkipBuffer (cmdActualData.Source.Count);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Noop
			cmdStr = "NOOP\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Noop, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "NOOP --synchronize transaction NOW--\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Noop, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Quit
			cmdStr = "QUIT\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Quit, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Rset
			cmdStr = "rset\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Rset, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "RSET\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Rset, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Vrfy
			cmdStr = "VRFY\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Vrfy, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "VRFY John Doe\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Vrfy, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Helo
			var id = "insufficient\tsystem\tstorage";
			cmdStr = "HELO " + id + "\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Helo, cmd.CommandType);
			Assert.IsType<SmtpHeloCommand> (cmd);
			var cmdHelo = (SmtpHeloCommand)cmd;
			Assert.Equal (id, cmdHelo.ClientIdentification);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Ehlo
			cmdStr = "EHLO " + id + "\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Ehlo, cmd.CommandType);
			Assert.IsType<SmtpEhloCommand> (cmd);
			var cmdEhlo = (SmtpEhloCommand)cmd;
			Assert.Equal (id, cmdEhlo.ClientIdentification);
			Assert.Equal (cmdStr.Length, source.Offset);

			// MailFrom
			cmdStr = "MAIL from:someone@server.com\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "MAIL FROM:Peter_Parker <someone@server.com>\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "mail from:<>\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpMailFromCommand> (cmd);
			var cmdMailFrom = (SmtpMailFromCommand)cmd;
			Assert.Null (cmdMailFrom.ReturnPath);
			Assert.Equal (cmdStr.Length, source.Offset);
			Assert.Equal (ContentTransferEncoding.SevenBit, cmdMailFrom.RequestedContentTransferEncoding);

			cmdStr = "mail from:<> AUTH=me@server.com\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "mail from:<> BODY:8BITMIME\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "mail from:<> BODY=8BIT-MIME\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "mail from:<someone@server.com> RET=HDRS BODY=BINARYMIME\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "mail from:<someone@server.com> BODY=BINARYMIME BODY=8BITMIME\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "MAIL FROM:<someone@server.com> " + id + "\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "mail from:<> AUTH=<> BODY=8BITMIME\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpMailFromCommand> (cmd);
			cmdMailFrom = (SmtpMailFromCommand)cmd;
			Assert.Null (cmdMailFrom.ReturnPath);
			Assert.Equal (SmtpMailFromCommand.EmptyAddrSpec, cmdMailFrom.AssociatedMailbox);
			Assert.Equal (cmdStr.Length, source.Offset);
			Assert.Equal (ContentTransferEncoding.EightBit, cmdMailFrom.RequestedContentTransferEncoding);

			cmdStr = "mail from:<someone@server.com>    BODY=BINARYMIME  AUTH=<someone@server.com> \r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpMailFromCommand> (cmd);
			cmdMailFrom = (SmtpMailFromCommand)cmd;
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdMailFrom.ReturnPath);
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdMailFrom.AssociatedMailbox);
			Assert.Equal (cmdStr.Length, source.Offset);
			Assert.Equal (ContentTransferEncoding.Binary, cmdMailFrom.RequestedContentTransferEncoding);

			// RcptTo
			cmdStr = "RCPT TO:<support>\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "RCPT TO:support@www.ru\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "RCPT TO:<>\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "RCPT TO:<someone@server.com>\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.IsType<SmtpRcptToCommand> (cmd);
			var cmdRcptTo = (SmtpRcptToCommand)cmd;
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdRcptTo.Recipient);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "rcpt TO:<someone@server.com> " + id + "\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.IsType<SmtpRcptToCommand> (cmd);
			cmdRcptTo = (SmtpRcptToCommand)cmd;
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdRcptTo.Recipient);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Bdat
			var sampleData = "abcdefg";
			cmdStr = "BDAT\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + sampleData));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "BDAT -10\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + sampleData));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "bdat 10LAST\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + sampleData));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "BDAT 4647785733979898881\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + sampleData));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpBdatCommand> (cmd);
			var cmdBdat = (SmtpBdatCommand)cmd;
			Assert.Equal (4647785733979898881L, cmdBdat.Source.UnusedSize);
			Assert.Equal (sampleData.Length, cmdBdat.Source.Count);
			Assert.False (cmdBdat.IsLast);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "bDAT 2 LAST\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + sampleData));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpBdatCommand> (cmd);
			cmdBdat = (SmtpBdatCommand)cmd;
			Assert.Equal (2L, cmdBdat.Source.UnusedSize);
			Assert.Equal (2, cmdBdat.Source.Count);
			Assert.True (cmdBdat.IsLast);
			Assert.Equal (cmdStr.Length, source.Offset);
			cmdBdat.Source.SkipBuffer (cmdBdat.Source.Count);
			Assert.Equal (cmdStr.Length + 2, source.Offset);

			// StartTls
			cmdStr = "STARTTLS\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.StartTls, cmd.CommandType);
			Assert.Equal (cmdStr.Length, source.Offset);

			// Auth
			cmdStr = "AUTH\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Auth, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "AUTH NEW METHOD\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			Assert.Equal (SmtpCommandType.Auth, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "AUTH SOME\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			var cmdAuth = (SmtpAuthCommand)cmd;
			Assert.Null (cmdAuth.InitialResponse);
			Assert.Equal ("SOME", cmdAuth.Mechanism);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "AUTH NEW =\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			cmdAuth = (SmtpAuthCommand)cmd;
			Assert.Equal (0, cmdAuth.InitialResponse.Length);
			Assert.Equal ("NEW", cmdAuth.Mechanism);
			Assert.Equal (cmdStr.Length, source.Offset);

			cmdStr = "AUTH NEW AGFhYQBiYmI=\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.Command, null);
			cmdAuth = (SmtpAuthCommand)cmd;
			Assert.Equal (8, cmdAuth.InitialResponse.Length);
			Assert.Equal (0, cmdAuth.InitialResponse[0]);
			Assert.Equal (97, cmdAuth.InitialResponse[1]);
			Assert.Equal (97, cmdAuth.InitialResponse[2]);
			Assert.Equal (97, cmdAuth.InitialResponse[3]);
			Assert.Equal (0, cmdAuth.InitialResponse[4]);
			Assert.Equal (98, cmdAuth.InitialResponse[5]);
			Assert.Equal (98, cmdAuth.InitialResponse[6]);
			Assert.Equal (98, cmdAuth.InitialResponse[7]);
			Assert.Equal ("NEW", cmdAuth.Mechanism);
			Assert.Equal (cmdStr.Length, source.Offset);

			// waiting SASL response
			cmdStr = "AGFhYQBiYmI=\r\n";
			source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (cmdStr + _quitCommand));
			cmd = SmtpCommand.Parse (source, SmtpCommand.ExpectedInputType.AuthenticationResponse, null);
			Assert.Equal (SmtpCommandType.SaslResponse, cmd.CommandType);
			Assert.IsType<SmtpSaslResponseCommand> (cmd);
			var cmdSaslResponse = (SmtpSaslResponseCommand)cmd;
			Assert.Equal (8, cmdSaslResponse.Response.Length);
			Assert.Equal (0, cmdSaslResponse.Response[0]);
			Assert.Equal (97, cmdSaslResponse.Response[1]);
			Assert.Equal (97, cmdSaslResponse.Response[2]);
			Assert.Equal (97, cmdSaslResponse.Response[3]);
			Assert.Equal (0, cmdSaslResponse.Response[4]);
			Assert.Equal (98, cmdSaslResponse.Response[5]);
			Assert.Equal (98, cmdSaslResponse.Response[6]);
			Assert.Equal (98, cmdSaslResponse.Response[7]);
			Assert.Equal (cmdStr.Length, source.Offset);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ToString_ ()
		{
			var str = SmtpCommand.Data.ToString ();
			Assert.Equal ("DATA\r\n", str);

			str = SmtpCommand.Noop.ToString ();
			Assert.Equal ("NOOP\r\n", str);

			str = SmtpCommand.Quit.ToString ();
			Assert.Equal ("QUIT\r\n", str);

			str = SmtpCommand.Rset.ToString ();
			Assert.Equal ("RSET\r\n", str);

			var id = "my-007-id";
			str = new SmtpHeloCommand (id).ToString ();
			Assert.Equal ("HELO " + id + "\r\n", str);

			str = new SmtpEhloCommand (id).ToString ();
			Assert.Equal ("EHLO " + id + "\r\n", str);

			str = new SmtpMailFromCommand (null, ContentTransferEncoding.SevenBit, null).ToString ();
			Assert.Equal ("MAIL FROM:<>\r\n", str);
			str = new SmtpMailFromCommand (null, ContentTransferEncoding.EightBit, null).ToString ();
			Assert.Equal ("MAIL FROM:<> BODY=8BITMIME\r\n", str);
			var mailbox = new AddrSpec ("user", "server.net");
			str = new SmtpMailFromCommand (mailbox, ContentTransferEncoding.Binary, null).ToString ();
			Assert.Equal ("MAIL FROM:" + mailbox.ToAngleString () + " BODY=BINARYMIME\r\n", str);

			str = new SmtpRcptToCommand (mailbox).ToString ();
			Assert.Equal ("RCPT TO:" + mailbox.ToAngleString () + "\r\n", str);

			str = new SmtpVrfyCommand (id).ToString ();
			Assert.Equal ("VRFY " + id + "\r\n", str);

			var src = new ArrayBufferedSource (new byte[1]);
			str = new SmtpBdatCommand (src, 1, false).ToString ();
			Assert.Equal ("BDAT 1\r\n", str);
			str = new SmtpBdatCommand (src, 0x4000000000000000L, true).ToString ();
			Assert.Equal ("BDAT 4611686018427387904 LAST\r\n", str);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void Construction ()
		{
			// размер источника равен указанному
			var buf = new byte[100];
			var src = new ArrayBufferedSource (buf, 3, 10);
			var cmd = new SmtpBdatCommand (src, 10, false);
			Assert.NotNull (cmd.Source);
			Assert.Equal (10, cmd.Source.Count);
			cmd.Source.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.True (cmd.Source.IsExhausted);

			// размер источника меньше указанного
			src = new ArrayBufferedSource (buf, 2, 1);
			cmd = new SmtpBdatCommand (src, 33, false);
			Assert.NotNull (cmd.Source);
			Assert.Equal (1, cmd.Source.Count);
			cmd.Source.FillBufferAsync (CancellationToken.None).Wait ();
			cmd.Source.SkipBuffer (cmd.Source.Count);
			Assert.ThrowsAsync<NotEnoughDataException> (async () => await cmd.Source.FillBufferAsync (CancellationToken.None));

			// размер источника больше указанного
			src = new ArrayBufferedSource (buf, 15, 19);
			cmd = new SmtpBdatCommand (src, 8, false);
			Assert.NotNull (cmd.Source);
			Assert.Equal (8, cmd.Source.Count);
			cmd.Source.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.Equal (8, cmd.Source.UnusedSize);
		}
	}
}
