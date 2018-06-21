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

		internal static SmtpCommand Parse (BytesChunkEnumerator chunkEnumerator)
		{
			var isAngleBracketedValueFound = chunkEnumerator.MoveToNextBracketedValue (0x20, (byte)'<', (byte)'>');
			if (!isAngleBracketedValueFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, "Unrecognized 'RCPT TO' parameter.");
			}

			AddrSpec recepient = null;
			var recipient = chunkEnumerator.GetStringInBrackets ();
			try
			{
#if NETCOREAPP2_1
				recepient = AddrSpec.Parse (recipient);
#else
				recepient = AddrSpec.Parse (recipient.AsSpan ());
#endif
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, FormattableString.Invariant (
					$"Unrecognized 'RCPT TO' parameter '{recipient}'. {excpt}"));
			}

			return new SmtpRcptToCommand (recepient);
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"RCPT TO:{this.Recipient.ToAngleString ()}\r\n");
		}
	}
}
