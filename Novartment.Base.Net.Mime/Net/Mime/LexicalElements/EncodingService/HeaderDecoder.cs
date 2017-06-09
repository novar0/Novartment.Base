using System;
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
using static System.Linq.Enumerable;

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
		internal static string DecodeAtom (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// удаление комментариев и пробельного пространства
			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);

			if ((elements.Count != 1) || (elements[0].ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException (FormattableString.Invariant ($"Invalid value for type 'atom': \"{source}\"."));
			}

			return elements[0].Value;
		}

		/// <summary>
		/// Converts encoded 'unstructured' value to decoded source value.
		/// </summary>
		/// <param name="source">Source encoded value.</param>
		/// <returns>Decoded string value.</returns>
		internal static string DecodeUnstructured (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var parser = new StructuredStringReader (source);
			var result = new StringBuilder ();
			var prevIsWordEncoded = false;
			int start;
			int end;
			string lastWhiteSpace = null;
			while (!parser.IsExhausted)
			{
				// выделяем отдельно группы пробельных символов, потому что их надо пропускать если они между 'encoded-word'
				if ((parser.NextChar == ' ') || (parser.NextChar == '\t'))
				{
					start = parser.Position;
					end = parser.SkipClassChars (AsciiCharSet.Classes, (short)AsciiCharClasses.WhiteSpace);
					lastWhiteSpace = parser.Source.Substring (start, end - start);
				}
				else
				{
					start = parser.Position;
					end = parser.SkipClassChars (AsciiCharSet.Classes, (short)AsciiCharClasses.Visible);
					if (end <= start)
					{
						if (parser.NextChar > AsciiCharSet.MaxCharValue)
						{
							throw new FormatException ("Value contains invalid for 'unstructured' character U+" +
								Hex.OctetsUpper[parser.NextChar >> 8] + Hex.OctetsUpper[parser.NextChar & 0xff] +
								". Expected characters are U+0009 and U+0020...U+007E.");
						}
					}

					var value = parser.Source.Substring (start, end - start);
					var isWordEncoded = Rfc2047EncodedWord.IsValid (value);

					// RFC 2047 часть 6.2:
					// When displaying a particular header field that contains multiple 'encoded-word's,
					// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
					if ((!prevIsWordEncoded || !isWordEncoded) && (lastWhiteSpace != null))
					{
						result.Append (lastWhiteSpace);
					}

					prevIsWordEncoded = isWordEncoded;
					result.Append (isWordEncoded ? Rfc2047EncodedWord.Parse (value) : value);
					lastWhiteSpace = null;
				}
			}

			if (lastWhiteSpace != null)
			{
				result.Append (lastWhiteSpace);
			}

			return result.ToString ();
		}

		/// <summary>
		/// Decodes 'phrase' into resulting string.
		/// Only supports elements of type Separator, Atom and Quoted.
		/// </summary>
		/// <param name="source">String in 'phrase' format.</param>
		/// <returns>Decoded phrase.</returns>
		internal static string DecodePhrase (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
			if (elements.Count < 1)
			{
				throw new FormatException ("Empty value is invalid for format 'phrase'.");
			}

			return elements.Decode (elements.Count);
		}

		/// <summary>
		/// Creates collection of 'atom's from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of 'atoms'.</param>
		/// <returns>Collection of 'atom's.</returns>
		internal static IReadOnlyList<string> DecodeAtomList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elementSets = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue)
				.Split (item => item.EqualsSeparator (','));
			var result = new ArrayList<string> ();
			foreach (var elements in elementSets)
			{
				if ((elements.Count != 1) || (elements[0].ElementType != StructuredValueElementType.Value))
				{
					throw new FormatException ("Value does not conform to format 'comma-separated atoms'.");
				}

				result.Add (elements[0].Value);
			}

			return result;
		}

		/// <summary>
		/// Creates ValueWithType from specified string representation.
		/// </summary>
		/// <param name="source">String representation of NotificationFieldValue.</param>
		/// <returns>New NotificationFieldValue, created from specified string representation.</returns>
		internal static NotificationFieldValue DecodeNotificationFieldValue (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.Length < 3)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var idx = source.IndexOf (';'); // в 'atom' не может быть ';'. допускаем что в коментах тоже нет ';'
			if ((idx < 1) || ((idx + 1) >= source.Length))
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var typeTokens = StructuredValueElementCollection.Parse (source.Substring (0, idx), AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue);

			if ((typeTokens.Count != 1) || (typeTokens[0].ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			var value = source.Substring (idx + 1);

			var isValidNotificationFieldValue = NotificationFieldValueTypeHelper.TryParse (typeTokens[0].Value, out NotificationFieldValueKind valueType);
			if (!isValidNotificationFieldValue)
			{
				throw new FormatException ("Value does not conform to format 'type;value'.");
			}

			return new NotificationFieldValue (valueType, DecodeUnstructured (value).Trim ());
		}

		/// <summary>
		/// Converts two encoded 'unstructured' values (separated by semicolon) to decoded source values.
		/// </summary>
		/// <param name="source">Two encoded 'unstructured' values separated by semicolon.</param>
		/// <returns>Two decoded values.</returns>
		internal static TwoStrings DecodeUnstructuredPair (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var idx = source.IndexOf (';'); // коменты не распознаются. допускаем что в кодированных словах нет ';'
			var text1 = (idx >= 0) ? DecodeUnstructured (source.Substring (0, idx)) : DecodeUnstructured (source);
			var text2 = (idx >= 0) ? DecodeUnstructured (source.Substring (idx + 1)) : null;

			return new TwoStrings () { Value1 = text1, Value2 = text2 };
		}

		/// <summary>
		/// Converts encoded 'unstructured' value and string representation of date
		/// to decoded source value and date.
		/// </summary>
		/// <param name="source">Source encoded value and date.</param>
		/// <returns>Decoded string value and date.</returns>
		internal static TextAndTime DecodeUnstructuredAndDate (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// received       = "Received:" *received-token ";" date-time CRLF
			// received-token = word / angle-addr / addr-spec / domain
			var parser = new StructuredStringReader (source);
			var delimData = new DelimitedElement (
				parser.NextChar,
				';',
				new DelimitedElement ('\"', '\"', new DelimitedElement ('\\', 2), false),
				false);
			var idx = parser.SkipDelimited (delimData);
			if (idx >= (source.Length - 1))
			{
				throw new FormatException ("Value does not conform to format '*received-token;date-time'.");
			}

			var parameters = DecodeUnstructured (source.Substring (0, idx - 1));
			var date = InternetDateTime.Parse (source.Substring (idx));

			return new TextAndTime () { Text = parameters, Time = date };
		}

		/// <summary>
		/// Converts encoded 'phrase' value and angle-bracketed id.
		/// </summary>
		/// <param name="source">Source encoded value and id.</param>
		/// <returns>Decoded string value and id.</returns>
		internal static TwoStrings DecodePhraseAndId (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
			if ((elements.Count < 1) || (elements[elements.Count - 1].ElementType != StructuredValueElementType.AngleBracketedValue))
			{
				throw new FormatException ("Value does not conform format 'phrase' + <id>.");
			}

			string text = null;
			if (elements.Count > 1)
			{
				text = elements.Decode (elements.Count - 1);
			}

			var id = elements[elements.Count - 1].Value;
			return new TwoStrings () { Value1 = text, Value2 = id };
		}

		/// <summary>
		/// Creates collection of 'phrase's from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of 'phrase's.</param>
		/// <returns>Collection of 'phrase's.</returns>
		internal static IReadOnlyList<string> DecodePhraseList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elementSets = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue)
				.Split (item => item.EqualsSeparator (','));
			return elementSets.Select (elements => elements.Decode (elements.Count)).ToArray ().AsReadOnlyList ();
		}

		/// <summary>
		/// Creates collection of AddrSpecs from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of AddrSpecs.</param>
		/// <param name="enableEmptyAddrSpec">If true, enables empty 'addr-spec'.</param>
		/// <returns>Collection of AddrSpecs.</returns>
		internal static IReadOnlyList<AddrSpec> DecodeAddrSpecList (string source, bool enableEmptyAddrSpec = false)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);

			// non-compilant server may specify single address without angle brackets
			if (elements[0].ElementType != StructuredValueElementType.AngleBracketedValue)
			{
				var addr = AddrSpec.Parse (elements);
				return ReadOnlyList.Repeat (addr, 1);
			}

			var result = new ArrayList<AddrSpec> ();
			foreach (var token in elements)
			{
				if (token.ElementType != StructuredValueElementType.AngleBracketedValue)
				{
					throw new FormatException ("Value does not conform to list of 'angle-addr' format.");
				}

				var subTokens = StructuredValueElementCollection.Parse (token.Value, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
				if (enableEmptyAddrSpec)
				{
					var isComment = subTokens.All (item => item.ElementType == StructuredValueElementType.RoundBracketedValue);
					if (isComment)
					{
						continue;
					}
				}

				var addr = AddrSpec.Parse (subTokens);
				result.Add (addr);
			}

			return result;
		}

		/// <summary>
		/// Creates collection of Mailbox from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of Mailbox.</param>
		/// <returns>Collection of Mailbox.</returns>
		internal static IReadOnlyList<Mailbox> DecodeMailboxList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
			var elementSets = elements.Split (item => item.EqualsSeparator (','));
			return elementSets.Select (Mailbox.Parse).ToArray ().AsReadOnlyList ();
		}

		/// <summary>
		/// Creates collection of urls from specified comma-delimited string representation.
		/// </summary>
		/// <param name="source">String representation of collection of urls.</param>
		/// <returns>Collection of urls.</returns>
		internal static IReadOnlyList<string> DecodeAngleBracketedlList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

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
			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue);
			var elementSets = elements.Split (item => item.EqualsSeparator (','));
			return elementSets
				.TakeWhile (parameterElements => (parameterElements.Count == 1) && (parameterElements[0].ElementType == StructuredValueElementType.AngleBracketedValue))
				.Select (parameterElements => parameterElements[0].Value)
				.ToArray ().AsReadOnlyList ();
		}

		/// <summary>
		/// Creates atom value and collection of HeaderFieldParameter from specified string representation
		/// </summary>
		/// <param name="source">Source encoded atom value and collection of field parameters.</param>
		/// <returns>Decoded atom value and collection of HeaderFieldParameter.</returns>
		internal static StringAndParameters DecodeAtomAndParameterList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var parameters = new ArrayList<HeaderFieldParameter> ();
			var idx = source.IndexOf (';');
			if (idx >= 0)
			{
				var elements = StructuredValueElementCollection.Parse (source.Substring (idx + 1), AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);
				var elementSets = elements.Split (item => item.EqualsSeparator (';'));
				string parameterName = null;
				Encoding parameterEncoding = null;
				var parameterValue = string.Empty;
				foreach (var part in elementSets.Select (parameterElements => HeaderFieldParameterPart.Parse (parameterElements)))
				{
					if (part.Section == 0)
					{
						// начался новый параметр
						// возвращаем предыдущий параметр если был
						if (parameterName != null)
						{
							parameters.Add (new HeaderFieldParameter (parameterName, parameterValue));
						}

						parameterName = part.Name;
						parameterValue = string.Empty;
						try
						{
							parameterEncoding = Encoding.GetEncoding (part.Encoding ?? "us-ascii");
						}
						catch (ArgumentException excpt)
						{
							throw new FormatException (
								FormattableString.Invariant ($"'{part.Encoding}' is not valid code page name."),
								excpt);
						}
					}

					parameterValue += part.GetValue (parameterEncoding);
				}

				// возвращаем предыдущий параметр если был
				if (parameterName != null)
				{
					parameters.Add (new HeaderFieldParameter (parameterName, parameterValue));
				}

				source = source.Substring (0, idx);
			}

			var valueTokens = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue);
			if (valueTokens.Count != 1)
			{
				throw new FormatException (FormattableString.Invariant ($"Invalid value for type 'atom': \"{source}\"."));
			}

			return new StringAndParameters () { Text = valueTokens[0].Value, Parameters = parameters };
		}

		internal static ThreeStringsAndList DecodeDispositionAction (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);
			if ((elements.Count < 5) ||
				(elements[0].ElementType != StructuredValueElementType.Value) ||
				!elements[1].EqualsSeparator ('/') ||
				(elements[2].ElementType != StructuredValueElementType.Value) ||
				!elements[3].EqualsSeparator (';') ||
				(elements[4].ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException ("Specified value does not represent valid 'disposition-action'.");
			}

			var actionMode = elements[0].Decode ();
			var sendingMode = elements[2].Decode ();
			var dispositionType = elements[4].Decode ();
			var modifiers = new ArrayList<string> ();
			if (elements.Count > 5)
			{
				var isSlashSeparator = elements[5].EqualsSeparator ('/');
				if (!isSlashSeparator)
				{
					throw new FormatException ("Specified value does not represent valid 'disposition-action'.");
				}
			}

			var modifiersTokenSet = elements
				.Skip (6)
				.Split (item => item.EqualsSeparator (','));
			foreach (var modifierTokens in modifiersTokenSet)
			{
				if ((modifierTokens.Count == 1) && (modifierTokens[0].ElementType == StructuredValueElementType.Value))
				{
					modifiers.Add (modifierTokens[0].Decode ());
				}
			}

			return new ThreeStringsAndList ()
			{
				Value1 = actionMode,
				Value2 = sendingMode,
				Value3 = dispositionType,
				List = modifiers
			};
		}

		/// <summary>
		/// Creates collection of DispositionNotificationParameter from specified string representation
		/// </summary>
		/// <param name="source">Source encoded collection of parameters.</param>
		/// <returns>Decoded collection of DispositionNotificationParameter.</returns>
		internal static IReadOnlyList<DispositionNotificationParameter> DecodeDispositionNotificationParameterList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);
			var elementSets = elements.Split (item => item.EqualsSeparator (';'));
			var result = new ArrayList<DispositionNotificationParameter> ();
			foreach (var parameterElements in elementSets)
			{
				if ((parameterElements.Count < 5) ||
					(parameterElements[0].ElementType != StructuredValueElementType.Value) ||
					!parameterElements[1].EqualsSeparator ('=') ||
					(parameterElements[2].ElementType != StructuredValueElementType.Value) ||
					!parameterElements[3].EqualsSeparator (',') ||
					(parameterElements[4].ElementType != StructuredValueElementType.Value))
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

				var name = parameterElements[0].Value;

				var values = parameterElements
					.Skip (2)
					.Split (item => item.EqualsSeparator (','))
					.Select (set => set[0].Value)
					.ToArray ().AsReadOnlyList ();

				DispositionNotificationParameterImportance importance = DispositionNotificationParameterImportance.Unspecified;
				var isValidParameterImportance = (values.Count >= 2) && ParameterImportanceHelper.TryParse (values[0], out importance);
				if (!isValidParameterImportance)
				{
					throw new FormatException ("Invalid value of 'disposition-notification' parameter.");
				}

				result.Add (new DispositionNotificationParameter (name, importance, values.Skip (1)));
			}

			return result;
		}

		/// <summary>
		/// Creates collection of QualityValueParameter from specified string representation.
		/// </summary>
		/// <param name="source">String representation of collection of QualityValueParameters.</param>
		/// <returns>Collection of QualityValueParameters.</returns>
		internal static IReadOnlyList<QualityValueParameter> DecodeQualityValueParameterList (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Token, false, StructuredValueElementType.RoundBracketedValue);
			var elementSets = elements.Split (item => item.EqualsSeparator (','));
			decimal defaultQuality = 1.0m;
			var result = new ArrayList<QualityValueParameter> ();
			foreach (var parameterElements in elementSets)
			{
				if (parameterElements[0].ElementType != StructuredValueElementType.Value)
				{
					throw new FormatException ("Value does not conform to format 'language-q'. First item is not 'atom'.");
				}

				var value = parameterElements[0].Value;
				var quality = defaultQuality;
				if (parameterElements.Count > 1)
				{
					if ((parameterElements.Count != 5) ||
						!parameterElements[1].EqualsSeparator (';') ||
						(parameterElements[2].ElementType != StructuredValueElementType.Value) || (parameterElements[2].Value.ToUpperInvariant () != "Q") ||
						!parameterElements[3].EqualsSeparator ('=') ||
						(parameterElements[4].ElementType != StructuredValueElementType.Value))
					{
						throw new FormatException ("Value does not conform to format 'language-q'.");
					}

					var qualityStr = parameterElements[4].Value;
					var isValidNumber = decimal.TryParse (qualityStr, NumberStyles.AllowDecimalPoint, _NumberFormatDot, out quality);
					if (!isValidNumber ||
						(quality < 0.0m) ||
						(quality > 1.0m))
					{
						throw new FormatException ("Value does not conform to format 'language-q'. Value of 'quality' not in range 0...1.0.");
					}
				}

				result.Add (new QualityValueParameter (value, quality));

				defaultQuality -= 0.01m;
			}

			return result;
		}

		/// <summary>
		/// Creates Version from specified string representation.
		/// </summary>
		/// <param name="source">String representation of Version.</param>
		/// <returns>Decoded Version from string representation.</returns>
		internal static Version DecodeVersion (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, false, StructuredValueElementType.RoundBracketedValue);

			if ((elements.Count != 3) ||
				(elements[0].ElementType != StructuredValueElementType.Value) ||
				!elements[1].EqualsSeparator ('.') ||
				(elements[2].ElementType != StructuredValueElementType.Value))
			{
				throw new FormatException ("Value does not conform to format 'version'.");
			}

			var n1Str = elements[0].Decode ();
			var n2Str = elements[2].Decode ();
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
		internal static Task<IReadOnlyList<HeaderField>> LoadHeaderFieldsAsync (
			IBufferedSource headerSource,
			CancellationToken cancellationToken)
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

			var result = new ArrayList<HeaderField> ();
			var isEmpty = headerSource.IsEmpty ();
			return isEmpty ?
				Task.FromResult<IReadOnlyList<HeaderField>> (result) :
				LoadHeaderFieldsAsyncStateMachine ();

			async Task<IReadOnlyList<HeaderField>> LoadHeaderFieldsAsyncStateMachine ()
			{
				var fieldSource = new HeaderFieldSource (headerSource);
				bool nextFieldFound;
				do
				{
					try
					{
						var field = await HeaderDecoder.ParseFoldedFieldAsync (fieldSource, cancellationToken).ConfigureAwait (false);
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
		internal static async Task<HeaderField> ParseFoldedFieldAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			// RFC 5322 part 2.2: A field body MUST NOT include CR and LF except when used in "folding" and "unfolding"
			// RFC 5322 part 2.2.3: Each header field should be treated in its unfolded form for further syntactic and semantic evaluation.
			// RFC 5322 part 3.2.2: CRLF that appears in FWS is semantically "invisible".

			// RFC 5322 part 3.6.8 optional field:
			// field-name = 1*ftext
			// ftext      = %d33-57 / %d59-126 ; Printable US-ASCII characters not including ":".
			var idx = await BufferedSourceExtensions.IndexOfAsync (source, (byte)':', cancellationToken).ConfigureAwait (false) -
				source.Offset;
			if (idx < 1)
			{
				throw new FormatException ("Invalid name of header field.");
			}

			var name = AsciiCharSet.GetString (source.Buffer, source.Offset, idx);
			var isKnown = HeaderFieldNameHelper.TryParse (name, out HeaderFieldName knownName);
			if (!isKnown)
			{
				knownName = HeaderFieldName.Extension;
			}

			source.SkipBuffer (idx + 1);
			var size = source.Count;
			int assumedSize = source.IsExhausted ? size : Math.Max (8, size);

			// TODO: переделать формирование строки без использользования ArrayList<char> (через char[] ?)
			var unfoldedValue = new ArrayList<char> (assumedSize);
			int lastNonWsp = 0;
			do
			{
				lastNonWsp = Math.Max (lastNonWsp, AppendCharsFromSource (source, unfoldedValue));
				source.SkipBuffer (size);
				await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				size = source.Count;
			}
			while (size > 0);
			var value = new string (unfoldedValue.Array, unfoldedValue.Offset, lastNonWsp);
			return isKnown ?
				new HeaderField (knownName, value) :
				new ExtensionHeaderField (name, value);
		}

		private static int AppendCharsFromSource (IBufferedSource source, ArrayList<char> result)
		{
			int lastNonWsp = 0;
			var idx = source.Offset;
			while (idx < (source.Offset + source.Count))
			{
				var nextChar = source.Buffer[idx++];
				var nextNextChar = (idx < (source.Offset + source.Count)) ? source.Buffer[idx] : 0;

				if (nextChar > AsciiCharSet.MaxCharValue)
				{
					throw new FormatException (
						"Encountered invalid for header field value character U+00" +
						Hex.OctetsUpper[nextChar] +
						". Expected characters from U+0000 to U+00FF.");
				}

				// RFC 5322 part 2.2.3:
				// Unfolding is accomplished by simply removing any CRLF that is immediately followed by WSP
				// вариант когда после CRLF идёт не-WSP означает начало следующего поля и сюда никак не попадёт
				if ((nextChar == 0x0d) && (nextNextChar == 0x0a))
				{
					idx++;
				}
				else
				{
					var isWsp = (nextChar == 0x20) || (nextChar == 0x09);

					if (!isWsp || (result.Count > 0))
					{
						// пропускаем пробелы только в начале (когда result.Count == 0)
						result.Add ((char)nextChar);
						if (!isWsp)
						{
							lastNonWsp = result.Count; // запоминаем позицию последнего символа чтобы не учитывать завершающие пробелы
						}
					}
				}
			}

			return lastNonWsp;
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
