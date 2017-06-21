using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Net.Mime
{
#pragma warning disable SA1402 // File may only contain a single class
#pragma warning disable SA1649 // File name must match first type name
	// Content-Type header field media subtype names.
	// Full IANA registered list can be found from: http://www.iana.org/assignments/media-types.

	/// <summary>
	/// Application/xxx media sub-type names.
	/// </summary>
	public sealed class ApplicationMediaSubtypeNames
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
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Sdp",
			Justification = "'SDP' represents standard term.")]
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
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Pkcs",
			Justification = "'PKCS' represents standard term.")]
		public static readonly string Pkcs7Signature = "x-pkcs7-signature";

		/// <summary>
		/// "application/pkcs7-mime". Определено в RFC 5751.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Pkcs",
			Justification = "'PKCS' represents standard term.")]
		public static readonly string Pkcs7Mime = "pkcs7-mime";

		private ApplicationMediaSubtypeNames ()
		{
		}
	}

	/// <summary>
	/// Image/xxx media subtype names.
	/// </summary>
	public sealed class ImageMediaSubtypeNames
	{
		/// <summary>
		/// "image/png".
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Png",
			Justification = "'PNG' represents standard term.")]
		public static readonly string Png = "png";

		/// <summary>
		/// "image/svg+xml".
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Svg",
			Justification = "'SVG' represents standard term.")]
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

		private ImageMediaSubtypeNames ()
		{
		}
	}

	/// <summary>
	/// Text/xxx media types.
	/// </summary>
	public sealed class TextMediaSubtypeNames
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
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Rfc",
			Justification = "'RFC' represents standard term.")]
		public static readonly string Rfc822Headers = "rfc822-headers";

		/// <summary>
		/// "text/richtext". ОпределенО в RFC 2045,2046.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Richtext",
			Justification = "'Rich Text' represents standard term.")]
		public static readonly string RichText = "richtext";

		/// <summary>
		/// "text/xml". Определено в RFC 3023.
		/// </summary>
		public static readonly string Xml = "xml";

		private TextMediaSubtypeNames ()
		{
		}
	}

	/// <summary>
	/// Multipart/xxx media subtype names.
	/// </summary>
	public sealed class MultipartMediaSubtypeNames
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

		/// <summary>
		/// "multipart/signed". Определено в RFC 1847.
		/// </summary>
		public static readonly string Signed = "signed";

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

		private MultipartMediaSubtypeNames ()
		{
		}
	}

	/// <summary>
	/// Message/xxx media subtype names.
	/// </summary>
	public sealed class MessageMediaSubtypeNames
	{
		/// <summary>
		/// "message/rfc822".
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Rfc",
			Justification = "'RFC' represents standard term.")]
		public static readonly string Rfc822 = "rfc822";

		/// <summary>
		/// "message/disposition-notification".
		/// </summary>
		public static readonly string DispositionNotification = "disposition-notification";

		/// <summary>
		/// "message/delivery-status". Определено в RFC 3464.
		/// </summary>
		public static readonly string DeliveryStatus = "delivery-status";

		private MessageMediaSubtypeNames ()
		{
		}
	}
#pragma warning restore SA1649 // File name must match first type name
#pragma warning restore SA1402 // File may only contain a single class
}
