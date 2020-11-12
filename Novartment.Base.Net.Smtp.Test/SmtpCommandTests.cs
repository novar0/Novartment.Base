using System;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public class SmtpCommandTests
	{
		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseUnknown ()
		{
			var cmd = SmtpCommand.Parse (new char[] { (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0 }, SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);

			cmd = SmtpCommand.Parse ("HELO\tlocalhost", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);

			cmd = SmtpCommand.Parse (" QUIT", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);

			cmd = SmtpCommand.Parse ("DATA2", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);

			cmd = SmtpCommand.Parse ("MAIL FROM <someone@server.com>", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Unknown, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseData ()
		{
			var cmd = SmtpCommand.Parse ("DATA", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);

			cmd = SmtpCommand.Parse ("Data", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseNoop ()
		{
			var cmd = SmtpCommand.Parse ("NOOP", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Noop, cmd.CommandType);

			cmd = SmtpCommand.Parse ("NOOP --synchronize transaction NOW--", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Noop, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseQuit ()
		{
			var cmd = SmtpCommand.Parse ("QUIT", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Quit, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseRset ()
		{
			var cmd = SmtpCommand.Parse ("rset", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Rset, cmd.CommandType);

			cmd = SmtpCommand.Parse ("RSET", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Rset, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseVrfy ()
		{
			var cmd = SmtpCommand.Parse ("VRFY", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Vrfy, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("VRFY John Doe", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Vrfy, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseHelo ()
		{
			var id = "insufficient\tsystem\tstorage";
			var cmd = SmtpCommand.Parse ("HELO " + id, SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Helo, cmd.CommandType);
			Assert.IsType<SmtpHeloCommand> (cmd);
			var cmdHelo = (SmtpHeloCommand)cmd;
			Assert.Equal (id, cmdHelo.ClientIdentification);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseEhlo ()
		{
			var id = "insufficient\tsystem\tstorage";
			var cmd = SmtpCommand.Parse ("EHLO " + id, SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Ehlo, cmd.CommandType);
			Assert.IsType<SmtpEhloCommand> (cmd);
			var cmdEhlo = (SmtpEhloCommand)cmd;
			Assert.Equal (id, cmdEhlo.ClientIdentification);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseMailfrom ()
		{
			var id = "insufficient\tsystem\tstorage";
			var cmd = SmtpCommand.Parse ("MAIL from:someone@server.com", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("MAIL FROM:Peter_Parker <someone@server.com>", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("mail from:<>", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpMailFromCommand> (cmd);
			var cmdMailFrom = (SmtpMailFromCommand)cmd;
			Assert.Null (cmdMailFrom.ReturnPath);
			Assert.Equal (ContentTransferEncoding.SevenBit, cmdMailFrom.RequestedContentTransferEncoding);

			cmd = SmtpCommand.Parse ("mail from:<> AUTH=me@server.com", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("mail from:<> BODY:8BITMIME", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("mail from:<> BODY=8BIT-MIME", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("mail from:<someone@server.com> RET=HDRS BODY=BINARYMIME", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("mail from:<someone@server.com> BODY=BINARYMIME BODY=8BITMIME", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("MAIL FROM:<someone@server.com> " + id, SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("mail from:<> AUTH=<> BODY=8BITMIME", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpMailFromCommand> (cmd);
			cmdMailFrom = (SmtpMailFromCommand)cmd;
			Assert.Null (cmdMailFrom.ReturnPath);
			Assert.Equal (SmtpMailFromCommand.EmptyAddrSpec, cmdMailFrom.AssociatedMailbox);
			Assert.Equal (ContentTransferEncoding.EightBit, cmdMailFrom.RequestedContentTransferEncoding);

			cmd = SmtpCommand.Parse ("mail from:<someone@server.com>    BODY=BINARYMIME  AUTH=<someone@server.com> ", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			Assert.IsType<SmtpMailFromCommand> (cmd);
			cmdMailFrom = (SmtpMailFromCommand)cmd;
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdMailFrom.ReturnPath);
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdMailFrom.AssociatedMailbox);
			Assert.Equal (ContentTransferEncoding.Binary, cmdMailFrom.RequestedContentTransferEncoding);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseRcptto ()
		{
			var id = "insufficient\tsystem\tstorage";
			var cmd = SmtpCommand.Parse ("RCPT TO:<@support>", SmtpCommand.ExpectedInputType.Command);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);

			cmd = SmtpCommand.Parse ("RCPT TO:support@www.ru", SmtpCommand.ExpectedInputType.Command);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);

			cmd = SmtpCommand.Parse ("RCPT TO:<>", SmtpCommand.ExpectedInputType.Command);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);

			cmd = SmtpCommand.Parse ("RCPT TO:<someone@server.com>", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.IsType<SmtpRcptToCommand> (cmd);
			var cmdRcptTo = (SmtpRcptToCommand)cmd;
			Assert.Equal (new AddrSpec ("someone", "server.com"), cmdRcptTo.Recipient);

			cmd = SmtpCommand.Parse ("rcpt TO:<someone@server.com> " + id, SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseBdat ()
		{
			var cmd = SmtpCommand.Parse ("BDAT", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("BDAT -10", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("bdat LAST", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("BDAT 4647785733979898881", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpBdatCommand> (cmd);
			var cmdBdat = (SmtpBdatCommand)cmd;
			Assert.Equal (4647785733979898881, cmdBdat.Size);
			Assert.False (cmdBdat.IsLast);

			cmd = SmtpCommand.Parse ("bDAT 2 LAST", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpBdatCommand> (cmd);
			cmdBdat = (SmtpBdatCommand)cmd;
			Assert.Equal (2, cmdBdat.Size);
			Assert.True (cmdBdat.IsLast);

			cmd = SmtpCommand.Parse ("bdat 10LAST", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			Assert.IsType<SmtpBdatCommand> (cmd);
			cmdBdat = (SmtpBdatCommand)cmd;
			Assert.Equal (10, cmdBdat.Size);
			Assert.True (cmdBdat.IsLast);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseStarttls ()
		{
			var cmd = SmtpCommand.Parse ("STARTTLS", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.StartTls, cmd.CommandType);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ParseAuth ()
		{
			var cmd = SmtpCommand.Parse ("AUTH", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Auth, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("AUTH NEW METHOD", SmtpCommand.ExpectedInputType.Command);
			Assert.Equal (SmtpCommandType.Auth, cmd.CommandType);
			Assert.IsType<SmtpInvalidSyntaxCommand> (cmd);

			cmd = SmtpCommand.Parse ("AUTH SOME", SmtpCommand.ExpectedInputType.Command);
			var cmdAuth = (SmtpAuthCommand)cmd;
			Assert.Equal (0, cmdAuth.InitialResponse.Length);
			Assert.Equal ("SOME", cmdAuth.Mechanism);

			cmd = SmtpCommand.Parse ("AUTH NEW =", SmtpCommand.ExpectedInputType.Command);
			cmdAuth = (SmtpAuthCommand)cmd;
			Assert.Equal (0, cmdAuth.InitialResponse.Length);
			Assert.Equal ("NEW", cmdAuth.Mechanism);

			cmd = SmtpCommand.Parse ("AUTH NEW AGFhYQBiYmI=", SmtpCommand.ExpectedInputType.Command);
			cmdAuth = (SmtpAuthCommand)cmd;
			Assert.Equal (8, cmdAuth.InitialResponse.Length);
			Assert.Equal (0, cmdAuth.InitialResponse.Span[0]);
			Assert.Equal (97, cmdAuth.InitialResponse.Span[1]);
			Assert.Equal (97, cmdAuth.InitialResponse.Span[2]);
			Assert.Equal (97, cmdAuth.InitialResponse.Span[3]);
			Assert.Equal (0, cmdAuth.InitialResponse.Span[4]);
			Assert.Equal (98, cmdAuth.InitialResponse.Span[5]);
			Assert.Equal (98, cmdAuth.InitialResponse.Span[6]);
			Assert.Equal (98, cmdAuth.InitialResponse.Span[7]);
			Assert.Equal ("NEW", cmdAuth.Mechanism);

			// waiting SASL response
			cmd = SmtpCommand.Parse ("AGFhYQBiYmI=", SmtpCommand.ExpectedInputType.AuthenticationResponse);
			Assert.Equal (SmtpCommandType.SaslResponse, cmd.CommandType);
			Assert.IsType<SmtpSaslResponseCommand> (cmd);
			var cmdSaslResponse = (SmtpSaslResponseCommand)cmd;
			Assert.Equal (8, cmdSaslResponse.Response.Length);
			Assert.Equal (0, cmdSaslResponse.Response.Span[0]);
			Assert.Equal (97, cmdSaslResponse.Response.Span[1]);
			Assert.Equal (97, cmdSaslResponse.Response.Span[2]);
			Assert.Equal (97, cmdSaslResponse.Response.Span[3]);
			Assert.Equal (0, cmdSaslResponse.Response.Span[4]);
			Assert.Equal (98, cmdSaslResponse.Response.Span[5]);
			Assert.Equal (98, cmdSaslResponse.Response.Span[6]);
			Assert.Equal (98, cmdSaslResponse.Response.Span[7]);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ToString_ ()
		{
			var str = SmtpCommand.CachedCmdData.ToString ();
			Assert.Equal ("DATA\r\n", str);

			str = SmtpCommand.CachedCmdNoop.ToString ();
			Assert.Equal ("NOOP\r\n", str);

			str = SmtpCommand.CachedCmdQuit.ToString ();
			Assert.Equal ("QUIT\r\n", str);

			str = SmtpCommand.CachedCmdRset.ToString ();
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
			Assert.Equal ("MAIL FROM:<" + mailbox + "> BODY=BINARYMIME\r\n", str);

			str = new SmtpRcptToCommand (mailbox).ToString ();
			Assert.Equal ("RCPT TO:<" + mailbox + ">\r\n", str);

			str = new SmtpVrfyCommand (id).ToString ();
			Assert.Equal ("VRFY " + id + "\r\n", str);

			var cmd = new SmtpBdatCommand (1, false);
			Assert.Equal ("BDAT 1\r\n", cmd.ToString ());
			cmd = new SmtpBdatCommand (0x4000000000000000L, true);
			Assert.Equal ("BDAT 4611686018427387904 LAST\r\n", cmd.ToString ());
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void Construction ()
		{
			// размер источника равен указанному
			Memory<byte> buf = new byte[100];
			var src = new MemoryBufferedSource (buf.Slice (3, 10));
			var cmd = new SmtpBdatCommand (10, false);
			cmd.SetSource (src);
			Assert.NotNull (cmd.SourceData);
			Assert.Equal (10, cmd.SourceData.Count);
			cmd.SourceData.LoadAsync ().AsTask ().Wait ();
			Assert.True (cmd.SourceData.IsExhausted);

			// размер источника меньше указанного
			src = new MemoryBufferedSource (buf.Slice (2, 1));
			cmd = new SmtpBdatCommand (33, false);
			cmd.SetSource (src);
			Assert.NotNull (cmd.SourceData);
			Assert.Equal (1, cmd.SourceData.Count);
			cmd.SourceData.LoadAsync ().AsTask ().Wait ();
			cmd.SourceData.Skip (cmd.SourceData.Count);
			Assert.ThrowsAsync<NotEnoughDataException> (() => cmd.SourceData.LoadAsync ().AsTask ());

			// размер источника больше указанного
			src = new MemoryBufferedSource (buf.Slice (15, 19));
			cmd = new SmtpBdatCommand (8, false);
			cmd.SetSource (src);
			Assert.NotNull (cmd.SourceData);
			Assert.Equal (8, cmd.SourceData.Count);
			cmd.SourceData.LoadAsync ().AsTask ().Wait ();
			Assert.Equal (8, cmd.SourceData.UnusedSize);
		}
	}
}
