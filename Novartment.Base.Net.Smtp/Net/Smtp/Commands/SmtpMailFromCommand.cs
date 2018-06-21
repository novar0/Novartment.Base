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

		internal static SmtpCommand Parse (BytesChunkEnumerator chunkEnumerator)
		{
			var isAngleBracketedValueFound = chunkEnumerator.MoveToNextBracketedValue (0x20, (byte)'<', (byte)'>');
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
#if NETCOREAPP2_1
					returnPath = AddrSpec.Parse (chunkEnumerator.GetStringInBrackets ());
#else
					returnPath = AddrSpec.Parse (chunkEnumerator.GetStringInBrackets ().AsSpan ());
#endif
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
			while (chunkEnumerator.MoveToNextChunk (0x20, 0x20, false))
			{
				var parameterNameAndValue = chunkEnumerator.GetStringMaskingInvalidChars ();
				var charsInParameterName = Math.Min (5, parameterNameAndValue.Length);
				var parameterName = parameterNameAndValue.Substring (0, charsInParameterName);
				var parameterValue = parameterNameAndValue.Substring (charsInParameterName);
				switch (parameterName.ToUpperInvariant ())
				{
					case "BODY=":
						switch (parameterValue.ToUpperInvariant ())
						{
							case "8BITMIME":
								if (bodyTypeSpecified)
								{
									return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "'MAIL FROM' BODY parameter specified more than once.");
								}

								bodyType = ContentTransferEncoding.EightBit;
								bodyTypeSpecified = true;
								break;
							case "BINARYMIME":
								if (bodyTypeSpecified)
								{
									return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "'MAIL FROM' BODY parameter specified more than once.");
								}

								bodyType = ContentTransferEncoding.Binary;
								bodyTypeSpecified = true;
								break;
							default:
								return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, FormattableString.Invariant (
									$"Unrecognized 'MAIL FROM' BODY parameter value '{parameterValue}'. Expected 8BITMIME or BINARYMIME."));
						}

						break;
					case "AUTH=":
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
								associatedMailbox = AddrSpec.Parse (parameterValue.AsSpan (1, parameterValue.Length - 2));
							}
							catch (FormatException)
							{
								return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, "Unrecognized 'MAIL FROM' AUTH parameter.");
							}
						}

						break;
					default:
						return new SmtpInvalidSyntaxCommand (SmtpCommandType.MailFrom, FormattableString.Invariant (
							$"Unrecognized 'MAIL FROM' parameter '{parameterNameAndValue}'. Expected BODY or AUTH."));
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
