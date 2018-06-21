using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpEhloCommand : SmtpCommand
	{
		internal SmtpEhloCommand (string clientIdentification)
			: base (SmtpCommandType.Ehlo)
		{
			this.ClientIdentification = clientIdentification;
		}

		internal string ClientIdentification { get; }

		internal static SmtpEhloCommand Parse (BytesChunkEnumerator chunkEnumerator)
		{
			chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
			return new SmtpEhloCommand (chunkEnumerator.GetStringMaskingInvalidChars ());
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"EHLO {this.ClientIdentification}\r\n");
		}
	}
}
