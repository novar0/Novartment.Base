using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpAuthCommand : SmtpCommand
	{
		internal SmtpAuthCommand (string mechanism, ReadOnlyMemory<byte> initialResponse)
			: base (SmtpCommandType.Auth)
		{
			this.Mechanism = mechanism;
			this.InitialResponse = initialResponse;
		}

		// список см. http://www.iana.org/assignments/sasl-mechanisms/sasl-mechanisms.xhtml
		internal string Mechanism { get; }

		internal ReadOnlyMemory<byte> InitialResponse { get; }

		internal static SmtpCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			var isParameterFound = chunkEnumerator.MoveToNextChunk (value, true, ' ');
			if (!isParameterFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, "Missed 'AUTH' mechanism parameter.");
			}

#if NETCOREAPP2_1
			var mechanism = new string (chunkEnumerator.GetString (value));
#else
			var mechanism = new string (chunkEnumerator.GetString (value).ToArray ());
#endif
			byte[] initialResponse = null;
			int responseSize = 0;
			var isInitialResponseFound = chunkEnumerator.MoveToNextChunk (value, true, (char)0x0d);
			if (isInitialResponseFound)
			{
				var initialResponseBase64 = chunkEnumerator.GetString (value);

				if ((initialResponseBase64.Length != 1) || (initialResponseBase64[0] != '='))
				{
#if NETCOREAPP2_1
					initialResponse = new byte[(initialResponseBase64.Length / 4 * 3) + 2];
					if (!Convert.TryFromBase64Chars (initialResponseBase64, initialResponse, out responseSize))
					{
						return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, FormattableString.Invariant (
							$"Unrecognized 'AUTH' initial-response parameter."));
					}
#else
					try
					{
						initialResponse = Convert.FromBase64String (new string (initialResponseBase64.ToArray ()));
						responseSize = initialResponse.Length;
					}
					catch (FormatException excpt)
					{
						return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, FormattableString.Invariant (
							$"Unrecognized 'AUTH' initial-response parameter. {excpt.Message}"));
					}
#endif
				}
			}

			return new SmtpAuthCommand (mechanism, initialResponse.AsMemory (0, responseSize));
		}

		public override string ToString ()
		{
			if (this.InitialResponse.Length < 1)
			{
				return "AUTH " + this.Mechanism + "\r\n";
			}

			int size;
			var buf = new char[(((this.InitialResponse.Length / 3) + 1) * 4) + 3];
#if NETCOREAPP2_1
			Convert.TryToBase64Chars (this.InitialResponse.Span, buf, out size, Base64FormattingOptions.None);
			var responseBase64 = new string (buf.AsSpan (0, size));
#else
			size = Convert.ToBase64CharArray (this.InitialResponse.ToArray (), 0, this.InitialResponse.Length, buf, 0, Base64FormattingOptions.None);
			var responseBase64 = new string (buf, 0, size);
#endif
			return "AUTH " + this.Mechanism + " " + responseBase64 + "\r\n";
		}
	}
}
