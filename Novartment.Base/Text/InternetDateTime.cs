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
#if NETCOREAPP2_1
			var isValidDay = int.TryParse (token, out int day);
#else
			var isValidDay = int.TryParse (new string (token.ToArray ()), out int day);
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
#if NETCOREAPP2_1
			var isValidYear = int.TryParse (token, out int year);
#else
			var isValidYear = int.TryParse (new string (token.ToArray ()), out int year);
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

#if NETCOREAPP2_1
			var isValidHour = int.TryParse (token.Slice (0, 2), out int hour);
#else
			var isValidHour = int.TryParse (new string (token.ToArray(), 0, 2), out int hour);
#endif
			if (!isValidHour)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid hours value.");
			}

#if NETCOREAPP2_1
			var isValidMinute = int.TryParse (token.Slice (3, 2), out int minute);
#else
			var isValidMinute = int.TryParse (new string (token.ToArray (), 3, 2), out int minute);
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

#if NETCOREAPP2_1
				var isValidSecond = int.TryParse (token.Slice (6), out second);
#else
				var isValidSecond = int.TryParse (new string (token.ToArray (), 6, token.Length - 6), out second);
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

			var str1 = dateTime.ToString ("dd MMM yyyy HH':'mm':'ss ", DateTimeFormatInfo.InvariantInfo);
			var str2 = dateTime.Offset.TotalHours.ToString ("+00;-00", CultureInfo.InvariantCulture);
			var str3 = Math.Abs (dateTime.Offset.Minutes).ToString ("00", CultureInfo.InvariantCulture);
			return str1 + str2 + str3;
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

			var outPos = 0;
			int charsWritten;
#if NETCOREAPP2_1
			dateTime.TryFormat (buf, out charsWritten, "dd MMM yyyy HH':'mm':'ss", DateTimeFormatInfo.InvariantInfo);
#else
			var tempStr = dateTime.ToString ("dd MMM yyyy HH':'mm':'ss", DateTimeFormatInfo.InvariantInfo);
			tempStr.AsSpan ().CopyTo (buf);
			charsWritten = tempStr.Length;
#endif
			outPos += charsWritten;
			buf[outPos++] = ' ';
#if NETCOREAPP2_1
			dateTime.Offset.TotalHours.TryFormat (buf.Slice (outPos), out charsWritten, "+00;-00", CultureInfo.InvariantCulture);
#else
			var tempStr2 = dateTime.Offset.TotalHours.ToString ("+00;-00", CultureInfo.InvariantCulture);
			tempStr2.AsSpan ().CopyTo (buf.Slice (outPos));
			charsWritten = tempStr2.Length;
#endif
			outPos += charsWritten;
#if NETCOREAPP2_1
			Math.Abs (dateTime.Offset.Minutes).TryFormat (buf.Slice (outPos), out charsWritten, "00", CultureInfo.InvariantCulture);
#else
			var tempStr3 = Math.Abs (dateTime.Offset.Minutes).ToString ("00", CultureInfo.InvariantCulture);
			tempStr3.AsSpan ().CopyTo (buf.Slice (outPos));
			charsWritten = tempStr3.Length;
