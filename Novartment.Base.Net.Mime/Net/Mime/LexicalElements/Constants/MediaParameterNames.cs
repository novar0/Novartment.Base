namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// MIME Content-Type field parameter names.
	/// </summary>
	internal static class MediaParameterNames
	{
		/// <summary>
		/// "name". Suggested file name to be used if the data were to be written to a file.
		/// Applicable to "octet-stream" subtypes. Определено в RFC 2046.</summary>
		internal static readonly string Name = "name";

		/// <summary>
		/// "charset". Identifies the character set of the body text for "text" types.
		/// Определено в RFC 2046.</summary>
		internal static readonly string Charset = "charset";

		/// <summary>
		/// "boundary". Identifies boundary line delimiting parts of "multipart" types.
		/// Определено в RFC 2046.</summary>
		internal static readonly string Boundary = "boundary";

		/// <summary>
		/// "protocol". Type and sub-type tokens of the Content-Type header field of the first body part
		/// of Multipart/Signed and Multipart/Encrypted message. Определено в RFC 1847.</summary>
		internal static readonly string Protocol = "protocol";

		/// <summary>
		/// "smime-type". Identifies security applied (signed or enveloped) along with infomation about the contained content.
		/// Applicable to "application/pkcs7-mime" type. Определено в RFC 2633.</summary>
		internal static readonly string SMimeType = "smime-type";

		/// <summary>
		/// "micalg". The Message Integrity Check (MIC) algorithm used in Multipart/Signed message.
		/// Определено в RFC 1847.</summary>
		internal static readonly string MicAlg = "micalg";

		/// <summary>
		/// "report-type". Identifies the type of report.
		/// Определено в RFC 6522.</summary>
		internal static readonly string ReportType = "report-type";
	}
}
