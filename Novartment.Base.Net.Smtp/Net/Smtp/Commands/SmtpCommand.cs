using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpCommand
	{
		internal static readonly SmtpCommand CachedCmdUnknown = new (SmtpCommandType.Unknown);
		internal static readonly SmtpCommand CachedCmdNoCommand = new (SmtpCommandType.NoCommand);
		internal static readonly SmtpCommand CachedCmdData = new (SmtpCommandType.Data);
		internal static readonly SmtpCommand CachedCmdNoop = new (SmtpCommandType.Noop);
		internal static readonly SmtpCommand CachedCmdQuit = new (SmtpCommandType.Quit);
		internal static readonly SmtpCommand CachedCmdRset = new (SmtpCommandType.Rset);
		internal static readonly SmtpCommand CachedCmdStartTls = new (SmtpCommandType.StartTls);
		internal static readonly StructuredStringFormat _TokenFormat = new (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Token, char.MaxValue, null);
		internal static readonly StructuredStringFormat _NumberFormat = new (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Digit, char.MaxValue, null);
		internal static readonly StructuredStringFormat _AnyVisibleCharFormat = new (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Visible, char.MaxValue, null);
		internal static readonly byte[] MessageEndMarker = new byte[] { 0x0d, 0x0a, (byte)'.', 0x0d, 0x0a };

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
				SmtpCommandType.Vrfy => SmtpVrfyCommand.Parse (source[pos..]),
				SmtpCommandType.Ehlo => SmtpEhloCommand.Parse (source[pos..]),
				SmtpCommandType.Helo => SmtpHeloCommand.Parse (source[pos..]),
				SmtpCommandType.MailFrom => SmtpMailFromCommand.Parse (source[pos..]),
				SmtpCommandType.RcptTo => SmtpRcptToCommand.Parse (source[pos..]),
				SmtpCommandType.Bdat => SmtpBdatCommand.Parse (source[pos..]),
				SmtpCommandType.Auth => SmtpAuthCommand.Parse (source[pos..]),
				_ => CachedCmdUnknown,
			};
			return result;
		}
	}
}
