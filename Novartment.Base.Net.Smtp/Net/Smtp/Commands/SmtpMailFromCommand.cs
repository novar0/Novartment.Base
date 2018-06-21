using System;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpMailFromCommand : SmtpCommand
	{
		internal static readonly AddrSpec _emptyAddrSpec = new AddrSpec (Guid.NewGuid ().ToString (), "local");

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

		internal static SmtpCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			var isAngleBracketedValueFound = chunkEnumerator.MoveToNextAngleBracketedValue (value);
			if (!isAngleBracketedValueFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "Unrecognized 'MAIL FROM' parameter.");
			}

			AddrSpec returnPath = null;
			if (chunkEnumerator.ChunkSize > 2)
			{
				// адрес не пустой
				try
				{
					returnPath = AddrSpec.Parse (chunkEnumerator.GetStringInBrackets (value));
				}
				catch (FormatException excpt)
				{
					return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, FormattableString.Invariant (
						$"Unrecognized 'MAIL FROM' parameter. {excpt.Message}"));
				}
			}

			// RFC 3030:
			// Mail-parameters  = esmtp-param *(SP esmtp-param)
			// esmtp-param      = esmtp-keyword ["=" esmtp-value]
			// body-value ::= "7BIT" / "8BITMIME" / "BINARYMIME"
			var bodyType = ContentTransferEncoding.SevenBit;
			var bodyTypeSpecified = false;
			AddrSpec associatedMailbox = null;
			while (chunkEnumerator.MoveToNextChunk (value, true, ' ', false))
			{
				var parameterNameAndValue = chunkEnumerator.GetString (value);
				var charsInParameterName = Math.Min (5, parameterNameAndValue.Length);
				var parameterName = parameterNameAndValue.Slice (0, charsInParameterName);
				var parameterValue = parameterNameAndValue.Slice (charsInParameterName);
				if ("BODY=".AsSpan ().Equals (parameterName, StringComparison.OrdinalIgnoreCase))
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
					if ("AUTH=".AsSpan ().Equals (parameterName, StringComparison.OrdinalIgnoreCase))
					{
						if ((parameterValue.Length < 2) || (parameterValue[0] != '<') || (parameterValue[parameterValue.Length - 1] != '>'))
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
								associatedMailbox = AddrSpec.Parse (parameterValue.Slice (1, parameterValue.Length - 2));
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

		public override string ToString ()
		{
			var pathStr = this.ReturnPath?.ToAngleString () ?? "<>";
			string result;
			switch (_requestedContentTransferEncoding)
			{
				case ContentTransferEncoding.EightBit:
					result = FormattableString.Invariant ($"MAIL FROM:{pathStr} BODY=8BITMIME");
					break;
				case ContentTransferEncoding.Binary:
					result = FormattableString.Invariant ($"MAIL FROM:{pathStr} BODY=BINARYMIME");
					break;
				default:
					result = FormattableString.Invariant ($"MAIL FROM:{pathStr}");
					break;
			}

			if (this.AssociatedMailbox != null)
			{
				result += " AUTH=" + ((this.AssociatedMailbox != EmptyAddrSpec) ?
					this.AssociatedMailbox.ToAngleString () :
					"<>");
			}

			result += "\r\n";

			return result;
		}
	}
}
