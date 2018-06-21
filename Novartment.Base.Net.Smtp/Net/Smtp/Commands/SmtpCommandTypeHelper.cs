namespace Novartment.Base.Net.Smtp
{
	internal static class SmtpCommandTypeHelper
	{
		internal static SmtpCommandType Parse (BytesChunkEnumerator chunkEnumerator)
		{
			var commandTypeStr = chunkEnumerator.GetString ().ToUpperInvariant ();

			if ((commandTypeStr == "MAIL") || (commandTypeStr == "RCPT"))
			{
				// если команда из двух слов, то добавляем второе
				// ищем второе слово оканчивающееся на ':'
				var isSecondWordFound = chunkEnumerator.MoveToNextChunk (0x0d, (byte)':', true);
				if (isSecondWordFound)
				{
					commandTypeStr += chunkEnumerator.GetString ().ToUpperInvariant ();
				}
			}

			switch (commandTypeStr)
			{
				case "DATA": return SmtpCommandType.Data;
				case "NOOP": return SmtpCommandType.Noop;
				case "QUIT": return SmtpCommandType.Quit;
				case "RSET": return SmtpCommandType.Rset;
				case "VRFY": return SmtpCommandType.Vrfy;
				case "EHLO": return SmtpCommandType.Ehlo;
				case "HELO": return SmtpCommandType.Helo;
				case "MAIL FROM:": return SmtpCommandType.MailFrom;
				case "RCPT TO:": return SmtpCommandType.RcptTo;
				case "BDAT": return SmtpCommandType.Bdat;
				case "STARTTLS": return SmtpCommandType.StartTls;
				case "AUTH": return SmtpCommandType.Auth;
				default: return SmtpCommandType.Unknown;
			}
		}
	}
}
