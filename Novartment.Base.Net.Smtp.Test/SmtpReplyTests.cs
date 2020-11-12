using System;
using System.Text;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public sealed class SmtpReplyTests
	{
		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void SmtpReply_Parse ()
		{
			var replyText1 = "junk551\r\n";
			var replyText2 = "354 some text\r\n";
			var replyText3 = "250-PIPELINING\r\n250-CHUNKING\r\n250 SMTPUTF8\r\n";
			var replyText4 = "421 \r\n";
			var replyText5 = "421 ?\r\n";
			var src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (replyText1 + replyText2 + replyText3 + replyText4 + replyText5 + "junk"));
			src.Skip (4);

			var reply = SmtpReply.Parse (src, null);
			Assert.Equal (551, reply.Code);
			Assert.False (reply.IsPositive);
			Assert.False (reply.IsPositiveIntermediate);
			Assert.Equal (0, reply.Text.Count);
			Assert.Equal (replyText1.Length, src.Offset);

			reply = SmtpReply.Parse (src, null);
			Assert.Equal (354, reply.Code);
			Assert.False (reply.IsPositive);
			Assert.True (reply.IsPositiveIntermediate);
			Assert.Equal (1, reply.Text.Count);
			Assert.Equal ("some text", reply.Text[0]);
			Assert.Equal (replyText1.Length + replyText2.Length, src.Offset);

			reply = SmtpReply.Parse (src, null);
			Assert.Equal (250, reply.Code);
			Assert.True (reply.IsPositive);
			Assert.Equal (3, reply.Text.Count);
			Assert.Equal ("PIPELINING", reply.Text[0]);
			Assert.Equal ("CHUNKING", reply.Text[1]);
			Assert.Equal ("SMTPUTF8", reply.Text[2]);
			Assert.Equal (replyText1.Length + replyText2.Length + replyText3.Length, src.Offset);

			reply = SmtpReply.Parse (src, null);
			Assert.Equal (421, reply.Code);
			Assert.False (reply.IsPositive);
			Assert.False (reply.IsPositiveIntermediate);
			Assert.Equal (0, reply.Text.Count);
			Assert.Equal (replyText1.Length + replyText2.Length + replyText3.Length + replyText4.Length, src.Offset);

			reply = SmtpReply.Parse (src, null);
			Assert.Equal (421, reply.Code);
			Assert.False (reply.IsPositive);
			Assert.False (reply.IsPositiveIntermediate);
			Assert.Equal (1, reply.Text.Count);
			Assert.Equal ("?", reply.Text[0]);
			Assert.Equal (replyText1.Length + replyText2.Length + replyText3.Length + replyText4.Length + replyText5.Length, src.Offset);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void SmtpReply_Parse_Exception ()
		{
			// нет CRLF
			Assert.Throws<FormatException> (() => SmtpReply.Parse (new MemoryBufferedSource (Encoding.ASCII.GetBytes ("junk")), null));

			// пустая строка
			Assert.Throws<FormatException> (() => SmtpReply.Parse (new MemoryBufferedSource (Encoding.ASCII.GetBytes ("\r\n")), null));

			// невалидный номер
			Assert.Throws<FormatException> (() => SmtpReply.Parse (new MemoryBufferedSource (Encoding.ASCII.GetBytes ("20+ OK\r\n")), null));

			// невалидный разделитель номера
			Assert.Throws<FormatException> (() => SmtpReply.Parse (new MemoryBufferedSource (Encoding.ASCII.GetBytes ("250+OK\r\n")), null));

			// слишком длинная строка
			var buf = new byte[520];
			int i = 0;
			buf[i++] = 0x32;
			buf[i++] = 0x32;
			buf[i++] = 0x32;
			for (; i < buf.Length - 2; i++)
			{
				buf[i] = 0x20;
			}

			buf[i++] = 0x0d;
			buf[i] = 0x0a;
			Assert.Throws<FormatException> (() => SmtpReply.Parse (new MemoryBufferedSource (buf), null));

			// невалидные для ASCII символы
			Assert.Throws<FormatException> (() => SmtpReply.Parse (new MemoryBufferedSource (Encoding.UTF8.GetBytes ("250 ♂☎\r\n")), null));
		}
	}
}
