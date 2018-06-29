using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpRcptToCommand : SmtpCommand
	{
		internal SmtpRcptToCommand (AddrSpec recipient)
			: base (SmtpCommandType.RcptTo)
		{
			this.Recipient = recipient;
		}

		internal AddrSpec Recipient { get; }

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			// RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF> ; Forward-path   = "<" Mailbox ">"
			var pos = 0;
			var pathEelement = StructuredHeaderFieldLexicalToken.ParseDotAtom (value, ref pos);
			if (pathEelement.TokenType != StructuredHeaderFieldLexicalTokenType.AngleBracketedValue)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, "Unrecognized 'RCPT TO' parameter.");
			}

			AddrSpec recepient;
			try
			{
				recepient = AddrSpec.Parse (value.Slice (pathEelement.Position, pathEelement.Length));
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, FormattableString.Invariant (
					$"Unrecognized 'RCPT TO' parameter. {excpt}"));
			}

			/*
			Нераспознанные параметры нельзя игнорировать согласно RFC 5321 part 4.1.1.11:
			If the server SMTP does not recognize or cannot implement one or more of the parameters
			associated with a particular MAIL FROM or RCPT TO command, it will return code 555.
			*/

			// пропускаем пробелы
			while ((pos < value.Length) && (value[pos] == ' '))
			{
				pos++;
			}

			if (pos < value.Length)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, "Unrecognized 'RCPT TO' parameter.");
			}

			return new SmtpRcptToCommand (recepient);
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"RCPT TO:<{this.Recipient}>\r\n");
		}
	}
}
