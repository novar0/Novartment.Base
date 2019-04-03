using System;

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

			// делаем терпимый к отклонениям от стандарта разбор
			var pos = 0;
			var commandType = SmtpCommandTypeHelper.Parse (source, ref pos);
			SmtpCommand result;
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
				case SmtpCommandType.Vrfy:
					result = SmtpVrfyCommand.Parse (source.Slice (pos));
					break;
				case SmtpCommandType.Ehlo:
					result = SmtpEhloCommand.Parse (source.Slice (pos));
					break;
				case SmtpCommandType.Helo:
					result = SmtpHeloCommand.Parse (source.Slice (pos));
					break;
				case SmtpCommandType.MailFrom:
					result = SmtpMailFromCommand.Parse (source.Slice (pos));
					break;
				case SmtpCommandType.RcptTo:
					result = SmtpRcptToCommand.Parse (source.Slice (pos));
					break;
				case SmtpCommandType.Bdat:
					result = SmtpBdatCommand.Parse (source.Slice (pos));
					break;
				case SmtpCommandType.Auth:
					result = SmtpAuthCommand.Parse (source.Slice (pos));
					break;
				default:
					result = CachedCmdUnknown;
					break;
			}

			return result;
		}
	}
}
