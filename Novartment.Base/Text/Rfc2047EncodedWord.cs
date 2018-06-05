using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Text;

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
		public static string Parse (string value) // Parse
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			// encoded-word := "=?" charset ["*" Language-Tag] "?" encoding "?" encoded-text "?="
			// charset = etoken
			// encoding = etoken
			if (value.Length < 8)
			{
				throw new FormatException (FormattableString.Invariant (
					$"Value '{value}' is not valid 'encoded-word'."));
			}

			var parser = new StructuredStringReader (value);
			parser.EnsureCodePoint ('=');
			parser.EnsureCodePoint ('?');
			var start = parser.Position;
			var end = parser.SkipClassChars (AsciiCharSet.Classes, (short)AsciiCharClasses.ExtendedToken);
			var charsetStr = value.Substring (start, end - start);
#if NETCOREAPP2_1
			var idx = charsetStr.IndexOf ('*', StringComparison.Ordinal);
#else
			var idx = charsetStr.IndexOf ('*');
#endif
			if (idx > 0)
			{
				charsetStr = charsetStr.Substring (0, idx);

				// var lang = charsetStr.Substring (idx + 1);
			}

			Encoding encoding;
			try
			{
				encoding = Encoding.GetEncoding (charsetStr);
			}
			catch (ArgumentException e)
			{
				throw new FormatException (
					FormattableString.Invariant (
					$"Invalid 'charset' parameter '{charsetStr}' in 'encoded-word'-value '{value}'."),
					e);
			}

			parser.EnsureCodePoint ('?');
			var wordEncodingChar = parser.SkipCodePoint ();
			var binaryEncoding = false;
			switch (wordEncodingChar)
			{
				case 'q':
				case 'Q':
					break;
				case 'b':
				case 'B':
					binaryEncoding = true;
					break;
				default:
					throw new FormatException (FormattableString.Invariant (
						$"Unsupported value of 'encoding' ('{wordEncodingChar}') in 'encoded-word' value '{value}'. Expected 'Q' or 'B'."));
			}

			parser.EnsureCodePoint ('?');
			start = parser.Position;
			end = value.Length - 2;
			if ((value[end] != '?') || (value[end + 1] != '='))
			{
				throw new FormatException (FormattableString.Invariant (
					$"Ending '?=' not found in 'encoded-word'-value '{value}'."));
			}

			return binaryEncoding ?
				ParseBString (value, start, end, encoding) :
				ParseQString (value, start, end, encoding);
		}

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

			return IsValid (value, 0, value.Length);
		}

		/// <summary>
		/// Проверяет указанную часть строки на соответствие формату RFC 2047 'encoded-word'.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <param name="index">Позиция первого символа для проверки.</param>
		/// <param name="count">Количество символов для проверки.</param>
		/// <returns>True если строка соответствует формату RFC 2047 'encoded-word'.</returns>
		public static bool IsValid (string value, int index, int count)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			if ((index < 0) || (index > value.Length) || ((index == value.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > value.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			/* RFC 2047 2.
				encoded-word = "=?" charset "?" encoding "?" encoded-text "?="

				An 'encoded-word' may not be more than 75 characters long, including
				'charset', 'encoding', 'encoded-text', and delimiters. If it is
				desirable to encode more text than will fit in an 'encoded-word' of
				75 characters, multiple 'encoded-word's (separated by CarriageReturnLinefeed SPACE) may
				be used.

				RFC 2231 (updates syntax)
				encoded-word := "=?" charset ["*" language] "?" encoded-text "?="
			*/

			return (count > 8) &&
					(value[index] == '=') &&
					(value[index + 1] == '?') &&
					(value[index + count - 2] == '?') &&
					(value[index + count - 1] == '=') &&
					AsciiCharSet.IsAllOfClass (value, index, count, AsciiCharClasses.Visible);
		}

		// Конвертирует "B"-encoded строку в массив байт согласно RFC 2045 часть 6.8.</summary>
		// Отказался от использования Convert.FromBase64String() потому, что там лояльно относятся к лишним пробелам.
		private static string ParseBString (string value, int offset, int endOffset, Encoding encoding)
		{
			var buffer = ArrayPool<byte>.Shared.Rent ((((endOffset - offset) / 4) * 3) + 2);
			try
			{
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
									throw new FormatException (FormattableString.Invariant (
										$"Invalid position {offset - 1} of char '=' in base64-encoded string '{value}'."));
								}

								num2 = num2 << 12;
								if ((num2 & 0x80000000) == 0)
								{
									throw new FormatException (FormattableString.Invariant (
										$"Invalid position {offset - 1} of char '=' in base64-encoded string '{value}'."));
								}

								buffer[bufferOffset++] = (byte)(num2 >> 16);
							}
							else
							{
								num2 = num2 << 6;
								if ((num2 & 0x80000000) == 0)
								{
									throw new FormatException (FormattableString.Invariant (
										$"Invalid position {offset - 1} of char '=' in base64-encoded string '{value}'."));
								}

								buffer[bufferOffset++] = (byte)(num2 >> 16);
								buffer[bufferOffset++] = (byte)(num2 >> 8);
							}

							return encoding.GetString (buffer, 0, bufferOffset);
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
											$"Invalid char in position {offset - 1} of base64-encoded string '{value}'."));
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
					throw new FormatException (FormattableString.Invariant (
						$"Incorrectly ended base64-encoded string '{value}'."));
				}

				return encoding.GetString (buffer, 0, bufferOffset);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		// Конвертирует "Q"-encoded строку в массив байт согласно RFC 2047 часть 4.2.
		private static string ParseQString (string value, int offset, int endOffset, Encoding encoding)
		{
			if (endOffset <= offset)
			{
				return string.Empty;
			}

			var buffer = ArrayPool<byte>.Shared.Rent (endOffset - offset);
			try
			{
				var bufferOffset = 0;
				while (offset < endOffset)
				{
					var c = value[offset];
					switch (c)
					{
						case '_': // The 8-bit hexadecimal value 20 (e.g., ISO-8859-1 SPACE) may be represented as "_" (underscore, ASCII 95.)
							buffer[bufferOffset++] = 0x20;
							offset++;
							break;
						case '=': // Any 8-bit value may be represented by a "=" followed by two hexadecimal digits.
							if ((offset + 2) >= endOffset)
							{
								throw new FormatException (FormattableString.Invariant (
									$"Invalid HEX char in position {offset} of q-encoded string '{value}'."));
							}

							// This must be encoded 8-bit byte
							buffer[bufferOffset++] = Hex.ParseByte (value, offset + 1);
							offset += 3;
							break;
						default: // Just write back all other bytes
							var isVisibleChar = AsciiCharSet.IsCharOfClass (c, AsciiCharClasses.Visible);
							if (!isVisibleChar)
							{
								throw new FormatException (FormattableString.Invariant (
									$"Invalid char in position {offset} of q-encoded string '{value}'."));
							}

							buffer[bufferOffset++] = (byte)c;
							offset++;
							break;
					}
				}

				return encoding.GetString (buffer, 0, bufferOffset);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}
	}
}
