using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpVrfyCommand : SmtpCommand
	{
		internal SmtpVrfyCommand (string parameters)
			: base (SmtpCommandType.Vrfy)
		{
			this.Parameters = parameters;
		}

		internal string Parameters { get; }

		internal static SmtpCommand Parse (BytesChunkEnumerator chunkEnumerator)
		{
			bool isChunkFound = chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
			return isChunkFound ?
				(SmtpCommand)new SmtpVrfyCommand (chunkEnumerator.GetStringMaskingInvalidChars ()) :
				new SmtpInvalidSyntaxCommand (SmtpCommandType.Vrfy, "Missed 'VRFY' parameter.");
		}

		public override string ToString ()
		{
			return FormattableString.Invariant ($"VRFY {this.Parameters}\r\n");
		}
	}
}
