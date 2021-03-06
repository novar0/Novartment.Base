using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal sealed class SmtpMailFromCommand : SmtpCommand
	{
		internal static readonly AddrSpec _emptyAddrSpec = new (Guid.NewGuid ().ToString (), "local");
		private static readonly StructuredStringFormat _DotAtomFormat = new (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Atom, '.', new StructuredStringTokenCustomFormat[] { StructuredStringTokenFormatPath.Instance });

		private readonly ContentTransferEncoding _requestedContentTransferEncoding;

		internal SmtpMailFromCommand (
			AddrSpec returnPath,
			ContentTransferEncoding requestedContentTransferEncoding,
			AddrSpec associatedMailbox)
			: base (SmtpCommandType.MailFrom)
		{
			this.ReturnPath = returnPath;
			_requestedContentTransferEncoding = requestedContentTransferEncoding;
			this.AssociatedMailbox = associatedMailbox;
		}

		internal static AddrSpec EmptyAddrSpec => _emptyAddrSpec;

		internal AddrSpec ReturnPath { get; }

		internal AddrSpec AssociatedMailbox { get; }

		internal ContentTransferEncoding RequestedContentTransferEncoding => _requestedContentTransferEncoding;

		public override string ToString ()
		{
			var pathStr = this.ReturnPath?.ToString () ?? string.Empty;
			var result = _requestedContentTransferEncoding switch
			{
				ContentTransferEncoding.EightBit => FormattableString.Invariant ($"MAIL FROM:<{pathStr}> BODY=8BITMIME"),
				ContentTransferEncoding.Binary => FormattableString.Invariant ($"MAIL FROM:<{pathStr}> BODY=BINARYMIME"),
				_ => FormattableString.Invariant ($"MAIL FROM:<{pathStr}>"),
			};
			if (this.AssociatedMailbox != null)
			{
				result += " AUTH=" + ((this.AssociatedMailbox != EmptyAddrSpec) ?
					"<" + this.AssociatedMailbox + ">" :
					"<>");
			}

			result += "\r\n";

			return result;
		}

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			/*
			RFC 5321:
			"MAIL FROM:" Reverse-path [SP esmtp-param *(SP esmtp-param)] CRLF
			Reverse-path   = "<" Mailbox ">" / "<>"
			esmtp-param    = esmtp-keyword ["=" esmtp-value]
			esmtp-keyword  = (ALPHA / DIGIT) *(ALPHA / DIGIT / "-")
			esmtp-value    = 1*(%d33-60 / %d62-126) ; any CHAR excluding "=", SP, and control characters.
			*/

			var pos = 0;
			var pathToken = _DotAtomFormat.ParseToken (value, ref pos);
			if (!(pathToken.Format is StructuredStringTokenFormatPath))
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "Unrecognized 'MAIL FROM' parameter.");
			}

			var returnPath = pathToken.Length > 2 ?
				AddrSpec.Parse (value.Slice (pathToken.Position + 1, pathToken.Length - 2)) :
				null;

			/*
			Дальше разбираем параметры, нераспознанные нельзя игнорировать согласно RFC 5321 part 4.1.1.11:
			If the server SMTP does not recognize or cannot implement one or more of the parameters
			associated with a particular MAIL FROM or RCPT TO command, it will return code 555.
			*/

			value = value[pos..];
			pos = 0;

			var bodyType = ContentTransferEncoding.SevenBit;
			var bodyTypeSpecified = false;
			AddrSpec associatedMailbox = null;
			while (true)
			{
				// пропускаем начальные пробелы
				while ((pos < value.Length) && (value[pos] == ' '))
				{
					pos++;
				}

				if (pos >= value.Length)
				{
					break;
				}

				// ограничиваем параметр пробелом перед следующим параметром
				var posEnd = value[pos..].IndexOf (' ');
				if (posEnd < 0)
				{
					posEnd = value.Length;
				}
				else
				{
					posEnd += pos;
				}

				var valuePos = value[pos..posEnd].IndexOf ('=');
				if (valuePos < 0)
				{
					return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "Unrecognized 'MAIL FROM' AUTH parameter.");
				}

				var parameterName = value.Slice (pos, valuePos);
				var parameterValue = value.Slice (pos + valuePos + 1, posEnd - pos - valuePos - 1);
				pos = posEnd;

				if ("BODY".AsSpan ().Equals (parameterName, StringComparison.OrdinalIgnoreCase))
				{
					if ("8BITMIME".AsSpan ().Equals (parameterValue, StringComparison.OrdinalIgnoreCase))
					{
						if (bodyTypeSpecified)
						{
							return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "'MAIL FROM' BODY parameter specified more than once.");
						}

						bodyType = ContentTransferEncoding.EightBit;
						bodyTypeSpecified = true;
					}
					else
					{
						if ("BINARYMIME".AsSpan ().Equals (parameterValue, StringComparison.OrdinalIgnoreCase))
						{
							if (bodyTypeSpecified)
							{
								return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "'MAIL FROM' BODY parameter specified more than once.");
							}

							bodyType = ContentTransferEncoding.Binary;
							bodyTypeSpecified = true;
						}
						else
						{
							return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, FormattableString.Invariant (
								$"Unrecognized 'MAIL FROM' BODY parameter value. Expected 8BITMIME or BINARYMIME."));
						}
					}
				}
				else
				{
					if ("AUTH".AsSpan ().Equals (parameterName, StringComparison.OrdinalIgnoreCase))
					{
						if ((parameterValue.Length < 2) || (parameterValue[0] != '<') || (parameterValue[^1] != '>'))
						{
							return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "Unrecognized 'MAIL FROM' AUTH parameter.");
						}

						if (parameterValue.Length < 3)
						{
							associatedMailbox = SmtpMailFromCommand.EmptyAddrSpec;
						}
						else
						{
							try
							{
								associatedMailbox = AddrSpec.Parse (parameterValue[1..^1]);
							}
							catch (FormatException)
							{
								return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "Unrecognized 'MAIL FROM' AUTH parameter.");
							}
						}
					}
					else
					{
						return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, FormattableString.Invariant (
							$"Unrecognized 'MAIL FROM' parameter. Expected BODY or AUTH."));
					}
				}
			}

			return new SmtpMailFromCommand (returnPath, bodyType, associatedMailbox);
		}
	}
}
