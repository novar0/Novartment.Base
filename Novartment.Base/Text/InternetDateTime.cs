using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Операции для конвертирования DateTimeOffset в строковое представление согласно RFC 2822 и обратно.
	/// </summary>
	public static class InternetDateTime
	{
		private static readonly uint[] _monthAbbreviations = new uint[]
		{
			'J' + ('a' << 8) + ('n' << 16),
			'F' + ('e' << 8) + ('b' << 16),
			'M' + ('a' << 8) + ('r' << 16),
			'A' + ('p' << 8) + ('r' << 16),
			'M' + ('a' << 8) + ('y' << 16),
			'J' + ('u' << 8) + ('n' << 16),
			'J' + ('u' << 8) + ('l' << 16),
			'A' + ('u' << 8) + ('g' << 16),
			'S' + ('e' << 8) + ('p' << 16),
			'O' + ('c' << 8) + ('t' << 16),
			'N' + ('o' << 8) + ('v' << 16),
			'D' + ('e' << 8) + ('c' << 16),
		};

		/// <summary>
		/// Конвертирует RFC 2822 строковое представление времени в объект типа DateTimeOffset.
		/// </summary>
		/// <param name="value">
		/// Строковое представление времени согласно RFC 2822.
		/// Комментарии (текст в круглых скобках) не допускаются.</param>
		/// <returns>Объект типа DateTimeOffset соответствующий указанному строковому представлению.</returns>
		public static DateTimeOffset Parse (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return Parse (value.AsSpan ());
		}

		/// <summary>
		/// Конвертирует RFC 2822 строковое представление времени в объект типа DateTimeOffset.
		/// </summary>
		/// <param name="source">
		/// Строковое представление времени согласно RFC 2822.
		/// Комментарии (текст в круглых скобках) не допускаются.</param>
		/// <returns>Объект типа DateTimeOffset соответствующий указанному строковому представлению.</returns>
		public static DateTimeOffset Parse (ReadOnlySpan<char> source)
		{
			/*
			date-time   = [ day-of-week "," ] date time [CFWS]
			day-of-week = ([FWS] day-name)
			day-name    = "Mon" / "Tue" / "Wed" / "Thu" / "Fri" / "Sat" / "Sun"
			date        = day month year
			day         = ([FWS] 1*2DIGIT FWS)
			month       = "Jan" / "Feb" / "Mar" / "Apr" / "May" / "Jun" / "Jul" / "Aug" / "Sep" / "Oct" / "Nov" / "Dec"
			year        = (FWS 4*DIGIT FWS)
			time        = time-of-day zone
			time-of-day = hour ":" minute [ ":" second ]
			hour        = 2DIGIT
			minute      = 2DIGIT
			second      = 2DIGIT
			zone        = (FWS ( "+" / "-" ) 4DIGIT)
			*/

			var pos = 0;
			SkipWhiteSpace (source, ref pos);
			var token = ReadNonWhiteSpace (source, ref pos);

			if (token.Length < 1)
			{
				throw new FormatException ("Invalid string representation of data/time. Value is empty.");
			}

			// Skip optional [ day-of-week "," ]
			if (token[token.Length - 1] == ',')
			{
				SkipWhiteSpace (source, ref pos);
				token = ReadNonWhiteSpace (source, ref pos);
			}

			// day
#if NETSTANDARD2_0
			var isValidDay = int.TryParse (new string (token.ToArray ()), out int day);
#else
			var isValidDay = int.TryParse (token, out int day);
#endif
			if (!isValidDay)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid day value.");
			}

			// month
			SkipWhiteSpace (source, ref pos);
			token = ReadNonWhiteSpace (source, ref pos);
			var month = GetMonth (token);

			// year
			SkipWhiteSpace (source, ref pos);
			token = ReadNonWhiteSpace (source, ref pos);
#if NETSTANDARD2_0
			var isValidYear = int.TryParse (new string (token.ToArray ()), out int year);
#else
			var isValidYear = int.TryParse (token, out int year);
#endif
			if (!isValidYear)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid year.");
			}

			// RFC 2822 part 4.3:
			// If a two digit year is encountered whose value is between 00 and 49, the year is interpreted by adding 2000,
			// ending up with a value between 2000 and 2049.
			// If a two digit year is encountered with a value between 50 and 99, or any three digit year is encountered,
			// the year is interpreted by adding 1900.
			if (year < 100)
			{
				if (year < 50)
				{
					year += 2000;
				}
				else
				{
					year += 1900;
				}
			}

			// hour:minute[:seconds]
			SkipWhiteSpace (source, ref pos);
			token = ReadNonWhiteSpace (source, ref pos);
			if ((token.Length < 5) || (token[2] != ':'))
			{
				throw new FormatException ("Invalid string representation of data/time, semicolon not found in time value.");
			}

