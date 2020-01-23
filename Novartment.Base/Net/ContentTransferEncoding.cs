namespace Novartment.Base.Net
{
	/// <summary>
	/// The MIME-standardized content transfer encoding.
	/// </summary>
	public enum ContentTransferEncoding
	{
		/// <summary>No transfer encoding specified.</summary>
		Unspecified = 0,

		/// <summary>
		/// Encoding an arbitrary sequence of bytes in a form suitable for direct use in a Internet Message.
		/// Does not create overhead and maintains readability when used for data consisting mainly of US-ASCII characters.
		/// Large overhead (200%) for the rest of the characters.
		/// Defined in RFC 2045 part 6.7.
		/// </summary>
		QuotedPrintable = 1,

		/// <summary>
		/// Encoding an arbitrary sequence of bytes in a form suitable for direct use in a Internet Message.
		/// Suitable for any source data, has a fixed overhead of 33%.
		/// Defined in RFC 2045 part 6.8.
		/// </summary>
		Base64 = 2,

		/// <summary>
		/// Up to 998 bytes per line, values are limited to the range 1..127, and values 10 and 13 are only allowed as CRLF at the end of the line.
		/// No overhead. Defined in RFC 2045 part 6.2.
		/// </summary>
		SevenBit = 3,

		/// <summary>
		/// Up to 998 bytes per line, values are unlimited except values 10 and 13 are only allowed as CRLF at the end of the line.
		/// No overhead. Defined in RFC 2045 part 6.2.
		/// </summary>
		EightBit = 4,

		/// <summary>
		/// Raw random byte sequence. No overhead.
		/// Application is limited only to places where support is specially declared.
		/// Defined in RFC 3030.
		/// </summary>
		Binary = 5,
	}
}
