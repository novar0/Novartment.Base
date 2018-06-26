using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Text;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из отдельных частей значения и параметров.
	/// </summary>
	public class HeaderFieldBuilder
	{
		private readonly HeaderFieldName _name;
		private readonly IAdjustableList<HeaderFieldParameter> _parameters = new ArrayList<HeaderFieldParameter> ();
		private readonly IReadOnlyList<string> _valueParts;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilder с указанным именем и набором частей значения поля.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="valueParts">Набор частей значения поля заголовка.</param>
		private HeaderFieldBuilder (HeaderFieldName name, IReadOnlyList<string> valueParts)
		{
			_name = name;
			_valueParts = valueParts;
		}

		public HeaderFieldName Name => _name;

		public IReadOnlyList<string> ValueParts => _valueParts;

		/// <summary>
		/// Создает поле заголовка из указанного значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">Значение поля заголовка.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateExactValue (HeaderFieldName name, string value)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			Contract.EndContractBlock ();

			return new HeaderFieldBuilder (name, ReadOnlyList.Repeat (value, 1));
		}

		/// <summary>
		/// Создает поле заголовка из указанной коллекции значений-идентификаторов языка.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="languages">Коллекция значений-идентификаторов языка.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateLanguageList (HeaderFieldName name, IReadOnlyCollection<string> languages)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (languages == null)
			{
				throw new ArgumentNullException (nameof (languages));
			}

			Contract.EndContractBlock ();

			return new HeaderFieldBuilder (name, languages.AppendSeparator (','));
		}

		/// <summary>
		/// Создает поле заголовка из указанной коллекции интернет-идентификаторов.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="addrSpecs">Коллекция языков в формате интернет-идентификаторов.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateAddrSpecList (HeaderFieldName name, IReadOnlyCollection<AddrSpec> addrSpecs)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (addrSpecs == null)
			{
				throw new ArgumentNullException (nameof (addrSpecs));
			}

			Contract.EndContractBlock ();

			return new HeaderFieldBuilder (name, addrSpecs.Select (item => item.ToAngleString ()).ToList ());
		}

		/// <summary>
		/// Создает поле заголовка из указанного значения типа 'unstructured'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'unstructured'.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateUnstructured (HeaderFieldName name, string text)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string> ();
			HeaderEncoder.EncodeUnstructured (result, text);
			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из указанного значения типа 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreatePhrase (HeaderFieldName name, string text)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string> ();
			HeaderEncoder.EncodePhrase (result, text);
			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из типа и 'unstructured'-значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="type">Тип (значение типа 'atom').</param>
		/// <param name="value">'unstructured' значение.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateAtomAndUnstructured (HeaderFieldName name, string type, string value)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			var isValidAtom = AsciiCharSet.IsAllOfClass (type, AsciiCharClasses.Atom);
			if (!isValidAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string>
			{
				type + ";",
			};
			if (value != null)
			{
				HeaderEncoder.EncodeUnstructured (result, value);
			}

			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из двух 'unstructured' значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value1">Обязательное 'unstructured' значение 1.</param>
		/// <param name="value2">Необязательное 'unstructured' значение 2.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateUnstructuredPair (HeaderFieldName name, string value1, string value2)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (value1 == null)
			{
				throw new ArgumentNullException (nameof (value1));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string> ();
			HeaderEncoder.EncodeUnstructured (result, value1);
			var isValue2Empty = string.IsNullOrEmpty (value2);
			if (isValue2Empty)
			{
				return new HeaderFieldBuilder (name, result);
			}

			result[result.Count - 1] += ';';

			HeaderEncoder.EncodeUnstructured (result, value2);

			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из '*tokens' значения и даты.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">'*tokens' значение.</param>
		/// <param name="dateTimeOffset">Дата.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateTokensAndDate (HeaderFieldName name, string value, DateTimeOffset dateTimeOffset)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			Contract.EndContractBlock ();

			var tokens = new ArrayList<string> ();
			HeaderEncoder.EncodeTokens (tokens, value);
			if (tokens.Count > 0)
			{
				tokens[tokens.Count - 1] += ';';
			}
			else
			{
				tokens.Add (";");
			}

			tokens.Add (dateTimeOffset.ToInternetString ());
			return new HeaderFieldBuilder (name, tokens);
		}

		/// <summary>
		/// Создает поле заголовка из идентификатора и 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="id">Идентификатор (значение типа 'dot-atom-text').</param>
		/// <param name="phrase">Произвольная 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreatePhraseAndId (HeaderFieldName name, string id, string phrase = null)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (id == null)
			{
				throw new ArgumentNullException (nameof (id));
			}

			Contract.EndContractBlock ();

			var isValidName = AsciiCharSet.IsValidInternetDomainName (id);
			if (!isValidName)
			{
				throw new ArgumentOutOfRangeException (nameof (id), FormattableString.Invariant ($"Invalid value for type 'atom': \"{id}\"."));
			}

			var valueParts = new ArrayList<string> ();
			if (phrase != null)
			{
				HeaderEncoder.EncodePhrase (valueParts, phrase);
			}

			valueParts.Add ("<" + id + ">");

			return new HeaderFieldBuilder (name, valueParts);
		}

		/// <summary>
		/// Создает поле заголовка из коллекции 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="values">Коллекция 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreatePhraseList (HeaderFieldName name, IReadOnlyList<string> values)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (values == null)
			{
				throw new ArgumentNullException (nameof (values));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string> ();
			for (var idx = 0; idx < values.Count; idx++)
			{
				HeaderEncoder.EncodePhrase (result, values[idx]);
				if (idx < (values.Count - 1))
				{
					result[result.Count - 1] += ',';
				}
			}

			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из Mailbox.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="mailbox">Mailbox.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateMailbox (HeaderFieldName name, Mailbox mailbox)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string> ();
			HeaderEncoder.EncodeMailbox (result, mailbox);
			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из коллекции Mailbox.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="mailboxes">Коллекция Mailbox.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateMailboxList (HeaderFieldName name, IReadOnlyList<Mailbox> mailboxes)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (mailboxes == null)
			{
				throw new ArgumentNullException (nameof (mailboxes));
			}

			Contract.EndContractBlock ();

			var result = new ArrayList<string> ();
			for (var idx = 0; idx < mailboxes.Count; idx++)
			{
				HeaderEncoder.EncodeMailbox (result, mailboxes[idx]);
				if (idx < (mailboxes.Count - 1))
				{
					result[result.Count - 1] += ',';
				}
			}

			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из коллекции url-значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="urls">Коллекция url-значений.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateAngleBracketedList (HeaderFieldName name, IReadOnlyList<string> urls)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (urls == null)
			{
				throw new ArgumentNullException (nameof (urls));
			}

			Contract.EndContractBlock ();

			// TODO: добавить валидацию каждого значения в urls
			var result = new ArrayList<string> ();
			for (var idx = 0; idx < urls.Count; idx++)
			{
				if (idx < (urls.Count - 1))
				{
					result.Add ("<" + urls[idx] + ">,");
				}
				else
				{
					result.Add ("<" + urls[idx] + ">");
				}
			}

			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Создает поле заголовка из трех атомов плюс опциональной коллекции атомов
		/// в формате 'actionMode/sendingMode; type/4_1,4_2,4_3,4_4 ...'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="actionMode">Обязательное значение 1.</param>
		/// <param name="sendingMode">Обязательное значение 2.</param>
		/// <param name="type">Обязательное значение 3.</param>
		/// <param name="modifiers">Необязательная коллекция значений.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateDisposition (
			HeaderFieldName name,
			string actionMode,
			string sendingMode,
			string type,
			IReadOnlyCollection<string> modifiers)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (actionMode == null)
			{
				throw new ArgumentNullException (nameof (actionMode));
			}

			if (sendingMode == null)
			{
				throw new ArgumentNullException (nameof (sendingMode));
			}

			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			var isAtom = AsciiCharSet.IsAllOfClass (actionMode, AsciiCharClasses.Atom);
			if (!isAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (actionMode));
			}

			isAtom = AsciiCharSet.IsAllOfClass (sendingMode, AsciiCharClasses.Atom);
			if (!isAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (sendingMode));
			}

			isAtom = AsciiCharSet.IsAllOfClass (type, AsciiCharClasses.Atom);
			if (!isAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			Contract.EndContractBlock ();

			if (modifiers?.Count > 0)
			{
				foreach (var modifier in modifiers)
				{
					isAtom = AsciiCharSet.IsAllOfClass (modifier, AsciiCharClasses.Atom);
					if (!isAtom)
					{
						throw new FormatException (FormattableString.Invariant ($"Invalid value for type 'atom': \"{modifier}\"."));
					}
				}

				type += "/" + string.Join (",", modifiers);
			}

			return new HeaderFieldBuilder (
				name,
				new ArrayList<string> (2) { actionMode + "/" + sendingMode + ";", type });
		}

		/// <summary>
		/// Создает поле заголовка из указанной коллекции DispositionNotificationParameter.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="parameters">Коллекция DispositionNotificationParameter.</param>
		/// <returns>Поле заголовка.</returns>
		public static HeaderFieldBuilder CreateDispositionNotificationParameterList (
			HeaderFieldName name,
			IReadOnlyList<DispositionNotificationParameter> parameters)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (parameters == null)
			{
				throw new ArgumentNullException (nameof (parameters));
			}

			Contract.EndContractBlock ();

			// TODO: добавить валидацию каждого значения в parameters
			var result = new ArrayList<string> ();
			for (var idx = 0; idx < parameters.Count; idx++)
			{
				if (idx < (parameters.Count - 1))
				{
					result.Add (parameters[idx].ToString () + ";");
				}
				else
				{
					result.Add (parameters[idx].ToString ());
				}
			}

			return new HeaderFieldBuilder (name, result);
		}

		/// <summary>
		/// Добавляет параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public void AddParameter (string name, string value)
		{
			_parameters.Add (new HeaderFieldParameter (name, value));
		}

		/// <summary>
		/// Генерирует двоичное представление поле заголовка для передачи по протоколу.
		/// Производится фолдинг по указанной длине строки.
		/// </summary>
		/// <param name="buf">Буфер куда будет сгнерировано тело.</param>
		/// <param name="maxLineLength">Максимальная длина строки, по которой будет производиться фолдинг значения поля заголовка.</param>
		/// <returns>Количество байт, записанных в буфер.</returns>
		public int EncodeToBinaryTransportRepresentation (Span<byte> buf, int maxLineLength)
		{
			// RFC 5322:
			// 1) FWS (the folding white space token) indicates a place where folding may take place.
			// 1) token for "CFWS" is defined for places where comments and/or FWS can occur.
			// 2) a CarriageReturnLinefeed may be inserted before any WSP in FWS or CFWS.
			// 3) CarriageReturnLinefeed MUST NOT be inserted in such a way that any line of a folded header field is made up entirely of WSP characters and nothing else.

			// формируем склеенную из частей строку вставляя где надо переводы строки и пробелы
			var name = HeaderFieldNameHelper.GetName (_name);
			var lineLen = name.Length;
			AsciiCharSet.GetBytes (name.AsSpan (), buf);
			buf[lineLen++] = (byte)':';
			var outPos = lineLen;

			// кодируем все части значения тела
			for (var idx = 0; idx < _valueParts.Count; idx++)
			{
				// если есть параметры то последнюю часть значения дополняем знаком ';'
				var extraSemicolon = (_parameters.Count > 0) && (idx == (_valueParts.Count - 1));
				var part = _valueParts[idx];
				if (part != null)
				{
					CreatePart (part, extraSemicolon, buf, maxLineLength, ref outPos, ref lineLen);
				}
			}

			// кодируем все параметры
			for (var idx = 0; idx < _parameters.Count; idx++)
			{
				var parameter = _parameters[idx];

				// кодируем все сегменты параметра
				var parameterValueBytes = Encoding.UTF8.GetBytes (parameter.Value);
				var segments = EstimatingEncoder.CutBySize (parameter.Name, parameterValueBytes);
				var segmentIdx = 0;
				while (segmentIdx < segments.Length)
				{
					var element = HeaderFieldBodyParameterEncoder.GetNextSegment (parameter.Name, parameterValueBytes, segments, ref segmentIdx);

					// дополняем знаком ';' все части всех параметров кроме последней
					var extraSemicolon = (segmentIdx >= segments.Length) && (idx != (_parameters.Count - 1));
					CreatePart (element, extraSemicolon, buf, maxLineLength, ref outPos, ref lineLen);
				}
			}

			buf[outPos++] = (byte)'\r';
			buf[outPos++] = (byte)'\n';

			return outPos;
		}

		private void CreatePart (string part, bool extraSemicolon, Span<byte> buf, int maxLineLength, ref int outPos, ref int lineLen)
		{
			var partLength = part.Length + (extraSemicolon ? 1 : 0);
			if (part.Length < 1)
			{
				return;
			}

			var needWhiteSpace = (part[0] != ' ') && (part[0] != '\t');
			if (needWhiteSpace)
			{
				partLength++;
			}

			lineLen += partLength;
			if (lineLen > maxLineLength)
			{
				// если накопленная строка с добавлением новой части превысит maxLineLength, то перед новой частью добавляем перевод строки
				lineLen = partLength + 1; // плюс пробел
				buf[outPos++] = (byte)'\r';
				buf[outPos++] = (byte)'\n';
			}

			if (needWhiteSpace)
			{
				buf[outPos++] = (byte)' ';
			}

#if NETCOREAPP2_1
			AsciiCharSet.GetBytes (part, buf.Slice (outPos));
#else
			AsciiCharSet.GetBytes (part.AsSpan (), buf.Slice (outPos));
#endif
			outPos += part.Length;

			if (extraSemicolon)
			{
				buf[outPos++] = (byte)';';
			}
		}
	}
}
