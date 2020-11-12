using System;

namespace Novartment.Base.Net.Smtp
{
	internal sealed class SmtpEhloCommand : SmtpCommand
	{
		internal SmtpEhloCommand (ReadOnlySpan<char> clientIdentification)
			: base (SmtpCommandType.Ehlo)
		{
#if NETSTANDARD2_0
			this.ClientIdentification = new string (clientIdentification.ToArray ());
#else
			this.ClientIdentification = new string (clientIdentification);
#endif
		}

		internal string ClientIdentification { get; }

		public override string ToString ()
		{
			return FormattableString.Invariant ($"EHLO {this.ClientIdentification}\r\n");
		}

		internal static SmtpEhloCommand Parse (ReadOnlySpan<char> value)
		{
			// ehlo = "EHLO" SP ( Domain / address-literal ) CRLF
			return new SmtpEhloCommand (value.Trim ());
		}
	}
}
