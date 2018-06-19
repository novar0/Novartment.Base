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
		internal static bool TryParse (ReadOnlySpan<char> source, out ContentMediaType result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var isText = MediaTypeNames.Text.AsSpan ().SequenceEqual (source);
			if (isText)
			{
				result = ContentMediaType.Text;
				return true;
			}

			var isImage = MediaTypeNames.Image.AsSpan ().SequenceEqual (source);
			if (isImage)
			{
				result = ContentMediaType.Image;
				return true;
			}

			var isAudio = MediaTypeNames.Audio.AsSpan ().SequenceEqual (source);
			if (isAudio)
			{
				result = ContentMediaType.Audio;
				return true;
			}

			var isVideo = MediaTypeNames.Video.AsSpan ().SequenceEqual (source);
			if (isVideo)
			{
				result = ContentMediaType.Video;
				return true;
			}

			var isApplication = MediaTypeNames.Application.AsSpan ().SequenceEqual (source);
			if (isApplication)
			{
				result = ContentMediaType.Application;
				return true;
			}

			var isMultipart = MediaTypeNames.Multipart.AsSpan ().SequenceEqual (source);
			if (isMultipart)
			{
				result = ContentMediaType.Multipart;
				return true;
			}

			var isMessage = MediaTypeNames.Message.AsSpan ().SequenceEqual (source);
			if (isMessage)
			{
				result = ContentMediaType.Message;
				return true;
			}

			result = ContentMediaType.Unspecified;
			return false;
		}
	}
}
