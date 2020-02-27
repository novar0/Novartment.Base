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
		internal static readonly StructuredStringFormat _TokenFormat = new StructuredStringFormat (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Token, false, null);
		internal static readonly StructuredStringFormat _NumberFormat = new StructuredStringFormat (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Digit, false, null);
		internal static readonly StructuredStringFormat _AnyVisibleCharFormat = new StructuredStringFormat (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Visible, false, null);

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
			return this.CommandType switch
			{
				SmtpCommandType.Data => "DATA\r\n",
				SmtpCommandType.Rset => "RSET\r\n",
				SmtpCommandType.Noop => "NOOP\r\n",
				SmtpCommandType.Quit => "QUIT\r\n",
				SmtpCommandType.StartTls => "STARTTLS\r\n",
				_ => throw new InvalidOperationException ("Command's string representation undefined."),
			};
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
			var result = commandType switch
			{
				SmtpCommandType.Data => CachedCmdData,
				SmtpCommandType.Noop => CachedCmdNoop,
				SmtpCommandType.Quit => CachedCmdQuit,
				SmtpCommandType.Rset => CachedCmdRset,
				SmtpCommandType.StartTls => CachedCmdStartTls,
				SmtpCommandType.Vrfy => SmtpVrfyCommand.Parse (source.Slice (pos)),
				SmtpCommandType.Ehlo => SmtpEhloCommand.Parse (source.Slice (pos)),
				SmtpCommandType.Helo => SmtpHeloCommand.Parse (source.Slice (pos)),
				SmtpCommandType.MailFrom => SmtpMailFromCommand.Parse (source.Slice (pos)),
				SmtpCommandType.RcptTo => SmtpRcptToCommand.Parse (source.Slice (pos)),
				SmtpCommandType.Bdat => SmtpBdatCommand.Parse (source.Slice (pos)),
				SmtpCommandType.Auth => SmtpAuthCommand.Parse (source.Slice (pos)),
				_ => CachedCmdUnknown,
			};
			return result;
		}
	}
}
