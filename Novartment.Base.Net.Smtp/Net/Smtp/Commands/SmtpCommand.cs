using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpCommand
	{
		internal static readonly SmtpCommand CachedCmdUnknown = new SmtpCommand (SmtpCommandType.Unknown);
		internal static readonly SmtpCommand CachedCmdNoCommand = new SmtpCommand (SmtpCommandType.NoCommand);
		internal static readonly SmtpCommand CachedCmdData = new SmtpCommand (SmtpCommandType.Data);
		internal static readonly SmtpCommand CachedCmdNoop = new SmtpCommand (SmtpCommandType.Noop);
		internal static readonly SmtpCommand CachedCmdQuit = new SmtpCommand (SmtpCommandType.Quit);
		internal static readonly SmtpCommand CachedCmdRset = new SmtpCommand (SmtpCommandType.Rset);
		internal static readonly SmtpCommand CachedCmdStartTls = new SmtpCommand (SmtpCommandType.StartTls);

		protected SmtpCommand (SmtpCommandType commandType)
		{
			this.CommandType = commandType;
		}

		internal enum ExpectedInputType
		{
			Command,
			Data,
			AuthenticationResponse,
		}

		internal SmtpCommandType CommandType { get; }

		public override string ToString ()
		{
			// остальные команды сами переопределяют метод ToString()
			switch (this.CommandType)
			{
				case SmtpCommandType.Data:
					return "DATA\r\n";
				case SmtpCommandType.Rset:
					return "RSET\r\n";
				case SmtpCommandType.Noop:
					return "NOOP\r\n";
				case SmtpCommandType.Quit:
					return "QUIT\r\n";
				case SmtpCommandType.StartTls:
					return "STARTTLS\r\n";
			}

			throw new InvalidOperationException ("Command's string representation undefined.");
		}

		// Разбор синтаксиса. Никаких исключений. Если что не так, возвращает SmtpInvalidSyntaxCommand.
		internal static SmtpCommand Parse (ReadOnlySpan<char> source, ExpectedInputType expectedInputType)
		{
			if (expectedInputType == ExpectedInputType.AuthenticationResponse)
			{
				var authResponseCmd = SmtpSaslResponseCommand.Parse (source);
				return authResponseCmd;
			}

			var chunkEnumerator = new BytesChunkEnumerator ();
			var isChunkFound = chunkEnumerator.MoveToNextChunk (source, false, ' ');
			if (!isChunkFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Unknown, "Line is empty.");
			}

			// делаем терпимый к отклонениям от стандарта разбор
			SmtpCommand result = null;
			var pos = 0;
			//var commandTypeElement = StructuredValueParser.GetNextElementToken (source, ref pos);
			var commandType = SmtpCommandTypeHelper.Parse (source, chunkEnumerator);
			switch (commandType)
			{
				case SmtpCommandType.Data: // data = "DATA" CRLF
					result = CachedCmdData;
					break;
				case SmtpCommandType.Noop: // noop = "NOOP" [ SP String ] CRLF
					result = CachedCmdNoop;
					break;
				case SmtpCommandType.Quit: // quit = "QUIT" CRLF
					result = CachedCmdQuit;
					break;
				case SmtpCommandType.Rset: // rset = "RSET" CRLF
					result = CachedCmdRset;
					break;
				case SmtpCommandType.StartTls:
					result = CachedCmdStartTls;
					break;
				case SmtpCommandType.Vrfy: // vrfy = "VRFY" SP String CRLF
					result = SmtpVrfyCommand.Parse (source, chunkEnumerator);
					break;
				case SmtpCommandType.Ehlo: // ehlo = "EHLO" SP ( Domain / address-literal ) CRLF
					result = SmtpEhloCommand.Parse (source, chunkEnumerator);
					break;
				case SmtpCommandType.Helo: // helo = "HELO" SP Domain CRLF
					result = SmtpHeloCommand.Parse (source, chunkEnumerator);
					break;
				case SmtpCommandType.MailFrom: // MAIL FROM:<reverse-path> [SP <mail-parameters> ] <CRLF> ; Reverse-path   = "<" Mailbox ">" / "<>"
					result = SmtpMailFromCommand.Parse (source, chunkEnumerator);
					break;
				case SmtpCommandType.RcptTo: // RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF> ; Forward-path   = "<" Mailbox ">"
					result = SmtpRcptToCommand.Parse (source, chunkEnumerator);
					break;
				case SmtpCommandType.Bdat: // bdat-cmd = "BDAT" SP chunk-size [ SP end-marker ] CR LF
					result = SmtpBdatCommand.Parse (source, chunkEnumerator);
					break;
				case SmtpCommandType.Auth:
					result = SmtpAuthCommand.Parse (source, chunkEnumerator);
					break;
				default:
					result = CachedCmdUnknown;
					break;
			}

			return result;
		}
	}
}
