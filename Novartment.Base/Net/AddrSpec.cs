using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Интернет-идентификатор типа 'локальная_часть@домен'
	/// согласно формату "addr-spec", описанному в RFC 5322 часть 3.4.1.
	/// </summary>
	public class AddrSpec :
		IEquatable<AddrSpec>
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса AddrSpec с указанными локальной частью и доменом.
		/// </summary>
		/// <param name="localPart">Локальная часть интернет-идентификатора.</param>
		/// <param name="domain">Домен интернет-идентификатора.</param>
		public AddrSpec(string localPart, string domain)
		{
			/*
			RFC 5322 часть 3.4.1:
			addr-spec      = local-part "@" domain
			local-part     = dot-atom / quoted-string
			domain         = dot-atom / domain-literal
			domain-literal = [CFWS] "[" *([FWS] dtext) [FWS] "]" [CFWS]
			dtext          = %d33-90 / %d94-126 / ;  Printable US-ASCII characters not including "[", "]", or "\"

			+ RFC 2047 часть 5
			An 'encoded-word' MUST NOT appear in any portion of an 'addr-spec'
			*/

			if (localPart == null)
			{
				throw new ArgumentNullException(nameof(localPart));
			}

			if (localPart.Length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(localPart));
			}

			if (domain == null)
			{
				throw new ArgumentNullException(nameof(domain));
			}

			if (domain.Length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(domain));
			}

			Contract.EndContractBlock();

			var isLocalPartValidChars = AsciiCharSet.IsAllOfClass(localPart, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
			if (!isLocalPartValidChars)
			{
				throw new ArgumentOutOfRangeException(nameof(localPart));
			}

			var isDomainValidChars = AsciiCharSet.IsAllOfClass(domain, AsciiCharClasses.Domain | AsciiCharClasses.WhiteSpace);
			if (!isDomainValidChars)
			{
				throw new ArgumentOutOfRangeException(nameof(domain));
			}

			this.LocalPart = localPart;
			this.Domain = domain;
		}

		/// <summary>
		/// Получает локальную часть интернет-идентификатора.
		/// </summary>
		public string LocalPart { get; }

		/// <summary>
		/// Получает домен интернет-идентификатора.
		/// </summary>
		public string Domain { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator ==(AddrSpec first, AddrSpec second)
		{
			return ReferenceEquals(first, null) ?
				ReferenceEquals(second, null) :
				first.Equals(second);
		}

		/// <summary>
		/// Определяет неравенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first не равно second; в противном случае — False.</returns>
		public static bool operator !=(AddrSpec first, AddrSpec second)
		{
			return !(ReferenceEquals(first, null) ?
				ReferenceEquals(second, null) :
				first.Equals(second));
		}

		/// <summary>
		/// Создаёт интернет-идентификатор из указанного строкового представления.
		/// </summary>
		/// <param name="source">Строковое представление интернет-идентификатора.</param>
		/// <returns>Интернет-идентификатор, созданный из строкового представления.</returns>
		public static AddrSpec Parse (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var elements = StructuredValueElementCollection.Parse (
				source,
				AsciiCharClasses.Atom,
				true,
				StructuredValueElementType.RoundBracketedValue);
			return Parse (elements);
		}

		/// <summary>
		/// Создаёт интернет-идентификатор из указанной коллекции элементов значения.
		/// </summary>
		/// <param name="elements">Коллекции элементов значения, которые составляют интернет-идентификатор.</param>
		/// <returns>Интернет-идентификатор, созданный из коллекции элементов значения.</returns>
		public static AddrSpec Parse (IReadOnlyList<StructuredValueElement> elements)
		{
			if (elements == null)
			{
				throw new ArgumentNullException (nameof (elements));
			}

			Contract.EndContractBlock ();

			/*
			addr-spec       =  local-part "@" domain
			local-part      =  dot-atom / quoted-string
			domain          =  dot-atom / domain-literal
			domain-literal  =  [CFWS] "[" *([FWS] dtext) [FWS] "]" [CFWS]
			*/

			string localPart;
			string domain;

			if (elements.Count == 1)
			{
				// особый случай для совместимости со старыми реализациями
				localPart = elements[0].Value;
				domain = "localhost";
			}
			else
			{
				if ((elements.Count != 3) ||
					((elements[0].ElementType != StructuredValueElementType.Value) && (elements[0].ElementType != StructuredValueElementType.QuotedValue)) ||
					!elements[1].EqualsSeparator ('@') ||
					((elements[2].ElementType != StructuredValueElementType.Value) && (elements[2].ElementType != StructuredValueElementType.SquareBracketedValue)))
				{
					throw new FormatException ("Value does not conform to format 'addr-spec'.");
				}

				localPart = elements[0].Decode ();
				domain = elements[2].Decode ();
			}

			return new AddrSpec (localPart, domain);
		}

		/// <summary>
		/// Преобразовывает значение объекта в эквивалентное ему строковое представление.
		/// </summary>
		/// <returns>Строковое представление значения объекта.</returns>
		public override string ToString ()
		{
			var localPart = AsciiCharSet.IsValidInternetDomainName (this.LocalPart) ? this.LocalPart : AsciiCharSet.Quote (this.LocalPart);
			return AsciiCharSet.IsValidInternetDomainName (this.Domain) ?
				localPart + "@" + this.Domain :
				localPart + "@[" + this.Domain + "]";
		}

		/// <summary>
		/// Преобразовывает значение объекта в эквивалентное ему строковое представление, окруженное треугольными скобками.
		/// </summary>
		/// <returns>Строковое представление значения объекта, окруженное треугольными скобками.</returns>
		public string ToAngleString ()
		{
			var localPart = AsciiCharSet.IsValidInternetDomainName (this.LocalPart) ? this.LocalPart : AsciiCharSet.Quote (this.LocalPart);
			return AsciiCharSet.IsValidInternetDomainName (this.Domain) ?
				"<" + localPart + "@" + this.Domain + ">" :
				"<" + localPart + "@[" + this.Domain + "]>";
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
			// RFC 3798 part 2.1:
			// The comparison MUST be case-sensitive for the local-part and case-insensitive for the domain part.
			return this.LocalPart.GetHashCode () ^ StringComparer.OrdinalIgnoreCase.GetHashCode (this.Domain);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as AddrSpec;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (AddrSpec other)
		{
			// RFC 3798 part 2.1:
			// The comparison MUST be case-sensitive for the local-part and case-insensitive for the domain part.
			if (other == null)
			{
				return false;
			}

			return
				string.Equals (this.LocalPart, other.LocalPart, StringComparison.Ordinal) &&
				string.Equals (this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase);
		}
	}
}
