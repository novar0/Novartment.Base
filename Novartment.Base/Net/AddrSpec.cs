using System;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net
{
	/// <summary>
	/// An Internet identifier of the form 'local-part@domain'
	/// in accordance with the "addr-spec" format described in RFC 5322 part 3.4.1.
	/// </summary>
	public class AddrSpec :
		IEquatable<AddrSpec>
	{
		internal class TokenFormatComment : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatComment () : base ('(', ')', StructuredStringIngoreTokenType.EscapedChar, true) { }
		}

		internal class TokenFormatId : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatId () : base ('<', '>', StructuredStringIngoreTokenType.QuotedValue, false) { }
		}

		internal class TokenFormatLiteral : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatLiteral () : base ('[', ']', StructuredStringIngoreTokenType.EscapedChar, false) { }

			public override int DecodeToken (ReadOnlySpan<char> source, Span<char> buffer)
			{
				int dstIdx = 0;
				var endPos = source.Length - 1;
				for (var srcIdx = 1; srcIdx < endPos; srcIdx++)
				{
					var ch = source[srcIdx];
					if (ch == '\\')
					{
						srcIdx++;
						ch = source[srcIdx];
					}

					buffer[dstIdx++] = ch;
				}

				return dstIdx;
			}
		}

		private static readonly StructuredStringFormat DotAtomFormat = new StructuredStringFormat (
			AsciiCharClasses.WhiteSpace,
			AsciiCharClasses.Atom,
			'.',
			new StructuredStringTokenCustomFormat[] { new StructuredStringTokenQuotedStringFormat (), new TokenFormatComment (), new TokenFormatId (), new TokenFormatLiteral () });

		/// <summary>
		/// Initializes a new instance of the AddrSpec class
		/// the with the specified locally interpreted part and the domain.
		/// </summary>
		/// <param name="localPart">
		/// The locally interpreted part of the Internet identifier.
		/// Only printable US-ASCII characters are allowed.
		/// The maximum length of a local-part is 64.
		/// </param>
		/// <param name="domain">
		/// The domain of the Internet identifier.
		/// Only printable US-ASCII characters not including "[", "]", or "\" are allowed.
		/// The maximum length of a domain is 255.
		/// </param>
		public AddrSpec (string localPart, string domain)
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
				throw new ArgumentNullException (nameof (localPart));
			}

			// RFC 5321 4.5.3.1.1: The maximum total length of a user name or other local-part is 64 octets.
			if ((localPart.Length < 1) || (localPart.Length > 64))
			{
				throw new ArgumentOutOfRangeException (nameof (localPart));
			}

			if (domain == null)
			{
				throw new ArgumentNullException (nameof (domain));
			}

			// RFC 5321 4.5.3.1.2: The maximum total length of a domain name or number is 255 octets.
			if ((domain.Length < 1) || (domain.Length > 255))
			{
				throw new ArgumentOutOfRangeException (nameof (domain));
			}

			Contract.EndContractBlock ();

			var isLocalPartValidChars = AsciiCharSet.IsAllOfClass (localPart, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
			if (!isLocalPartValidChars)
			{
				throw new ArgumentOutOfRangeException (nameof (localPart));
			}

			var isDomainValidChars = AsciiCharSet.IsAllOfClass (domain, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace) &&
				(domain.IndexOfAny (new char[] { '[', ']', '\\' }) < 0);
			if (!isDomainValidChars)
			{
				throw new ArgumentOutOfRangeException (nameof (domain));
			}

			this.LocalPart = localPart;
			this.Domain = domain;
		}

		/// <summary>
		/// Gets the locally interpreted part of the Internet identifier.
		/// </summary>
		public string LocalPart { get; }

		/// <summary>
		/// Gets the domain of the Internet identifier.
		/// </summary>
		public string Domain { get; }

		/// <summary>
		/// Returns a value that indicates whether two AddrSpec objects are equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two AddrSpec objects are equal; otherwise, False.</returns>
		public static bool operator == (AddrSpec first, AddrSpec second)
		{
			return first is null ?
				second is null :
				first.Equals (second);
		}

		/// <summary>
		/// Returns a value that indicates whether two AddrSpec objects are not equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two AddrSpec objects are not equal; otherwise, False.</returns>
		public static bool operator != (AddrSpec first, AddrSpec second)
		{
			return !(first is null ?
				second is null :
				first.Equals (second));
		}

		/// <summary>
		/// Creates an Internet identifier from the specified string representation.
		/// </summary>
		/// <param name="source">String representation of the Internet identifier.</param>
		/// <returns>The Internet identifier created from a string representation.</returns>
		public static AddrSpec Parse (string source) => Parse (source.AsSpan ());

		/// <summary>
		/// Creates an Internet identifier from the specified string representation.
		/// </summary>
		/// <param name="source">String representation of the Internet identifier.</param>
		/// <returns>The Internet identifier created from a string representation.</returns>
		public static AddrSpec Parse (ReadOnlySpan<char> source)
		{
			/*
			addr-spec       =  local-part "@" domain
			local-part      =  dot-atom / quoted-string
			domain          =  dot-atom / domain-literal
			domain-literal  =  [CFWS] "[" *([FWS] dtext) [FWS] "]" [CFWS]
			*/

			/*
			An 'encoded-word' MUST NOT appear in any portion of an 'addr-spec'.
			*/

			string localPart;
			string domain;

			var parserPos = 0;
			StructuredStringToken token1;
			do
			{
				token1 = DotAtomFormat.ParseToken (source, ref parserPos);
			} while (token1.Format is TokenFormatComment);

			StructuredStringToken token2;
			do
			{
				token2 = DotAtomFormat.ParseToken (source, ref parserPos);
			} while (token2.Format is TokenFormatComment);

			if ((token1.Format != null) && !(token1.Format is StructuredStringTokenSeparatorFormat) && (token2.Format == null))
			{
				// особый случай для совместимости со старыми реализациями
#if NETSTANDARD2_0
				localPart = token1.Format is StructuredStringTokenValueFormat ?
					new string (source.Slice (token1.Position, token1.Length).ToArray ()) :
					new string (source.Slice (token1.Position + 1, token1.Length - 2).ToArray ());
#else
				localPart = token1.Format is StructuredStringTokenValueFormat ?
					new string (source.Slice (token1.Position, token1.Length)) :
					new string (source.Slice (token1.Position + 1, token1.Length - 2));
#endif
				domain = "localhost";
			}
			else
			{
				StructuredStringToken token3;
				do
				{
					token3 = DotAtomFormat.ParseToken (source, ref parserPos);
				} while (token3.Format is TokenFormatComment);

				StructuredStringToken token4;
				do
				{
					token4 = DotAtomFormat.ParseToken (source, ref parserPos);
				} while (token4.Format is TokenFormatComment);

				if ((token4.Format != null) ||
					(!(token1.Format is StructuredStringTokenValueFormat) && !(token1.Format is StructuredStringTokenQuotedStringFormat)) ||
					!token2.IsSeparator (source, '@') ||
					(!(token3.Format is StructuredStringTokenValueFormat) && !(token3.Format is TokenFormatLiteral)))
				{
					throw new FormatException ("Value does not conform to format 'addr-spec'.");
				}

				// RFC 5321 4.5.3.1.2: The maximum total length of a domain name or number is 255 octets.
#if NETSTANDARD2_0
				var buf = new char[255];
				var len = token1.Format.DecodeToken (source.Slice (token1.Position, token1.Length), buf);
				localPart = new string (buf, 0, len);
				len = token3.Format.DecodeToken (source.Slice (token3.Position, token3.Length), buf);
				domain = new string (buf, 0, len);
#else
				Span<char> buf = stackalloc char[255];
				var len = token1.Format.DecodeToken (source.Slice (token1.Position, token1.Length), buf);
				localPart = new string (buf.Slice (0, len));
				len = token3.Format.DecodeToken (source.Slice (token3.Position, token3.Length), buf);
				domain = new string (buf.Slice (0, len));
#endif
			}

			return new AddrSpec (localPart, domain);
		}

		/// <summary>
		/// Returns a string that represents the this object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString ()
		{
			var isValidLocalPart = AsciiCharSet.IsValidInternetDomainName (this.LocalPart);
			var isValidDomain = AsciiCharSet.IsValidInternetDomainName (this.Domain);
			var localPart = isValidLocalPart ? this.LocalPart : AsciiCharSet.Quote (this.LocalPart);
			return isValidDomain ?
				localPart + "@" + this.Domain :
				localPart + "@[" + this.Domain + "]";
		}

		/// <summary>
		/// Converts the value of an object to its equivalent string representation.
		/// </summary>
		/// <param name="buf">The buffer where the string representation of the value of the object will be written.</param>
		/// <returns>The number of characters written to the buffer.</returns>
		public int ToString (Span<char> buf)
		{
			var isValidLocalPart = AsciiCharSet.IsValidInternetDomainName (this.LocalPart);
			var isValidDomain = AsciiCharSet.IsValidInternetDomainName (this.Domain);
			int pos;
			if (isValidLocalPart)
			{
				this.LocalPart.AsSpan ().CopyTo (buf);
				pos = this.LocalPart.Length;
			}
			else
			{
				pos = AsciiCharSet.Quote (this.LocalPart.AsSpan (), buf);
			}

			buf[pos++] = '@';
			if (!isValidDomain)
			{
				buf[pos++] = '[';
			}

			this.Domain.AsSpan ().CopyTo (buf[pos..]);
			pos += this.Domain.Length;
			if (!isValidDomain)
			{
				buf[pos++] = ']';
			}

			return pos;
		}

		/// <summary>
		/// Returns the hash code for this object.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode ()
		{
			// RFC 3798 part 2.1:
			// The comparison MUST be case-sensitive for the local-part and case-insensitive for the domain part.
#if NETSTANDARD2_0
			return this.LocalPart.GetHashCode () ^ StringComparer.OrdinalIgnoreCase.GetHashCode (this.Domain);
#else
			return this.LocalPart.GetHashCode (StringComparison.Ordinal) ^ StringComparer.OrdinalIgnoreCase.GetHashCode (this.Domain);
#endif
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">The object that you want to compare with the current object.</param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as AddrSpec;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">The object that you want to compare with the current object.</param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public bool Equals (AddrSpec other)
		{
			// RFC 3798 part 2.1:
			// The comparison MUST be case-sensitive for the local-part and case-insensitive for the domain part.
			return (other != null) &&
				string.Equals (this.LocalPart, other.LocalPart, StringComparison.Ordinal) &&
				string.Equals (this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase);
		}
	}
}
