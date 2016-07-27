using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Novartment.Base.Text;
using Novartment.Base.Collections;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	internal struct SmtpReplyWithGroupingMark
	{
		internal SmtpReply Reply { get; }

		internal bool CanBeGrouped { get; }

		internal SmtpReplyWithGroupingMark (SmtpReply reply, bool canBeGrouped)
		{
			Reply = reply;
			CanBeGrouped = canBeGrouped;
		}
	}

	internal class SmtpReply
	{
		internal IReadOnlyList<string> Text { get; }

		internal int Code { get; }

		// RFC 5321 part 4.2. SMTP Replies:
		// ... a sender-SMTP MUST be prepared to handle codes not specified in this document and MUST do so by interpreting the first digit only.
		// RFC 5321 part 4.3.2:
		// SMTP clients SHOULD, when possible, interpret only the first digit of the reply and
		// MUST be prepared to deal with unrecognized reply codes by interpreting the first digit only.
		internal bool IsPositive => (this.Code < 300);

		internal bool IsPositiveIntermediate => (this.Code >= 300) && (this.Code < 400);

		internal bool IsTransientNegative => (this.Code >= 400) && (this.Code < 500);

		internal bool IsPermanentNegative => (this.Code >= 500);

		private SmtpReply (int code, IReadOnlyList<string> text)
		{
			this.Code = code;
			this.Text = text;
		}

		internal readonly static SmtpReply ReadyToStartTls = new SmtpReply (
			220, new string[] { "Ready to start TLS" });
		internal readonly static SmtpReply Disconnect = new SmtpReply (
			221, new string[] { "Bye" });
		internal readonly static SmtpReply AuthenticationSucceeded = new SmtpReply (
			235, new string[] { "Authentication Succeeded" });
		internal readonly static SmtpReply OK = new SmtpReply (
			250, new string[] { "OK" });
		internal readonly static SmtpReply CannotVerifyUser = new SmtpReply (
			252, new string[] { "Cannot VRFY user, but will accept message" });
		internal readonly static SmtpReply DataStart = new SmtpReply (
			354, new string[] { "OK" });
		internal readonly static SmtpReply ServiceNotAvailable = new SmtpReply (
			421, new string[] { "Service shutting down and closing transmission channel" });
		internal readonly static SmtpReply MailboxTemporarilyUnavailable = new SmtpReply (
			450, new string[] { "Mailbox temporarily unavailable (busy or temporarily blocked for policy reasons)" });
		internal readonly static SmtpReply LocalError = new SmtpReply (
			451, new string[] { "Local error in processing" });
		internal readonly static SmtpReply TooManyRecipients = new SmtpReply (
			452, new string[] { "Too many recipients" });
		internal readonly static SmtpReply TlsNotAvailable = new SmtpReply (
			454, new string[] { "TLS not available due to temporary reason" });
		internal readonly static SmtpReply NotImplemented = new SmtpReply (
			500, new string[] { "Syntax error, command unrecognized" });
		internal readonly static SmtpReply LineTooLong = new SmtpReply (
			500, new string[] { "Syntax error, line too long" });
		internal readonly static SmtpReply SyntaxErrorInParameter = new SmtpReply (
			501, new string[] { "Syntax error in parameters or arguments" });
		internal readonly static SmtpReply BadSequenceOfCommands = new SmtpReply (
			503, new string[] { "Bad sequence of commands" });
		internal readonly static SmtpReply UnrecognizedAuthenticationType = new SmtpReply (
			504, new string[] { "504 Unrecognized authentication type." });
		internal readonly static SmtpReply MustUseStartTlsFirst = new SmtpReply (
			530, new string[] { "Must issue a STARTTLS command first." });
		internal readonly static SmtpReply AuthenticationRequired = new SmtpReply (
			530, new string[] { "Authentication required." });
		internal readonly static SmtpReply AuthenticationCredentialsInvalid = new SmtpReply (
			535, new string[] { "Authentication credentials invalid." });
		internal readonly static SmtpReply EncryptionRequiredForAuthentication = new SmtpReply (
			538, new string[] { "Encryption required for requested authentication mechanism." });
		internal readonly static SmtpReply MailboxUnavailable = new SmtpReply (
			550, new string[] { "Mailbox unavailable (not found, no access, or rejected for policy reasons)" });
		internal readonly static SmtpReply MailboxNotAllowed = new SmtpReply (
			553, new string[] { "Requested action not taken: mailbox name not allowed" });
		internal readonly static SmtpReply UnableToInitializeSecurity = new SmtpReply (
			554, new string[] { "Unable to initialize security subsystem" });
		internal readonly static SmtpReply NoValidRecipients = new SmtpReply (
			554, new string[] { "No valid recipients" });

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
			var text = new ArrayList<string> ();
			text.Add (hostFqdn);
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

		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Trace(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		internal static SmtpReply Parse (IBufferedSource source, ILogWriter logger)
		{
			// RFC 5321 part 4.2:
			// Only the EHLO, EXPN, and HELP commands are expected to result in multiline replies in normal circumstances;
			// however, multiline replies are allowed for any command.
			var elements = new ArrayList<string> (1);
			bool isLastElement = false;
			int lastNumber = -1;
			do
			{
				int idx = 0;
				do
				{
					if (idx >= (source.Count - 1))
					{
						throw new FormatException ("CRLF not found.");
					}
					idx++;
				} while ((source.Buffer[source.Offset + idx - 1] != 0x0d) || (source.Buffer[source.Offset + idx] != 0x0a));
				idx++;
				logger?.Trace ("<<< " + AsciiCharSet.GetStringMaskingInvalidChars (source.Buffer, source.Offset, idx - 2, '?'));

				// RFC 5321 part 4.5.3.1.5:
				// The maximum total length of a reply line including the reply code and the <CRLF> is 512 octets.
				if (idx > 512)
				{
					throw new FormatException ("Reply line too long. Maximum 512 bytes.");
				}
				if (idx < 5) // including CRLF
				{
					throw new FormatException ("Reply line do not contains 3-digit code.");
				}
				var codeChar1 = source.Buffer[source.Offset];
				var codeChar2 = source.Buffer[source.Offset + 1];
				var codeChar3 = source.Buffer[source.Offset + 2];
				// RFC 5321 part 4.2: Reply-code  = %x32-35 %x30-35 %x30-39
				if ((codeChar1 < 0x32) || (codeChar1 > 0x35) ||
					(codeChar2 < 0x30) || (codeChar2 > 0x39) ||
					(codeChar3 < 0x30) || (codeChar3 > 0x39))
				{
					throw new FormatException ("Reply line do not contains valid 3-digit code.");
				}
				lastNumber = (source.Buffer[source.Offset] - 0x030) * 100 +
					(source.Buffer[source.Offset + 1] - 0x30) * 10 +
					(source.Buffer[source.Offset + 2] - 0x30);

				if (idx < 6) // 'nnn CRLF' = 6 bytes
				{
					isLastElement = true;
				}
				else
				{
					var separator = source.Buffer[source.Offset + 3];
					if ((separator != 0x20) && (separator != 0x2d))
					{
						throw new FormatException ("Reply line contains invalid character after 3-digit code.");
					}
					isLastElement = (separator != 0x2d);
					if (idx > 6)
					{
						var text = AsciiCharSet.GetString (source.Buffer, source.Offset + 4, idx - 6); // 'nnn CRLF' = 6 bytes
						elements.Add (text);
					}
				}
				source.SkipBuffer (idx);
			} while (!isLastElement);

			return new SmtpReply (lastNumber, elements);
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
