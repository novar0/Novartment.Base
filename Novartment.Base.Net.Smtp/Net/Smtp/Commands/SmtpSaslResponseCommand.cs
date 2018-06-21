using System;
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

		internal static SmtpCommand Parse (ReadOnlySpan<byte> responseSrc)
		{
			// RFC 4953 part 4: If the client wishes to cancel the authentication exchange, it issues a line with a single "*".
			if ((responseSrc.Length == 1) && responseSrc[0] == (byte)'*')
			{
				return new SmtpSaslResponseCommand (default);
			}

			try
			{
				var responseBase64 = AsciiCharSet.GetString (responseSrc);
				var response = Convert.FromBase64String (responseBase64);
				return new SmtpSaslResponseCommand (response);
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.SaslResponse, "Unrecognized authentication response." + excpt.Message);
			}

		}

		public override string ToString ()
		{
			return Convert.ToBase64String (_response) + "\r\n";
		}
	}
}
