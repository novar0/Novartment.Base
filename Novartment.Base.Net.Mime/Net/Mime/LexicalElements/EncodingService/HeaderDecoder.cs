using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	internal class TokenFormatQuotedString : StructuredStringTokenDelimitedFormat
	{
		internal TokenFormatQuotedString ()
			: base ('\"', '\"', IngoreTokenType.EscapedChar, false)
		{
		}

		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			int idx = 0;
			var endPos = token.Position + token.Length - 1;
			for (var i = token.Position + 1; i < endPos; i++)
			{
				var ch = source[i];
				if (ch == '\\')
				{
					i++;
					ch = source[i];
				}

				buffer[idx++] = ch;
			}

			return idx;
		}
	}

	internal class TokenFormatComment : StructuredStringTokenDelimitedFormat
	{
		internal TokenFormatComment ()
			: base ('(', ')', IngoreTokenType.EscapedChar, true)
		{
		}
	}

	internal class TokenFormatId : StructuredStringTokenDelimitedFormat
	{
		internal TokenFormatId ()
			: base ('<', '>', IngoreTokenType.QuotedValue, false)
		{
		}
	}

	internal class TokenFormatLiteral : StructuredStringTokenDelimitedFormat
	{
		internal TokenFormatLiteral ()
			: base ('[', ']', IngoreTokenType.EscapedChar, false)
		{
		}

		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			int idx = 0;
			var endPos = token.Position + token.Length - 1;
			for (var i = token.Position + 1; i < endPos; i++)
			{
				var ch = source[i];
				if (ch == '\\')
				{
					i++;
					ch = source[i];
				}

				buffer[idx++] = ch;
			}

			return idx;
		}
	}

	/// <summary>
	/// Декодер для семантических элементов MIME-сообщений.
	/// На входе получает MIME-кодированное текстовое представление,
	/// на выходе выдаёт декодированный объект.
	/// </summary>
	internal static class HeaderDecoder
	{
		internal static readonly byte[] CarriageReturnLinefeed = new byte[] { 0x0d, 0x0a };

		internal static readonly byte[] CarriageReturnLinefeed2 = new byte[] { 0x0d, 0x0a, 0x0d, 0x0a };

		internal static readonly int MaximumHeaderFieldBodySize = 16384;

		/*
		Неструктурированное поля (unstructured) это:
		любые нераспознанные поля,
		Subject,
		Comments,
		Content-Description,
		Final-Log-ID
		Original-Envelope-Id
		Failure, Error, Warning
		значения "-type" полей: Reporting-MTA, DSN-Gateway, Received-From-MTA, Original-Recipient, Final-Recipient, Remote-MTA, Diagnostic-Code, MDN-Gateway
		пара значений в Reporting-UA

		unstructured (*text по старому) =
		1) произвольный набор VCHAR,SP,HTAB
		2) может встретиться 'encoded-word' отделённый FWSP
		3) WSP окруженные VCHAR являются пригодными для фолдинга
		4) элементы в кавычках не распознаются как quoted-string
		4) элементы в круглых скобках не распознаются как коменты
		6) может быть пустым

		Основные элементы структурированных полей:
		atom               = [CFWS] 1*atext [CFWS]
		dot-atom           = [CFWS] 1*atext *("." 1*atext) [CFWS]
		quoted-string      = [CFWS] DQUOTE *([FWS] qcontent) [FWS] DQUOTE [CFWS]        // внутри нигде не допустимы encoded-word
		addr-spec          = (dot-atom / quoted-string) "@" (dot-atom / domain-literal) // внутри нигде не допустимы encoded-word
		domain-literal     = [CFWS] "[" *([FWS] dtext) [FWS] "]" [CFWS]
		angle-addr(msg-id) = [CFWS] "<" addr-spec ">" [CFWS]
		mailbox            = ([phrase] angle-addr) / addr-spec

		Фраза (phrase) это display-name в mailbox, элемент списка в keywords, описание в list-id.
		phrase =
		1) набор atom, encoded-word или quoted-string разделенных WSP или comment
		2) WSP между элементами являются пригодными для фолдинга
		3) не может быть пустым

		Правила применения кодированных слов (encoded-word) согласно RFC 2047:
		+ MAY replace a 'unstructured' token in any Subject or Comments header field, any extension message header field, or any MIME body part field for which the field body is defined as 'unstructured'.
		+ MAY appear within a 'comment'.
		+ MAY appear as a replacement for a 'word' entity within a 'phrase'.

		- MUST NOT appear in any portion of an 'addr-spec'.
		- MUST NOT appear within a 'quoted-string'.
		- MUST NOT be used in a Received header field.
		- MUST NOT be used in parameter of a MIME Content-Type or Content-Disposition field.
		- MUST NOT be used in any structured field body except within a 'comment' or 'phrase'.
		*/

		private static readonly NumberFormatInfo _numberFormatDot = new NumberFormatInfo { NumberDecimalSeparator = ".", NumberGroupSeparator = "," };

		/// <summary>
		/// Парсер структурированного значения типа RFC 822 'atom' из его исходного ASCII-строкового представления.
		/// </summary>
		internal static readonly StructuredStringFormat DotAtomFormat = new StructuredStringFormat (
			AsciiCharClasses.WhiteSpace,
			AsciiCharClasses.Atom,
			true,
			new StructuredStringCustomTokenFormat[] { new TokenFormatQuotedString (), new TokenFormatComment (), new TokenFormatId (), new TokenFormatLiteral ()});

		/// <summary>
		/// Парсер структурированного значения типа RFC 822 'dot-atom' из его исходного ASCII-строкового представления.
		/// </summary>
		internal static readonly StructuredStringFormat AtomFormat = new StructuredStringFormat (
			AsciiCharClasses.WhiteSpace,
			AsciiCharClasses.Atom,
			false,
			new StructuredStringCustomTokenFormat[] { new TokenFormatQuotedString (), new TokenFormatComment (), new TokenFormatId (), new TokenFormatLiteral () });

		/// <summary>
		/// Парсер структурированного значения типа RFC 2045 'token' из его исходного ASCII-строкового представления.
		/// </summary>
		internal static readonly StructuredStringFormat TokenFormat = new StructuredStringFormat (
			AsciiCharClasses.WhiteSpace,
			AsciiCharClasses.Token,
			false,
			new StructuredStringCustomTokenFormat[] { new TokenFormatQuotedString (), new TokenFormatComment (), new TokenFormatId (), new TokenFormatLiteral () });

		/// <summary>
		/// Decodes 'atom' value from specified representation.
		/// </summary>
		/// <param name="source">Representation of atom.</param>
		/// <returns>Decoded atom value from specified representation.</returns>
		internal static ReadOnlySpan<char> DecodeAtom (ReadOnlySpan<char> source)
		{
			// комментарии и пробельное пространство пропускается
			var pos = 0;
			StructuredStringToken token1;
			do
			{
				token1 = StructuredStringToken.Parse (TokenFormat, source, ref pos);
			} while (token1.Format is TokenFormatComment);

			StructuredStringToken token2;
			do
			{
				token2 = StructuredStringToken.Parse (TokenFormat, source, ref pos);
			} while (token2.Format is TokenFormatComment);

			if ((token2.Format != null) || !(token1.Format is StructuredStringValueTokenFormat))
			{
				throw new FormatException ("Invalid value for type 'atom'.");
			}

			return source.Slice (token1.Position, token1.Length);
		}

		/// <summary>
		/// Converts encoded 'unstructured' value to decoded source value.
		/// </summary>
		internal static string DecodeUnstructured (ReadOnlySpan<char> source, bool trim, char[] outBuf)
		{
			byte[] byteBuf = null;
			var outPos = 0;
			var prevIsWordEncoded = false;
			var lastWhiteSpacePos = 0;
			var lastWhiteSpaceLength = 0;
			var pos = 0;
			try
			{
				while (pos < source.Length)
				{
					var octet = source[pos];
					if ((octet == ' ') || (octet == '\t'))
					{
						// выделяем отдельно группы пробельных символов, потому что их надо пропускать если они между 'encoded-word'
						lastWhiteSpacePos = pos;
						while (pos < source.Length)
						{
							octet = source[pos];
							if ((octet != ' ') && (octet != '\t'))
							{
								break;
							}

							pos++;
						}

						lastWhiteSpaceLength = pos - lastWhiteSpacePos;
					}
					else
					{
						var valuePos = pos;
						var asciiClasses = AsciiCharSet.ValueClasses.Span;
						while (pos < source.Length)
						{
							octet = source[pos];
							if ((octet >= asciiClasses.Length) || ((asciiClasses[octet] & AsciiCharClasses.Visible) == 0))
							{
								break;
							}

							pos++;
						}

						var valueLength = pos - valuePos;

						if (valueLength < 1)
						{
							octet = source[pos];
							if (octet > AsciiCharSet.MaxCharValue)
							{
								throw new FormatException (FormattableString.Invariant (
									$"Value contains invalid for 'unstructured' character U+{octet:x}. Expected characters are U+0009 and U+0020...U+007E."));
							}
						}

						var isWordEncoded = (valueLength > 8) &&
							(source[valuePos] == '=') &&
							(source[valuePos + 1] == '?') &&
							(source[pos - 2] == '?') &&
							(source[pos - 1] == '=');

						// RFC 2047 часть 6.2:
						// When displaying a particular header field that contains multiple 'encoded-word's,
						// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
						if ((!prevIsWordEncoded || !isWordEncoded) && (lastWhiteSpaceLength > 0) && (!trim || (outPos > 0)))
						{
							source.Slice (lastWhiteSpacePos, lastWhiteSpaceLength).CopyTo (outBuf.AsSpan (outPos));
							outPos += lastWhiteSpaceLength;
						}

						prevIsWordEncoded = isWordEncoded;
						if (isWordEncoded)
						{
							if (byteBuf == null)
							{
								byteBuf = ArrayPool<byte>.Shared.Rent (HeaderFieldBuilder.MaxLineLengthRequired);
							}

							outPos += Rfc2047EncodedWord.Parse (source.Slice (valuePos, valueLength), outBuf.AsSpan (outPos));
						}
						else
						{
							source.Slice (valuePos, valueLength).CopyTo (outBuf.AsSpan (outPos));
							outPos += valueLength;
						}

						lastWhiteSpacePos = lastWhiteSpaceLength = 0;
					}
				}
			}
			finally
			{
				if (byteBuf != null)
				{
					ArrayPool<byte>.Shared.Return (byteBuf);
				}
			}

			if (!trim && (lastWhiteSpaceLength > 0))
			{
				source.Slice (lastWhiteSpacePos, lastWhiteSpaceLength).CopyTo (outBuf.AsSpan (outPos));
				outPos += lastWhiteSpaceLength;
			}

			return new string (outBuf, 0, outPos);
		}

		/// <summary>
		/// Decodes 'phrase' into resulting string.
		/// Only supports tokens of type Separator, Atom and Quoted.
		/// </summary>
		internal static string DecodePhrase (ReadOnlySpan<char> source, char[] outBuf)
		{
			var parserPos = 0;
			var outPos = 0;
			var prevIsWordEncoded = false;
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (DotAtomFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					if (outPos < 1)
					{
						throw new FormatException ("Empty value is invalid for format 'phrase'.");
					}

					break;
				}

				// RFC 2047 часть 6.2:
				// When displaying a particular header field that contains multiple 'encoded-word's,
				// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
				var isWordEncoded = IsWordEncoded (token, source);
				if ((outPos > 0) && (!prevIsWordEncoded || !isWordEncoded))
				{
					// RFC 5322 часть 3.2.2:
					// Runs of FWS, comment, or CFWS that occur between lexical tokens in a structured header field
					// are semantically interpreted as a single space character.
					outBuf[outPos++] = ' ';
				}

				outPos += (token.Format is StructuredStringValueTokenFormat) ?
					DecodeTokenWithEncodedWord (token, source, outBuf.AsSpan (outPos)) :
					token.Decode (source, outBuf.AsSpan (outPos));
				prevIsWordEncoded = isWordEncoded;
			}

			return new string (outBuf, 0, outPos);
		}

		/// <summary>
		/// Creates collection of 'atom's from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of 'atoms'.</param>
		/// <returns>Collection of 'atom's.</returns>
		internal static IReadOnlyList<string> DecodeAtomList (ReadOnlySpan<char> source)
		{
			var parserPos = 0;
			var result = new ArrayList<string> ();
			var lastItemIsSeparator = true;
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					break;
				}

				var isSeparator = token.IsSeparator (source, ',');
				if (isSeparator)
				{
					if (lastItemIsSeparator)
					{
						throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
					}

					lastItemIsSeparator = true;
				}
				else
				{
					if (!lastItemIsSeparator || !(token.Format is StructuredStringValueTokenFormat))
					{
						throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
					}

					lastItemIsSeparator = false;

#if NETSTANDARD2_0
					var str = new string (source.Slice (token.Position, token.Length).ToArray ());
#else
					var str = new string (source.Slice (token.Position, token.Length));
#endif
					result.Add (str);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates ValueWithType from specified string representation.
		/// </summary>
		internal static NotificationFieldValue DecodeNotificationFieldValue (ReadOnlySpan<char> source, char[] outBuf)
		{
			if (source.Length < 3)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var parserPos = 0;
			StructuredStringToken typeToken;
			do
			{
				typeToken = StructuredStringToken.Parse (DotAtomFormat, source, ref parserPos);
			} while (typeToken.Format is TokenFormatComment);

			if (typeToken.Format == null)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var typeSpan = source.Slice (typeToken.Position, typeToken.Length);
			var isValidNotificationFieldValue = NotificationFieldValueTypeHelper.TryParse (typeSpan, out NotificationFieldValueKind valueType);
			if (!isValidNotificationFieldValue)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			StructuredStringToken separatorToken;
			do
			{
				separatorToken = StructuredStringToken.Parse (DotAtomFormat, source, ref parserPos);
			} while (separatorToken.Format is TokenFormatComment);

			if (!separatorToken.IsSeparator (source, ';'))
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var valueStr = DecodeUnstructured (source.Slice (separatorToken.Position + 1), true, outBuf);

			return new NotificationFieldValue (valueType, valueStr);
		}

		/// <summary>
		/// Converts two encoded 'unstructured' values (separated by semicolon) to decoded source values.
		/// </summary>
		internal static TwoStrings DecodeUnstructuredPair (ReadOnlySpan<char> source, char[] outBuf)
		{
			// коменты не распознаются. допускаем что в кодированных словах нет ';'
			var idx = source.IndexOf (';');
			var text1 = (idx >= 0) ? DecodeUnstructured (source.Slice (0, idx), true, outBuf) : DecodeUnstructured (source, true, outBuf);
			var text2 = (idx >= 0) ? DecodeUnstructured (source.Slice (idx + 1), true, outBuf) : null;

			return new TwoStrings (text1, text2);
		}

		/// <summary>
		/// Converts encoded 'unstructured' value and string representation of date
		/// to decoded source value and date.
		/// </summary>
		/// <param name="source">Source encoded value and date.</param>
		/// <returns>Decoded string value and date.</returns>
		internal static TextAndTime DecodeTokensAndDate (ReadOnlySpan<char> source)
		{
			/*
			received       = "Received:" *received-token ";" date-time CRLF
			received-token = word / angle-addr / addr-spec / domain
			*/

			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var parametersPos = FindPositionOfSemicolonSkippingQuotedValues (source);
#if NETSTANDARD2_0
			var str = new string (source.Slice (0, parametersPos - 1).ToArray ());
#else
			var str = new string (source.Slice (0, parametersPos - 1));
#endif
			var date = InternetDateTime.Parse (source.Slice (parametersPos));

			return new TextAndTime (str, date);
		}

		/// <summary>
		/// Converts encoded 'phrase' value and angle-bracketed id.
		/// </summary>
		internal static TwoStrings DecodePhraseAndId (ReadOnlySpan<char> source, char[] outBuf)
		{
			var parserPos = 0;
			var outPos = 0;
			var prevIsWordEncoded = false;
			StructuredStringToken lastToken = default;
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (DotAtomFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					if ((outPos < 1) && (lastToken.Format == null))
					{
						throw new FormatException ("Value does not conform format 'phrase' + <id>.");
					}

					break;
				}

				if (lastToken.Format != null)
				{
					// RFC 2047 часть 6.2:
					// When displaying a particular header field that contains multiple 'encoded-word's,
					// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
					var isWordEncoded = IsWordEncoded (lastToken, source);
					if ((outPos > 0) && (!prevIsWordEncoded || !isWordEncoded))
					{
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical tokens in a structured header field
						// are semantically interpreted as a single space character.
						outBuf[outPos++] = ' ';
					}

					outPos += (lastToken.Format is StructuredStringValueTokenFormat) ?
						DecodeTokenWithEncodedWord (lastToken, source, outBuf.AsSpan (outPos)) :
						lastToken.Decode (source, outBuf.AsSpan (outPos));
					prevIsWordEncoded = isWordEncoded;
				}

				lastToken = token;
			}

			if (!(lastToken.Format is TokenFormatId))
			{
				throw new FormatException ("Value does not conform format 'phrase' + <id>.");
			}

			var text = (outPos > 0) ? new string (outBuf, 0, outPos) : null;
#if NETSTANDARD2_0
			var id = new string (source.Slice (lastToken.Position + 1, lastToken.Length - 2).ToArray ());
#else
			var id = new string (source.Slice (lastToken.Position + 1, lastToken.Length - 2));
#endif
			return new TwoStrings (text, id);
		}

		/// <summary>
		/// Creates collection of 'phrase's from specified comma-delimited string representation.
		/// </summary>
		internal static IReadOnlyList<string> DecodePhraseList (ReadOnlySpan<char> source, char[] outBuf)
		{
			var parserPos = 0;
			var result = new ArrayList<string> ();
			var outPos = 0;
			var prevIsWordEncoded = false;
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					break;
				}

				var isSeparator = token.IsSeparator (source, ',');
				if (isSeparator)
				{
					if (outPos > 0)
					{
						var str = new string (outBuf, 0, outPos);
						result.Add (str);
						outPos = 0;
					}
				}
				else
				{
					// RFC 2047 часть 6.2:
					// When displaying a particular header field that contains multiple 'encoded-word's,
					// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
					var isWordEncoded = IsWordEncoded (token, source);
					if ((outPos > 0) && (!prevIsWordEncoded || !isWordEncoded))
					{
						// RFC 5322 часть 3.2.2:
						// Runs of FWS, comment, or CFWS that occur between lexical tokens in a structured header field
						// are semantically interpreted as a single space character.
						outBuf[outPos++] = ' ';
					}

					outPos += (token.Format is StructuredStringValueTokenFormat) ?
						DecodeTokenWithEncodedWord (token, source, outBuf.AsSpan (outPos)) :
						token.Decode (source, outBuf.AsSpan (outPos));
					prevIsWordEncoded = isWordEncoded;
				}
			}

			if (outPos > 0)
			{
				var str = new string (outBuf, 0, outPos);
				result.Add (str);
			}

			return result;
		}

		/// <summary>
		/// Creates collection of AddrSpecs from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of AddrSpecs.</param>
		/// <param name="enableEmptyAddrSpec">If true, enables empty 'addr-spec'.</param>
		/// <returns>Collection of AddrSpecs.</returns>
		internal static IReadOnlyList<AddrSpec> DecodeAddrSpecList (ReadOnlySpan<char> source, bool enableEmptyAddrSpec = false)
		{
			// non-compilant server may specify single address without angle brackets
			var pos = 0;
			StructuredStringToken firstToken;
			do
			{
				firstToken = StructuredStringToken.Parse (DotAtomFormat, source, ref pos);
			} while (firstToken.Format is TokenFormatComment);

			if ((firstToken.Format != null) && !(firstToken.Format is TokenFormatId))
			{
				var addr = AddrSpec.Parse (source);
				return ReadOnlyList.Repeat (addr, 1);
			}

			var parserPos = 0; // начинаем парсинг сначала
			var result = new ArrayList<AddrSpec> ();
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (DotAtomFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					break;
				}

				if (!(token.Format is TokenFormatId))
				{
					throw new FormatException ("Value does not conform to list of 'angle-addr' format.");
				}

				var subSource = source.Slice (token.Position + 1, token.Length - 2);
				if (enableEmptyAddrSpec)
				{
					// проверяем содержит ли subSource значимые токены
					// отсутствие значимых токенов (пустое значение) обычно недопустимо для addr-spec, но тут можно
					var subParserPos = 0;
					StructuredStringToken subToken;
					do
					{
						subToken = StructuredStringToken.Parse (DotAtomFormat, subSource, ref subParserPos);
					} while (subToken.Format is TokenFormatComment);

					// если не нашли значимых токенов (пустое значение), просто пропускаем это значение
					if (subToken.Format == null)
					{
						continue;
					}
				}

				var addr = AddrSpec.Parse (subSource);
				result.Add (addr);
			}

			return result;
		}

		/// <summary>
		/// Creates collection of Mailbox from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of Mailbox.</param>
		/// <returns>Collection of Mailbox.</returns>
		internal static IReadOnlyList<Mailbox> DecodeMailboxList (ReadOnlySpan<char> source)
		{
			var parserPos = 0;
			var result = new ArrayList<Mailbox> ();
			var tokensStartPosition = -1;
			var tokensEndPosition = -1;
			while (true)
			{
				var pos = parserPos;
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (DotAtomFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					break;
				}

				var isSeparator = token.IsSeparator (source, ',');
				if (isSeparator)
				{
					if (tokensEndPosition > tokensStartPosition)
					{
						result.Add (Mailbox.Parse (source.Slice (tokensStartPosition, tokensEndPosition - tokensStartPosition)));
						tokensStartPosition = tokensEndPosition = -1;
					}
				}
				else
				{
					if (tokensStartPosition < 0)
					{
						tokensStartPosition = pos;
					}

					tokensEndPosition = parserPos;
				}
			}

			if (tokensEndPosition > tokensStartPosition)
			{
				result.Add (Mailbox.Parse (source.Slice (tokensStartPosition, tokensEndPosition - tokensStartPosition)));
			}

			return result;
		}

		/// <summary>
		/// Creates collection of urls from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of urls.</param>
		/// <returns>Collection of urls.</returns>
		internal static IReadOnlyList<string> DecodeAngleBracketedlList (ReadOnlySpan<char> source)
		{
			/*
			1) Except where noted for specific fields, if the content of the
				field (following any leading whitespace, including comments)
				begins with any character other than the opening angle bracket
				'<', the field SHOULD be ignored.
			2) Any characters following an angle bracket enclosed URL SHOULD be
				ignored, unless a comma is the first non-whitespace/comment
				character after the closing angle bracket.
			3) If a sub-item (comma-separated item) within the field is not an
				angle-bracket enclosed URL, the remainder of the field (the
				current, and all subsequent, sub-items) SHOULD be ignored.
			*/

			var parserPos = 0;
			var result = new ArrayList<string> ();
			var lastItemIsSeparator = true;
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					break;
				}

				var isSeparator = token.IsSeparator (source, ',');
				if (isSeparator)
				{
					if (lastItemIsSeparator)
					{
						break;
					}

					lastItemIsSeparator = true;
				}
				else
				{
					if (!lastItemIsSeparator || !(token.Format is TokenFormatId))
					{
						break;
					}

					lastItemIsSeparator = false;

#if NETSTANDARD2_0
					var str = new string (source.Slice (token.Position + 1, token.Length - 2).ToArray ());
#else
					var str = new string (source.Slice (token.Position + 1, token.Length - 2));
#endif
					result.Add (str);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates atom value and collection of HeaderFieldParameter from specified string representation.
		/// </summary>
		internal static StringAndParameters DecodeAtomAndParameterList (ReadOnlySpan<char> source, char[] outBuf)
		{
			// Content-Type and Content-Disposition fields
			var parserPos = 0;
			StructuredStringToken valueToken;
			do
			{
				valueToken = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
			} while (valueToken.Format is TokenFormatComment);
			var atom = source.Slice (valueToken.Position, valueToken.Length);

			var parameters = new ArrayList<HeaderFieldBodyParameter> ();
			while (true)
			{
				var param = HeaderFieldBodyParameter.Parse (source, outBuf, ref parserPos);
				if (param == null)
				{
					return new StringAndParameters (atom, parameters);
				}

				parameters.Add (param);
			};

		}

		internal static ThreeStringsAndList DecodeDispositionAction (ReadOnlySpan<char> source)
		{
			var parserPos = 0;
			StructuredStringToken actionModeToken;
			do
			{
				actionModeToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (actionModeToken.Format is TokenFormatComment);

			StructuredStringToken separatorSlashToken1;
			do
			{
				separatorSlashToken1 = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (separatorSlashToken1.Format is TokenFormatComment);

			StructuredStringToken sendingModeToken;
			do
			{
				sendingModeToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (sendingModeToken.Format is TokenFormatComment);

			StructuredStringToken separatorSemicolonToken;
			do
			{
				separatorSemicolonToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (separatorSemicolonToken.Format is TokenFormatComment);

			StructuredStringToken dispositionTypeToken;
			do
			{
				dispositionTypeToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (dispositionTypeToken.Format is TokenFormatComment);

			StructuredStringToken separatorSlashToken2;
			do
			{
				separatorSlashToken2 = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (separatorSlashToken2.Format is TokenFormatComment);

			if (
				!(actionModeToken.Format is StructuredStringValueTokenFormat) ||
				!separatorSlashToken1.IsSeparator (source, '/') ||
				!(sendingModeToken.Format is StructuredStringValueTokenFormat) ||
				!separatorSemicolonToken.IsSeparator (source, ';') ||
				!(dispositionTypeToken.Format is StructuredStringValueTokenFormat))
			{
				throw new FormatException ("Specified value does not represent valid 'disposition-action'.");
			}

#if NETSTANDARD2_0
			var actionMode = new string (source.Slice (actionModeToken.Position, actionModeToken.Length).ToArray ());
			var sendingMode = new string (source.Slice (sendingModeToken.Position, sendingModeToken.Length).ToArray ());
			var dispositionType = new string (source.Slice (dispositionTypeToken.Position, dispositionTypeToken.Length).ToArray ());
#else
			var actionMode = new string (source.Slice (actionModeToken.Position, actionModeToken.Length));
			var sendingMode = new string (source.Slice (sendingModeToken.Position, sendingModeToken.Length));
			var dispositionType = new string (source.Slice (dispositionTypeToken.Position, dispositionTypeToken.Length));
#endif
			if (separatorSlashToken2.Format != null)
			{
				var isSlashSeparator = separatorSlashToken2.IsSeparator (source, '/');
				if (!isSlashSeparator)
				{
					throw new FormatException ("Specified value does not represent valid 'disposition-action'.");
				}
			}

			var lastItemIsSeparator = true;
			var modifiers = new ArrayList<string> ();
			while (true)
			{
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					break;
				}

				var isSeparator = token.IsSeparator (source, ',');
				if (isSeparator)
				{
					if (lastItemIsSeparator)
					{
						throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
					}

					lastItemIsSeparator = true;
				}
				else
				{
					if (!lastItemIsSeparator || !(token.Format is StructuredStringValueTokenFormat))
					{
						throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
					}

					lastItemIsSeparator = false;

#if NETSTANDARD2_0
					modifiers.Add (new string (source.Slice (token.Position, token.Length).ToArray ()));
#else
					modifiers.Add (new string (source.Slice (token.Position, token.Length)));
#endif
				}
			}

			return new ThreeStringsAndList (actionMode, sendingMode, dispositionType, modifiers);
		}

		/// <summary>
		/// Creates collection of DispositionNotificationParameter from specified string representation.
		/// </summary>
		/// <param name="source">Source encoded collection of parameters.</param>
		/// <returns>Decoded collection of DispositionNotificationParameter.</returns>
		internal static IReadOnlyList<DispositionNotificationParameter> DecodeDispositionNotificationParameterList (ReadOnlySpan<char> source)
		{
			/*
			disposition-notification-parameter-list = disposition-notification-parameter *([FWS] ";" [FWS] disposition-notification-parameter)
			disposition-notification-parameter = attribute [FWS] "=" [FWS] importance [FWS] "," [FWS] value *([FWS] "," [FWS] value)
			importance = "required" / "optional"
			attribute = Atom
			value = atom / quoted-string
			*/

			var parserPos = 0;
			var result = new ArrayList<DispositionNotificationParameter> ();

			while (true)
			{
				StructuredStringToken attributeToken;
				do
				{
					attributeToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (attributeToken.Format is TokenFormatComment);

				if (attributeToken.Format == null)
				{
					break;
				}

				StructuredStringToken equalityToken;
				do
				{
					equalityToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (equalityToken.Format is TokenFormatComment);

				StructuredStringToken importanceToken;
				do
				{
					importanceToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (importanceToken.Format is TokenFormatComment);

				if (
					!(attributeToken.Format is StructuredStringValueTokenFormat) ||
					!equalityToken.IsSeparator (source, '=') ||
					!(importanceToken.Format is StructuredStringValueTokenFormat))
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

#if NETSTANDARD2_0
				var name = new string (source.Slice (attributeToken.Position, attributeToken.Length).ToArray ());
#else
				var name = new string (source.Slice (attributeToken.Position, attributeToken.Length));
#endif

				var importance = DispositionNotificationParameterImportance.Unspecified;
				var isValidParameterImportance = ParameterImportanceHelper.TryParse (source.Slice (importanceToken.Position, importanceToken.Length), out importance);
				if (!isValidParameterImportance)
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

				// перебираем элементы значения, идущие через запятую
				var values = new ArrayList<string> ();
				while (true)
				{
					StructuredStringToken separatorToken;
					do
					{
						separatorToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
					} while (separatorToken.Format is TokenFormatComment);

					// точка-с-запятой означает начало нового параметра если она после элемента значения
					if ((separatorToken.Format == null) || ((values.Count > 0) && separatorToken.IsSeparator (source, ';')))
					{
						break;
					}

					if (!separatorToken.IsSeparator (source, ','))
					{
						throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
					}

					StructuredStringToken valueToken;
					do
					{
						valueToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
					} while (valueToken.Format is TokenFormatComment);

					var isDoubleQuotedValue = valueToken.Format is TokenFormatQuotedString;
					if (!(valueToken.Format is StructuredStringValueTokenFormat) && !isDoubleQuotedValue)
					{
						throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
					}

#if NETSTANDARD2_0
					var valueSrc = isDoubleQuotedValue ?
						new string (source.Slice (valueToken.Position + 1, valueToken.Length - 2).ToArray ()) :
						new string (source.Slice (valueToken.Position, valueToken.Length).ToArray ());
#else
					var valueSrc = isDoubleQuotedValue ?
						new string (source.Slice (valueToken.Position + 1, valueToken.Length - 2)) :
						new string (source.Slice (valueToken.Position, valueToken.Length));
#endif
					values.Add (valueSrc);
				}

				if (values.Count < 1)
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

				result.Add (new DispositionNotificationParameter (name, importance, values));
			}

			return result;
		}

		/// <summary>
		/// Creates collection of QualityValueParameter from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of QualityValueParameters.</param>
		/// <returns>Collection of QualityValueParameters.</returns>
		internal static IReadOnlyList<QualityValueParameter> DecodeQualityValueParameterList (ReadOnlySpan<char> source)
		{
			var parserPos = 0;
			decimal defaultQuality = 1.0m;
			var result = new ArrayList<QualityValueParameter> ();
			while (true)
			{
				result.Add (DecodeQualityValueParameter (source, defaultQuality, ref parserPos));
				defaultQuality -= 0.01m;

				StructuredStringToken separatorToken;
				do
				{
					separatorToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (separatorToken.Format is TokenFormatComment);

				if (separatorToken.Format == null)
				{
					break;
				}

				if (!separatorToken.IsSeparator (source, ','))
				{
					throw new FormatException ("Invalid value of QualityValue parameter list.");
				}
			}

			return result;
		}

		/// <summary>
		/// Creates Version from specified string representation.
		/// </summary>
		/// <param name="source">String representation of Version.</param>
		/// <returns>Decoded Version from string representation.</returns>
		internal static Version DecodeVersion (ReadOnlySpan<char> source)
		{
			var parserPos = 0;
			StructuredStringToken numberToken1;
			do
			{
				numberToken1 = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
			} while (numberToken1.Format is TokenFormatComment);

			StructuredStringToken separatorDotToken;
			do
			{
				separatorDotToken = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
			} while (separatorDotToken.Format is TokenFormatComment);

			StructuredStringToken numberToken2;
			do
			{
				numberToken2 = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
			} while (numberToken2.Format is TokenFormatComment);

			StructuredStringToken excessToken;
			do
			{
				excessToken = StructuredStringToken.Parse (AtomFormat, source, ref parserPos);
			} while (excessToken.Format is TokenFormatComment);

			if ((excessToken.Format != null) ||
				!(numberToken1.Format is StructuredStringValueTokenFormat) ||
				!separatorDotToken.IsSeparator (source, '.') ||
				!(numberToken2.Format is StructuredStringValueTokenFormat))
			{
				throw new FormatException ("Value does not conform to format 'version'.");
			}

			var n1Str = source.Slice (numberToken1.Position, numberToken1.Length);
			var n2Str = source.Slice (numberToken2.Position, numberToken2.Length);
#if NETSTANDARD2_0
			var n1 = int.Parse (new string (n1Str.ToArray ()), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture);
			var n2 = int.Parse (new string (n2Str.ToArray ()), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture);
#else
			var n1 = int.Parse (n1Str, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture);
			var n2 = int.Parse (n2Str, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture);
#endif
			return new Version (n1, n2);
		}

		/// <summary>
		/// Loads header field lines terminated by empty line.
		/// </summary>
		/// <param name="headerSource">Source lines consist of header result.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Collection of HeaderFiel objects.</returns>
		/// <remarks>
		/// According to RFC 2822 part 2.2:
		/// Header fields are lines composed of a field name, followed by a colon (':'), followed by a field body, and terminated by CarriageReturnLinefeed.
		/// A field name MUST be composed of printable US-ASCII characters (i.e., characters that have values between 33 and 126, inclusive), except colon.
		/// A field body may be composed of any US-ASCII characters, except for CR and LF.
		/// </remarks>
		internal static Task<IReadOnlyList<HeaderField>> LoadHeaderAsync (IBufferedSource headerSource, CancellationToken cancellationToken = default)
		{
			if (headerSource == null)
			{
				throw new ArgumentNullException (nameof (headerSource));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<IReadOnlyList<HeaderField>> (cancellationToken);
			}

			var isEmpty = headerSource.IsEmpty ();
			return isEmpty ?
				Task.FromResult<IReadOnlyList<HeaderField>> (Array.Empty<HeaderField> ()) :
				LoadHeaderAsyncStateMachine ();

			async Task<IReadOnlyList<HeaderField>> LoadHeaderAsyncStateMachine ()
			{
				var result = new ArrayList<HeaderField> ();
				var buffer = ArrayPool<byte>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
				try
				{
					var fieldSource = new HeaderFieldSource (headerSource);
					bool nextFieldFound;
					do
					{
						try
						{
							var field = await LoadHeaderFieldAsync (fieldSource, buffer, cancellationToken).ConfigureAwait (false);
							result.Add (field);
						}

						// ignore incorrect result
						catch (FormatException)
						{
						}

						nextFieldFound = await fieldSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
					}
					while (nextFieldFound);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return (buffer);
				}

				return result;
			}
		}

		internal static int CopyWithUnfold (ReadOnlySpan<byte> body, Span<char> buffer)
		{
			/*
			RFC 5322
			part 2.2: A field body MUST NOT include CR and LF except when used in "folding" and "unfolding"
			part 2.2.3: Each header field should be treated in its unfolded form for further syntactic and semantic evaluation.
			part 3.2.2: CRLF that appears in FWS is semantically "invisible".
			*/

			var srcPos = 0;
			var dstPos = 0;
			while (srcPos < body.Length)
			{
				var octet1 = body[srcPos++];

				// RFC 5322 part 2.2.3:
				// Unfolding is accomplished by simply removing any CRLF that is immediately followed by WSP
				// вариант когда после CRLF идёт не-WSP означает начало следующего поля и сюда никак не попадёт
				if ((octet1 == CarriageReturnLinefeed[0]) && (srcPos < body.Length) && (body[srcPos] == CarriageReturnLinefeed[1]))
				{
					srcPos++;
				}
				else
				{
					buffer[dstPos++] = (char)octet1;
				}
			}

			return dstPos;
		}

		// Проверяет, является ли токен "encoded-word" согласно RFC 2047.
		internal static bool IsWordEncoded (StructuredStringToken token, ReadOnlySpan<char> source)
		{
			var pos = token.Position;
			var len = token.Length;
			return (len > 8) &&
				(source[pos] == '=') &&
				(source[pos + 1] == '?') &&
				(source[pos + len - 2] == '?') &&
				(source[pos + len - 1] == '=');
		}

		private static int DecodeTokenWithEncodedWord (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			var src = source.Slice (token.Position, token.Length);
			var isWordEncoded =
				(src.Length > 8) &&
				(src[0] == '=') &&
				(src[1] == '?') &&
				(src[src.Length - 2] == '?') &&
				(src[src.Length - 1] == '=');

			if (isWordEncoded)
			{
				return Rfc2047EncodedWord.Parse (src, buffer);
			}
			else
			{
				src.CopyTo (buffer);
				return src.Length;
			}
		}

		internal static int DecodeParameterExtendedValue (ReadOnlySpan<char> source, Span<char> destination, Encoding encoding)
		{
			int offset = 0;
#if NETSTANDARD2_0
			var buf = new byte[source.Length];
#else
			Span<byte> buf = (source.Length < 1024) ? stackalloc byte[source.Length] : new byte[source.Length];
#endif
			var i = 0;
			while (i < source.Length)
			{
				var char1 = source[i++];
				if (char1 != '%')
				{
					buf[offset++] = (byte)char1;
				}
				else
				{
					if ((i + 1) >= source.Length)
					{
						throw new FormatException ("Invalid format of header field parameter.");
					}

					char1 = source[i++];
					var char2 = source[i++];
					buf[offset++] = Hex.ParseByte (char1, char2);
				}
			}

#if NETSTANDARD2_0
			var str = encoding.GetString (buf, 0, offset);
			str.AsSpan ().CopyTo (destination);
			var size = str.Length;
#else
			var size = encoding.GetChars (buf.Slice (0, offset), destination);
#endif
			return size;
		}

		private static async Task<HeaderField> LoadHeaderFieldAsync (IBufferedSource fieldSource, Memory<byte> buffer, CancellationToken cancellationToken)
		{
			// загружаем имя поля
			var nameSize = await fieldSource.ReadToMarkerAsync ((byte)':', buffer, cancellationToken).ConfigureAwait (false);
			if ((nameSize < 1) || fieldSource.IsEmpty ())
			{
				throw new FormatException ("Not found name of header field.");
			}

			var name = AsciiCharSet.GetString (buffer.Span.Slice (0, nameSize));
			var isKnown = HeaderFieldNameHelper.TryParse (name, out HeaderFieldName knownName);
			if (!isKnown)
			{
				knownName = HeaderFieldName.Extension;
			}

			// двоеточие после имени
			fieldSource.Skip (1);

			// загружаем остатак в виде тела поля
			var valueSize = 0;
			while (true)
			{
				await fieldSource.LoadAsync (cancellationToken).ConfigureAwait (false);
				var available = fieldSource.Count;
				if (available <= 0)
				{
					break;
				}

				cancellationToken.ThrowIfCancellationRequested ();
				fieldSource.BufferMemory.Slice (fieldSource.Offset, available).CopyTo (buffer);
				valueSize += available;
				fieldSource.Skip (available);
			}

			var field = isKnown ?
				new HeaderField (knownName, buffer.Span.Slice (0, valueSize)) :
				new ExtensionHeaderField (name, buffer.Span.Slice (0, valueSize));

			return field;
		}

		// находит позицию символа ';' пропуская значения в кавычках
		private static int FindPositionOfSemicolonSkippingQuotedValues (this ReadOnlySpan<char> source)
		{
			var pos = 0;

			while (pos < source.Length)
			{
				var octet = source[pos];
				if (octet == '\"')
				{
					// началось значение quoted-string, пропускаем всё до завершающих кавычек
					while (true)
					{
						if (pos >= source.Length)
						{
							throw new FormatException (FormattableString.Invariant ($"Quoted value end marker not found in source."));
						}

						octet = source[pos];
						if (octet == '\\')
						{
							// пропускаем квотированный символ чтобы не спутать его с завершающей кавычкой
							pos += 2;
							if (pos >= source.Length)
							{
								throw new FormatException ("Unexpected end of quoted token.");
							}
						}
						else
						{
							pos++;
							if (octet == '\"')
							{
								break;
							}
						}
					}
				}
				else
				{
					pos++;
					if (octet == ';')
					{
						return pos;
					}
				}
			}

			throw new FormatException (FormattableString.Invariant ($"Ending end marker ';' not found in source."));
		}

		private static QualityValueParameter DecodeQualityValueParameter (ReadOnlySpan<char> source, decimal defaultQuality, ref int parserPos)
		{
			/*
			language-q = language-range [";" [CFWS] "q=" qvalue ] [CFWS]
			value      = ( "0" [ "." 0*3DIGIT ] ) / ( "1" [ "." 0*3("0") ] )
			*/

			StructuredStringToken valueToken;
			do
			{
				valueToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
			} while (valueToken.Format is TokenFormatComment);

			if (!(valueToken.Format is StructuredStringValueTokenFormat))
			{
				throw new FormatException ("Value does not conform to format 'language-q'. First item is not 'atom'.");
			}

#if NETSTANDARD2_0
			var value = new string (source.Slice (valueToken.Position, valueToken.Length).ToArray ());
#else
			var value = new string (source.Slice (valueToken.Position, valueToken.Length));
#endif
			var quality = defaultQuality;

			var subParserPos = parserPos;
			StructuredStringToken separatorToken;
			do
			{
				separatorToken = StructuredStringToken.Parse (TokenFormat, source, ref subParserPos);
			} while (separatorToken.Format is TokenFormatComment);

			var isSemicolon = separatorToken.IsSeparator (source, ';');
			if (isSemicolon)
			{
				parserPos = subParserPos;
				StructuredStringToken separatorToken1;
				do
				{
					separatorToken1 = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (separatorToken1.Format is TokenFormatComment);

				StructuredStringToken separatorToken2;
				do
				{
					separatorToken2 = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (separatorToken2.Format is TokenFormatComment);

				StructuredStringToken qualityToken;
				do
				{
					qualityToken = StructuredStringToken.Parse (TokenFormat, source, ref parserPos);
				} while (qualityToken.Format is TokenFormatComment);

				if (
					!(separatorToken1.Format is StructuredStringValueTokenFormat) || ((source[separatorToken1.Position] != 'q') && (source[separatorToken1.Position] != 'Q')) ||
					!separatorToken2.IsSeparator (source, '=') ||
					!(qualityToken.Format is StructuredStringValueTokenFormat))
				{
					throw new FormatException ("Value does not conform to format 'language-q'.");
				}

#if NETSTANDARD2_0
				var isValidNumber = decimal.TryParse (
					new string (source.Slice (qualityToken.Position, qualityToken.Length).ToArray ()),
					NumberStyles.AllowDecimalPoint,
					_numberFormatDot,
					out quality);
#else
				var isValidNumber = decimal.TryParse (
					source.Slice (qualityToken.Position, qualityToken.Length),
					NumberStyles.AllowDecimalPoint,
					_numberFormatDot,
					out quality);
#endif
				if (!isValidNumber ||
					(quality < 0.0m) ||
					(quality > 1.0m))
				{
					throw new FormatException ("Value does not conform to format 'language-q'. Value of 'quality' not in range 0...1.0.");
				}
			}

			return new QualityValueParameter (value, quality);
		}

		internal readonly ref struct TextAndTime
		{
			internal string Text { get; }
			internal DateTimeOffset Time { get; }

			public TextAndTime (string text, DateTimeOffset time)
			{
				this.Text = text;
				this.Time = time;
			}
		}

		internal readonly ref struct TwoStrings
		{
			internal string Value1 { get; }
			internal string Value2 { get; }

			public TwoStrings (string value1, string value2)
			{
				this.Value1 = value1;
				this.Value2 = value2;
			}
		}

		internal readonly ref struct ThreeStringsAndList
		{
			internal string Value1 { get; }
			internal string Value2 { get; }
			internal string Value3 { get; }
			internal IReadOnlyList<string> List { get; }

			public ThreeStringsAndList (string value1, string value2, string value3, IReadOnlyList<string> list)
			{
				this.Value1 = value1;
				this.Value2 = value2;
				this.Value3 = value3;
				this.List = list;
			}
		}

		internal readonly ref struct StringAndParameters
		{
			internal ReadOnlySpan<char> Text { get; }

			internal IReadOnlyList<HeaderFieldBodyParameter> Parameters { get; }

			internal StringAndParameters (ReadOnlySpan<char> text, IReadOnlyList<HeaderFieldBodyParameter> parameters)
			{
				this.Text = text;
				this.Parameters = parameters;
			}

		}
	}
}
