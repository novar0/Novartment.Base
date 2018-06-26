﻿using System;
using Novartment.Base.Text;

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

		internal static SmtpCommand Parse (ReadOnlySpan<char> responseSrc)
		{
			// RFC 4953 part 4: If the client wishes to cancel the authentication exchange, it issues a line with a single "*".
			if ((responseSrc.Length == 1) && responseSrc[0] == (byte)'*')
			{
				return new SmtpSaslResponseCommand (default);
			}

			byte[] response = null;
			int responseSize = 0;
#if NETCOREAPP2_1
			response = new byte[(responseSrc.Length / 4 * 3) + 2];
			if (!Convert.TryFromBase64Chars (responseSrc, response, out responseSize))
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.SaslResponse, "Unrecognized authentication response.");
			}
#else
					try
					{
						response = Convert.FromBase64String (new string (responseSrc.ToArray ()));
						responseSize = response.Length;
					}
					catch (FormatException excpt)
					{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.SaslResponse, "Unrecognized authentication response." + excpt.Message);
					}
#endif
			return new SmtpSaslResponseCommand (response.AsSpan (0, responseSize));
		}

		public override string ToString ()
		{
			int size;
			var buf = new char[(((_response.Length / 3) + 1) * 4) + 3];
#if NETCOREAPP2_1
			Convert.TryToBase64Chars (_response, buf, out size, Base64FormattingOptions.None);
			var responseBase64 = new string (buf.AsSpan (0, size));
#else
			size = Convert.ToBase64CharArray (_response, 0, _response.Length, buf, 0, Base64FormattingOptions.None);
			var responseBase64 = new string (buf, 0, size);
#endif
			return responseBase64 + "\r\n";
		}
	}
}