using System;

namespace Novartment.Base.Net.Mime
{
	internal static class DispositionTypeHelper
	{
		/// <summary>
		/// Gets string name of ContentDispositionType enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of ContentDispositionType enumeration value.</returns>
		internal static string GetName (this ContentDispositionType value)
		{
			return value switch
			{
				ContentDispositionType.Inline => DispositionTypeNames.Inline,
				ContentDispositionType.Attachment => DispositionTypeNames.Attachment,
				ContentDispositionType.FormData => DispositionTypeNames.FormData,
				_ => throw new NotSupportedException (FormattableString.Invariant ($"Unsupported value of DispositionType: '{value}'.")),
			};
		}

		/// <summary>
		/// Parses string representation of ContentDispositionType enumeration value.
		/// </summary>
		/// <param name="source">String representation of ContentDispositionType enumeration value.</param>
		/// <param name="result">When this method returns, contains the ContentDispositionType value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (ReadOnlySpan<char> source, out ContentDispositionType result)
		{
			var isInline = DispositionTypeNames.Inline.AsSpan ().SequenceEqual (source);
			if (isInline)
			{
				result = ContentDispositionType.Inline;
				return true;
			}

			var isAttachment = DispositionTypeNames.Attachment.AsSpan ().SequenceEqual (source);
			if (isAttachment)
			{
				result = ContentDispositionType.Attachment;
				return true;
			}

			var isFormData = DispositionTypeNames.FormData.AsSpan ().SequenceEqual (source);
			if (isFormData)
			{
				result = ContentDispositionType.FormData;
				return true;
			}

			result = ContentDispositionType.Unspecified;
			return false;
		}
	}
}
