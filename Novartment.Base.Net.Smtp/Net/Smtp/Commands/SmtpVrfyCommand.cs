﻿using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpVrfyCommand : SmtpCommand
	{
		internal SmtpVrfyCommand (ReadOnlySpan<char> parameters)
			: base (SmtpCommandType.Vrfy)
		{
#if NETCOREAPP2_1
			this.Parameters = new string (parameters);
#else
			this.Parameters = new string (parameters.ToArray ());
#endif
		}

		internal string Parameters { get; }

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			// vrfy = "VRFY" SP String CRLF
			var trimmedValue = value.Trim ();
			return trimmedValue.Length > 0 ?
				(SmtpCommand)new SmtpVrfyCommand (trimmedValue) :
				new SmtpInvalidSyntaxCommand (SmtpCommandType.Vrfy, "Missed 'VRFY' parameter.");
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"VRFY {this.Parameters}\r\n");
		}
	}
}