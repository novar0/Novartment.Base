using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	internal static class TransferEncodingHelper
	{
		/// <summary>
		/// Gets string name of ContentTransferEncoding enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of ContentTransferEncoding enumeration value.</returns>
		internal static string GetName (this ContentTransferEncoding value)
		{
			switch (value)
			{
				case ContentTransferEncoding.QuotedPrintable: return TransferEncodingNames.QuotedPrintable;
				case ContentTransferEncoding.Base64: return TransferEncodingNames.Base64;
				case ContentTransferEncoding.SevenBit: return TransferEncodingNames.SevenBit;
				case ContentTransferEncoding.EightBit: return TransferEncodingNames.EightBit;
				case ContentTransferEncoding.Binary: return TransferEncodingNames.Binary;
				default:
					throw new NotSupportedException ("Unsupported value of ContentTransferEncoding '" + value + "'.");
			}
		}

		/// <summary>
		/// Parses string representation of ContentTransferEncoding enumeration value.
		/// </summary>
		/// <param name="source">String representation of ContentTransferEncoding enumeration value.</param>
		/// <param name="result">When this method returns, contains the ContentTransferEncoding value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out ContentTransferEncoding result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var isQuotedPrintable = TransferEncodingNames.QuotedPrintable.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isQuotedPrintable)
			{
				result = ContentTransferEncoding.QuotedPrintable;
				return true;
			}

			var isBase64 = TransferEncodingNames.Base64.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isBase64)
			{
				result = ContentTransferEncoding.Base64;
				return true;
			}

			var isSevenBit = TransferEncodingNames.SevenBit.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isSevenBit)
			{
				result = ContentTransferEncoding.SevenBit;
				return true;
			}

			var isEightBit = TransferEncodingNames.EightBit.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isEightBit)
			{
				result = ContentTransferEncoding.EightBit;
				return true;
			}

			var isBinary = TransferEncodingNames.Binary.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isBinary)
			{
				result = ContentTransferEncoding.Binary;
				return true;
			}

			result = ContentTransferEncoding.Unspecified;
			return false;
		}

		/// <summary>
		/// Получает метод кодирования из списка допустимых для композитных типов так,
		/// чтобы оно могло быть использовано для представления указанного метода кодирования части.
		/// </summary>
		/// <param name="value">Метод кодирования части.</param>
		/// <returns>
		/// Такое значение метода кодирования из списка допустимых для композитных типов,
		/// чтобы оно могло быть использовано для представления указанного метода кодирования части.
		/// </returns>
		internal static ContentTransferEncoding GetSuitableCompositeMediaTypeTransferEncoding (this ContentTransferEncoding value)
		{
			// According to RFC 2045 part 6.4:
			// it is EXPRESSLY FORBIDDEN to use any encodings other than "7bit", "8bit", or "binary" with any composite media type,
			// i.e. one that recursively includes other Content-Type fields.
			switch (value)
			{
				case ContentTransferEncoding.Unspecified: // по умолчанию означает "7bit"
				case ContentTransferEncoding.QuotedPrintable:
				case ContentTransferEncoding.Base64:
				case ContentTransferEncoding.SevenBit:
					return ContentTransferEncoding.SevenBit;
				case ContentTransferEncoding.EightBit:
					return ContentTransferEncoding.EightBit;
				case ContentTransferEncoding.Binary:
					return ContentTransferEncoding.Binary;
				default:
					throw new NotSupportedException ("Unsupported value of ContentTransferEncoding '" + value + "'.");
			}
		}
	}
}
