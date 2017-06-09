namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Content-Type header field media type names.
	/// Full IANA registered list can be found from: http://www.iana.org/assignments/media-types.
	/// </summary>
	internal static class MediaTypeNames
	{
		/// <summary>"text". Определено в RFC 2046 4.1.
		/// A "CHARSET" parameter may be used to indicate the character set of the body text.</summary>
		internal static readonly string Text = "text";

		/// <summary>"image". Определено в RFC 2046 4.2.</summary>
		internal static readonly string Image = "image";

		/// <summary>"audio". Определено в RFC 2046 4.3.</summary>
		internal static readonly string Audio = "audio";

		/// <summary>"video". Определено в RFC 2046 4.4.</summary>
		internal static readonly string Video = "video";

		/// <summary>"application". Определено в RFC 2046 4.5. Parameters:
		/// TYPE -- the general type or category of binary data,
		/// PADDING -- the number of bits of padding that were appended to the bit-stream comprising the actual contents,
		/// NAME -- a suggested name for the binary data if stored as a file.</summary>
		internal static readonly string Application = "application";

		/// <summary>"multipart". Определено в RFC 2046 5.1. Requires "boundary" parameter.</summary>
		internal static readonly string Multipart = "multipart";

		/// <summary>"message". Определено в 2046 5.2.</summary>
		internal static readonly string Message = "message";
	}
}
