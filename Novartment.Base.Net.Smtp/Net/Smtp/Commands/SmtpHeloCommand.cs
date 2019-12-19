using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpHeloCommand : SmtpCommand
	{
		internal SmtpHeloCommand (ReadOnlySpan<char> clientIdentification)
			: base (SmtpCommandType.Helo)
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
			return FormattableString.Invariant ($"HELO {this.ClientIdentification}\r\n");
		}

		internal static SmtpHeloCommand Parse (ReadOnlySpan<char> value)
		{
			// helo = "HELO" SP Domain CRLF
			return new SmtpHeloCommand (value.Trim ());
		}
	}
}
