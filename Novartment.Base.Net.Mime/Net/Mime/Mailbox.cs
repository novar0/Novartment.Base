using System;
using System.Collections.Generic;
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
			return ReferenceEquals (first, null) ?
				ReferenceEquals (second, null) :
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
			return !(ReferenceEquals (first, null) ?
				ReferenceEquals (second, null) :
				first.Equals (second));
		}

		/// <summary>
		/// Создаёт почтовый ящик из строкового представления.
		/// </summary>
		/// <param name="source">Строковое представление почтового ящика.</param>
		/// <returns>Почтовый ящик, созданный из строкового представления.</returns>
		public static Mailbox Parse (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (source, AsciiCharClasses.Atom, true, StructuredValueElementType.RoundBracketedValue);
			return Parse (elements);
		}

		/// <summary>
		/// Создаёт почтовый ящик из указанной коллекции элементов значения.
		/// </summary>
		/// <param name="elements">Коллекции элементов значения, которые составляют почтовый ящик.</param>
		/// <returns>Почтовый ящик, созданный из коллекции элементов значения.</returns>
		public static Mailbox Parse (IReadOnlyList<StructuredValueElement> elements)
		{
			if (elements == null)
			{
				throw new ArgumentNullException (nameof (elements));
			}

			Contract.EndContractBlock ();

			// mailbox      = name-addr / addr-spec
			// name-addr    = [display-name] angle-addr
			// angle-addr   = [CFWS] "<" addr-spec ">" [CFWS]
			// display-name = phrase
			var cnt = elements.Count;

			if (cnt < 1)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			if (cnt == 1)
			{
				if ((elements[0].ElementType == StructuredValueElementType.AngleBracketedValue) || // angle-addr
					(elements[0].ElementType == StructuredValueElementType.QuotedValue))
				{
					// non-standard form of addr-spec: "addrs@server.com"
					return new Mailbox (AddrSpec.Parse (elements[0].Value), null);
				}

				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			if ((cnt == 3) &&
				((elements[0].ElementType == StructuredValueElementType.Value) || (elements[0].ElementType == StructuredValueElementType.QuotedValue)) &&
				elements[1].EqualsSeparator ('@') &&
				((elements[2].ElementType == StructuredValueElementType.Value) || (elements[2].ElementType == StructuredValueElementType.SquareBracketedValue)))
			{
				// addr-spec
				var localPart = elements[0].Decode ();
				var domain = elements[2].Decode ();
				var addr = new AddrSpec (localPart, domain);
				return new Mailbox (addr);
			}

			if (elements[cnt - 1].ElementType != StructuredValueElementType.AngleBracketedValue)
			{
				throw new FormatException ("Value does not conform to format 'mailbox'.");
			}

			// [display-name] angle-addr
			var displayName = elements.Decode (elements.Count - 1);
			var addr2 = AddrSpec.Parse (elements[cnt - 1].Value);
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
			return ToString ().GetHashCode ();
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
