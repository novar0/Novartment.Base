using System;

namespace Novartment.Base.Net.Smtp
{
	internal sealed class SmtpVrfyCommand : SmtpCommand
	{
		internal SmtpVrfyCommand (ReadOnlySpan<char> parameters)
			: base (SmtpCommandType.Vrfy)
		{
#if NETSTANDARD2_0
			this.Parameters = new string (parameters.ToArray ());
#else
			this.Parameters = new string (parameters);
#endif
		}

		internal string Parameters { get; }

		public override string ToString ()
		{
			return FormattableString.Invariant ($"VRFY {this.Parameters}\r\n");
		}

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			// vrfy = "VRFY" SP String CRLF
			var trimmedValue = value.Trim ();
			return trimmedValue.Length > 0 ?
				(SmtpCommand)new SmtpVrfyCommand (trimmedValue) :
				new SmtpInvalidSyntaxCommand (SmtpCommandType.Vrfy, "Missed 'VRFY' parameter.");
		}
	}
}
