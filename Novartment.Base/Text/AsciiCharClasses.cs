using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Тип ASCII символа, такой как цифра, буква алфавита, токен.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1028:EnumStorageShouldBeInt32",
		Justification = "Size matters for large arrays of this enum.")]
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Ascii",
		Justification = "'ASCII' represents standard term.")]
	[Flags]
	public enum AsciiCharClasses : short
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// White space (non-printable) characters.
		/// </summary>
		WhiteSpace = 0x0001,

		/// <summary>
		/// Visible (Printable) US-ASCII characters 33...126.
		/// </summary>
		Visible = 0x0002,

		/// <summary>
		/// Decimal digits [0...9].
		/// </summary>
		Digit = 0x0004,

		/// <summary>
		/// Alpha characters [A...Z] and [a...z].
		/// </summary>
		Alpha = 0x0008,

		/// <summary>
		/// RFC 822 'atom' text.
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]".</code>.
		/// </summary>
		Atom = 0x0010,

		/// <summary>
		/// RFC 2045 'token' used as parameter names and values text in parameterized fields such as
		/// 'Content-Type', 'Content-Disposition' and 'Disposition-Notification-Options'.
		/// Updated in RFC 2231.
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]"/?=*'%</code>.
		/// </summary>
		Token = 0x0020,

		/// <summary>
		/// RFC 2047 'token' used as 'charset' and 'encoding'.
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]"/?=."</code>.
		/// </summary>
		ExtendedToken = 0x0040,

		/// <summary>
		/// RFC 1738 'scheme' URL scheme text. Lower case alpha charactes, digits and <code>+-.</code>.
		/// </summary>
		UrlScheme = 0x0080,

		/// <summary>
		/// RFC 1738 'xchar' URL schemepart text. Alpha charactes, digits and <code>$-_.+!*'(),;/?:@&amp;=~</code>.
		/// </summary>
		UrlSchemePart = 0x0100,

		/// <summary>
		/// RFC 5322 'dtext' domain-literal text. Printable US-ASCII characters not including <code>[]\</code>.
		/// </summary>
		Domain = 0x0200,

		/// <summary>
		/// RFC 2047 "Q"-encoded 'encoded-word' characters not required to be encoded for unstructured values.
		/// </summary>
		QEncodingAllowedInUnstructured = 0x0400,

		/// <summary>
		/// RFC 2047 "Q"-encoded 'encoded-word' characters not required to be encoded for structured values.
		/// </summary>
		QEncodingAllowedInStructured = 0x0800,

		/// <summary>
		/// Алфавит кодировки Base64 согласно RFC 2045 часть 6.8.
		/// </summary>
		Base64Alphabet = 0x1000,
	}
}
