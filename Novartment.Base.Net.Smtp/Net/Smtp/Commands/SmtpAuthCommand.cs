﻿using System;
using Novartment.Base.Text;

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

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			/*
			RFC 4954:
			"AUTH" SP sasl-mech [SP initial-response] *(CRLF [base64]) [CRLF cancel-response] CRLF
			initial-response= base64 / "="

			*(CRLF [base64]) [CRLF cancel-response] тут не рассматривается, он придёт как отдельная команда
			*/

			var pos = 0;
			var saslMechElement = StructuredValueParser.GetNextElement (value, ref pos, AsciiCharClasses.Visible, false);
			if (saslMechElement.ElementType != StructuredValueElementType.Value)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, "Unrecognized 'AUTH' mechanism parameter.");
			}

#if NETCOREAPP2_1
			var mechanism = new string (value.Slice (saslMechElement.StartPosition, saslMechElement.Length));
#else
			var mechanism = new string (value.Slice (saslMechElement.StartPosition, saslMechElement.Length).ToArray ());
#endif

			var initialEesponseElement = StructuredValueParser.GetNextElement (value, ref pos, AsciiCharClasses.Visible, false);
			if (!initialEesponseElement.IsValid || ((initialEesponseElement.Length == 1) && (value[initialEesponseElement.StartPosition] == '=')))
			{
				return new SmtpAuthCommand (mechanism, default);
			}

			byte[] initialResponse;
			int responseSize;
			var initialResponseBase64 = value.Slice (initialEesponseElement.StartPosition, initialEesponseElement.Length);
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
