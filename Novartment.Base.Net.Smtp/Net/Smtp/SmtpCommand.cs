using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpCommand
	{
		internal static readonly SmtpCommand Unknown = new SmtpCommand (SmtpCommandType.Unknown);
		internal static readonly SmtpCommand NoCommand = new SmtpCommand (SmtpCommandType.NoCommand);
		internal static readonly SmtpCommand Data = new SmtpCommand (SmtpCommandType.Data);
		internal static readonly SmtpCommand Noop = new SmtpCommand (SmtpCommandType.Noop);
		internal static readonly SmtpCommand Quit = new SmtpCommand (SmtpCommandType.Quit);
		internal static readonly SmtpCommand Rset = new SmtpCommand (SmtpCommandType.Rset);
		internal static readonly SmtpCommand StartTls = new SmtpCommand (SmtpCommandType.StartTls);

		protected SmtpCommand (SmtpCommandType commandType)
		{
			this.CommandType = commandType;
		}

		internal enum ExpectedInputType
		{
			Command,
			Data,
			AuthenticationResponse,
		}

		internal SmtpCommandType CommandType { get; }

		public override string ToString ()
		{
			switch (this.CommandType)
			{
				case SmtpCommandType.Data:
					return "DATA\r\n";
				case SmtpCommandType.Rset:
					return "RSET\r\n";
				case SmtpCommandType.Noop:
					return "NOOP\r\n";
				case SmtpCommandType.Quit:
					return "QUIT\r\n";
				case SmtpCommandType.StartTls:
					return "STARTTLS\r\n";
			}

			throw new InvalidOperationException ("Command's string representation undefined.");
		}

		// Разбор синтаксиса.
		internal static SmtpCommand Parse (IBufferedSource source, ExpectedInputType expectedInputType, ILogger logger)
		{
			if (expectedInputType == ExpectedInputType.Data)
			{
				// TODO: сделать первые два байта разделителя (0x0d, 0x0a) частью данных
				return new SmtpActualDataCommand (source, true);
			}

			var countToCRLF = FindCRLF (source);
			if (countToCRLF < 0)
			{
				source.SkipBuffer (source.Count);
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Unknown, "Ending CRLF not found in command.");
			}

			logger?.LogTrace ("<<< " + AsciiCharSet.GetStringMaskingInvalidChars (source.BufferMemory.Span.Slice (source.Offset, countToCRLF), '?'));

			// RFC 5321 part 4.5.3.1.4:
			// The maximum total length of a command line including the command word and the <CRLF> is 512 octets.
			// RFC 5321 part 4.5.3.1.6:
			// The maximum total length of a text line including the <CRLF> is 1000 octets
			if (countToCRLF >= 1000)
			{
				source.SkipBuffer (countToCRLF);
				return new SmtpTooLongCommand ();
			}

			if (expectedInputType == ExpectedInputType.AuthenticationResponse)
			{
				return ParseAuthenticationResponse (source, countToCRLF);
			}

			var chunkEnumerator = new BytesChunkEnumerator (source.BufferMemory, source.Offset, countToCRLF);
			countToCRLF += 2;
			var isChunkFound = chunkEnumerator.MoveToNextChunk (0x0d, 0x20);
			if (!isChunkFound)
			{
				source.SkipBuffer (countToCRLF);
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Unknown, "Line is empty.");
			}

			// делаем терпимый к отклонениям от стандарта разбор
			SmtpCommand result = null;
			try
			{
				var commandType = GetCommandType (chunkEnumerator);
				switch (commandType)
				{
					case SmtpCommandType.Data: // data = "DATA" CRLF
						result = SmtpCommand.Data;
						break;
					case SmtpCommandType.Noop: // noop = "NOOP" [ SP String ] CRLF
						result = SmtpCommand.Noop;
						break;
					case SmtpCommandType.Quit: // quit = "QUIT" CRLF
						result = SmtpCommand.Quit;
						break;
					case SmtpCommandType.Rset: // rset = "RSET" CRLF
						result = SmtpCommand.Rset;
						break;
					case SmtpCommandType.Vrfy: // vrfy = "VRFY" SP String CRLF
						isChunkFound = chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
						result = isChunkFound ?
							(SmtpCommand)new SmtpVrfyCommand (chunkEnumerator.GetStringMaskingInvalidChars ()) :
							new SmtpInvalidSyntaxCommand (SmtpCommandType.Vrfy, "Missed 'VRFY' parameter.");
						break;
					case SmtpCommandType.Ehlo: // ehlo = "EHLO" SP ( Domain / address-literal ) CRLF
						chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
						result = new SmtpEhloCommand (chunkEnumerator.GetStringMaskingInvalidChars ());
						break;
					case SmtpCommandType.Helo: // helo = "HELO" SP Domain CRLF
						chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
						result = new SmtpHeloCommand (chunkEnumerator.GetStringMaskingInvalidChars ());
						break;
					case SmtpCommandType.MailFrom: // MAIL FROM:<reverse-path> [SP <mail-parameters> ] <CRLF> ; Reverse-path   = "<" Mailbox ">" / "<>"
						result = ParseMailFrom (chunkEnumerator);
						break;
					case SmtpCommandType.RcptTo: // RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF> ; Forward-path   = "<" Mailbox ">"
						result = ParseRcptTo (chunkEnumerator);
						break;
					case SmtpCommandType.Bdat: // bdat-cmd = "BDAT" SP chunk-size [ SP end-marker ] CR LF
						source.SkipBuffer (countToCRLF);
						countToCRLF = 0;
						result = ParseBdat (chunkEnumerator, source);
						break;
					case SmtpCommandType.StartTls:
						result = SmtpCommand.StartTls;
						break;
					case SmtpCommandType.Auth:
						result = ParseAuth (chunkEnumerator);
						break;
					default:
						result = SmtpCommand.Unknown;
						break;
				}
			}
			finally
			{
				// даже при исключении надо пропустить всю строку
				source.SkipBuffer (countToCRLF);
			}

			return result;
		}

		private static SmtpCommandType GetCommandType (BytesChunkEnumerator chunkEnumerator)
		{
			var commandTypeStr = chunkEnumerator.GetString ().ToUpperInvariant ();

			if ((commandTypeStr == "MAIL") || (commandTypeStr == "RCPT"))
			{
				// если команда из двух слов, то добавляем второе
				// ищем второе слово оканчивающееся на ':'
				var isSecondWordFound = chunkEnumerator.MoveToNextChunk (0x0d, (byte)':', true);
				if (isSecondWordFound)
				{
					commandTypeStr += chunkEnumerator.GetString ().ToUpperInvariant ();
				}
			}

			switch (commandTypeStr)
			{
				case "DATA": return SmtpCommandType.Data;
				case "NOOP": return SmtpCommandType.Noop;
				case "QUIT": return SmtpCommandType.Quit;
				case "RSET": return SmtpCommandType.Rset;
				case "VRFY": return SmtpCommandType.Vrfy;
				case "EHLO": return SmtpCommandType.Ehlo;
				case "HELO": return SmtpCommandType.Helo;
				case "MAIL FROM:": return SmtpCommandType.MailFrom;
				case "RCPT TO:": return SmtpCommandType.RcptTo;
				case "BDAT": return SmtpCommandType.Bdat;
				case "STARTTLS": return SmtpCommandType.StartTls;
				case "AUTH": return SmtpCommandType.Auth;
				default: return SmtpCommandType.Unknown;
			}
		}

		private static int FindCRLF (IBufferedSource source)
		{
			var buffer = source.BufferMemory.Span;
			var offset = source.Offset;
			var count = source.Count;

			int countToCRLF = 0;
			while ((buffer[countToCRLF + offset] != 0x0d) || (buffer[countToCRLF + offset + 1] != 0x0a))
			{
				if (countToCRLF >= (count - 2))
				{
					return -1;
				}

				countToCRLF++;
			}

			return countToCRLF;
		}

		private static SmtpCommand ParseAuthenticationResponse (IBufferedSource source, int countToCRLF)
		{
			// RFC 4953 part 4: If the client wishes to cancel the authentication exchange, it issues a line with a single "*".
			if ((countToCRLF == 1) && source.BufferMemory.Span[source.Offset] == (byte)'*')
			{
				return new SmtpSaslResponseCommand (null);
			}

			byte[] response;
			try
			{
				var responseBase64 = AsciiCharSet.GetString (source.BufferMemory.Span.Slice (source.Offset, countToCRLF));
				response = Convert.FromBase64String (responseBase64);
			}
			catch (FormatException excpt)
			{
				source.SkipBuffer (countToCRLF + 2);
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.SaslResponse, "Unrecognized authentication response." + excpt.Message);
			}

			source.SkipBuffer (countToCRLF + 2);
			return new SmtpSaslResponseCommand (response);
		}

		private static SmtpCommand ParseMailFrom (BytesChunkEnumerator chunkEnumerator)
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

		private static SmtpCommand ParseRcptTo (BytesChunkEnumerator chunkEnumerator)
		{
			var isAngleBracketedValueFound = chunkEnumerator.MoveToNextBracketedValue (0x20, (byte)'<', (byte)'>');
			if (!isAngleBracketedValueFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, "Unrecognized 'RCPT TO' parameter.");
			}

			AddrSpec recepient = null;
			var recipient = chunkEnumerator.GetStringInBrackets ();
			try
			{
#if NETCOREAPP2_1
				recepient = AddrSpec.Parse (recipient);
#else
				recepient = AddrSpec.Parse (recipient.AsSpan ());
#endif
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.RcptTo, FormattableString.Invariant (
					$"Unrecognized 'RCPT TO' parameter '{recipient}'. {excpt}"));
			}

			return new SmtpRcptToCommand (recepient);
		}

		private static SmtpCommand ParseBdat (BytesChunkEnumerator chunkEnumerator, IBufferedSource source)
		{
			// Любая ошибка в обработке BDAT - очень нехороший случай,
			// потому что клиент будет слать данные не дожидаясь ответа об ошибке,
			// а мы не знаем сколько этих данных и будем считать их командами.
			// Поэтому проявляем максимальную толерантность к нарушению формата этой команды.
			var isSizeFound = chunkEnumerator.MoveToNextChunk (0x20, 0x20);
			if (!isSizeFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Bdat, "Missed size parameter in 'BDAT' command.");
			}

			long size;
			bool isLast;
			var sizeStr = chunkEnumerator.GetString ();
			try
			{
				size = long.Parse (
					sizeStr,
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint,
					CultureInfo.InvariantCulture);

				// любые непробельные символы после размера считаем индикатором последней части
				isLast = chunkEnumerator.MoveToNextChunk (0x20, 0x20);
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Bdat, FormattableString.Invariant (
					$"Unrecognized size parameter '{sizeStr}' in 'BDAT' command. {excpt.Message}"));
			}

			return new SmtpBdatCommand (source, size, isLast);
		}

		private static SmtpCommand ParseAuth (BytesChunkEnumerator chunkEnumerator)
		{
			var isParameterFound = chunkEnumerator.MoveToNextChunk (0x20, 0x20);
			if (!isParameterFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, "Missed 'AUTH' mechanism parameter.");
			}

			var mechanism = chunkEnumerator.GetString ();
			byte[] initialResponse = null;
			var isInitialResponseFound = chunkEnumerator.MoveToNextChunk (0x20, 0x0d);
			if (isInitialResponseFound)
			{
				try
				{
					var initialResponseBase64 = chunkEnumerator.GetString ();
					initialResponse = (initialResponseBase64 != "=") ?
						Convert.FromBase64String (initialResponseBase64) :
						Array.Empty<byte> ();
				}
				catch (FormatException excpt)
				{
					return new SmtpInvalidSyntaxCommand (SmtpCommandType.Auth, FormattableString.Invariant (
						$"Unrecognized 'AUTH' initial-response parameter. {excpt.Message}"));
				}
			}

			return new SmtpAuthCommand (mechanism, initialResponse);
		}
	}

