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
		/// <param name="value">Значение, закодированное в формате RFC 2047 'encoded-word'.</param>
		/// <returns>Декодированное значение.</returns>
		public static string Parse (ReadOnlySpan<char> value)
		{
			// encoded-word := "=?" charset ["*" Language-Tag] "?" encoding "?" encoded-text "?="
			// charset = etoken
			// encoding = etoken
			if (value.Length < 8)
			{
#if NETCOREAPP2_1
				var str = new string (value);
#else
				var str = new string (value.ToArray ());
#endif
				throw new FormatException (FormattableString.Invariant (
					$"Value '{str}' is not valid 'encoded-word'."));
			}

			value = value.EnsureCodePoint ('=').EnsureCodePoint ('?');
			var charsetStr = value.GetSubstringOfClassChars (AsciiCharSet.Classes, (short)AsciiCharClasses.ExtendedToken);
			value = value.Slice (charsetStr.Length);
			var idx = charsetStr.IndexOf ('*');
			if (idx > 0)
			{
				charsetStr = charsetStr.Slice (0, idx);
			}

			Encoding encoding;
			try
			{
#if NETCOREAPP2_1
				encoding = Encoding.GetEncoding (new string (charsetStr));
#else
				encoding = Encoding.GetEncoding (new string (charsetStr.ToArray ()));
#endif
			}
			catch (ArgumentException e)
			{
#if NETCOREAPP2_1
				var paramStr = new string (charsetStr);
				var str = new string (value);
#else
				var paramStr = new string (charsetStr.ToArray ());
				var str = new string (value.ToArray ());
#endif
				throw new FormatException (
					FormattableString.Invariant ($"Invalid 'charset' parameter '{paramStr}' in 'encoded-word'-value '{str}'."),
					e);
			}

			value = value.EnsureCodePoint ('?');
			var wordEncodingChar = value.GetFirstCodePoint ();
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
#if NETCOREAPP2_1
					var str = new string (value);
#else
					var str = new string (value.ToArray ());
#endif
					throw new FormatException (FormattableString.Invariant (
						$"Unsupported value of 'encoding' ('{wordEncodingChar}') in 'encoded-word' value '{str}'. Expected 'Q' or 'B'."));
			}

			value = value.Slice (1).EnsureCodePoint ('?');
			var end = value.Length - 2;
			if ((value[end] != '?') || (value[end + 1] != '='))
			{
#if NETCOREAPP2_1
				var str = new string (value);
#else
				var str = new string (value.ToArray ());
#endif
				throw new FormatException (FormattableString.Invariant (
					$"Ending '?=' not found in 'encoded-word'-value '{str}'."));
			}

			value = value.Slice (0, value.Length - 2);
			return binaryEncoding ?
				ParseBString (value, encoding) :
				ParseQString (value, encoding);
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
				encoded-word = "=?" charset "?" encoding "?" encoded-text "?="

				An 'encoded-word' may not be more than 75 characters long, including
				'charset', 'encoding', 'encoded-text', and delimiters. If it is
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

		// Конвертирует "B"-encoded строку в массив байт согласно RFC 2045 часть 6.8.</summary>
		// Отказался от использования Convert.FromBase64String() потому, что там лояльно относятся к лишним пробелам.
		private static string ParseBString (ReadOnlySpan<char> value, Encoding encoding)
		{
			var endOffset = value.Length;
			var buffer = ArrayPool<byte>.Shared.Rent (((endOffset / 4) * 3) + 2);
			try
			{
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
#if NETCOREAPP2_1
									var str = new string (value);
#else
									var str = new string (value.ToArray ());
#endif
									throw new FormatException (FormattableString.Invariant (
										$"Invalid position {offset - 1} of char '=' in base64-encoded string '{str}'."));
								}

								num2 = num2 << 12;
								if ((num2 & 0x80000000) == 0)
								{
#if NETCOREAPP2_1
									var str = new string (value);
#else
									var str = new string (value.ToArray ());
#endif
									throw new FormatException (FormattableString.Invariant (
										$"Invalid position {offset - 1} of char '=' in base64-encoded string '{str}'."));
								}

								buffer[bufferOffset++] = (byte)(num2 >> 16);
							}
							else
							{
								num2 = num2 << 6;
								if ((num2 & 0x80000000) == 0)
								{
#if NETCOREAPP2_1
									var str = new string (value);
#else
									var str = new string (value.ToArray ());
#endif
									throw new FormatException (FormattableString.Invariant (
										$"Invalid position {offset - 1} of char '=' in base64-encoded string '{str}'."));
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
#if NETCOREAPP2_1
										var str = new string (value);
#else
										var str = new string (value.ToArray ());
#endif
										throw new FormatException (FormattableString.Invariant (
											$"Invalid char U+{num:x} in position {offset - 1} of base64-encoded string '{str}'."));
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
#if NETCOREAPP2_1
					var str = new string (value);
#else
					var str = new string (value.ToArray ());
#endif
					throw new FormatException (FormattableString.Invariant (
						$"Incorrectly ended base64-encoded string '{str}'."));
				}

				return encoding.GetString (buffer, 0, bufferOffset);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return (buffer);
			}
		}

		// Конвертирует "Q"-encoded строку в массив байт согласно RFC 2047 часть 4.2.
		private static string ParseQString (ReadOnlySpan<char> value, Encoding encoding)
		{
			if (value.Length < 1)
			{
				return string.Empty;
			}

			var endOffset = value.Length;
			var buffer = ArrayPool<byte>.Shared.Rent (endOffset);
			try
			{
				var offset = 0;
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
#if NETCOREAPP2_1
								var str = new string (value);
#else
								var str = new string (value.ToArray ());
#endif
								throw new FormatException (FormattableString.Invariant (
									$"Invalid HEX char U+{c:x} in position {offset} of q-encoded string '{str}'."));
							}

							// This must be encoded 8-bit byte
							buffer[bufferOffset++] = Hex.ParseByte (value[offset + 1], value[offset + 2]);
							offset += 3;
							break;
						default: // Just write back all other bytes
							var isVisibleChar = AsciiCharSet.IsCharOfClass (c, AsciiCharClasses.Visible);
							if (!isVisibleChar)
							{
#if NETCOREAPP2_1
								var str = new string (value);
#else
								var str = new string (value.ToArray ());
#endif
								throw new FormatException (FormattableString.Invariant (
									$"Invalid char U+{c:x} in position {offset} of q-encoded string '{str}'."));
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
