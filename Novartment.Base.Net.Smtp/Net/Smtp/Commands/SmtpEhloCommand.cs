using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpEhloCommand : SmtpCommand
	{
		internal SmtpEhloCommand (ReadOnlySpan<char> clientIdentification)
			: base (SmtpCommandType.Ehlo)
		{
#if NETCOREAPP2_1
			this.ClientIdentification = new string (clientIdentification);
#else
			this.ClientIdentification = new string (clientIdentification.ToArray ());
#endif
		}

		internal string ClientIdentification { get; }

		internal static SmtpEhloCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			chunkEnumerator.MoveToNextChunk (value, true, (char)0x0d);
			return new SmtpEhloCommand (chunkEnumerator.GetString (value));
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"EHLO {this.ClientIdentification}\r\n");
		}
	}
}
