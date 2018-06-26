﻿using System;
using Novartment.Base.Text;

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

		internal static SmtpEhloCommand Parse (ReadOnlySpan<char> value)
		{
			// ehlo = "EHLO" SP ( Domain / address-literal ) CRLF
			return new SmtpEhloCommand (value.Trim ());
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"EHLO {this.ClientIdentification}\r\n");
		}
	}
}