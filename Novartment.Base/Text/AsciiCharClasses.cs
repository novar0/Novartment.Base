using System;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Classes that an ASCII character belongs to, such as a number, an alphabet letter, or a token.
	/// </summary>
	[Flags]
#pragma warning disable CA1028 // Enum Storage should be Int32
	public enum AsciiCharClasses : short
#pragma warning restore CA1028 // Enum Storage should be Int32
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// The white space (non-printable) characters.
		/// </summary>
		WhiteSpace = 0x0001,

		/// <summary>
		/// The visible (Printable) US-ASCII characters 33...126.
		/// </summary>
		Visible = 0x0002,

		/// <summary>
		/// The decimal digits [0...9].
		/// </summary>
		Digit = 0x0004,

		/// <summary>
		/// The alpha characters [A...Z] and [a...z].
		/// </summary>
		Alpha = 0x0008,

		/// <summary>
		/// The RFC 5322 'atom' is used as an identifier of various kinds.
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]".</code>.
		/// </summary>
		/// <remarks>
		/// Alpha + Digit + <code>!#$%&amp;'*+-/=?^_`{|}~</code>.
		/// Same as Visible excluding all kind of brackets, 'at', 'backslash (reverse solidus)', 'dot', 'comma', 'colon' and 'semicolon'.
		/// </remarks>
		Atom = 0x0010,

		/// <summary>
		/// The RFC 2045 'token' is used as a header field parameter names and values.
		/// Updated in RFC 2231.
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]"/?=*'%</code>.
		/// </summary>
		/// <remarks>
		/// Alpha + Digit + <code>!#$&amp;+-^_`{|}~</code>.
		/// Same as Atom plus 'dot' and excluding 'solidus', 'question', 'equality', 'asterix', 'apostrophe' and 'percent'.
		/// </remarks>
		Token = 0x0020,

		/// <summary>
		/// The RFC 2047 'token' is used as a 'charset' and 'encoding' identifiers.
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]"/?=."</code>.
		/// </summary>
		/// <remarks>
		/// Same as Atom and excluding 'solidus', 'question', 'equality'.
		/// </remarks>
		ExtendedToken = 0x0040,

		/// <summary>
		/// The RFC 1738 'scheme' is used as a URL scheme text. Lower case alpha charactes, digits and <code>+-.</code>.
		/// </summary>
		UrlScheme = 0x0080,

		/// <summary>
		/// The RFC 1738 'xchar' is used as a URL schemepart text. Alpha charactes, digits and <code>$-_.+!*'(),;/?:@&amp;=~</code>.
		/// </summary>
		UrlSchemePart = 0x0100,

		/// <summary>
		/// The RFC 2047 "Q"-encoded 'encoded-word' used for characters not required to be encoded for unstructured values.
		/// </summary>
		QEncodingAllowedInUnstructured = 0x0200,

		/// <summary>
		/// The RFC 2047 "Q"-encoded 'encoded-word' used for characters not required to be encoded for structured values.
		/// </summary>
		QEncodingAllowedInStructured = 0x0400,

		/// <summary>
		/// The Base64-encoding alphabet according to RFC 2045 part 6.8.
		/// Alpha charactes, digits and <code>+/=</code>.
		/// </summary>
		Base64Alphabet = 0x800,
	}
}
