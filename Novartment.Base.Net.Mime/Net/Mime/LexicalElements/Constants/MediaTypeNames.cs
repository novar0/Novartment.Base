using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	internal static class MediaTypeHelper
	{
		/// <summary>
		/// Gets string name of ContentMediaType enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of ContentMediaType enumeration value.</returns>
		internal static string GetName (this ContentMediaType value)
		{
			switch (value)
			{
				case ContentMediaType.Text: return MediaTypeNames.Text;
				case ContentMediaType.Image: return MediaTypeNames.Image;
				case ContentMediaType.Audio: return MediaTypeNames.Audio;
				case ContentMediaType.Video: return MediaTypeNames.Video;
				case ContentMediaType.Application: return MediaTypeNames.Application;
				case ContentMediaType.Multipart: return MediaTypeNames.Multipart;
				case ContentMediaType.Message: return MediaTypeNames.Message;
				default:
					throw new NotSupportedException ("Unsupported value of MediaType '" + value + "'.");
			}
		}

		/// <summary>
		/// Parses string representation of ContentMediaType enumeration value.
		/// </summary>
		/// <param name="source">String representation of ContentMediaType enumeration value.</param>
		/// <param name="result">When this method returns, contains the ContentMediaType value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out ContentMediaType result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			var isText = MediaTypeNames.Text.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isText)
			{
				result = ContentMediaType.Text;
				return true;
			}
			var isImage = MediaTypeNames.Image.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isImage)
			{
				result = ContentMediaType.Image;
				return true;
			}
			var isAudio = MediaTypeNames.Audio.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isAudio)
			{
				result = ContentMediaType.Audio;
				return true;
			}
			var isVideo = MediaTypeNames.Video.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isVideo)
			{
				result = ContentMediaType.Video;
				return true;
			}
			var isApplication = MediaTypeNames.Application.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isApplication)
			{
				result = ContentMediaType.Application;
				return true;
			}
			var isMultipart = MediaTypeNames.Multipart.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isMultipart)
			{
				result = ContentMediaType.Multipart;
				return true;
			}
			var isMessage = MediaTypeNames.Message.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isMessage)
			{
				result = ContentMediaType.Message;
				return true;
			}
			result = ContentMediaType.Unspecified;
			return false;
		}
	}

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
