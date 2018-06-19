using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text.CharSpanExtensions;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Операции для конвертирования строк в форму "encoded-word" и обратно согласно RFC 2047.
	/// </summary>
	public static class Rfc2047EncodedWord
	{
		/// <summary>
		/// Декодирует RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="value">Значение, закодированное в формате RFC 2047 'encoded-word'.</param>
		/// <returns>Декодированное значение.</returns>
		public static string Parse (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return Parse (value.AsSpan ());
		}

		/// <summary>
		/// Декодирует RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="source">Значение, закодированное в формате RFC 2047 'encoded-word'.</param>
		/// <returns>Декодированное значение.</returns>
		public static string Parse (ReadOnlySpan<char> source)
		{
			var (textEncoding, binaryEncoding, valuePos, valueSize) = SplitParts (source);
			var valueStr = source.Slice (valuePos, valueSize);
			int resultSize;

			byte[] buffer = null;
			try
			{
				if (binaryEncoding)
				{
					buffer = ArrayPool<byte>.Shared.Rent (((source.Length / 4) * 3) + 2);
					resultSize = ParseBString (valueStr, buffer);
				}
				else
				{
					buffer = ArrayPool<byte>.Shared.Rent (source.Length);
					resultSize = ParseQString (valueStr, buffer);
				}

				return textEncoding.GetString (buffer, 0, resultSize);
			}
			finally
			{
				if (buffer != null)
				{
					ArrayPool<byte>.Shared.Return (buffer);
				}
			}
		}

#if NETCOREAPP2_1

		/// <summary>
		/// Декодирует RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="source">Значение, закодированное в формате RFC 2047 'encoded-word'.</param>
		/// <param name="buffer">Буфер, в который будет помещено декодированное значение.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public static int Parse (ReadOnlySpan<char> source, Span<char> buffer)
		{
			var (textEncoding, binaryEncoding, valuePos, valueSize) = SplitParts (source);
			var value = source.Slice (valuePos, valueSize);
			int resultSize;
			byte[] bufferTemp = null;
			try
			{
				if (binaryEncoding)
				{
					bufferTemp = ArrayPool<byte>.Shared.Rent (((source.Length / 4) * 3) + 2);
					resultSize = ParseBString (value, bufferTemp);
				}
				else
				{
					bufferTemp = ArrayPool<byte>.Shared.Rent (source.Length);
					resultSize = ParseQString (value, bufferTemp);
				}

				resultSize = textEncoding.GetChars (bufferTemp.AsSpan (0, resultSize), buffer);
				return resultSize;
			}
			finally
			{
				if (bufferTemp != null)
				{
					ArrayPool<byte>.Shared.Return (bufferTemp);
				}
			}
		}

#endif

		/// <summary>
		/// Проверяет строку на соответствие формату RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <returns>True если строка соответствует формату RFC 2047 'encoded-word'.</returns>
		public static bool IsValid (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return IsValid (value.AsSpan ());
		}

		/// <summary>
		/// Проверяет указанную часть строки на соответствие формату RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <returns>True если строка соответствует формату RFC 2047 'encoded-word'.</returns>
		public static bool IsValid (ReadOnlySpan<char> value)
		{
			/* RFC 2047 2.
				encoded-word = "=?" charset "?" textEncoding "?" encoded-text "?="

				An 'encoded-word' may not be more than 75 characters long, including
				'charset', 'textEncoding', 'encoded-text', and delimiters. If it is
				desirable to encode more text than will fit in an 'encoded-word' of
				75 characters, multiple 'encoded-word's (separated by CarriageReturnLinefeed SPACE) may
				be used.

				RFC 2231 (updates syntax)
				encoded-word := "=?" charset ["*" language] "?" encoded-text "?="
			*/

			return (value.Length > 8) &&
					(value[0] == '=') &&
					(value[1] == '?') &&
					(value[value.Length - 2] == '?') &&
					(value[value.Length - 1] == '=') &&
					AsciiCharSet.IsAllOfClass (value, AsciiCharClasses.Visible);
		}

		private static (Encoding textEncoding, bool binaryEncoding, int valuePos, int valueSize) SplitParts (ReadOnlySpan<char> source)
		{
			// encoded-word := "=?" charset ["*" Language-Tag] "?" textEncoding "?" encoded-text "?="
			// charset = etoken
			// textEncoding = etoken
			if (source.Length < 8)
			{
				throw new FormatException ("Specified value is too short for 'encoded-word'.");
			}

			if ((source[0] != '=') || (source[1] != '?'))
			{
				throw new FormatException ("Specified 'encoded-word' value does not contain required start chars '=?'.");
			}

			source = source.Slice (2);

			// пропускаем все символы класса RFC 2047 'token'
			var charsetAndLangStrLength = 0;
			while (charsetAndLangStrLength < source.Length)
			{
				var character = source[charsetAndLangStrLength];

				if ((character >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[character] & (short)AsciiCharClasses.ExtendedToken) == 0))
				{
					break;
				}

				charsetAndLangStrLength++;
			}

			// у остатка после пропуска должен быть префикс ?Q? и суффикс ?=
			if ((source.Length < (charsetAndLangStrLength + 5)) || (source[charsetAndLangStrLength] != '?'))
			{
				throw new FormatException ("Specified 'encoded-word' value does not contain '?' char after 'charset'.");
			}

			var charsetStr = source.Slice (0, charsetAndLangStrLength);
			var idx = charsetStr.IndexOf ('*');
			if (idx > 0)
			{
				charsetStr = charsetStr.Slice (0, idx);
			}

			Encoding textEncoding;
			try
			{
#if NETCOREAPP2_1
				textEncoding = Encoding.GetEncoding (new string (charsetStr));
#else
				textEncoding = Encoding.GetEncoding (new string (charsetStr.ToArray ()));
#endif
			}
			catch (ArgumentException e)
			{
				throw new FormatException ($"Invalid 'charset' parameter in 'encoded-word'-value.", e);
			}

			var textEncodingChar = source[charsetAndLangStrLength + 1];
			var binaryEncoding = false;
			switch (textEncodingChar)
			{
				case 'q':
				case 'Q':
					break;
				case 'b':
				case 'B':
					binaryEncoding = true;
					break;
				default:
					throw new FormatException ("Unsupported value of 'textEncoding' in 'encoded-word' value. Expected 'Q' or 'B'.");
			}

			if (source[charsetAndLangStrLength + 2] != '?')
			{
				throw new FormatException ("Char '?' not found after 'textEncoding' in 'encoded-word' value.");
			}

			if ((source[source.Length - 2] != '?') || (source[source.Length - 1] != '='))
			{
				throw new FormatException ("Ending '?=' chars not found in 'encoded-word' value.");
			}

			return (textEncoding, binaryEncoding, charsetAndLangStrLength + 5, source.Length - charsetAndLangStrLength - 5);
		}

		// Конвертирует "B"-encoded строку в массив байт согласно RFC 2045 часть 6.8.</summary>
		// Отказался от использования Convert.FromBase64String() потому, что там лояльно относятся к лишним пробелам.
		private static int ParseBString (ReadOnlySpan<char> value, Span<byte> buffer)
		{
			var endOffset = value.Length;
			var offset = 0;
			int bufferOffset = 0;
			uint num2 = 0xff;
			while (offset < endOffset)
			{
				uint num = value[offset++];
				switch (num)
				{
					case '+': num = 62; break;
					case '/': num = 63; break;
					case '=':
						if (offset != endOffset)
						{
							if ((offset != (endOffset - 1)) || (value[offset] != '='))
							{
								throw new FormatException ("Invalid position of char '=' in base64-encoded string.");
							}

							num2 = num2 << 12;
							if ((num2 & 0x80000000) == 0)
							{
								throw new FormatException ("Invalid position of char '=' in base64-encoded string.");
							}

							buffer[bufferOffset++] = (byte)(num2 >> 16);
						}
						else
						{
							num2 = num2 << 6;
							if ((num2 & 0x80000000) == 0)
							{
								throw new FormatException ("Invalid position of char '=' in base64-encoded string.");
							}

							buffer[bufferOffset++] = (byte)(num2 >> 16);
							buffer[bufferOffset++] = (byte)(num2 >> 8);
						}

						return bufferOffset;
					default:
						if ((num - 'A') <= 25)
						{
							num -= 'A';
						}
						else
						{
							if ((num - 'a') <= 25)
							{
								num -= 71;
							}
							else
							{
								if ((num - '0') <= 9)
								{
									num += 4;
								}
								else
								{
									throw new FormatException (FormattableString.Invariant (
										$"Invalid char U+{num:x} in base64-encoded string."));
								}
							}
						}

						break;
				}

				num2 = (num2 << 6) | num;
				if ((num2 & 0x80000000) != 0)
				{
					buffer[bufferOffset++] = (byte)(num2 >> 16);
					buffer[bufferOffset++] = (byte)(num2 >> 8);
					buffer[bufferOffset++] = (byte)num2;
					num2 = 0xff;
				}
			}

			if (num2 != 0xff)
			{
				throw new FormatException ("Incorrectly ended base64-encoded string.");
			}

			return bufferOffset;
		}

		// Конвертирует "Q"-encoded строку в массив байт согласно RFC 2047 часть 4.2.
		private static int ParseQString (ReadOnlySpan<char> value, Span<byte> buffer)
		{
			if (value.Length < 1)
			{
				return 0;
			}

			var endOffset = value.Length;
			var offset = 0;
			var bufferOffset = 0;
			while (offset < endOffset)
			{
				var octet = value[offset];
				switch (octet)
				{
					case '_': // The 8-bit hexadecimal value 20 (e.g., ISO-8859-1 SPACE) may be represented as "_" (underscore, ASCII 95.)
						buffer[bufferOffset++] = 0x20;
						offset++;
						break;
					case '=': // Any 8-bit value may be represented by a "=" followed by two hexadecimal digits.
						if ((offset + 2) >= endOffset)
						{
							throw new FormatException (FormattableString.Invariant (
								$"Invalid HEX char U+{octet:x} in position {offset} of q-encoded string."));
						}

						// This must be encoded 8-bit byte
						buffer[bufferOffset++] = Hex.ParseByte (value[offset + 1], value[offset + 2]);
						offset += 3;
						break;
					default: // Just write back all other bytes
						var isVisibleChar = AsciiCharSet.IsCharOfClass (octet, AsciiCharClasses.Visible);
						if (!isVisibleChar)
						{
							throw new FormatException (FormattableString.Invariant (
								$"Invalid char U+{octet:x} in position {offset} of q-encoded string."));
						}

						buffer[bufferOffset++] = (byte)octet;
						offset++;
						break;
				}
			}

			return bufferOffset;
		}
	}
}