#endif
			outPos += charsWritten;
			return outPos;
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

			DateTimeKind kind = DateTimeKind.Local;

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

		/// <summary>
		/// Пропускает пробельное пространство.
		/// </summary>
		private static int SkipWhiteSpace (ReadOnlySpan<char> source, ref int pos)
		{
			while (pos < source.Length)
			{
				var character = source[pos];

				if ((character >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[character] & (short)AsciiCharClasses.WhiteSpace) == 0))
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
			while (pos < source.Length)
			{
				var character = source[pos];

				if ((character < AsciiCharSet.Classes.Count) && ((AsciiCharSet.Classes[character] & (short)AsciiCharClasses.WhiteSpace) != 0))
				{
					break;
				}

				pos++;
			}

			return source.Slice (startPos, pos - startPos);
		}

		private static int GetMonth (ReadOnlySpan<char> monthSpan)
		{
			int month;
#if NETCOREAPP2_1
			var monthStr = new string (monthSpan);
#else
			var monthStr = new string (monthSpan.ToArray ());
#endif
			switch (monthStr.ToUpperInvariant ())
			{
				case "JAN": month = 1; break;
				case "FEB": month = 2; break;
				case "MAR": month = 3; break;
				case "APR": month = 4; break;
				case "MAY": month = 5; break;
				case "JUN": month = 6; break;
				case "JUL": month = 7; break;
				case "AUG": month = 8; break;
				case "SEP": month = 9; break;
				case "OCT": month = 10; break;
				case "NOV": month = 11; break;
				case "DEC": month = 12; break;
				default: throw new FormatException ("Invalid string representation of data/time. Invalid month '" + monthStr + "'.");
			}

			return month;
		}

		private static int GetTimeZone (ReadOnlySpan<char> timeZoneSpan)
		{
			int timeZoneMinutes;

			if ((timeZoneSpan[0] == '+') || (timeZoneSpan[0] == '-'))
			{
#if NETCOREAPP2_1
				var n1str = timeZoneSpan.Slice (1, 2);
				var n2str = timeZoneSpan.Slice (3, 2);
#else
				var arr = timeZoneSpan.ToArray ();
				var n1str = new string (arr, 1, 2);
				var n2str = new string (arr, 3, 2);
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
#if NETCOREAPP2_1
			var timeZoneStr = new string (timeZoneSpan).ToUpperInvariant ();
#else
			var timeZoneStr = new string (timeZoneSpan.ToArray ()).ToUpperInvariant ();
#endif
			switch (timeZoneStr)
			{
#pragma warning disable SA1119 // Statement must not use unnecessary parenthesis
				case "A": timeZoneMinutes = ((01 * 60) + 00); break; // Alpha Time Zone (military).
				case "ACDT": timeZoneMinutes = ((10 * 60) + 30); break; // Australian Central Daylight Time.
				case "ACST": timeZoneMinutes = ((09 * 60) + 30); break; // Australian Central Standard Time.
				case "ADT": timeZoneMinutes = -((03 * 60) + 00); break; // Atlantic Daylight Time.
				case "AEDT": timeZoneMinutes = ((11 * 60) + 00); break; // Australian Eastern Daylight Time.
				case "AEST": timeZoneMinutes = ((10 * 60) + 00); break; // Australian Eastern Standard Time.
				case "AKDT": timeZoneMinutes = -((08 * 60) + 00); break; // Alaska Daylight Time.
				case "AKST": timeZoneMinutes = -((09 * 60) + 00); break; // Alaska Standard Time.
				case "AST": timeZoneMinutes = -((04 * 60) + 00); break; // Atlantic Standard Time.
				case "AWDT": timeZoneMinutes = ((09 * 60) + 00); break; // Australian Western Daylight Time.
				case "AWST": timeZoneMinutes = ((08 * 60) + 00); break; // Australian Western Standard Time.
				case "B": timeZoneMinutes = ((02 * 60) + 00); break; // Bravo Time Zone (millitary).
				case "BST": timeZoneMinutes = ((01 * 60) + 00); break; // British Summer Time.
				case "C": timeZoneMinutes = ((03 * 60) + 00); break; // Charlie Time Zone (millitary).
				case "CDT": timeZoneMinutes = -((05 * 60) + 00); break; // Central Daylight Time.
				case "CEDT": timeZoneMinutes = ((02 * 60) + 00); break; // Central European Daylight Time.
				case "CEST": timeZoneMinutes = ((02 * 60) + 00); break; // Central European Summer Time.
				case "CET": timeZoneMinutes = ((01 * 60) + 00); break; // Central European Time.
				case "CST": timeZoneMinutes = -((06 * 60) + 00); break; // Central Standard Time.
				case "CXT": timeZoneMinutes = ((01 * 60) + 00); break; // Christmas Island Time.
				case "D": timeZoneMinutes = ((04 * 60) + 00); break; // Delta Time Zone (military).
				case "E": timeZoneMinutes = ((05 * 60) + 00); break; // Echo Time Zone (military).
				case "EDT": timeZoneMinutes = -((04 * 60) + 00); break; // Eastern Daylight Time.
				case "EEDT": timeZoneMinutes = ((03 * 60) + 00); break; // Eastern European Daylight Time.
				case "EEST": timeZoneMinutes = ((03 * 60) + 00); break; // Eastern European Summer Time.
				case "EET": timeZoneMinutes = ((02 * 60) + 00); break; // Eastern European Time.
				case "EST": timeZoneMinutes = -((05 * 60) + 00); break; // Eastern Standard Time.
				case "F": timeZoneMinutes = ((06 * 60) + 00); break; // Foxtrot Time Zone (military).
				case "G": timeZoneMinutes = ((07 * 60) + 00); break; // Golf Time Zone (military).
				case "GMT": timeZoneMinutes = 0000; break; // Greenwich Mean Time.
				case "H": timeZoneMinutes = ((08 * 60) + 00); break; // Hotel Time Zone (military).
				case "I": timeZoneMinutes = ((09 * 60) + 00); break; // India Time Zone (military).
				case "IST": timeZoneMinutes = ((01 * 60) + 00); break; // Irish Summer Time.
				case "K": timeZoneMinutes = ((10 * 60) + 00); break; // Kilo Time Zone (millitary).
				case "L": timeZoneMinutes = ((11 * 60) + 00); break; // Lima Time Zone (millitary).
				case "M": timeZoneMinutes = ((12 * 60) + 00); break; // Mike Time Zone (millitary).
				case "MDT": timeZoneMinutes = -((06 * 60) + 00); break; // Mountain Daylight Time.
				case "MST": timeZoneMinutes = -((07 * 60) + 00); break; // Mountain Standard Time.
				case "N": timeZoneMinutes = -((01 * 60) + 00); break; // November Time Zone (military).
				case "NDT": timeZoneMinutes = -((02 * 60) + 30); break; // Newfoundland Daylight Time.
				case "NFT": timeZoneMinutes = ((11 * 60) + 30); break; // Norfolk (Island) Time.
				case "NST": timeZoneMinutes = -((03 * 60) + 30); break; // Newfoundland Standard Time.
				case "O": timeZoneMinutes = -((02 * 60) + 00); break; // Oscar Time Zone (military).
				case "P": timeZoneMinutes = -((03 * 60) + 00); break; // Papa Time Zone (military).
				case "PDT": timeZoneMinutes = -((07 * 60) + 00); break; // Pacific Daylight Time.
				case "PST": timeZoneMinutes = -((08 * 60) + 00); break; // Pacific Standard Time.
				case "Q": timeZoneMinutes = -((04 * 60) + 00); break; // Quebec Time Zone (military).
				case "R": timeZoneMinutes = -((05 * 60) + 00); break; // Romeo Time Zone (military).
				case "S": timeZoneMinutes = -((06 * 60) + 00); break; // Sierra Time Zone (military).
				case "T": timeZoneMinutes = -((07 * 60) + 00); break; // Tango Time Zone (military).
				case "U": timeZoneMinutes = -((08 * 60) + 00); break; // Uniform Time Zone (military).
				case "UTC": timeZoneMinutes = 0000; break; // Coordinated Universal Time.
				case "V": timeZoneMinutes = -((09 * 60) + 00); break; // Victor Time Zone (militray).
				case "W": timeZoneMinutes = -((10 * 60) + 00); break; // Whiskey Time Zone (military).
				case "WEDT": timeZoneMinutes = ((01 * 60) + 00); break; // Western European Daylight Time.
				case "WEST": timeZoneMinutes = ((01 * 60) + 00); break; // Western European Summer Time.
				case "WET": timeZoneMinutes = 0000; break; // Western European Time.
				case "WST": timeZoneMinutes = ((08 * 60) + 00); break; // Western Standard Time.
				case "X": timeZoneMinutes = -((11 * 60) + 00); break; // X-ray Time Zone (military).
				case "Y": timeZoneMinutes = -((12 * 60) + 00); break; // Yankee Time Zone (military).
				case "Z": timeZoneMinutes = 0000; break; // Zulu Time Zone (military).
#pragma warning restore SA1119 // Statement must not use unnecessary parenthesis
				default: throw new FormatException ("Invalid string representation of data/time. Invalid time zone '" + timeZoneStr + "'.");
			}

			return timeZoneMinutes;
		}
	}
}
