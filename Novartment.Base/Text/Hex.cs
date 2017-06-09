using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы получения и разбора строкового шестнадцатеричного представления байтов.
	/// </summary>
	public static class Hex
	{
		/// <summary>
		/// Таблица шестнадцатеричных строковых передставлений (один символ) 4-битного числа.
		/// Содержит -1 если символ не представляет собой шестнадцатеричное строковое представление числа.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Security",
			"CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
			Justification = "IReadOnlyList is immutable.")]
		public static readonly IReadOnlyList<int> Chars = new ReadOnlyArray<int>(new int[]
		{
#pragma warning disable SA1001 // Commas must be spaced correctly
#pragma warning disable SA1021 // Negative signs must be spaced correctly
			-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
			0,1,2,3,4,5,6,7,8,9,
			-1,-1,-1,-1,-1,-1,-1,
			10,11,12,13,14,15,
			-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
			10,11,12,13,14,15
		});
#pragma warning restore SA1021 // Negative signs must be spaced correctly
#pragma warning restore SA1001 // Commas must be spaced correctly

		/// <summary>
		/// Таблица шестнадцатеричных строковых передставлений 8-битных чисел.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Security",
			"CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
			Justification = "IReadOnlyList is immutable.")]
		public static readonly IReadOnlyList<string> OctetsUpper = new ReadOnlyArray<string>(new string[]
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
			"F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF"
		});

		/// <summary>
		/// Преобразовывает двухзначное шестнадцатеричное строковое представление в байт.
		/// </summary>
		/// <param name="source">Двухзначное шестнадцатеричное строковое представление байта.</param>
		/// <returns>Байт, полученный из шестнадцатеричного строкового представления.</returns>
		public static byte ParseByte (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.Length < 2)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Contract.EndContractBlock ();

			return ParseByte (source[0], source[1]);
		}

		/// <summary>
		/// Преобразовывает двухзначное шестнадцатеричное строковое представление в байт.
		/// </summary>
		/// <param name="source">Двухзначное шестнадцатеричное строковое представление байта.</param>
		/// <param name="index">Начальная позиция в строке.</param>
		/// <returns>Байт, полученный из шестнадцатеричного строкового представления.</returns>
		public static byte ParseByte (string source, int index)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.Length < 2)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			if ((index < 0) || (index > (source.Length - 2)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			return ParseByte (source[index], source[index + 1]);
		}

		/// <summary>
		/// Преобразовывает двухзначное шестнадцатеричное строковое представление в байт.
		/// </summary>
		/// <param name="character1">Первый символ (старшие 4 бита).</param>
		/// <param name="character2">Второй символ (младшие 4 бита).</param>
		/// <returns>Байт, полученный из шестнадцатеричного строкового представления.</returns>
		public static byte ParseByte (char character1, char character2)
		{
			var b1 = (character1 < Chars.Count) ? Chars[character1] : -1;
			if (b1 < 0)
			{
				throw new FormatException ("Invalid HEX char U+" + OctetsUpper[character1 >> 8] + OctetsUpper[character1 & 0xff] + ".");
			}

			var b2 = (character2 < Chars.Count) ? Chars[character2] : -1;
			if (b2 < 0)
			{
				throw new FormatException ("Invalid HEX char U+" + OctetsUpper[character2 >> 8] + OctetsUpper[character2 & 0xff] + ".");
			}

			return (byte)((b1 << 4) | b2);
		}

		/// <summary>
		/// Преобразовывает шестнадцатеричное строковое представление в массив байт.
		/// </summary>
		/// <param name="source">Шестнадцатеричное строковое представление байт.</param>
		/// <returns>Массив байт, полученный из шестнадцатеричного строкового представления.</returns>
		public static byte[] ParseArray (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return ParseArray (source, 0, source.Length);
		}

		/// <summary>
		/// Преобразовывает шестнадцатеричное строковое представление в массив байт.
		/// </summary>
		/// <param name="source">Шестнадцатеричное строковое представление байт.</param>
		/// <param name="offset">Начальная позиция в строке.</param>
		/// <param name="count">Количество знаков строки, начиная от начальной позиции.</param>
		/// <returns>Массив байт, полученный из шестнадцатеричного строкового представления.</returns>
		public static byte[] ParseArray (string source, int offset, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if ((source.Length & 1) != 0)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			if ((offset < 0) || (offset > source.Length) || ((offset == source.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || ((offset + count) > source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			var size = count >> 1;
			var result = new byte[size];
			for (var index = 0; index < size; index++)
			{
				result[index] = ParseByte (source[(index << 1) + offset], source[(index << 1) + offset + 1]);
			}

			return result;
		}

		/// <summary>
		/// Преобразовывает массив байт в строковое шестнадцатеричное представление прописными буквами.
		/// </summary>
		/// <param name="data">Массив байт для преобразования в строковое шестнадцатеричное представление.</param>
		/// <returns>Строковое шестнадцатеричное представление прописными буквами указанного массива байтов.</returns>
		public static string ToHexStringUpper (this byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			if (data.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (data));
			}

			Contract.EndContractBlock ();

			var buf = new char[data.Length << 1];
			for (var index = 0; index < data.Length; index++)
			{
				buf[index << 1] = OctetsUpper[data[index]][0];
				buf[(index << 1) + 1] = OctetsUpper[data[index]][1];
			}

			return new string (buf);
		}
	}
}
