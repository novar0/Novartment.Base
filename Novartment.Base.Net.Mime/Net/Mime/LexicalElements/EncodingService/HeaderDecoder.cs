﻿using System;
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

		private static readonly NumberFormatInfo _NumberFormatDot = new NumberFormatInfo { NumberDecimalSeparator = ".", NumberGroupSeparator = "," };

		/// <summary>
		/// Decodes 'atom' value from specified representation.
		/// </summary>
		/// <param name="source">Representation of atom.</param>
		/// <returns>Decoded atom value from specified representation.</returns>
		internal static string DecodeAtom (ReadOnlySpan<byte> source)
		{
			// удаление комментариев и пробельного пространства
			var pos = 0;
			var element1 = StructuredValueParser.GetNextElementToken (source, ref pos);
			var element2 = StructuredValueParser.GetNextElementToken (source, ref pos);
			if (element2.IsValid || (element1.ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException ("Invalid value for type 'atom'.");
			}

			return AsciiCharSet.GetString (source.Slice (element1.StartPosition, element1.Length));
		}

		/// <summary>
		/// Converts encoded 'unstructured' value to decoded source value.
		/// </summary>
		/// <param name="source">Source encoded value.</param>
		/// <returns>Decoded string value.</returns>
		internal static string DecodeUnstructured (ReadOnlySpan<byte> source)
		{
			var result = new StringBuilder ();
			var prevIsWordEncoded = false;
			var lastWhiteSpacePos = 0;
			var lastWhiteSpaceLength = 0;
			var pos = 0;
			while (pos < source.Length)
			{
				// выделяем отдельно группы пробельных символов, потому что их надо пропускать если они между 'encoded-word'
				var octet = source[pos];
				if ((octet == ' ') || (octet == '\t'))
				{
					lastWhiteSpacePos = pos;
					while (pos < source.Length)
					{
						octet = source[pos];
						if ((octet >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[octet] & (short)AsciiCharClasses.WhiteSpace) == 0))
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
					while (pos < source.Length)
					{
						octet = source[pos];
						if ((octet >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[octet] & (short)AsciiCharClasses.Visible) == 0))
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
					if ((!prevIsWordEncoded || !isWordEncoded) && (lastWhiteSpaceLength > 0))
					{
						result.Append (AsciiCharSet.GetString (source.Slice (lastWhiteSpacePos, lastWhiteSpaceLength)));
					}

					var valueStr = AsciiCharSet.GetString (source.Slice (valuePos, valueLength));
					prevIsWordEncoded = isWordEncoded;
					if (isWordEncoded)
					{
						result.Append (Rfc2047EncodedWord.Parse (valueStr));
					}
					else
					{
						result.Append (valueStr);
					}

					lastWhiteSpacePos = lastWhiteSpaceLength = 0;
				}
			}

			if (lastWhiteSpaceLength > 0)
			{
				result.Append (AsciiCharSet.GetString (source.Slice (lastWhiteSpacePos, lastWhiteSpaceLength)));
			}

			return result.ToString ();
		}

		/// <summary>
		/// Decodes 'phrase' into resulting string.
		/// Only supports elements of type Separator, Atom and Quoted.
		/// </summary>
		/// <param name="source">String in 'phrase' format.</param>
		/// <returns>Decoded phrase.</returns>
		internal static string DecodePhrase (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			var decoder = new StructuredValuePhraseDecoder ();
			var isEmpty = true;
			while (true)
			{
				var element = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
				if (!element.IsValid)
				{
					if (isEmpty)
					{
						throw new FormatException ("Empty value is invalid for format 'phrase'.");
					}

					break;
				}

				isEmpty = false;
				decoder.AddElement (source, element);
			}

			return decoder.GetResult ();
		}

		/// <summary>
		/// Creates collection of 'atom's from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of 'atoms'.</param>
		/// <returns>Collection of 'atom's.</returns>
		internal static IReadOnlyList<string> DecodeAtomList (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			var result = new ArrayList<string> ();
			var lastItemIsSeparator = true;
			while (true)
			{
				var item = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				if (!item.IsValid)
				{
					break;
				}

				var isSeparator = (item.ElementType == StructuredValueElementType.Separator) && (item.Length == 1) && (source[item.StartPosition] == (byte)',');
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
					if (!lastItemIsSeparator || (item.ElementType != StructuredValueElementType.Value))
					{
						throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
					}

					lastItemIsSeparator = false;

					result.Add (AsciiCharSet.GetString (source.Slice (item.StartPosition, item.Length)));
				}
			}

			return result;
		}

		/// <summary>
		/// Creates ValueWithType from specified string representation.
		/// </summary>
		/// <param name="source">String representation of NotificationFieldValue.</param>
		/// <returns>New NotificationFieldValue, created from specified string representation.</returns>
		internal static NotificationFieldValue DecodeNotificationFieldValue (ReadOnlySpan<byte> source)
		{
			if (source.Length < 3)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var parserPos = 0;
			var typeElement = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
			if (!typeElement.IsValid)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var typeStr = typeElement.DecodeElement (source);
			var isValidNotificationFieldValue = NotificationFieldValueTypeHelper.TryParse (typeStr, out NotificationFieldValueKind valueType);
			if (!isValidNotificationFieldValue)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var separatorElement = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
			if ((separatorElement.ElementType != StructuredValueElementType.Separator) || (separatorElement.Length != 1) || (source[separatorElement.StartPosition] != (byte)';'))
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var value = source.Slice (separatorElement.StartPosition + separatorElement.Length);
			var valueStr = DecodeUnstructured (value).Trim ();

			return new NotificationFieldValue (valueType, valueStr);
		}

		/// <summary>
		/// Converts two encoded 'unstructured' values (separated by semicolon) to decoded source values.
		/// </summary>
		/// <param name="source">Two encoded 'unstructured' values separated by semicolon.</param>
		/// <returns>Two decoded values.</returns>
		internal static TwoStrings DecodeUnstructuredPair (ReadOnlySpan<byte> source)
		{
			// коменты не распознаются. допускаем что в кодированных словах нет ';'
			var idx = source.IndexOf ((byte)';');
			var text1 = (idx >= 0) ? DecodeUnstructured (source.Slice (0, idx)) : DecodeUnstructured (source);
			var text2 = (idx >= 0) ? DecodeUnstructured (source.Slice (idx + 1)) : null;

			return new TwoStrings () { Value1 = text1, Value2 = text2 };
		}

		/// <summary>
		/// Converts encoded 'unstructured' value and string representation of date
		/// to decoded source value and date.
		/// </summary>
		/// <param name="source">Source encoded value and date.</param>
		/// <returns>Decoded string value and date.</returns>
		internal static TextAndTime DecodeUnstructuredAndDate (ReadOnlySpan<byte> source)
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

			var parametersPos = FindPositionAfterSemicolon (source);
			var str = AsciiCharSet.GetString (source.Slice (0, parametersPos - 1));
			var date = InternetDateTime.Parse (AsciiCharSet.GetString (source.Slice (parametersPos))); // TODO: предусмотреть вариант наличия коментов до или после даты

			return new TextAndTime () { Text = str, Time = date };
		}

		/// <summary>
		/// Converts encoded 'phrase' value and angle-bracketed id.
		/// </summary>
		/// <param name="source">Source encoded value and id.</param>
		/// <returns>Decoded string value and id.</returns>
		internal static TwoStrings DecodePhraseAndId (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			StructuredValuePhraseDecoder decoder = null;
			bool isEmpty = true;
			var lastElement = StructuredValueElement.Invalid;
			while (true)
			{
				var element = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
				if (!element.IsValid)
				{
					if (isEmpty)
					{
						throw new FormatException ("Value does not conform format 'phrase' + <id>.");
					}

					break;
				}

				isEmpty = false;

				if (lastElement.IsValid)
				{
					if (decoder == null)
					{
						decoder = new StructuredValuePhraseDecoder ();
					}

					decoder.AddElement (source, lastElement);
				}

				lastElement = element;
			}

			if (lastElement.ElementType != StructuredValueElementType.AngleBracketedValue)
			{
				throw new FormatException ("Value does not conform format 'phrase' + <id>.");
			}

			string text = decoder?.GetResult ();
			var id = AsciiCharSet.GetString (source.Slice (lastElement.StartPosition, lastElement.Length));
			return new TwoStrings () { Value1 = text, Value2 = id };
		}

		/// <summary>
		/// Creates collection of 'phrase's from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of 'phrase's.</param>
		/// <returns>Collection of 'phrase's.</returns>
		internal static IReadOnlyList<string> DecodePhraseList (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			StructuredValuePhraseDecoder decoder = null;
			bool isDecoderEmpty = true;
			var result = new ArrayList<string> ();
			while (true)
			{
				var item = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
				if (!item.IsValid)
				{
					break;
				}

				var isSeparator = (item.ElementType == StructuredValueElementType.Separator) && (item.Length == 1) && (source[item.StartPosition] == (byte)',');
				if (isSeparator)
				{
					if (!isDecoderEmpty)
					{
						result.Add (decoder.GetResult ());
						isDecoderEmpty = true;
					}
				}
				else
				{
					if (isDecoderEmpty)
					{
						decoder = new StructuredValuePhraseDecoder ();
					}

					decoder.AddElement (source, item);
					isDecoderEmpty = false;
				}
			}

			if (!isDecoderEmpty)
			{
				result.Add (decoder.GetResult ());
			}

			return result;
		}

		/// <summary>
		/// Creates collection of AddrSpecs from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of AddrSpecs.</param>
		/// <param name="enableEmptyAddrSpec">If true, enables empty 'addr-spec'.</param>
		/// <returns>Collection of AddrSpecs.</returns>
		internal static IReadOnlyList<AddrSpec> DecodeAddrSpecList (ReadOnlySpan<byte> source, bool enableEmptyAddrSpec = false)
		{
			// non-compilant server may specify single address without angle brackets
			var pos = 0;
			var firstElement = StructuredValueParser.GetNextElementDotAtom (source, ref pos);
			if (firstElement.IsValid && (firstElement.ElementType != StructuredValueElementType.AngleBracketedValue))
			{
				var addr = AddrSpec.Parse (source);
				return ReadOnlyList.Repeat (addr, 1);
			}

			var parserPos = 0; // начинаем парсинг сначала
			var result = new ArrayList<AddrSpec> ();
			while (true)
			{
				var token = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
				if (!token.IsValid)
				{
					break;
				}

				if (token.ElementType != StructuredValueElementType.AngleBracketedValue)
				{
					throw new FormatException ("Value does not conform to list of 'angle-addr' format.");
				}

				var subSource = source.Slice (token.StartPosition, token.Length);
				if (enableEmptyAddrSpec)
				{
					var isComment = true;
					var subParserPos = 0;
					while (true)
					{
						var subToken = StructuredValueParser.GetNextElementDotAtom (subSource, ref subParserPos);
						if (!subToken.IsValid)
						{
							break;
						}

						if (subToken.ElementType != StructuredValueElementType.RoundBracketedValue)
						{
							isComment = false;
							break;
						}
					}

					if (isComment)
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
		internal static IReadOnlyList<Mailbox> DecodeMailboxList (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			var result = new ArrayList<Mailbox> ();
			var elementsStartPosition = -1;
			var elementsEndPosition = -1;
			while (true)
			{
				var pos = parserPos;
				var item = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
				if (!item.IsValid)
				{
					break;
				}

				var isSeparator = (item.ElementType == StructuredValueElementType.Separator) && (item.Length == 1) && (source[item.StartPosition] == (byte)',');
				if (isSeparator)
				{
					if (elementsEndPosition > elementsStartPosition)
					{
						result.Add (Mailbox.Parse (source.Slice (elementsStartPosition, elementsEndPosition - elementsStartPosition)));
						elementsStartPosition = elementsEndPosition = -1;
					}
				}
				else
				{
					if (elementsStartPosition < 0)
					{
						elementsStartPosition = pos;
					}

					elementsEndPosition = parserPos;
				}
			}

			if (elementsEndPosition > elementsStartPosition)
			{
				result.Add (Mailbox.Parse (source.Slice (elementsStartPosition, elementsEndPosition - elementsStartPosition)));
			}

			return result;
		}

		/// <summary>
		/// Creates collection of urls from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of urls.</param>
		/// <returns>Collection of urls.</returns>
		internal static IReadOnlyList<string> DecodeAngleBracketedlList (ReadOnlySpan<byte> source)
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
				var item = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
				if (!item.IsValid)
				{
					break;
				}

				var isSeparator = (item.ElementType == StructuredValueElementType.Separator) && (item.Length == 1) && (source[item.StartPosition] == (byte)',');
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
					if (!lastItemIsSeparator || (item.ElementType != StructuredValueElementType.AngleBracketedValue))
					{
						break;
					}

					lastItemIsSeparator = false;

					result.Add (AsciiCharSet.GetString (source.Slice (item.StartPosition, item.Length)));
				}
			}

			return result;
		}

		/// <summary>
		/// Creates atom value and collection of HeaderFieldParameter from specified string representation.
		/// </summary>
		/// <param name="source">Source encoded atom value and collection of field parameters.</param>
		/// <returns>Decoded atom value and collection of HeaderFieldParameter.</returns>
		internal static StringAndParameters DecodeAtomAndParameterList (ReadOnlySpan<byte> source)
		{
			// Content-Type and Content-Disposition fields
			var parserPos = 0;
			var valueElement = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
			var value = AsciiCharSet.GetString (source.Slice (valueElement.StartPosition, valueElement.Length));

			var parameterDecoder = new HeaderFieldParameterDecoder ();
			while (true)
			{
				var item = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				if (!item.IsValid)
				{
					break;
				}

				var isSeparator = (item.ElementType == StructuredValueElementType.Separator) && (item.Length == 1) && (source[item.StartPosition] == (byte)';');
				if (!isSeparator)
				{
					throw new FormatException ("Value does not conform to 'atom *(; parameter)' format.");
				}

				var part = HeaderFieldParameterPart.Parse (source, ref parserPos);

				parameterDecoder.AddPart (part);
			}

			return new StringAndParameters () { Text = value, Parameters = parameterDecoder.GetResult () };
		}

		internal static ThreeStringsAndList DecodeDispositionAction (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			var element1 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			var element2 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			var element3 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			var element4 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			var element5 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			var element6 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			if (
				(element1.ElementType != StructuredValueElementType.Value) ||
				(element2.ElementType != StructuredValueElementType.Separator) || (element2.Length != 1) || (source[element2.StartPosition] != (byte)'/') ||
				(element3.ElementType != StructuredValueElementType.Value) ||
				(element4.ElementType != StructuredValueElementType.Separator) || (element4.Length != 1) || (source[element4.StartPosition] != (byte)';') ||
				(element5.ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException ("Specified value does not represent valid 'disposition-action'.");
			}

			var actionMode = element1.DecodeElement (source);
			var sendingMode = element3.DecodeElement (source);
			var dispositionType = element5.DecodeElement (source);
			if (element6.IsValid)
			{
				var isSlashSeparator = (element6.ElementType == StructuredValueElementType.Separator) && (element6.Length == 1) && (source[element6.StartPosition] == (byte)'/');
				if (!isSlashSeparator)
				{
					throw new FormatException ("Specified value does not represent valid 'disposition-action'.");
				}
			}

			var lastItemIsSeparator = true;
			var modifiers = new ArrayList<string> ();
			while (true)
			{
				var item = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				if (!item.IsValid)
				{
					break;
				}

				var isSeparator = (item.ElementType == StructuredValueElementType.Separator) && (item.Length == 1) && (source[item.StartPosition] == (byte)',');
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
					if (!lastItemIsSeparator || (item.ElementType != StructuredValueElementType.Value))
					{
						throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
					}

					lastItemIsSeparator = false;

					modifiers.Add (item.DecodeElement (source));
				}
			}

			return new ThreeStringsAndList ()
			{
				Value1 = actionMode,
				Value2 = sendingMode,
				Value3 = dispositionType,
				List = modifiers,
			};
		}

		/// <summary>
		/// Creates collection of DispositionNotificationParameter from specified string representation.
		/// </summary>
		/// <param name="source">Source encoded collection of parameters.</param>
		/// <returns>Decoded collection of DispositionNotificationParameter.</returns>
		internal static IReadOnlyList<DispositionNotificationParameter> DecodeDispositionNotificationParameterList (ReadOnlySpan<byte> source)
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
				var attributeElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				if (!attributeElement.IsValid)
				{
					break;
				}

				var equalityElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				var importanceElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);

				if (
					(attributeElement.ElementType != StructuredValueElementType.Value) ||
					(equalityElement.ElementType != StructuredValueElementType.Separator) || (equalityElement.Length != 1) || (source[equalityElement.StartPosition] != (byte)'=') ||
					(importanceElement.ElementType != StructuredValueElementType.Value))
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

				var name = AsciiCharSet.GetString (source.Slice (attributeElement.StartPosition, attributeElement.Length));

				var importance = DispositionNotificationParameterImportance.Unspecified;
				var isValidParameterImportance = ParameterImportanceHelper.TryParse (source.Slice (importanceElement.StartPosition, importanceElement.Length), out importance);
				if (!isValidParameterImportance)
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

				// перебираем элементы значения, идущие через запятую
				var values = new ArrayList<string> ();
				while (true)
				{
					var separatorElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);

					// точка-с-запятой означает начало нового параметра если она после элемента значения
					if (!separatorElement.IsValid ||
						((values.Count > 0) && (separatorElement.ElementType == StructuredValueElementType.Separator) && (separatorElement.Length == 1) && (source[separatorElement.StartPosition] == (byte)';')))
					{
						break;
					}

					if ((separatorElement.ElementType != StructuredValueElementType.Separator) || (separatorElement.Length != 1) || (source[separatorElement.StartPosition] != (byte)','))
					{
						throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
					}

					var valueElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);

					if ((valueElement.ElementType != StructuredValueElementType.Value) && (valueElement.ElementType != StructuredValueElementType.QuotedValue))
					{
						throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
					}

					var valueSrc = valueElement.DecodeElement (source);
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
		internal static IReadOnlyList<QualityValueParameter> DecodeQualityValueParameterList (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			decimal defaultQuality = 1.0m;
			var result = new ArrayList<QualityValueParameter> ();
			while (true)
			{
				result.Add (DecodeQualityValueParameter (source, defaultQuality, ref parserPos));
				defaultQuality -= 0.01m;

				var separatorElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				if (!separatorElement.IsValid)
				{
					break;
				}

				if ((separatorElement.ElementType != StructuredValueElementType.Separator) || (separatorElement.Length != 1) || (source[separatorElement.StartPosition] != (byte)','))
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
		internal static Version DecodeVersion (ReadOnlySpan<byte> source)
		{
			var parserPos = 0;
			var element1 = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
			var element2 = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
			var element3 = StructuredValueParser.GetNextElementAtom (source, ref parserPos);
			var element4 = StructuredValueParser.GetNextElementAtom (source, ref parserPos);

			if (element4.IsValid ||
				(element1.ElementType != StructuredValueElementType.Value) ||
				(element2.ElementType != StructuredValueElementType.Separator) || (element2.Length != 1) || (source[element2.StartPosition] != (byte)'.') ||
				(element3.ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException ("Value does not conform to format 'version'.");
			}

			var n1Str = element1.DecodeElement (source);
			var n2Str = element3.DecodeElement (source);
			var n1 = int.Parse (
					n1Str,
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
					CultureInfo.InvariantCulture);
			var n2 = int.Parse (
					n2Str,
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
					CultureInfo.InvariantCulture);
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
		internal static Task<IReadOnlyList<HeaderField>> LoadHeaderFieldsAsync (IBufferedSource headerSource, CancellationToken cancellationToken)
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
				LoadHeaderFieldsAsyncStateMachine ();

			async Task<IReadOnlyList<HeaderField>> LoadHeaderFieldsAsyncStateMachine ()
			{
				var result = new ArrayList<HeaderField> ();
				var buffer = new byte[MaximumHeaderFieldBodySize];
				var fieldSource = new HeaderFieldSource (headerSource);
				bool nextFieldFound;
				do
				{
					try
					{
						var field = await HeaderDecoder.ParseFoldedFieldAsync (fieldSource, buffer, cancellationToken).ConfigureAwait (false);
						result.Add (field);
					}

					// ignore incorrect result
					catch (FormatException)
					{
					}

					nextFieldFound = await fieldSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
				}
				while (nextFieldFound);

				return result;
			}
		}

		// Вычленяет имя и значение из строкового представления поля заголовка.
		// Значение подвергается анфолдингу (замене множества пробельных знаков и переводов строки на один пробел).
		internal static async Task<HeaderField> ParseFoldedFieldAsync (IBufferedSource source, Memory<byte> buffer, CancellationToken cancellationToken)
		{
			/*
			RFC 5322
			part 2.2: A field body MUST NOT include CR and LF except when used in "folding" and "unfolding"
			part 2.2.3: Each header field should be treated in its unfolded form for further syntactic and semantic evaluation.
			part 3.2.2: CRLF that appears in FWS is semantically "invisible".
			*/

			var nameSize = await source.CopyToBufferUntilMarkerAsync ((byte)':', buffer, cancellationToken).ConfigureAwait (false);
			if ((nameSize < 1) || source.IsEmpty ())
			{
				throw new FormatException ("Not found name of header field.");
			}

			var name = AsciiCharSet.GetString (buffer.Span.Slice (0, nameSize));
			var isKnown = HeaderFieldNameHelper.TryParse (name, out HeaderFieldName knownName);
			if (!isKnown)
			{
				knownName = HeaderFieldName.Extension;
			}

			source.SkipBuffer (1); // двоеточие после имени

			var valueSize = 0;
			while (true)
			{
				// RFC 5322 part 2.2.3:
				// Unfolding is accomplished by simply removing any CRLF that is immediately followed by WSP
				// вариант когда после CRLF идёт не-WSP означает начало следующего поля и сюда никак не попадёт
				var valuePartSize = await source.CopyToBufferUntilMarkerAsync (0x0d, 0x0a, buffer.Slice (valueSize), cancellationToken).ConfigureAwait (false);
				valueSize += valuePartSize;
				if (source.IsEmpty ())
				{
					break;
				}

				source.SkipBuffer (2); // 0x0d, 0x09
			}

			return isKnown ?
				new HeaderField (knownName, TrimWhiteSpace (buffer.Span.Slice (0, valueSize))) :
				new ExtensionHeaderField (name, TrimWhiteSpace (buffer.Span.Slice (0, valueSize)));
		}

		// находит позицию символа ';' пропуская значения в кавычках
		private static int FindPositionAfterSemicolon (this ReadOnlySpan<byte> source)
		{
			var pos = 0;

			while (pos < source.Length)
			{
				var octet = source[pos];
				if (octet == (byte)'\"')
				{
					// началось значение quoted-string, пропускаем всё до завершающих кавычек
					while (true)
					{
						if (pos >= source.Length)
						{
							throw new FormatException (FormattableString.Invariant ($"Quoted value end marker not found in source."));
						}

						octet = source[pos];
						if (octet == (byte)'\\')
						{
							// пропускаем квотированный символ чтобы не спутать его с завершающей кавычкой
							pos += 2;
							if (pos >= source.Length)
							{
								throw new FormatException ("Unexpected end of quoted element.");
							}
						}
						else
						{
							pos++;
							if (octet == (byte)'\"')
							{
								break;
							}
						}
					}
				}
				else
				{
					pos++;
					if (octet == (byte)';')
					{
						return pos;
					}
				}
			}

			throw new FormatException (FormattableString.Invariant ($"Ending end marker ';' not found in source."));
		}

		private static QualityValueParameter DecodeQualityValueParameter (ReadOnlySpan<byte> source, decimal defaultQuality, ref int parserPos)
		{
			/*
			language-q = language-range [";" [CFWS] "q=" qvalue ] [CFWS]
			value      = ( "0" [ "." 0*3DIGIT ] ) / ( "1" [ "." 0*3("0") ] )
			*/

			var valueElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);
			if (valueElement.ElementType != StructuredValueElementType.Value)
			{
				throw new FormatException ("Value does not conform to format 'language-q'. First item is not 'atom'.");
			}

			var value = AsciiCharSet.GetString (source.Slice (valueElement.StartPosition, valueElement.Length));
			var quality = defaultQuality;

			var subParserPos = parserPos;
			var separatorElement = StructuredValueParser.GetNextElementToken (source, ref subParserPos);
			var isSemicolon = (separatorElement.ElementType == StructuredValueElementType.Separator) && (separatorElement.Length == 1) && (source[separatorElement.StartPosition] == (byte)';');
			if (isSemicolon)
			{
				parserPos = subParserPos;
				var separatorElement1 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				var separatorElement2 = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				var qualityElement = StructuredValueParser.GetNextElementToken (source, ref parserPos);
				if (
					(separatorElement1.ElementType != StructuredValueElementType.Value) || (separatorElement1.Length != 1) || ((source[separatorElement1.StartPosition] != (byte)'q') && (source[separatorElement1.StartPosition] != (byte)'Q')) ||
					(separatorElement2.ElementType != StructuredValueElementType.Separator) || (separatorElement2.Length != 1) || (source[separatorElement2.StartPosition] != (byte)'=') ||
					(qualityElement.ElementType != StructuredValueElementType.Value))
				{
					throw new FormatException ("Value does not conform to format 'language-q'.");
				}

				var qualityStr = AsciiCharSet.GetString (source.Slice (qualityElement.StartPosition, qualityElement.Length));
				var isValidNumber = decimal.TryParse (qualityStr, NumberStyles.AllowDecimalPoint, _NumberFormatDot, out quality);
				if (!isValidNumber ||
					(quality < 0.0m) ||
					(quality > 1.0m))
				{
					throw new FormatException ("Value does not conform to format 'language-q'. Value of 'quality' not in range 0...1.0.");
				}
			}

			return new QualityValueParameter (value, quality);
		}

		// пропускаем начальные и конечные пробелы (0x20 и 0x09)
		private static ReadOnlySpan<byte> TrimWhiteSpace (ReadOnlySpan<byte> source)
		{
			var startPos = 0;
			var endPos = source.Length;

			while ((startPos < endPos) && ((source[startPos] == 0x20) || (source[startPos] == 0x09)))
			{
				startPos++;
			}

			do
			{
				endPos--;
			}
			while ((startPos < endPos) && ((source[endPos] == 0x20) || (source[endPos] == 0x09)));

			return source.Slice (startPos, endPos - startPos + 1);
		}

		internal struct TextAndTime
		{
			internal string Text;
			internal DateTimeOffset Time;
		}

		internal struct TwoStrings
		{
			internal string Value1;
			internal string Value2;
		}

		internal struct ThreeStringsAndList
		{
			internal string Value1;
			internal string Value2;
			internal string Value3;
			internal IReadOnlyList<string> List;
		}

		internal struct StringAndParameters
		{
			internal string Text;
			internal IReadOnlyList<HeaderFieldParameter> Parameters;
		}
	}
}
