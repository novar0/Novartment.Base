using System;
using Microsoft.Extensions.Logging;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpCommand
	{
		internal static readonly SmtpCommand CachedCmdUnknown = new SmtpCommand (SmtpCommandType.Unknown);
		internal static readonly SmtpCommand CachedCmdNoCommand = new SmtpCommand (SmtpCommandType.NoCommand);
		internal static readonly SmtpCommand CachedCmdData = new SmtpCommand (SmtpCommandType.Data);
		internal static readonly SmtpCommand CachedCmdNoop = new SmtpCommand (SmtpCommandType.Noop);
		internal static readonly SmtpCommand CachedCmdQuit = new SmtpCommand (SmtpCommandType.Quit);
		internal static readonly SmtpCommand CachedCmdRset = new SmtpCommand (SmtpCommandType.Rset);
		internal static readonly SmtpCommand CachedCmdStartTls = new SmtpCommand (SmtpCommandType.StartTls);

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
			// остальные команды сами переопределяют метод ToString()
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
				var authResponseCmd = SmtpSaslResponseCommand.Parse (source.BufferMemory.Span.Slice (source.Offset, countToCRLF));
				source.SkipBuffer (countToCRLF + 2);
				return authResponseCmd;
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
				var commandType = SmtpCommandTypeHelper.Parse (chunkEnumerator);
				switch (commandType)
				{
					case SmtpCommandType.Data: // data = "DATA" CRLF
						result = CachedCmdData;
						break;
					case SmtpCommandType.Noop: // noop = "NOOP" [ SP String ] CRLF
						result = CachedCmdNoop;
						break;
					case SmtpCommandType.Quit: // quit = "QUIT" CRLF
						result = CachedCmdQuit;
						break;
					case SmtpCommandType.Rset: // rset = "RSET" CRLF
						result = CachedCmdRset;
						break;
					case SmtpCommandType.StartTls:
						result = CachedCmdStartTls;
						break;
					case SmtpCommandType.Vrfy: // vrfy = "VRFY" SP String CRLF
						result = SmtpVrfyCommand.Parse (chunkEnumerator);
						break;
					case SmtpCommandType.Ehlo: // ehlo = "EHLO" SP ( Domain / address-literal ) CRLF
						result = SmtpEhloCommand.Parse (chunkEnumerator);
						break;
					case SmtpCommandType.Helo: // helo = "HELO" SP Domain CRLF
						result = SmtpHeloCommand.Parse (chunkEnumerator);
						break;
					case SmtpCommandType.MailFrom: // MAIL FROM:<reverse-path> [SP <mail-parameters> ] <CRLF> ; Reverse-path   = "<" Mailbox ">" / "<>"
						result = SmtpMailFromCommand.Parse (chunkEnumerator);
						break;
					case SmtpCommandType.RcptTo: // RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF> ; Forward-path   = "<" Mailbox ">"
						result = SmtpRcptToCommand.Parse (chunkEnumerator);
						break;
					case SmtpCommandType.Bdat: // bdat-cmd = "BDAT" SP chunk-size [ SP end-marker ] CR LF
						source.SkipBuffer (countToCRLF);
						countToCRLF = 0;
						result = SmtpBdatCommand.Parse (chunkEnumerator, source);
						break;
					case SmtpCommandType.Auth:
						result = SmtpAuthCommand.Parse (chunkEnumerator);
						break;
					default:
						result = CachedCmdUnknown;
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
	}
}
