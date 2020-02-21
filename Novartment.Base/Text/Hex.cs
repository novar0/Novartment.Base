using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Methods for getting and parsing the string hexadecimal representation of bytes.
	/// </summary>
	public static class Hex
	{
		private static readonly ReadOnlyMemory<int> Chars = new int[]
		{
#pragma warning disable SA1001 // Commas must be spaced correctly
#pragma warning disable SA1021 // Negative signs must be spaced correctly
			-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
			0,1,2,3,4,5,6,7,8,9,
			-1,-1,-1,-1,-1,-1,-1,
			10,11,12,13,14,15,
			-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
			10,11,12,13,14,15,
		};
#pragma warning restore SA1021 // Negative signs must be spaced correctly
#pragma warning restore SA1001 // Commas must be spaced correctly

		/// <summary>
		/// A table of hexadecimal string representations of 8-bit numbers.
		/// </summary>
		public static readonly ReadOnlyMemory<string> OctetsUpper = new string[]
		{
			"00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
			"10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
			"20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
			"30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
			"40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
			"50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
			"60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
			"70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
			"80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
			"90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
			"A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
			"B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
			"C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
			"D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
			"E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
			"F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF",
		};

		/// <summary>
		/// Converts a two-digit hexadecimal string representation to a byte.
		/// </summary>
		/// <param name="character1">The first digit (the higher 4 bits).</param>
		/// <param name="character2">The second digit (the lower 4 bits).</param>
		/// <returns>A byte obtained from a hexadecimal string representation.</returns>
		public static byte ParseByte (char character1, char character2)
		{
			var chars = Chars.Span;
			var octets = OctetsUpper.Span;
			var b1 = (character1 < chars.Length) ? chars[character1] : -1;
			if (b1 < 0)
			{
				throw new FormatException ("Invalid HEX char U+" + octets[character1 >> 8] + octets[character1 & 0xff] + ".");
			}

			var b2 = (character2 < chars.Length) ? chars[character2] : -1;
			if (b2 < 0)
			{
				throw new FormatException ("Invalid HEX char U+" + octets[character2 >> 8] + octets[character2 & 0xff] + ".");
			}

			return (byte)((b1 << 4) | b2);
		}

		/// <summary>
		/// Converts a hexadecimal string representation to an array of bytes.
		/// </summary>
		/// <param name="source">The hexadecimal string.</param>
		/// <returns>An array of bytes obtained from a hexadecimal string representation.</returns>
		public static byte[] ParseArray (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var result = new byte[source.Length / 2];
			ParseArray (source.AsSpan (), result);
			return result;
		}

		/// <summary>
		/// Converts a hexadecimal string representation to a range of memory.
		/// </summary>
		/// <param name="source">The hexadecimal string.</param>
		/// <param name="buffer">The buffer to which the resulting sequence of bytes will be written.</param>
		/// <returns>The number of bytes written to the buffer.</returns>
		public static int ParseArray (ReadOnlySpan<char> source, Span<byte> buffer)
		{
			var dstIdx = 0;
			for (var index = 0; index < source.Length; index += 2)
			{
				buffer[dstIdx++] = ParseByte (source[index], source[index + 1]);
			}

			return dstIdx;
		}

		/// <summary>
		/// Converts a range of memory to a string hexadecimal representation in uppercase letters.
		/// </summary>
		/// <param name="source">A range of memory to convert to a string hexadecimal representation.</param>
		/// <param name="buffer">The buffer where the string hexadecimal representation will be written.</param>
		/// <returns>The number of characters written to the buffer.</returns>
		public static int ToHexStringUpper (ReadOnlySpan<byte> source, Span<char> buffer)
		{
			if (buffer.Length < (source.Length * 2))
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			Contract.EndContractBlock ();

			var octets = OctetsUpper.Span;
			for (var index = 0; index < source.Length; index++)
			{
				buffer[index << 1] = octets[source[index]][0];
				buffer[(index << 1) + 1] = octets[source[index]][1];
			}

			return source.Length * 2;
		}
	}
}
