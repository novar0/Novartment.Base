using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Почтовый ящик согласно спецификации в RFC 5322 часть 3.4.
	/// </summary>
	public class Mailbox :
		IEquatable<Mailbox>
	{
		// RFC 5322 3.4.
		// mailbox    = name-addr / addr-spec
		// name-addr  = [display-name] angle-addr
		// angle-addr = [CFWS] "&lt;" addr-spec "&gt;" [CFWS]
		// addr-spec  = local-part "@" domain

		/// <summary>
		/// Инициализирует новый экземпляр класса Mailbox с указанным адресом и именем.
		/// </summary>
		/// <param name="address">Строковое представление адреса почтового ящика.</param>
		/// <param name="displayName">Имя почтового ящика. Может быть не указано (значение null).</param>
		public Mailbox (AddrSpec address, string displayName = null)
		{
			if (address == null)
			{
				throw new ArgumentNullException (nameof (address));
			}

			Contract.EndContractBlock ();

			this.Address = address;
			this.Name = displayName;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса Mailbox с указанным адресом и именем.
		/// </summary>
		/// <param name="address">Адрес почтового ящика.</param>
		/// <param name="displayName">Имя почтового ящика. Может быть не указано (значение null).</param>
		public Mailbox (string address, string displayName = null)
		{
			if (address == null)
			{
				throw new ArgumentNullException (nameof (address));
			}

			Contract.EndContractBlock ();

			this.Address = AddrSpec.Parse (address.AsSpan ());
			this.Name = displayName;
		}

		/// <summary>
		/// Получает или устанавливает имя почтового ящика.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает адрес почтового ящика.
		/// </summary>
		public AddrSpec Address { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator == (Mailbox first, Mailbox second)
		{
			return first is null ?
				second is null :
				first.Equals (second);
		}

		/// <summary>
		/// Определяет неравенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first не равно second; в противном случае — False.</returns>
		public static bool operator != (Mailbox first, Mailbox second)
		{
			return !(first is null ?
				second is null :
				first.Equals (second));
		}

		/// <summary>
		/// Создаёт почтовый ящик из указанной коллекции элементов значения.
		/// </summary>
		/// <param name="source">Исходное ASCII-строковое значение.</param>
		/// <returns>Почтовый ящик, созданный из коллекции элементов значения.</returns>
		public static Mailbox Parse (ReadOnlySpan<char> source)
		{
			/*
			mailbox      = name-addr / addr-spec
			name-addr    = [display-name] angle-addr
			angle-addr   = [CFWS] "<" addr-spec ">" [CFWS]
			display-name = phrase
			*/

			var parserPos = 0;
			var token1 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);

			if (!token1.IsValid)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			// один элемент
			var token2 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
			if (token1.IsValid && !token2.IsValid)
			{
				if ((token1.TokenType == StructuredHeaderFieldLexicalTokenType.AngleBracketedValue) || // angle-addr
					(token1.TokenType == StructuredHeaderFieldLexicalTokenType.QuotedValue))
				{
					// non-standard form of addr-spec: "addrs@server.com"
					return new Mailbox (AddrSpec.Parse (source.Slice (token1.Position, token1.Length)), null);
				}

				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			// три элемента
			var token3 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
			var token4 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
			if (!token4.IsValid &&
				((token1.TokenType == StructuredHeaderFieldLexicalTokenType.Value) || (token1.TokenType == StructuredHeaderFieldLexicalTokenType.QuotedValue)) &&
				(token2.TokenType == StructuredHeaderFieldLexicalTokenType.Separator) && (source[token2.Position] == '@') &&
				((token3.TokenType == StructuredHeaderFieldLexicalTokenType.Value) || (token3.TokenType == StructuredHeaderFieldLexicalTokenType.SquareBracketedValue)))
			{
				// addr-spec
				var localPart = token1.Decode (source);
				var domain = token3.Decode (source);
				var addr = new AddrSpec (localPart, domain);
				return new Mailbox (addr);
			}

			// более трёх элементов. считываем как фразу, последний токен которой будет адресом
			parserPos = 0; // сбрасываем декодирование на начало

			string displayName;
			var outPos = 0;
			var prevIsWordEncoded = false;
			StructuredHeaderFieldLexicalToken lastToken = default;
			var outBuf = ArrayPool<char>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
			try
			{
				while (true)
				{
					var token = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
					if (!token.IsValid)
					{
						if (outPos < 1)
						{
							throw new FormatException ("Value does not conform format 'phrase' + <id>.");
						}

						break;
					}

					if (lastToken.IsValid)
					{
						if ((lastToken.TokenType != StructuredHeaderFieldLexicalTokenType.QuotedValue) && (lastToken.TokenType != StructuredHeaderFieldLexicalTokenType.Value))
						{
							throw new FormatException ("Value does not conform to format 'mailbox'.");
						}

						// RFC 2047 часть 6.2:
						// When displaying a particular header field that contains multiple 'encoded-word's,
						// any 'linear-white-space' that separates a pair of adjacent 'encoded-word's is ignored
						var isWordEncoded = lastToken.IsWordEncoded (source);
						if ((outPos > 0) && (!prevIsWordEncoded || !isWordEncoded))
						{
							// RFC 5322 часть 3.2.2:
							// Runs of FWS, comment, or CFWS that occur between lexical elements in a structured header field
							// are semantically interpreted as a single space character.
							outBuf[outPos++] = ' ';
						}

						outPos += lastToken.Decode (source, outBuf.AsSpan (outPos));
						prevIsWordEncoded = isWordEncoded;
					}

					lastToken = token;
				}

#if NETSTANDARD2_0
				displayName = new string (outBuf, 0, outPos);
#else
				displayName = new string (outBuf.AsSpan (0, outPos));
#endif
			}
			finally
			{
				ArrayPool<char>.Shared.Return (outBuf);
			}

			if (lastToken.TokenType != StructuredHeaderFieldLexicalTokenType.AngleBracketedValue)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			var addr2 = AddrSpec.Parse (source.Slice (lastToken.Position, lastToken.Length));
			return new Mailbox (addr2, displayName);
		}

		/// <summary>
		/// Получает строковое представление объекта.
		/// </summary>
		/// <returns>Строковое представление объекта.</returns>
		public override string ToString ()
		{
			return (this.Name == null) ?
				"<" + this.Address + ">" :
				this.Name + " <" + this.Address + ">";
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
#if NETSTANDARD2_0
			return ToString ().GetHashCode ();
#else
			return ToString ().GetHashCode (StringComparison.Ordinal);
#endif
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as Mailbox;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (Mailbox other)
		{
			if (other == null)
			{
				return false;
			}

			return string.Equals (this.Name, other.Name, StringComparison.Ordinal) && this.Address.Equals (other.Address);
		}
	}
}
