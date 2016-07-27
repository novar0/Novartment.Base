using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Операции для конвертирования DateTimeOffset в строковое представление согласно RFC 2822 и обратно.
	/// </summary>
	public static class InternetDateTime
	{
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

			var tokens = value.Split (null as char[], StringSplitOptions.RemoveEmptyEntries);

			// date-time   = [ day-of-week "," ] date time [CFWS]
			// day-of-week = ([FWS] day-name)
			// day-name    = "Mon" / "Tue" / "Wed" / "Thu" / "Fri" / "Sat" / "Sun"
			// date        = day month year
			// day         = ([FWS] 1*2DIGIT FWS)
			// month       = "Jan" / "Feb" / "Mar" / "Apr" / "May" / "Jun" / "Jul" / "Aug" / "Sep" / "Oct" / "Nov" / "Dec"
			// year        = (FWS 4*DIGIT FWS)
			// time        = time-of-day zone
			// time-of-day = hour ":" minute [ ":" second ]
			// hour        = 2DIGIT
			// minute      = 2DIGIT
			// second      = 2DIGIT
			// zone        = (FWS ( "+" / "-" ) 4DIGIT)

			if (tokens.Length < 1)
			{
				throw new FormatException ("Invalid string representation of data/time. Too little parts.");
			}
			var idx = 0;
			// Skip optional [ day-of-week "," ]
			if (tokens[idx][tokens[idx].Length - 1] == ',')
			{
				idx++;
			}
			// day
			int day = 0;
			var isValidDay = (idx < tokens.Length) && Int32.TryParse (tokens[idx], out day);
			if (!isValidDay)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid day '" + tokens[idx] + "'.");
			}
			idx++;
			// month
			if (idx >= tokens.Length)
			{
				throw new FormatException ("Invalid string representation of data/time. Too little parts.");
			}
			var month = GetMonth (tokens[idx]);
			idx++;
			// year
			int year = 0;
			var isValidYear = (idx < tokens.Length) && Int32.TryParse (tokens[idx], out year);
			if (!isValidYear)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid year '" + tokens[idx] + "'.");
			}
			// RFC 2822 part 4.3:
			// If a two digit year is encountered whose value is between 00 and 49, the year is interpreted by adding 2000,
			// ending up with a value between 2000 and 2049.
			// If a two digit year is encountered with a value between 50 and 99, or any three digit year is encountered,
			// the year is interpreted by adding 1900.
			if (tokens[idx].Length < 4)
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
			idx++;

			if (idx >= tokens.Length)
			{
				throw new FormatException ("Invalid string representation of data/time. Too little parts.");
			}
			var tkns = tokens[idx].Split (':');
			// hour:minute[:seconds]
			int hour = 0;
			int minute = 0;
			int second = 0;
			var isValidHourMinute = (tkns.Length >= 2) &&
				(tkns.Length <= 3) &&
				Int32.TryParse (tkns[0], out hour) &&
				Int32.TryParse (tkns[1], out minute);
			if (!isValidHourMinute)
			{
				throw new FormatException ("Invalid string representation of data/time. Invalid hour '" + tkns[0] + "' or minute '" + tkns[1] + "'.");
			}
			if (tkns.Length == 3)
			{
				var isValidSecond = Int32.TryParse (tkns[2], out second);
				if (!isValidSecond)
				{
					throw new FormatException ("Invalid string representation of data/time. Invalid second '" + tkns[2] + "'.");
				}
			}
			idx++;

			// timezone
			if (idx >= tokens.Length)
			{
				throw new FormatException ("Invalid string representation of data/time. Too little parts.");
			}
			var timeZoneMinutes = GetTimeZone (tokens[idx]);

			return new DateTimeOffset (year, month, day, hour, minute, second, new TimeSpan (0, timeZoneMinutes, 0));
		}

		[SuppressMessage ("Microsoft.Maintainability",
			"CA1502:AvoidExcessiveComplexity",
			Justification = "Method not too complex.")]
		private static int GetMonth (string monthStr)
		{
			int month;
			switch (monthStr.ToUpper ())
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

		[SuppressMessage ("Microsoft.Maintainability",
			"CA1505:AvoidUnmaintainableCode",
			Justification = "Method not too complex."),
		SuppressMessage ("Microsoft.Maintainability",
			"CA1502:AvoidExcessiveComplexity",
			Justification = "Method not too complex.")]
		private static int GetTimeZone (string timeZoneStr)
		{
			int timeZoneMinutes;
			if ((timeZoneStr[0] == '+') || (timeZoneStr[0] == '-'))
			{
				timeZoneMinutes = Int32.Parse (timeZoneStr.Substring (1, 2), CultureInfo.InvariantCulture) * 60 +
					Int32.Parse (timeZoneStr.Substring (3, 2), CultureInfo.InvariantCulture);
				if (timeZoneStr[0] == '-')
				{
					timeZoneMinutes = -timeZoneMinutes;
				}
			}
			// We have RFC 822 date with abbrevated time zone name. For example: GMT.
			else
			{
				#region time zones

				switch (timeZoneStr.ToUpper ())
				{
					case "A": timeZoneMinutes = ((01 * 60) + 00); break;// Alpha Time Zone (military).
					case "ACDT": timeZoneMinutes = ((10 * 60) + 30); break;// Australian Central Daylight Time.
					case "ACST": timeZoneMinutes = ((09 * 60) + 30); break;// Australian Central Standard Time.
					case "ADT": timeZoneMinutes = -((03 * 60) + 00); break;// Atlantic Daylight Time.
					case "AEDT": timeZoneMinutes = ((11 * 60) + 00); break;// Australian Eastern Daylight Time.
					case "AEST": timeZoneMinutes = ((10 * 60) + 00); break;// Australian Eastern Standard Time.
					case "AKDT": timeZoneMinutes = -((08 * 60) + 00); break;// Alaska Daylight Time.
					case "AKST": timeZoneMinutes = -((09 * 60) + 00); break;// Alaska Standard Time.
					case "AST": timeZoneMinutes = -((04 * 60) + 00); break;// Atlantic Standard Time.
					case "AWDT": timeZoneMinutes = ((09 * 60) + 00); break;// Australian Western Daylight Time.
					case "AWST": timeZoneMinutes = ((08 * 60) + 00); break;// Australian Western Standard Time.
					case "B": timeZoneMinutes = ((02 * 60) + 00); break;// Bravo Time Zone (millitary).
					case "BST": timeZoneMinutes = ((01 * 60) + 00); break;// British Summer Time.
					case "C": timeZoneMinutes = ((03 * 60) + 00); break;// Charlie Time Zone (millitary).
					case "CDT": timeZoneMinutes = -((05 * 60) + 00); break;// Central Daylight Time.
					case "CEDT": timeZoneMinutes = ((02 * 60) + 00); break;// Central European Daylight Time.
					case "CEST": timeZoneMinutes = ((02 * 60) + 00); break;// Central European Summer Time.
					case "CET": timeZoneMinutes = ((01 * 60) + 00); break;// Central European Time.
					case "CST": timeZoneMinutes = -((06 * 60) + 00); break;// Central Standard Time.
					case "CXT": timeZoneMinutes = ((01 * 60) + 00); break;// Christmas Island Time.
					case "D": timeZoneMinutes = ((04 * 60) + 00); break;// Delta Time Zone (military).
					case "E": timeZoneMinutes = ((05 * 60) + 00); break;// Echo Time Zone (military).
					case "EDT": timeZoneMinutes = -((04 * 60) + 00); break;// Eastern Daylight Time.
					case "EEDT": timeZoneMinutes = ((03 * 60) + 00); break;// Eastern European Daylight Time.
					case "EEST": timeZoneMinutes = ((03 * 60) + 00); break;// Eastern European Summer Time.
					case "EET": timeZoneMinutes = ((02 * 60) + 00); break;// Eastern European Time.
					case "EST": timeZoneMinutes = -((05 * 60) + 00); break;// Eastern Standard Time.
					case "F": timeZoneMinutes = (06 * 60 + 00); break;// Foxtrot Time Zone (military).
					case "G": timeZoneMinutes = ((07 * 60) + 00); break;// Golf Time Zone (military).
					case "GMT": timeZoneMinutes = 0000; break;// Greenwich Mean Time.
					case "H": timeZoneMinutes = ((08 * 60) + 00); break;// Hotel Time Zone (military).
					case "I": timeZoneMinutes = ((09 * 60) + 00); break;// India Time Zone (military).
					case "IST": timeZoneMinutes = ((01 * 60) + 00); break;// Irish Summer Time.
					case "K": timeZoneMinutes = ((10 * 60) + 00); break;// Kilo Time Zone (millitary).
					case "L": timeZoneMinutes = ((11 * 60) + 00); break;// Lima Time Zone (millitary).
					case "M": timeZoneMinutes = ((12 * 60) + 00); break;// Mike Time Zone (millitary).
					case "MDT": timeZoneMinutes = -((06 * 60) + 00); break;// Mountain Daylight Time.
					case "MST": timeZoneMinutes = -((07 * 60) + 00); break;// Mountain Standard Time.
					case "N": timeZoneMinutes = -((01 * 60) + 00); break;// November Time Zone (military).
					case "NDT": timeZoneMinutes = -((02 * 60) + 30); break;// Newfoundland Daylight Time.
					case "NFT": timeZoneMinutes = ((11 * 60) + 30); break;// Norfolk (Island) Time.
					case "NST": timeZoneMinutes = -((03 * 60) + 30); break;// Newfoundland Standard Time.
					case "O": timeZoneMinutes = -((02 * 60) + 00); break;// Oscar Time Zone (military).
					case "P": timeZoneMinutes = -((03 * 60) + 00); break;// Papa Time Zone (military).
					case "PDT": timeZoneMinutes = -((07 * 60) + 00); break;// Pacific Daylight Time.
					case "PST": timeZoneMinutes = -((08 * 60) + 00); break;// Pacific Standard Time.
					case "Q": timeZoneMinutes = -((04 * 60) + 00); break;// Quebec Time Zone (military).
					case "R": timeZoneMinutes = -((05 * 60) + 00); break;// Romeo Time Zone (military).
					case "S": timeZoneMinutes = -((06 * 60) + 00); break;// Sierra Time Zone (military).
					case "T": timeZoneMinutes = -((07 * 60) + 00); break;// Tango Time Zone (military).
					case "U": timeZoneMinutes = -((08 * 60) + 00); break;// Uniform Time Zone (military).
					case "UTC": timeZoneMinutes = 0000; break;// Coordinated Universal Time.
					case "V": timeZoneMinutes = -((09 * 60) + 00); break;// Victor Time Zone (militray).
					case "W": timeZoneMinutes = -((10 * 60) + 00); break;// Whiskey Time Zone (military).
					case "WEDT": timeZoneMinutes = ((01 * 60) + 00); break;// Western European Daylight Time.
					case "WEST": timeZoneMinutes = ((01 * 60) + 00); break;// Western European Summer Time.
					case "WET": timeZoneMinutes = 0000; break;// Western European Time.
					case "WST": timeZoneMinutes = ((08 * 60) + 00); break;// Western Standard Time.
					case "X": timeZoneMinutes = -((11 * 60) + 00); break;// X-ray Time Zone (military).
					case "Y": timeZoneMinutes = -((12 * 60) + 00); break;// Yankee Time Zone (military).
					case "Z": timeZoneMinutes = 0000; break;// Zulu Time Zone (military).
					default: throw new FormatException ("Invalid string representation of data/time. Invalid time zone '" + timeZoneStr + "'.");
				}

				#endregion
			}
			return timeZoneMinutes;
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
	}
}
