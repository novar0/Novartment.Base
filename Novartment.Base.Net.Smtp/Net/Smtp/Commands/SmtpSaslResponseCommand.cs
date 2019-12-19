using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpSaslResponseCommand : SmtpCommand
	{
		private readonly byte[] _response;

		internal SmtpSaslResponseCommand (ReadOnlySpan<byte> response)
			: base (SmtpCommandType.SaslResponse)
		{
			_response = new byte[response.Length];
			response.CopyTo (_response);
		}

		internal ReadOnlyMemory<byte> Response => _response;

		internal bool IsCancelRequest => _response.Length < 1;

		public override string ToString ()
		{
			var buf = new char[(((_response.Length / 3) + 1) * 4) + 5];
#if NETSTANDARD2_0
			var size = Convert.ToBase64CharArray (_response, 0, _response.Length, buf, 0, Base64FormattingOptions.None);
#else
			Convert.TryToBase64Chars (_response, buf, out int size, Base64FormattingOptions.None);
#endif
			buf[size++] = '\r';
			buf[size++] = '\n';
			return new string (buf, 0, size);
		}

		internal static SmtpCommand Parse (ReadOnlySpan<char> responseSrc)
		{
			// RFC 4953 part 4: If the client wishes to cancel the authentication exchange, it issues a line with a single "*".
			if ((responseSrc.Length == 1) && responseSrc[0] == (byte)'*')
			{
				return new SmtpSaslResponseCommand (default);
			}

			byte[] response = null;
			int responseSize = 0;
#if NETSTANDARD2_0
			try
			{
				response = Convert.FromBase64String (new string (responseSrc.ToArray ()));
				responseSize = response.Length;
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.SaslResponse, "Unrecognized authentication response." + excpt.Message);
			}
#else
			response = new byte[(responseSrc.Length / 4 * 3) + 2];
			if (!Convert.TryFromBase64Chars (responseSrc, response, out responseSize))
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.SaslResponse, "Unrecognized authentication response.");
			}
#endif
			return new SmtpSaslResponseCommand (response.AsSpan (0, responseSize));
		}
	}
}
