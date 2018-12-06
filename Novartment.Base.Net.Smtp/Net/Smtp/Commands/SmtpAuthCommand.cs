using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpAuthCommand : SmtpCommand
	{
		internal SmtpAuthCommand (string mechanism, ReadOnlySpan<byte> initialResponse)
			: base (SmtpCommandType.Auth)
		{
			this.Mechanism = mechanism;
			var buf = new byte[initialResponse.Length];
			initialResponse.CopyTo (buf);
			this.InitialResponse = buf;
		}

		// список см. http://www.iana.org/assignments/sasl-mechanisms/sasl-mechanisms.xhtml
		internal string Mechanism { get; }

		internal ReadOnlyMemory<byte> InitialResponse { get; }

		public override string ToString ()
		{
			if (this.InitialResponse.Length < 1)
			{
				return "AUTH " + this.Mechanism + "\r\n";
			}

			var buf = new char[this.Mechanism.Length +
				(((this.InitialResponse.Length / 3) + 1) * 4) + 11];
			var pos = 0;
			buf[pos++] = 'A';
			buf[pos++] = 'U';
			buf[pos++] = 'T';
			buf[pos++] = 'H';
			buf[pos++] = ' ';
			for (int i = 0; i < this.Mechanism.Length; i++)
			{
				buf[pos++] = this.Mechanism[i];
			}

			buf[pos++] = ' ';

			int size;
#if NETCOREAPP2_2
			Convert.TryToBase64Chars (this.InitialResponse.Span, buf.AsSpan (pos), out size, Base64FormattingOptions.None);
#else
			size = Convert.ToBase64CharArray (this.InitialResponse.ToArray (), 0, this.InitialResponse.Length, buf, pos, Base64FormattingOptions.None);
#endif
			pos += size;
			buf[pos++] = '\r';
			buf[pos++] = '\n';
			return new string (buf, 0, size);
		}

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			/*
			RFC 4954:
			"AUTH" SP sasl-mech [SP initial-response] *(CRLF [base64]) [CRLF cancel-response] CRLF
			sasl-mech    = 1*(UPPER-ALPHA / DIGIT / HYPHEN / UNDERSCORE)
			initial-response= base64 / "="

			*(CRLF [base64]) [CRLF cancel-response] тут не рассматривается, он придёт как отдельная команда
			*/

			var pos = 0;
			var saslMechToken = StructuredHeaderFieldLexicalToken.ParseToken (value, ref pos);
			if (saslMechToken.TokenType != StructuredHeaderFieldLexicalTokenType.Value)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, "Unrecognized 'AUTH' mechanism parameter.");
			}

#if NETCOREAPP2_2
			var mechanism = new string (value.Slice (saslMechToken.Position, saslMechToken.Length));
#else
			var mechanism = new string (value.Slice (saslMechToken.Position, saslMechToken.Length).ToArray ());
#endif

			var initialEesponseToken = StructuredHeaderFieldLexicalToken.Parse (value, ref pos, AsciiCharClasses.Visible, false);
			if (!initialEesponseToken.IsValid || ((initialEesponseToken.Length == 1) && (value[initialEesponseToken.Position] == '=')))
			{
				return new SmtpAuthCommand (mechanism, default);
			}

			byte[] initialResponse;
			int responseSize;
			var initialResponseBase64 = value.Slice (initialEesponseToken.Position, initialEesponseToken.Length);
#if NETCOREAPP2_2
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
			return new SmtpAuthCommand (mechanism, initialResponse.AsSpan (0, responseSize));
		}
	}
}
