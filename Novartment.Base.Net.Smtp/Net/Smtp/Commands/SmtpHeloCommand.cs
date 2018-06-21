using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpHeloCommand : SmtpCommand
	{
		internal SmtpHeloCommand (string clientIdentification)
			: base (SmtpCommandType.Helo)
		{
			this.ClientIdentification = clientIdentification;
		}

		internal string ClientIdentification { get; }

		internal static SmtpHeloCommand Parse (BytesChunkEnumerator chunkEnumerator)
		{
			chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
			return new SmtpHeloCommand (chunkEnumerator.GetStringMaskingInvalidChars ());
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"HELO {this.ClientIdentification}\r\n");
		}
	}
}
