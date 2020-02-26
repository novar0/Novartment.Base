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
		// display-name = phrase

		private static readonly StructuredStringParser DotAtomParser = new StructuredStringParser (
			AsciiCharClasses.WhiteSpace,
			AsciiCharClasses.Atom,
			true,
			StructuredStringParser.StructuredHeaderFieldBodyFormats);

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
			StructuredStringToken token1;
			do
			{
				token1 = DotAtomParser.Parse (source, ref parserPos);
			} while (token1.IsRoundBracketedValue (source));

			if (!token1.IsValid)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			StructuredStringToken token2;
			do
			{
				token2 = DotAtomParser.Parse (source, ref parserPos);
			} while (token2.IsRoundBracketedValue (source));

			// один элемент
			if (token1.IsValid && !token2.IsValid)
			{
				if (token1.IsAngleBracketedValue (source) || token1.IsDoubleQuotedValue (source))
				{
					// non-standard form of addr-spec: "addrs@server.com"
					return new Mailbox (AddrSpec.Parse (source.Slice (token1.Position + 1, token1.Length - 2)), null);
				}

				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			StructuredStringToken token3;
			do
			{
				token3 = DotAtomParser.Parse (source, ref parserPos);
			} while (token3.IsRoundBracketedValue (source));

			StructuredStringToken token4;
			do
			{
				token4 = DotAtomParser.Parse (source, ref parserPos);
			} while (token4.IsRoundBracketedValue (source));

			// три элемента
			if (!token4.IsValid &&
				((token1.TokenType == StructuredStringTokenType.Value) || (token1.IsDoubleQuotedValue (source))) &&
				token2.IsSeparator (source, '@') &&
				((token3.TokenType == StructuredStringTokenType.Value) || token3.IsSquareBracketedValue (source)))
			{
				/*
				RFC 5322 part 3.4:
				There is an alternate simple form of a mailbox where
				the addr-spec address appears alone, without the recipient's name or
				the angle brackets.
				*/

				return new Mailbox (AddrSpec.Parse (source.Slice (token1.Position, token3.Position + token3.Length - token1.Position)), null);
			}

			// более трёх элементов. считываем как фразу, последний токен которой будет адресом
			parserPos = 0; // сбрасываем декодирование на начало

			string displayName;
			var outPos = 0;
			var prevIsWordEncoded = false;
			StructuredStringToken lastToken = default;
			var outBuf = ArrayPool<char>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
			try
			{
				while (true)
				{
					StructuredStringToken token;
					do
					{
						token = DotAtomParser.Parse (source, ref parserPos);
					} while (token.IsRoundBracketedValue (source));

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
						if (!lastToken.IsDoubleQuotedValue (source) && (lastToken.TokenType != StructuredStringTokenType.Value))
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

						outPos += DecodeToken (lastToken, source, outBuf.AsSpan (outPos));
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

			if (!lastToken.IsAngleBracketedValue (source))
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			var addr2 = AddrSpec.Parse (source.Slice (lastToken.Position + 1, lastToken.Length - 2));
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

		private static int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> destination)
		{
			switch (token.TokenType)
			{
				case StructuredStringTokenType.DelimitedValue when (token.IsSquareBracketedValue (source) || token.IsDoubleQuotedValue (source)):
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

						destination[idx++] = ch;
					}

					return idx;

				case StructuredStringTokenType.Separator:
					destination[0] = source[token.Position];
					return 1;

				case StructuredStringTokenType.Value:
					var src = source.Slice (token.Position, token.Length);
					var isWordEncoded = (src.Length > 8) &&
						(src[0] == '=') &&
						(src[1] == '?') &&
						(src[src.Length - 2] == '?') &&
						(src[src.Length - 1] == '=');
					if (!isWordEncoded)
					{
						src.CopyTo (destination);
						return src.Length;
					}

					// декодируем 'encoded-word'
					var resultSize = Rfc2047EncodedWord.Parse (src, destination);
					return resultSize;

				default:
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Token of type '{token.TokenType}' is complex and can not be decoded to discrete value."));
			}
		}
	}
}