#if NETSTANDARD2_0
			var isValidHour = int.TryParse (new string (token.ToArray(), 0, 2), out int hour);
#else
			var isValidHour = int.TryParse (token.Slice (0, 2), out int hour);
#endif
			if (!isValidHour)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid hours value.");
			}

#if NETSTANDARD2_0
			var isValidMinute = int.TryParse (new string (token.ToArray (), 3, 2), out int minute);
#else
			var isValidMinute = int.TryParse (token.Slice (3, 2), out int minute);
#endif
			if (!isValidMinute)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid minutes value.");
			}

			int second = 0;
			if (token.Length > 5)
			{
				if ((token.Length < 7) || (token[5] != ':'))
				{
					throw new FormatException ("Invalid string representation of data/time, semicolon not found after minute value.");
				}

#if NETSTANDARD2_0
				var isValidSecond = int.TryParse (new string (token.ToArray (), 6, token.Length - 6), out second);
#else
				var isValidSecond = int.TryParse (token.Slice (6), out second);
#endif
				if (!isValidSecond)
				{
					throw new FormatException ("Invalid string representation of data/time. Invalid seconds value.");
				}
			}

			// timezone
			SkipWhiteSpace (source, ref pos);
			token = ReadNonWhiteSpace (source, ref pos);
			if (token.Length < 1)
			{
				throw new FormatException ("Invalid string representation of data/time. Missed time zone.");
			}

			var timeZoneMinutes = GetTimeZone (token);

			SkipWhiteSpace (source, ref pos);
			if (pos < source.Length)
			{
				throw new FormatException ("Invalid string representation of data/time. Unrecognized tail characters.");
			}

			return new DateTimeOffset (year, month, day, hour, minute, second, new TimeSpan (0, timeZoneMinutes, 0));
		}

		/// <summary>
		/// Конвертирует DateTimeOffset в RFC 2822 строковое представление времени.
		/// </summary>
		/// <param name="dateTime">DateTimeOffset для конвертирования в строковое представление.</param>
		/// <returns>RFC 2822 строковое представление времени.</returns>
		public static string ToInternetString (this DateTimeOffset dateTime)
		{
			if (((int)dateTime.Offset.TotalHours < -12) || ((int)dateTime.Offset.TotalHours) > 12)
			{
				throw new ArgumentOutOfRangeException (nameof (dateTime));
			}

			Contract.EndContractBlock ();

			var buf = new char[26];
			ToInternetString (dateTime, buf.AsSpan ());
			return new string (buf);
		}

		/// <summary>
		/// Конвертирует DateTimeOffset в RFC 2822 строковое представление времени.
		/// </summary>
		/// <param name="dateTime">DateTimeOffset для конвертирования в строковое представление.</param>
		/// <param name="buf">Буфер, куда будет записано строковое представление.</param>
		/// <returns>Количество знаков, записанных в буфер.</returns>
		public static int ToInternetString (this DateTimeOffset dateTime, Span<char> buf)
		{
			if (((int)dateTime.Offset.TotalHours < -12) || ((int)dateTime.Offset.TotalHours) > 12)
			{
				throw new ArgumentOutOfRangeException (nameof (dateTime));
			}

			Contract.EndContractBlock ();

			var value = dateTime.DateTime;
			var offset = dateTime.Offset;

			WriteTwoDecimalDigits ((uint)value.Day, buf, 0);
			buf[2] = ' ';

			uint monthAbbrev = _monthAbbreviations[value.Month - 1];
			buf[3] = (char)(byte)monthAbbrev;
			monthAbbrev >>= 8;
			buf[4] = (char)(byte)monthAbbrev;
			monthAbbrev >>= 8;
			buf[5] = (char)(byte)monthAbbrev;
			buf[6] = ' ';

			WriteFourDecimalDigits ((uint)value.Year, buf, 7);
			buf[11] = ' ';

			WriteTwoDecimalDigits ((uint)value.Hour, buf, 12);
			buf[14] = ':';

			WriteTwoDecimalDigits ((uint)value.Minute, buf, 15);
			buf[17] = ':';

			WriteTwoDecimalDigits ((uint)value.Second, buf, 18);
			buf[20] = ' ';

			char sign;

			if (offset < default (TimeSpan))
			{
				sign = '-';
				offset = TimeSpan.FromTicks (-offset.Ticks);
			}
			else
			{
				sign = '+';
			}

			buf[21] = sign;
			WriteTwoDecimalDigits ((uint)offset.Hours, buf, 22);
			WriteTwoDecimalDigits ((uint)offset.Minutes, buf, 24);

			return 26;
		}

		/// <summary>
		/// Конвертирует DateTimeOffset в RFC 2822 строковое представление времени.
		/// </summary>
		/// <param name="dateTime">DateTimeOffset для конвертирования в строковое представление.</param>
		/// <param name="buf">Буфер, куда будет записано строковое представление.</param>
		/// <returns>Количество знаков, записанных в буфер.</returns>
		public static int ToInternetUtf8String (this DateTimeOffset dateTime, Span<byte> buf)
		{
			if (((int)dateTime.Offset.TotalHours < -12) || ((int)dateTime.Offset.TotalHours) > 12)
			{
				throw new ArgumentOutOfRangeException (nameof (dateTime));
			}

			Contract.EndContractBlock ();

			var value = dateTime.DateTime;
			var offset = dateTime.Offset;

			WriteTwoDecimalDigits ((uint)value.Day, buf, 0);
			buf[2] = (byte)' ';

			uint monthAbbrev = _monthAbbreviations[value.Month - 1];
			buf[3] = (byte)monthAbbrev;
			monthAbbrev >>= 8;
			buf[4] = (byte)monthAbbrev;
			monthAbbrev >>= 8;
			buf[5] = (byte)monthAbbrev;
			buf[6] = (byte)' ';

			WriteFourDecimalDigits ((uint)value.Year, buf, 7);
			buf[11] = (byte)' ';

			WriteTwoDecimalDigits ((uint)value.Hour, buf, 12);
			buf[14] = (byte)':';

			WriteTwoDecimalDigits ((uint)value.Minute, buf, 15);
			buf[17] = (byte)':';

			WriteTwoDecimalDigits ((uint)value.Second, buf, 18);
			buf[20] = (byte)' ';

			byte sign;

			if (offset < default (TimeSpan))
			{
				sign = (byte)'-';
				offset = TimeSpan.FromTicks (-offset.Ticks);
			}
			else
			{
				sign = (byte)'+';
			}

			buf[21] = sign;
			WriteTwoDecimalDigits ((uint)offset.Hours, buf, 22);
			WriteTwoDecimalDigits ((uint)offset.Minutes, buf, 24);

			return 26;
		}

		private static void WriteTwoDecimalDigits (uint value, Span<byte> buf, int pos)
		{
			uint temp = '0' + value;
			value /= 10;
			buf[pos + 1] = (byte)(temp - (value * 10));
			buf[pos] = (byte)('0' + value);
		}

		private static void WriteTwoDecimalDigits (uint value, Span<char> buf, int pos)
		{
			uint temp = '0' + value;
			value /= 10;
			buf[pos + 1] = (char)(byte)(temp - (value * 10));
			buf[pos] = (char)(byte)('0' + value);
		}

		private static void WriteFourDecimalDigits (uint value, Span<byte> buf, int pos)
		{
			uint temp = '0' + value;
			value /= 10;
			buf[pos + 3] = (byte)(temp - (value * 10));
			temp = '0' + value;
			value /= 10;
			buf[pos + 2] = (byte)(temp - (value * 10));
			temp = '0' + value;
			value /= 10;
			buf[pos + 1] = (byte)(temp - (value * 10));
			buf[pos] = (byte)('0' + value);
		}

		private static void WriteFourDecimalDigits (uint value, Span<char> buf, int pos)
		{
			uint temp = '0' + value;
			value /= 10;
			buf[pos + 3] = (char)(byte)(temp - (value * 10));
			temp = '0' + value;
			value /= 10;
			buf[pos + 2] = (char)(byte)(temp - (value * 10));
			temp = '0' + value;
			value /= 10;
			buf[pos + 1] = (char)(byte)(temp - (value * 10));
			buf[pos] = (char)(byte)('0' + value);
		}

		/// <summary>
		/// Пропускает пробельное пространство.
		/// </summary>
		private static int SkipWhiteSpace (ReadOnlySpan<char> source, ref int pos)
		{
			var asciiClasses = AsciiCharSet.Classes.Span;
			while (pos < source.Length)
			{
				var character = source[pos];

				if ((character >= asciiClasses.Length) || ((asciiClasses[character] & AsciiCharClasses.WhiteSpace) == 0))
				{
					break;
				}

				pos++;
			}

			return pos;
		}

		/// <summary>
		/// Выделяет подстроку, состоящую из любых символов, кроме символов указанного типа.
		/// </summary>
		private static ReadOnlySpan<char> ReadNonWhiteSpace (ReadOnlySpan<char> source, ref int pos)
		{
			var startPos = pos;
			var asciiClasses = AsciiCharSet.Classes.Span;
			while (pos < source.Length)
			{
				var character = source[pos];

				if ((character < asciiClasses.Length) && ((asciiClasses[character] & AsciiCharClasses.WhiteSpace) != 0))
				{
					break;
				}

				pos++;
			}

			return source.Slice (startPos, pos - startPos);
		}

		private static int GetMonth (ReadOnlySpan<char> monthSpan)
		{
#if NETSTANDARD2_0
			var monthStr = new string (monthSpan.ToArray ());
#else
			var monthStr = new string (monthSpan);
#endif
			var month = (monthStr.ToUpperInvariant ()) switch
			{
				"JAN" => 1,
				"FEB" => 2,
				"MAR" => 3,
				"APR" => 4,
				"MAY" => 5,
				"JUN" => 6,
				"JUL" => 7,
				"AUG" => 8,
				"SEP" => 9,
				"OCT" => 10,
				"NOV" => 11,
				"DEC" => 12,
				_ => throw new FormatException ("Invalid string representation of data/time. Invalid month '" + monthStr + "'."),
			};
			return month;
		}

		private static int GetTimeZone (ReadOnlySpan<char> timeZoneSpan)
		{
			int timeZoneMinutes;

			if ((timeZoneSpan[0] == '+') || (timeZoneSpan[0] == '-'))
			{
#if NETSTANDARD2_0
				var arr = timeZoneSpan.ToArray ();
				var n1str = new string (arr, 1, 2);
				var n2str = new string (arr, 3, 2);
#else
				var n1str = timeZoneSpan.Slice (1, 2);
				var n2str = timeZoneSpan.Slice (3, 2);
#endif
				timeZoneMinutes = (int.Parse (n1str, NumberStyles.None, CultureInfo.InvariantCulture) * 60) +
					int.Parse (n2str, NumberStyles.None, CultureInfo.InvariantCulture);
				if (timeZoneSpan[0] == '-')
				{
					timeZoneMinutes = -timeZoneMinutes;
				}

				return timeZoneMinutes;
			}

			// We have RFC 822 date with abbrevated time zone name. For example: GMT.
#if NETSTANDARD2_0
			var timeZoneStr = new string (timeZoneSpan.ToArray ()).ToUpperInvariant ();
#else
			var timeZoneStr = new string (timeZoneSpan).ToUpperInvariant ();
#endif
			timeZoneMinutes = timeZoneStr switch
			{
				"A" => ((01 * 60) + 00),
				"ACDT" => ((10 * 60) + 30),
				"ACST" => ((09 * 60) + 30),
				"ADT" => -((03 * 60) + 00),
				"AEDT" => ((11 * 60) + 00),
				"AEST" => ((10 * 60) + 00),
				"AKDT" => -((08 * 60) + 00),
				"AKST" => -((09 * 60) + 00),
				"AST" => -((04 * 60) + 00),
				"AWDT" => ((09 * 60) + 00),
				"AWST" => ((08 * 60) + 00),
				"B" => ((02 * 60) + 00),
				"BST" => ((01 * 60) + 00),
				"C" => ((03 * 60) + 00),
				"CDT" => -((05 * 60) + 00),
				"CEDT" => ((02 * 60) + 00),
				"CEST" => ((02 * 60) + 00),
				"CET" => ((01 * 60) + 00),
				"CST" => -((06 * 60) + 00),
				"CXT" => ((01 * 60) + 00),
				"D" => ((04 * 60) + 00),
				"E" => ((05 * 60) + 00),
				"EDT" => -((04 * 60) + 00),
				"EEDT" => ((03 * 60) + 00),
				"EEST" => ((03 * 60) + 00),
				"EET" => ((02 * 60) + 00),
				"EST" => -((05 * 60) + 00),
				"F" => ((06 * 60) + 00),
				"G" => ((07 * 60) + 00),
				"GMT" => 0000,
				"H" => ((08 * 60) + 00),
				"I" => ((09 * 60) + 00),
				"IST" => ((01 * 60) + 00),
				"K" => ((10 * 60) + 00),
				"L" => ((11 * 60) + 00),
				"M" => ((12 * 60) + 00),
				"MDT" => -((06 * 60) + 00),
				"MST" => -((07 * 60) + 00),
				"N" => -((01 * 60) + 00),
				"NDT" => -((02 * 60) + 30),
				"NFT" => ((11 * 60) + 30),
				"NST" => -((03 * 60) + 30),
				"O" => -((02 * 60) + 00),
				"P" => -((03 * 60) + 00),
				"PDT" => -((07 * 60) + 00),
				"PST" => -((08 * 60) + 00),
				"Q" => -((04 * 60) + 00),
				"R" => -((05 * 60) + 00),
				"S" => -((06 * 60) + 00),
				"T" => -((07 * 60) + 00),
				"U" => -((08 * 60) + 00),
				"UTC" => 0000,
				"V" => -((09 * 60) + 00),
				"W" => -((10 * 60) + 00),
				"WEDT" => ((01 * 60) + 00),
				"WEST" => ((01 * 60) + 00),
				"WET" => 0000,
				"WST" => ((08 * 60) + 00),
				"X" => -((11 * 60) + 00),
				"Y" => -((12 * 60) + 00),
				"Z" => 0000,
				_ => throw new FormatException ("Invalid string representation of data/time. Invalid time zone '" + timeZoneStr + "'."),
			};
			return timeZoneMinutes;
		}
	}
}
