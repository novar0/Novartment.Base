using System;
using System.Diagnostics.Contracts;

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
			switch (value)
			{
				case ContentDispositionType.Inline: return DispositionTypeNames.Inline;
				case ContentDispositionType.Attachment: return DispositionTypeNames.Attachment;
				case ContentDispositionType.FormData: return DispositionTypeNames.FormData;
				default:
					throw new NotSupportedException (FormattableString.Invariant ($"Unsupported value of DispositionType: '{value}'."));
			}
		}

		/// <summary>
		/// Parses string representation of ContentDispositionType enumeration value.
		/// </summary>
		/// <param name="source">String representation of ContentDispositionType enumeration value.</param>
		/// <param name="result">When this method returns, contains the ContentDispositionType value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out ContentDispositionType result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var isInline = DispositionTypeNames.Inline.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isInline)
			{
				result = ContentDispositionType.Inline;
				return true;
			}

			var isAttachment = DispositionTypeNames.Attachment.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isAttachment)
			{
				result = ContentDispositionType.Attachment;
				return true;
			}

			var isFormData = DispositionTypeNames.FormData.Equals (source, StringComparison.OrdinalIgnoreCase);
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
