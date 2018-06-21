using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpAuthCommand : SmtpCommand
	{
		internal SmtpAuthCommand (string mechanism, byte[] initialResponse)
			: base (SmtpCommandType.Auth)
		{
			this.Mechanism = mechanism;
			this.InitialResponse = initialResponse;
		}

		// список см. http://www.iana.org/assignments/sasl-mechanisms/sasl-mechanisms.xhtml
		internal string Mechanism { get; }

		internal byte[] InitialResponse { get; }

		internal static SmtpCommand Parse (BytesChunkEnumerator chunkEnumerator)
		{
			var isParameterFound = chunkEnumerator.MoveToNextChunk (0x20, 0x20);
			if (!isParameterFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, "Missed 'AUTH' mechanism parameter.");
			}

			var mechanism = chunkEnumerator.GetString ();
			byte[] initialResponse = null;
			var isInitialResponseFound = chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
			if (isInitialResponseFound)
			{
				try
				{
					var initialResponseBase64 = chunkEnumerator.GetString ();
					initialResponse = (initialResponseBase64 != "=") ?
						Convert.FromBase64String (initialResponseBase64) :
						Array.Empty<byte> ();
				}
				catch (FormatException excpt)
				{
					return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, FormattableString.Invariant (
						$"Unrecognized 'AUTH' initial-response parameter. {excpt.Message}"));
				}
			}

			return new SmtpAuthCommand (mechanism, initialResponse);
		}

		public override string ToString ()
		{
			return (this.InitialResponse != null) ?
				"AUTH " + this.Mechanism + " " + Convert.ToBase64String (this.InitialResponse) + "\r\n" :
				"AUTH " + this.Mechanism + "\r\n";
		}
	}
}
