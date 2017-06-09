namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// MIME content transfer encodings. Определено в RFC 2045 6.
	/// </summary>
	internal static class TransferEncodingNames
	{
		/// <summary>
		/// "7bit". Up to 998 octets per line of the code range 1..127 with CR and LF (codes 13 and 10 respectively) only allowed to
		/// appear as part of a CarriageReturnLinefeed line ending. This is the default value.
		/// Определено в RFC 2045 6.2.
		/// </summary>
		internal static readonly string SevenBit = "7bit";

		/// <summary>
		/// "8bit". Up to 998 octets per line with CR and LF (codes 13 and 10 respectively) only allowed to appear as part of a CarriageReturnLinefeed line ending.
		/// Определено в RFC 2045 6.2.
		/// </summary>
		internal static readonly string EightBit = "8bit";

		/// <summary>
		/// "quoted-printable". Used to encode arbitrary octet sequences into a form that satisfies the rules of 7bit.
		/// Designed to be efficient and mostly human readable when used for text data consisting primarily of US-ASCII characters
		/// but also containing a small proportion of bytes with values outside that range.
		/// Определено в RFC 2045 6.7.
		/// </summary>
		internal static readonly string QuotedPrintable = "quoted-printable";

		/// <summary>
		/// "base64". Used to encode arbitrary octet sequences into a form that satisfies the rules of 7bit. Has a fixed overhead and is
		/// intended for non text data and text that is not ASCII heavy.
		/// Определено в RFC 2045 6.8.
		/// </summary>
		internal static readonly string Base64 = "base64";

		/// <summary>
		/// "binary". Any sequence of octets. This type is not widely used. Определено в RFC 3030.
		/// </summary>
		internal static readonly string Binary = "binary";
	}
}
