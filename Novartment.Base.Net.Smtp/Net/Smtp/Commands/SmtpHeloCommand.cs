using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpHeloCommand : SmtpCommand
	{
		internal SmtpHeloCommand (ReadOnlySpan<char> clientIdentification)
			: base (SmtpCommandType.Helo)
		{
#if NETCOREAPP2_1
			this.ClientIdentification = new string (clientIdentification);
#else
			this.ClientIdentification = new string (clientIdentification.ToArray ());
#endif
		}

		internal string ClientIdentification { get; }

		internal static SmtpHeloCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			chunkEnumerator.MoveToNextChunk (value, true, (char)0x0d);
			return new SmtpHeloCommand (chunkEnumerator.GetString (value));
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"HELO {this.ClientIdentification}\r\n");
		}
	}
}
