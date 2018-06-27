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
	public abstract class HeaderFieldBuilder
	{
		private readonly IAdjustableList<HeaderFieldParameter> _parameters = new ArrayList<HeaderFieldParameter> ();

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilder с указанным именем и набором частей значения поля.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		protected HeaderFieldBuilder (HeaderFieldName name)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			Contract.EndContractBlock ();

			this.Name = name;
		}

		public HeaderFieldName Name { get; }

		internal abstract IReadOnlyList<string> GetParts ();

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
			var name = HeaderFieldNameHelper.GetName (this.Name);
			var lineLen = name.Length;
			AsciiCharSet.GetBytes (name.AsSpan (), buf);
			buf[lineLen++] = (byte)':';
			var outPos = lineLen;

			// кодируем все части значения тела
			var valueParts = GetParts ();
			for (var idx = 0; idx < valueParts.Count; idx++)
			{
				// если есть параметры то последнюю часть значения дополняем знаком ';'
				var extraSemicolon = (_parameters.Count > 0) && (idx == (valueParts.Count - 1));
				var part = valueParts[idx];
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

				var pos = 0;
				var segmentIdx = 0;
				while (true)
				{
					var element = HeaderFieldBodyParameterEncoder.GetNextSegment (parameter.Name, parameterValueBytes, segmentIdx, ref pos);
					segmentIdx++;
					if (element == null)
					{
						break;
					}

					var isLastSegment = pos >= parameterValueBytes.Length;

					// дополняем знаком ';' все части всех параметров кроме последней
					var extraSemicolon = isLastSegment && (idx != (_parameters.Count - 1));
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

	public class HeaderFieldBuilderExactValue : HeaderFieldBuilder
	{
		private readonly string _value;

		/// <summary>
		/// Создает поле заголовка из указанного значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">Значение поля заголовка.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderExactValue (HeaderFieldName name, string value)
			: base (name)
		{
			_value = value;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			return ReadOnlyList.Repeat (_value, 1);
		}
	}

	public class HeaderFieldBuilderUnstructured : HeaderFieldBuilder
	{
		private readonly string _text;

		/// <summary>
		/// Создает поле заголовка из указанного значения типа 'unstructured'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'unstructured'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderUnstructured (HeaderFieldName name, string text)
			: base (name)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			_text = text;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			HeaderEncoder.EncodeUnstructured (result, _text);
			return result;
		}
	}

	public class HeaderFieldBuilderPhrase : HeaderFieldBuilder
	{
		private readonly string _text;

		/// <summary>
		/// Создает поле заголовка из указанного значения типа 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderPhrase (HeaderFieldName name, string text)
			: base (name)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			_text = text;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			HeaderEncoder.EncodePhrase (result, _text);
			return result;
		}
	}

	public class HeaderFieldBuilderMailbox : HeaderFieldBuilder
	{
		private readonly Mailbox _mailbox;

		/// <summary>
		/// Создает поле заголовка из Mailbox.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="mailbox">Mailbox.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderMailbox (HeaderFieldName name, Mailbox mailbox)
			: base (name)
		{
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			_mailbox = mailbox;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			HeaderEncoder.EncodeMailbox (result, _mailbox);
			return result;
		}
	}

	public class HeaderFieldBuilderLanguageList : HeaderFieldBuilder
	{
		private readonly IReadOnlyCollection<string> _languages;

		/// <summary>
		/// Создает поле заголовка из указанной коллекции значений-идентификаторов языка.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="languages">Коллекция значений-идентификаторов языка.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderLanguageList (HeaderFieldName name, IReadOnlyCollection<string> languages)
			: base (name)
		{
			if (languages == null)
			{
				throw new ArgumentNullException (nameof (languages));
			}

			Contract.EndContractBlock ();

			_languages = languages;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			return _languages.AppendSeparator (',');
		}
	}

	public class HeaderFieldBuilderAddrSpecList : HeaderFieldBuilder
	{
		private readonly IReadOnlyCollection<AddrSpec> _addrSpecs;

		/// <summary>
		/// Создает поле заголовка из указанной коллекции интернет-идентификаторов.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="addrSpecs">Коллекция языков в формате интернет-идентификаторов.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderAddrSpecList (HeaderFieldName name, IReadOnlyCollection<AddrSpec> addrSpecs)
			: base (name)
		{
			if (addrSpecs == null)
			{
				throw new ArgumentNullException (nameof (addrSpecs));
			}

			Contract.EndContractBlock ();

			_addrSpecs = addrSpecs;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			return _addrSpecs.Select (item => item.ToAngleString ()).ToList ();
		}
	}

	public class HeaderFieldBuilderAtomAndUnstructured : HeaderFieldBuilder
	{
		private readonly string _type;
		private readonly string _value;

		/// <summary>
		/// Создает поле заголовка из типа и 'unstructured'-значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="type">Тип (значение типа 'atom').</param>
		/// <param name="value">'unstructured' значение.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderAtomAndUnstructured (HeaderFieldName name, string type, string value)
			: base (name)
		{
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

			_type = type;
			_value = value;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string>
			{
				_type + ";",
			};
			if (_value != null)
			{
				HeaderEncoder.EncodeUnstructured (result, _value);
			}

			return result;
		}
	}

	public class HeaderFieldBuilderUnstructuredPair : HeaderFieldBuilder
	{
		private readonly string _value1;
		private readonly string _value2;

		/// <summary>
		/// Создает поле заголовка из двух 'unstructured' значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value1">Обязательное 'unstructured' значение 1.</param>
		/// <param name="value2">Необязательное 'unstructured' значение 2.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderUnstructuredPair (HeaderFieldName name, string value1, string value2)
			: base (name)
		{
			if (value1 == null)
			{
				throw new ArgumentNullException (nameof (value1));
			}

			Contract.EndContractBlock ();

			_value1 = value1;
			_value2 = value2;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			HeaderEncoder.EncodeUnstructured (result, _value1);
			var isValue2Empty = string.IsNullOrEmpty (_value2);
			if (isValue2Empty)
			{
				return result;
			}

			result[result.Count - 1] += ';';

			HeaderEncoder.EncodeUnstructured (result, _value2);

			return result;
		}
	}

	public class HeaderFieldBuilderTokensAndDate : HeaderFieldBuilder
	{
		private readonly string _value;
		private readonly DateTimeOffset _dateTimeOffset;

		/// <summary>
		/// Создает поле заголовка из '*tokens' значения и даты.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">'*tokens' значение.</param>
		/// <param name="dateTimeOffset">Дата.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderTokensAndDate (HeaderFieldName name, string value, DateTimeOffset dateTimeOffset)
			: base (name)
		{
			_value = value;
			_dateTimeOffset = dateTimeOffset;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var tokens = new ArrayList<string> ();
			HeaderEncoder.EncodeTokens (tokens, _value);
			if (tokens.Count > 0)
			{
				tokens[tokens.Count - 1] += ';';
			}
			else
			{
				tokens.Add (";");
			}

			tokens.Add (_dateTimeOffset.ToInternetString ());
			return tokens;
		}
	}

	public class HeaderFieldBuilderPhraseAndId : HeaderFieldBuilder
	{
		private readonly string _id;
		private readonly string _phrase;

		/// <summary>
		/// Создает поле заголовка из идентификатора и 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="id">Идентификатор (значение типа 'dot-atom-text').</param>
		/// <param name="phrase">Произвольная 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderPhraseAndId (HeaderFieldName name, string id, string phrase = null)
			: base (name)
		{
			if (id == null)
			{
				throw new ArgumentNullException (nameof (id));
			}

			if (!AsciiCharSet.IsValidInternetDomainName (id))
			{
				throw new ArgumentOutOfRangeException (nameof (id), FormattableString.Invariant ($"Invalid value for type 'atom': \"{id}\"."));
			}

			Contract.EndContractBlock ();

			_id = id;
			_phrase = phrase;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var valueParts = new ArrayList<string> ();
			if (_phrase != null)
			{
				HeaderEncoder.EncodePhrase (valueParts, _phrase);
			}

			valueParts.Add ("<" + _id + ">");

			return valueParts;
		}
	}

	public class HeaderFieldBuilderPhraseList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _values;

		/// <summary>
		/// Создает поле заголовка из коллекции 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="values">Коллекция 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderPhraseList (HeaderFieldName name, IReadOnlyList<string> values)
			: base (name)
		{
			if (values == null)
			{
				throw new ArgumentNullException (nameof (values));
			}

			Contract.EndContractBlock ();

			_values = values;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			for (var idx = 0; idx < _values.Count; idx++)
			{
				HeaderEncoder.EncodePhrase (result, _values[idx]);
				if (idx < (_values.Count - 1))
				{
					result[result.Count - 1] += ',';
				}
			}

			return result;
		}
	}

	public class HeaderFieldBuilderMailboxList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<Mailbox> _mailboxes;

		/// <summary>
		/// Создает поле заголовка из коллекции Mailbox.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="mailboxes">Коллекция Mailbox.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderMailboxList (HeaderFieldName name, IReadOnlyList<Mailbox> mailboxes)
			: base (name)
		{
			if (mailboxes == null)
			{
				throw new ArgumentNullException (nameof (mailboxes));
			}

			Contract.EndContractBlock ();

			_mailboxes = mailboxes;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			for (var idx = 0; idx < _mailboxes.Count; idx++)
			{
				HeaderEncoder.EncodeMailbox (result, _mailboxes[idx]);
				if (idx < (_mailboxes.Count - 1))
				{
					result[result.Count - 1] += ',';
				}
			}

			return result;
		}
	}

	public class HeaderFieldBuilderAngleBracketedList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _urls;

		/// <summary>
		/// Создает поле заголовка из коллекции url-значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="urls">Коллекция url-значений.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderAngleBracketedList (HeaderFieldName name, IReadOnlyList<string> urls)
			: base (name)
		{
			if (urls == null)
			{
				throw new ArgumentNullException (nameof (urls));
			}

			Contract.EndContractBlock ();

			// TODO: добавить валидацию каждого значения в urls
			_urls = urls;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var result = new ArrayList<string> ();
			for (var idx = 0; idx < _urls.Count; idx++)
			{
				if (idx < (_urls.Count - 1))
				{
					result.Add ("<" + _urls[idx] + ">,");
				}
				else
				{
					result.Add ("<" + _urls[idx] + ">");
				}
			}

			return result;
		}
	}

	public class HeaderFieldBuilderDisposition : HeaderFieldBuilder
	{
		private readonly string _actionMode;
		private readonly string _sendingMode;
		private readonly string _type;
		private readonly IReadOnlyCollection<string> _modifiers;

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
		public HeaderFieldBuilderDisposition (HeaderFieldName name, string actionMode, string sendingMode, string type, IReadOnlyCollection<string> modifiers)
			: base (name)
		{
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

			_actionMode = actionMode;
			_sendingMode = sendingMode;
			_type = type;
			_modifiers = modifiers;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			var type = _type;
			if (_modifiers?.Count > 0)
			{
				foreach (var modifier in _modifiers)
				{
					var isAtom = AsciiCharSet.IsAllOfClass (modifier, AsciiCharClasses.Atom);
					if (!isAtom)
					{
						throw new FormatException (FormattableString.Invariant ($"Invalid value for type 'atom': \"{modifier}\"."));
					}
				}

				type += "/" + string.Join (",", _modifiers);
			}

			return new ArrayList<string> (2) { _actionMode + "/" + _sendingMode + ";", type };
		}
	}

	public class HeaderFieldBuilderDispositionNotificationParameterList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<DispositionNotificationParameter> _parameters;

		/// <summary>
		/// Создает поле заголовка из указанной коллекции DispositionNotificationParameter.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="parameters">Коллекция DispositionNotificationParameter.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName name, IReadOnlyList<DispositionNotificationParameter> parameters)
			: base (name)
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

			_parameters = parameters;
		}

		internal override IReadOnlyList<string> GetParts ()
		{
			// TODO: добавить валидацию каждого значения в parameters
			var result = new ArrayList<string> ();
			for (var idx = 0; idx < _parameters.Count; idx++)
			{
				if (idx < (_parameters.Count - 1))
				{
					result.Add (_parameters[idx].ToString () + ";");
				}
				else
				{
					result.Add (_parameters[idx].ToString ());
				}
			}

			return result;
		}
	}
}
