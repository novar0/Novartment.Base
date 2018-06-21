using System;

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

		internal static SmtpCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			bool isChunkFound = chunkEnumerator.MoveToNextChunk (value, true, (char)0x0d);
			return isChunkFound ?
				(SmtpCommand)new SmtpVrfyCommand (chunkEnumerator.GetString (value)) :
				new SmtpInvalidSyntaxCommand (SmtpCommandType.Vrfy, "Missed 'VRFY' parameter.");
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"VRFY {this.Parameters}\r\n");
		}
	}
}
