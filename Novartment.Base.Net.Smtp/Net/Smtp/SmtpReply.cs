using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal sealed class SmtpReply
	{
		internal static readonly SmtpReply ReadyToStartTls = new (
			220, new string[] { "Ready to start TLS" });

		internal static readonly SmtpReply Disconnect = new (
			221, new string[] { "Bye" });

		internal static readonly SmtpReply AuthenticationSucceeded = new (
			235, new string[] { "Authentication Succeeded" });

		internal static readonly SmtpReply OK = new (
			250, new string[] { "OK" });

		internal static readonly SmtpReply CannotVerifyUser = new (
			252, new string[] { "Cannot VRFY user, but will accept message" });

		internal static readonly SmtpReply DataStart = new (
			354, new string[] { "OK" });

		internal static readonly SmtpReply ServiceNotAvailable = new (
			421, new string[] { "Service shutting down and closing transmission channel" });

		internal static readonly SmtpReply MailboxTemporarilyUnavailable = new (
			450, new string[] { "Mailbox temporarily unavailable (busy or temporarily blocked for policy reasons)" });

		internal static readonly SmtpReply LocalError = new (
			451, new string[] { "Local error in processing" });

		internal static readonly SmtpReply TooManyRecipients = new (
			452, new string[] { "Too many recipients" });

		internal static readonly SmtpReply TlsNotAvailable = new (
			454, new string[] { "TLS not available due to temporary reason" });

		internal static readonly SmtpReply NotImplemented = new (
			500, new string[] { "Syntax error, command unrecognized" });

		internal static readonly SmtpReply LineTooLong = new (
			500, new string[] { "Syntax error, line too long" });

		internal static readonly SmtpReply SyntaxErrorInParameter = new (
			501, new string[] { "Syntax error in parameters or arguments" });

		internal static readonly SmtpReply BadSequenceOfCommands = new (
			503, new string[] { "Bad sequence of commands" });

		internal static readonly SmtpReply UnrecognizedAuthenticationType = new (
			504, new string[] { "504 Unrecognized authentication type." });

		internal static readonly SmtpReply MustUseStartTlsFirst = new (
			530, new string[] { "Must issue a STARTTLS command first." });

		internal static readonly SmtpReply AuthenticationRequired = new (
			530, new string[] { "Authentication required." });

		internal static readonly SmtpReply AuthenticationCredentialsInvalid = new (
			535, new string[] { "Authentication credentials invalid." });

		internal static readonly SmtpReply EncryptionRequiredForAuthentication = new (
			538, new string[] { "Encryption required for requested authentication mechanism." });

		internal static readonly SmtpReply MailboxUnavailable = new (
			550, new string[] { "Mailbox unavailable (not found, no access, or rejected for policy reasons)" });

		internal static readonly SmtpReply MailboxNotAllowed = new (
			553, new string[] { "Requested action not taken: mailbox name not allowed" });

		internal static readonly SmtpReply UnableToInitializeSecurity = new (
			554, new string[] { "Unable to initialize security subsystem" });

		internal static readonly SmtpReply NoValidRecipients = new (
			554, new string[] { "No valid recipients" });

		private SmtpReply (int code, IReadOnlyList<string> text)
		{
			this.Code = code;
			this.Text = text;
		}

		internal IReadOnlyList<string> Text { get; }

		internal int Code { get; }

		// RFC 5321 part 4.2. SMTP Replies:
		// ... a sender-SMTP MUST be prepared to handle codes not specified in this document and MUST do so by interpreting the first digit only.
		// RFC 5321 part 4.3.2:
		// SMTP clients SHOULD, when possible, interpret only the first digit of the reply and
		// MUST be prepared to deal with unrecognized reply codes by interpreting the first digit only.
		internal bool IsPositive => this.Code < 300;

		internal bool IsPositiveIntermediate => (this.Code >= 300) && (this.Code < 400);

		internal bool IsTransientNegative => (this.Code >= 400) && (this.Code < 500);

		internal bool IsPermanentNegative => this.Code >= 500;

		public override string ToString ()
		{
			if (this.Text.Count < 2)
			{
				return ((this.Text.Count < 1) || (this.Text[0] == null) || (this.Text[0].Length < 1)) ?
					FormattableString.Invariant ($"{this.Code}\r\n") :
					FormattableString.Invariant ($"{this.Code} {this.Text[0]}\r\n");
			}

			// multiline reply
			int i = 0;
			var result = new StringBuilder ();
			while (i < (this.Text.Count - 1))
			{
				result.Append (FormattableString.Invariant ($"{this.Code}-{this.Text[i]}\r\n"));
				i++;
			}

			result.Append (FormattableString.Invariant ($"{this.Code} {this.Text[i]}\r\n"));
			return result.ToString ();
		}

		internal static SmtpReply CreateServiceReady (string assemblyName, Version assemblyVersion)
		{
			return new SmtpReply (220, new string[] { assemblyName + " " + assemblyVersion });
		}

		internal static SmtpReply CreateHelloResponse (string hostFqdn, IReadOnlyCollection<string> supportedExtensions)
		{
			// RFC 5321 part 4.1.1.1
			// ehlo-ok-rsp =
			//   ( "250" SP Domain [ SP ehlo-greet ] CRLF )
			//   /
			//   ( "250-" Domain [ SP ehlo-greet ] CRLF *( "250-" ehlo-line CRLF ) "250" SP ehlo-line CRLF )
			// ehlo-line = ehlo-keyword *( SP ehlo-param )
			var text = new ArrayList<string>
			{
				hostFqdn,
			};
			foreach (var ext in supportedExtensions)
			{
				text.Add (ext);
			}

			return new SmtpReply (250, text);
		}

		internal static SmtpReply CreateSaslChallenge (byte[] challenge)
		{
			if (challenge == null)
			{
				// RFC 4954 part 4:
				// a server challenge that contains no data is defined as a 334 reply with no text part.
				// Note that there is still a space following the reply code, so the complete response line is "334 ".
				return new SmtpReply (334, new string[] { string.Empty });
			}

			return new SmtpReply (334, new string[] { Convert.ToBase64String (challenge) });
		}

		internal static SmtpReply Parse (IBufferedSource source, ILogger logger)
		{
			// RFC 5321 part 4.2:
			// Only the EHLO, EXPN, and HELP commands are expected to result in multiline replies in normal circumstances;
			// however, multiline replies are allowed for any command.
			var elements = new ArrayList<string> (1);
			bool isLast;
			int number;
			do
			{
				string text;
				int size;
				(number, text, isLast, size) = ParseLine (source.BufferMemory.Span.Slice (source.Offset, source.Count), logger);
				if (text != null)
				{
					elements.Add (text);
				}

				source.Skip (size);
			}
			while (!isLast);

			return new SmtpReply (number, elements);
		}

		private static string GetStringMaskingInvalidChars (ReadOnlySpan<byte> value)
		{
			if (value.Length < 1)
			{
				return string.Empty;
			}

			var result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				var b = value[i];
				result[i] = (b > AsciiCharSet.MaxCharValue) ? '?' : (char)b;
			}

			return new string (result);
		}

		private static (int, string, bool, int) ParseLine (ReadOnlySpan<byte> sourceBuf, ILogger logger)
		{
			int idx = 0;
			do
			{
				if (idx >= (sourceBuf.Length - 1))
				{
					throw new FormatException ("CRLF not found.");
				}

				idx++;
			}
			while ((sourceBuf[idx - 1] != 0x0d) || (sourceBuf[idx] != 0x0a));
			idx++;
			if ((logger != null) && logger.IsEnabled (LogLevel.Trace))
			{
				logger?.LogTrace ("<<< " + GetStringMaskingInvalidChars (sourceBuf.Slice (0, idx - 2)));
			}

			// RFC 5321 part 4.5.3.1.5:
			// The maximum total length of a reply line including the reply code and the <CRLF> is 512 octets.
			if (idx > 512)
			{
				throw new FormatException ("Reply line too long. Maximum 512 bytes.");
			}

			// including CRLF
			if (idx < 5)
			{
				throw new FormatException ("Reply line do not contains 3-digit code.");
			}

			var codeChar1 = sourceBuf[0];
			var codeChar2 = sourceBuf[1];
			var codeChar3 = sourceBuf[2];

			// RFC 5321 part 4.2: Reply-code  = %x32-35 %x30-35 %x30-39
			if ((codeChar1 < 0x32) || (codeChar1 > 0x35) ||
				(codeChar2 < 0x30) || (codeChar2 > 0x39) ||
				(codeChar3 < 0x30) || (codeChar3 > 0x39))
			{
				throw new FormatException ("Reply line does not contains valid 3-digit code.");
			}

			var number = ((codeChar1 - '0') * 100) + ((codeChar2 - '0') * 10) + (codeChar3 - '0');

			// 'nnn CRLF' = 6 bytes
			if (idx < 6)
			{
				return (number, null, true, idx);
			}

			var separator = sourceBuf[3];
			if ((separator != 0x20) && (separator != 0x2d))
			{
				throw new FormatException ("Reply line contains invalid character after 3-digit code.");
			}

			string text = null;
			var isLastElement = separator != 0x2d;
			if (idx > 6)
			{
				text = AsciiCharSet.GetString (sourceBuf.Slice (4, idx - 6)); // 'nnn CRLF' = 6 bytes
			}

			return (number, text, isLastElement, idx);
		}

		internal SmtpReplyWithGroupingMark DisallowGrouping ()
		{
			return new SmtpReplyWithGroupingMark (this, false);
		}

		internal SmtpReplyWithGroupingMark AllowGrouping ()
		{
			return new SmtpReplyWithGroupingMark (this, true);
		}
	}
}