#pragma warning disable SA1402 // File may only contain a single class
	internal class SmtpUnknownCommand : SmtpCommand
	{
		internal SmtpUnknownCommand (string message)
			: base (SmtpCommandType.Unknown)
		{
			this.Message = message;
		}

		internal string Message { get; }
	}

	internal class SmtpTooLongCommand : SmtpCommand
	{
		internal SmtpTooLongCommand ()
			: base (SmtpCommandType.Unknown)
		{
		}
	}

	internal class SmtpInvalidSyntaxCommand : SmtpCommand
	{
		internal SmtpInvalidSyntaxCommand (SmtpCommandType commandType, string message)
			: base (commandType)
		{
			this.Message = message;
		}

		internal string Message { get; }
	}

	internal class SmtpHeloCommand : SmtpCommand
	{
		internal SmtpHeloCommand (string clientIdentification)
			: base (SmtpCommandType.Helo)
		{
			this.ClientIdentification = clientIdentification;
		}

		internal string ClientIdentification { get; }

		public override string ToString ()
		{
			return "HELO " + this.ClientIdentification + "\r\n";
		}
	}

	internal class SmtpEhloCommand : SmtpCommand
	{
		internal SmtpEhloCommand (string clientIdentification)
			: base (SmtpCommandType.Ehlo)
		{
			this.ClientIdentification = clientIdentification;
		}

		internal string ClientIdentification { get; }

		public override string ToString ()
		{
			return "EHLO " + this.ClientIdentification + "\r\n";
		}
	}

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

		public override string ToString ()
		{
			var pathStr = this.ReturnPath?.ToAngleString () ?? "<>";
			string result;
			switch (_requestedContentTransferEncoding)
			{
				case ContentTransferEncoding.EightBit:
					result = "MAIL FROM:" + pathStr + " BODY=8BITMIME\r\n";
					break;
				case ContentTransferEncoding.Binary:
					result = "MAIL FROM:" + pathStr + " BODY=BINARYMIME\r\n";
					break;
				default:
					result = "MAIL FROM:" + pathStr + "\r\n";
					break;
			}

			if (this.AssociatedMailbox != null)
			{
				result += " AUTH=" + ((this.AssociatedMailbox != EmptyAddrSpec) ?
					this.AssociatedMailbox.ToAngleString () :
					"<>");
			}

			return result;
		}
	}

	internal class SmtpRcptToCommand : SmtpCommand
	{
		internal SmtpRcptToCommand (AddrSpec recipient)
			: base (SmtpCommandType.RcptTo)
		{
			this.Recipient = recipient;
		}

		internal AddrSpec Recipient { get; }

		public override string ToString ()
		{
			return "RCPT TO:" + this.Recipient.ToAngleString () + "\r\n";
		}
	}

	internal class SmtpVrfyCommand : SmtpCommand
	{
		internal SmtpVrfyCommand (string parameters)
			: base (SmtpCommandType.Vrfy)
		{
			this.Parameters = parameters;
		}

		internal string Parameters { get; }

		public override string ToString ()
		{
			return "VRFY " + this.Parameters + "\r\n";
		}
	}

	internal class SmtpBdatCommand : SmtpCommand
	{
		internal SmtpBdatCommand (IBufferedSource source, long size, bool isLast)
			: base (SmtpCommandType.Bdat)
		{
			this.Source = new SizeLimitedBufferedSource (source, size);
			this.Size = size;
			this.IsLast = isLast;
		}

		internal SizeLimitedBufferedSource Source { get; }

		internal long Size { get; }

		internal bool IsLast { get; }

		public override string ToString ()
		{
			return this.IsLast ?
				FormattableString.Invariant ($"BDAT {this.Size} LAST\r\n") :
				FormattableString.Invariant ($"BDAT {this.Size}\r\n");
		}
	}

	internal class SmtpActualDataCommand : SmtpCommand
	{
		internal SmtpActualDataCommand (IBufferedSource source, bool throwIfEndMarkerNotFound)
			: base (SmtpCommandType.ActualData)
		{
			this.Source = new TemplateSeparatedBufferedSource (
				source,
				new byte[] { 0x0d, 0x0a, (byte)'.', 0x0d, 0x0a },
				throwIfEndMarkerNotFound);
		}

		internal TemplateSeparatedBufferedSource Source { get; }
	}

	internal class SmtpAuthCommand : SmtpCommand
	{
		internal SmtpAuthCommand (string mechanism, byte[] initialResponse)
			: base (SmtpCommandType.Auth)
		{
			this.Mechanism = mechanism;
			this.InitialResponse = initialResponse;
		}

		// список см. http://www.iana.org/assignments/sasl-mechanisms/sasl-mechanisms.xhtml
		internal string Mechanism { get; }

		internal byte[] InitialResponse { get; }

		public override string ToString ()
		{
			return (this.InitialResponse != null) ?
				"AUTH " + this.Mechanism + " " + Convert.ToBase64String (this.InitialResponse) + "\r\n" :
				"AUTH " + this.Mechanism + "\r\n";
		}
	}

	internal class SmtpSaslResponseCommand : SmtpCommand
	{
		internal SmtpSaslResponseCommand (byte[] response)
			: base (SmtpCommandType.SaslResponse)
		{
			this.Response = response;
		}

		internal byte[] Response { get; }

		internal bool IsCancelRequest => this.Response == null;

		public override string ToString ()
		{
			return Convert.ToBase64String (this.Response) + "\r\n";
		}
	}
#pragma warning restore SA1402 // File may only contain a single class
}
