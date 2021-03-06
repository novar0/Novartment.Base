using System;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Операции для декодирования строк из формы "encoded-word" согласно RFC 2047.
	/// </summary>
	public static class Rfc2047EncodedWord
	{
		/// <summary>
		/// Декодирует RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="source">Значение, закодированное в формате RFC 2047 'encoded-word'.</param>
		/// <returns>Декодированное значение.</returns>
		public static string Parse (string source)
		{
			var (textEncoding, binaryEncoding, valuePos, valueSize) = SplitParts (source.AsSpan ());
			var valueStr = source.AsSpan ().Slice (valuePos, valueSize);

			int resultSize;
			if (binaryEncoding)
			{
				var bufSize = ((source.Length / 4) * 3) + 2;
#if NETSTANDARD2_0
				var byteBuf = new byte[bufSize];
				resultSize = ParseBString (valueStr, byteBuf);
				return textEncoding.GetString (byteBuf, 0, resultSize);
#else
				Span<byte> byteBuf = (bufSize < 1024) ? stackalloc byte[bufSize] : new byte[bufSize];
				resultSize = ParseBString (valueStr, byteBuf);
				return textEncoding.GetString (byteBuf.Slice (0, resultSize));
#endif
			}
			else
			{
#if NETSTANDARD2_0
				var byteBuf = new byte[source.Length];
				resultSize = ParseQString (valueStr, byteBuf);
				return textEncoding.GetString (byteBuf, 0, resultSize);
#else
				Span<byte> byteBuf = (source.Length < 1024) ? stackalloc byte[source.Length] : new byte[source.Length];
				resultSize = ParseQString (valueStr, byteBuf);
				return textEncoding.GetString (byteBuf.Slice (0, resultSize));
#endif
			}
		}

		/// <summary>
		/// Декодирует RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="source">Значение, закодированное в формате RFC 2047 'encoded-word'.</param>
		/// <param name="buffer">Буфер, в который будет помещено декодированное значение.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public static int Parse (ReadOnlySpan<char> source, Span<char> buffer)
		{
			var (textEncoding, binaryEncoding, valuePos, valueSize) = SplitParts (source);
			var valueStr = source.Slice (valuePos, valueSize);

			if (binaryEncoding)
			{
				var bufSize = ((source.Length / 4) * 3) + 2;
#if NETSTANDARD2_0
				var byteBuf = new byte[bufSize];
				var byteSize = ParseBString (valueStr, byteBuf);
				var resultStr = textEncoding.GetChars (byteBuf, 0, byteSize);
				resultStr.AsSpan ().CopyTo (buffer);
				return resultStr.Length;
#else
				Span<byte> byteBuf = (bufSize < 1024) ? stackalloc byte[bufSize] : new byte[bufSize];
				var byteSize = ParseBString (valueStr, byteBuf);
				return textEncoding.GetChars (byteBuf.Slice (0, byteSize), buffer);
#endif
			}
			else
			{
#if NETSTANDARD2_0
				var byteBuf = new byte[source.Length];
				var byteSize = ParseQString (valueStr, byteBuf);
				var resultStr = textEncoding.GetChars (byteBuf, 0, byteSize);
				resultStr.AsSpan ().CopyTo (buffer);
				return resultStr.Length;
#else
				Span<byte> byteBuf = (source.Length < 1024) ? stackalloc byte[source.Length] : new byte[source.Length];
				var byteSize = ParseQString (valueStr, byteBuf);
				return textEncoding.GetChars (byteBuf.Slice (0, byteSize), buffer);
#endif
			}
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

			source = source[2..];

			// пропускаем все символы класса RFC 2047 'token'
			var charsetAndLangStrLength = 0;
			var asciiClasses = AsciiCharSet.ValueClasses.Span;
			while (charsetAndLangStrLength < source.Length)
			{
				var character = source[charsetAndLangStrLength];

				if ((character >= asciiClasses.Length) || ((asciiClasses[character] & AsciiCharClasses.ExtendedToken) == 0))
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
#if NETSTANDARD2_0
				textEncoding = Encoding.GetEncoding (new string (charsetStr.ToArray ()));
#else
				textEncoding = Encoding.GetEncoding (new string (charsetStr));
#endif
			}
			catch (ArgumentException e)
			{
				throw new FormatException ($"Invalid 'charset' parameter in 'encoded-word'-value.", e);
			}

			var textEncodingChar = source[charsetAndLangStrLength + 1];
			var binaryEncoding = textEncodingChar switch
			{
				'q' or 'Q' => false,
				'b' or 'B' => true,
				_ => throw new FormatException ("Unsupported value of 'textEncoding' in 'encoded-word' value. Expected 'Q' or 'B'."),
			};
			if (source[charsetAndLangStrLength + 2] != '?')
			{
				throw new FormatException ("Char '?' not found after 'textEncoding' in 'encoded-word' value.");
			}

			if ((source[^2] != '?') || (source[^1] != '='))
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

							num2 <<= 12;
							if ((num2 & 0x80000000) == 0)
							{
								throw new FormatException ("Invalid position of char '=' in base64-encoded string.");
							}

							buffer[bufferOffset++] = (byte)(num2 >> 16);
						}
						else
						{
							num2 <<= 6;
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
			var charClasses = AsciiCharSet.ValueClasses.Span;
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
						var isVisibleChar = (octet < charClasses.Length) && ((charClasses[octet] & AsciiCharClasses.Visible) == AsciiCharClasses.Visible);
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
