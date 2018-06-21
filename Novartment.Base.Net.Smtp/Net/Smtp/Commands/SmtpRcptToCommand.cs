using System;

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

		internal static SmtpCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			var isAngleBracketedValueFound = chunkEnumerator.MoveToNextAngleBracketedValue (value);
			if (!isAngleBracketedValueFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, "Unrecognized 'RCPT TO' parameter.");
			}

			AddrSpec recepient = null;
			var recipient = chunkEnumerator.GetStringInBrackets (value);
			try
			{
				recepient = AddrSpec.Parse (recipient);
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, FormattableString.Invariant (
					$"Unrecognized 'RCPT TO' parameter. {excpt}"));
			}

			return new SmtpRcptToCommand (recepient);
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"RCPT TO:{this.Recipient.ToAngleString ()}\r\n");
		}
	}
}
