﻿using System;
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
			var token1 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
			var token2 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
			if (token1.IsValid && !token2.IsValid)
			{
				// особый случай для совместимости со старыми реализациями
#if NETSTANDARD2_0
				localPart = new string (source.Slice (token1.Position, token1.Length).ToArray ());
#else
				localPart = new string (source.Slice (token1.Position, token1.Length));
#endif
				domain = "localhost";
			}
			else
			{
				var token3 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
				var token4 = StructuredHeaderFieldLexicalToken.ParseDotAtom (source, ref parserPos);
				if (token4.IsValid ||
					((token1.TokenType != StructuredHeaderFieldLexicalTokenType.Value) && (token1.TokenType != StructuredHeaderFieldLexicalTokenType.QuotedValue)) ||
					(token2.TokenType != StructuredHeaderFieldLexicalTokenType.Separator) || (token2.Length != 1) || (source[token2.Position] != (byte)'@') ||
					((token3.TokenType != StructuredHeaderFieldLexicalTokenType.Value) && (token3.TokenType != StructuredHeaderFieldLexicalTokenType.SquareBracketedValue)))
				{
					throw new FormatException ("Value does not conform to format 'addr-spec'.");
				}

				// RFC 5321 4.5.3.1.2: The maximum total length of a domain name or number is 255 octets.
				var buf = new char[255];
				var len = token1.Decode (source, buf);
				localPart = new string (buf, 0, len);
				len = token3.Decode (source, buf);
				domain = new string (buf, 0, len);
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

			this.Domain.AsSpan ().CopyTo (buf.Slice (pos));
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
			return other == null
				? false
				: string.Equals (this.LocalPart, other.LocalPart, StringComparison.Ordinal) &&
				string.Equals (this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase);
		}
	}
}
