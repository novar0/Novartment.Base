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
