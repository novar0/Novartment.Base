using System;
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

			this.Address = AddrSpec.Parse (address);
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
		public static Mailbox Parse (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return Parse (source.AsSpan ());
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
			var element1 = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);

			if (!element1.IsValid)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			// один элемент
			var element2 = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
			if (element1.IsValid && !element2.IsValid)
			{
				if ((element1.ElementType == StructuredValueElementType.AngleBracketedValue) || // angle-addr
					(element1.ElementType == StructuredValueElementType.QuotedValue))
				{
					// non-standard form of addr-spec: "addrs@server.com"
					return new Mailbox (AddrSpec.Parse (source.Slice (element1.StartPosition, element1.Length)), null);
				}

				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			// три элемента
			var element3 = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
			var element4 = StructuredValueParser.GetNextElementDotAtom (source, ref parserPos);
			if (!element4.IsValid &&
				((element1.ElementType == StructuredValueElementType.Value) || (element1.ElementType == StructuredValueElementType.QuotedValue)) &&
				(element2.ElementType == StructuredValueElementType.Separator) && (element2.Length == 1) && (source[element2.StartPosition] == '@') &&
				((element3.ElementType == StructuredValueElementType.Value) || (element3.ElementType == StructuredValueElementType.SquareBracketedValue)))
			{
				// addr-spec
				var localPart = element1.Decode (source);
				var domain = element3.Decode (source);
				var addr = new AddrSpec (localPart, domain);
				return new Mailbox (addr);
			}

			// более трёх элементов
			parserPos = 0; // сбрасываем декодирование на начало
			var decoder = new StructuredValuePhraseDecoder ();
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
					decoder.AddElement (source, lastElement);
				}

				lastElement = element;
			}

			if (lastElement.ElementType != StructuredValueElementType.AngleBracketedValue)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			// [display-name] angle-addr
			var displayName = decoder.GetResult ();
			var addr2 = AddrSpec.Parse (source.Slice (lastElement.StartPosition, lastElement.Length));
			return new Mailbox (addr2, displayName);
		}

		/// <summary>
		/// Получает строковое представление объекта.
		/// </summary>
		/// <returns>Строковое представление объекта.</returns>
		public override string ToString ()
		{
			return (this.Name == null) ?
				this.Address.ToAngleString () :
				this.Name + " " + this.Address.ToAngleString ();
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
#if NETCOREAPP2_1
			return ToString ().GetHashCode (StringComparison.Ordinal);
#else
			return ToString ().GetHashCode ();
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
