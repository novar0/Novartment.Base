using System;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Smtp
{
	internal static class SmtpCommandTypeHelper
	{
		private static readonly ArrayList<Tuple<string, SmtpCommandType>> nameDictionary = new ArrayList<Tuple<string, SmtpCommandType>>
		{
			Tuple.Create ("DATA", SmtpCommandType.Data),
			Tuple.Create ("NOOP", SmtpCommandType.Noop),
			Tuple.Create ("QUIT", SmtpCommandType.Quit),
			Tuple.Create ("RSET", SmtpCommandType.Rset),
			Tuple.Create ("VRFY", SmtpCommandType.Vrfy),
			Tuple.Create ("EHLO", SmtpCommandType.Ehlo),
			Tuple.Create ("HELO", SmtpCommandType.Helo),
			Tuple.Create ("MAIL FROM", SmtpCommandType.MailFrom),
			Tuple.Create ("RCPT TO", SmtpCommandType.RcptTo),
			Tuple.Create ("BDAT", SmtpCommandType.Bdat),
			Tuple.Create ("STARTTLS", SmtpCommandType.StartTls),
			Tuple.Create ("AUTH", SmtpCommandType.Auth),
		};

		internal static SmtpCommandType Parse (ReadOnlySpan<char> source, ref int pos)
		{
			var firstWordStartPos = pos;
			var firstWordEndPos = source.Slice (pos).IndexOf (' ');
			if (firstWordEndPos < 1)
			{
				firstWordEndPos = source.Length - pos;
			}

			var commandTypeStr = source.Slice (pos, firstWordEndPos);
			pos += firstWordEndPos;

			if (((pos + firstWordEndPos) < source.Length) &&
				("MAIL".AsSpan ().Equals (commandTypeStr, StringComparison.OrdinalIgnoreCase) ||
				"RCPT".AsSpan ().Equals (commandTypeStr, StringComparison.OrdinalIgnoreCase)))
			{
				// если команда из двух слов, то добавляем второе
				// ищем второе слово оканчивающееся на ':'
				var secondWordEndPos = source.Slice (pos + 1).IndexOf (':');
				if (secondWordEndPos < 1)
				{
					return SmtpCommandType.Unknown;
				}

				commandTypeStr = source.Slice (firstWordStartPos, firstWordEndPos + 1 + secondWordEndPos);
				pos += secondWordEndPos + 2;
			}

			foreach (var entry in nameDictionary)
			{
				if (entry.Item1.AsSpan ().Equals (commandTypeStr, StringComparison.OrdinalIgnoreCase))
				{
					return entry.Item2;
				}
			}

			return SmtpCommandType.Unknown;
		}
	}
}
