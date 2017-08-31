namespace Novartment.Base.Net.Mime
{
#pragma warning disable SA1402 // File may only contain a single class
#pragma warning disable SA1649 // File name must match first type name
	// Content-Type header field media subtype names.
	// Full IANA registered list can be found from: http://www.iana.org/assignments/media-types.

	/// <summary>
	/// Application/xxx media sub-type names.
	/// </summary>
	public static class ApplicationMediaSubtypeNames
	{
		/// <summary>
		/// "application/octet-stream". Определено в RFC 2045,2046.
		/// </summary>
		public static readonly string OctetStream = "octet-stream";

		/// <summary>
		/// "application/pdf". Определено в RFC 3778.
		/// </summary>
		public static readonly string Pdf = "pdf";

		/// <summary>
		/// "application/sdp". Определено в RFC 4566.
		/// </summary>
		public static readonly string Sdp = "sdp";

		/// <summary>
		/// "application/xml". Defined RFC 3023.
		/// </summary>
		public static readonly string Xml = "xml";

		/// <summary>
		/// "application/zip". Определено в RFC 4566.
		/// </summary>
		public static readonly string Zip = "zip";

		/// <summary>
		/// "application/x-pkcs7-signature". Определено в RFC 2311,2633.
		/// </summary>
		public static readonly string Pkcs7Signature = "x-pkcs7-signature";

		/// <summary>
		/// "application/pkcs7-mime". Определено в RFC 5751.
		/// </summary>
		public static readonly string Pkcs7Mime = "pkcs7-mime";
	}

	/// <summary>
	/// Image/xxx media subtype names.
	/// </summary>
	public static class ImageMediaSubtypeNames
	{
		/// <summary>
		/// "image/png".
		/// </summary>
		public static readonly string Png = "png";

		/// <summary>
		/// "image/svg+xml".
		/// </summary>
		public static readonly string SvgXml = "svg+xml";

		/// <summary>
		/// "image/gif".
		/// </summary>
		public static readonly string Gif = "gif";

		/// <summary>
		/// "image/jpeg".
		/// </summary>
		public static readonly string Jpeg = "jpeg";

		/// <summary>
		/// "image/tiff".
		/// </summary>
		public static readonly string Tiff = "tiff";
	}

	/// <summary>
	/// Text/xxx media types.
	/// </summary>
	public static class TextMediaSubtypeNames
	{
		/// <summary>
		/// "text/calendar". Определено в RFC 2445.
		/// </summary>
		public static readonly string Calendar = "calendar";

		/// <summary>
		/// "text/css". Определено в RFC 2854
		/// </summary>
		public static readonly string Css = "css";

		/// <summary>
		/// "text/html". Определено в RFC 2854.
		/// </summary>
		public static readonly string Html = "html";

		/// <summary>
		/// "text/plain". Определено в RFC 2646,2046.
		/// </summary>
		public static readonly string Plain = "plain";

		/// <summary>
		/// "text/rfc822-headers". Определено в RFC 6522 часть 4.
		/// </summary>
		public static readonly string Rfc822Headers = "rfc822-headers";

		/// <summary>
		/// "text/richtext". ОпределенО в RFC 2045,2046.
		/// </summary>
		public static readonly string RichText = "richtext";

		/// <summary>
		/// "text/xml". Определено в RFC 3023.
		/// </summary>
		public static readonly string Xml = "xml";
	}

	/// <summary>
	/// Multipart/xxx media subtype names.
	/// </summary>
	public static class MultipartMediaSubtypeNames
	{
		/// <summary>
		/// "multipart/mixed". Определено в RFC 2045,2046.
		/// </summary>
		public static readonly string Mixed = "mixed";

		/// <summary>
		/// "multipart/parallel". Определено в RFC 2045,2046.
		/// </summary>
		public static readonly string Parallel = "parallel";

		/// <summary>
		/// "multipart/alternative". Определено в RFC 2045,2046.
		/// </summary>
		public static readonly string Alternative = "alternative";

		/// <summary>
		/// "multipart/digest". Определено в RFC 2045,2046.
		/// </summary>
		public static readonly string Digest = "digest";

#pragma warning disable CA1720 // Identifier contains type name
		/// <summary>
		/// "multipart/signed". Определено в RFC 1847.
		/// </summary>
		public static readonly string Signed = "signed";
#pragma warning restore CA1720 // Identifier contains type name

		/// <summary>
		/// "multipart/encrypted". Определено в RFC 1847.
		/// </summary>
		public static readonly string Encrypted = "encrypted";

		/// <summary>
		/// "multipart/form-data". Определено в RFC 2388.
		/// </summary>
		public static readonly string FormData = "form-data";

		/// <summary>
		/// "multipart/related". Определено в RFC 2387.
		/// </summary>
		public static readonly string Related = "related";

		/// <summary>
		/// "multipart/report". Определено в RFC 1892.
		/// </summary>
		public static readonly string Report = "report";

		/// <summary>
		/// "multipart/voice-message". Определено в RFC 2421,2423.
		/// </summary>
		public static readonly string ViceMessage = "voice-message";
	}

	/// <summary>
	/// Message/xxx media subtype names.
	/// </summary>
	public static class MessageMediaSubtypeNames
	{
		/// <summary>
		/// "message/rfc822".
		/// </summary>
		public static readonly string Rfc822 = "rfc822";

		/// <summary>
		/// "message/disposition-notification".
		/// </summary>
		public static readonly string DispositionNotification = "disposition-notification";

		/// <summary>
		/// "message/delivery-status". Определено в RFC 3464.
		/// </summary>
		public static readonly string DeliveryStatus = "delivery-status";
	}
#pragma warning restore SA1649 // File name must match first type name
#pragma warning restore SA1402 // File may only contain a single class
}
