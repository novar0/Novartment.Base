using System;

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
			return value switch
			{
				ContentTransferEncoding.QuotedPrintable => TransferEncodingNames.QuotedPrintable,
				ContentTransferEncoding.Base64 => TransferEncodingNames.Base64,
				ContentTransferEncoding.SevenBit => TransferEncodingNames.SevenBit,
				ContentTransferEncoding.EightBit => TransferEncodingNames.EightBit,
				ContentTransferEncoding.Binary => TransferEncodingNames.Binary,
				_ => throw new NotSupportedException ("Unsupported value of ContentTransferEncoding '" + value + "'."),
			};
		}

		/// <summary>
		/// Parses string representation of ContentTransferEncoding enumeration value.
		/// </summary>
		/// <param name="source">String representation of ContentTransferEncoding enumeration value.</param>
		/// <param name="result">When this method returns, contains the ContentTransferEncoding value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (ReadOnlySpan<char> source, out ContentTransferEncoding result)
		{
			var isQuotedPrintable = TransferEncodingNames.QuotedPrintable.AsSpan ().SequenceEqual (source);
			if (isQuotedPrintable)
			{
				result = ContentTransferEncoding.QuotedPrintable;
				return true;
			}

			var isBase64 = TransferEncodingNames.Base64.AsSpan ().SequenceEqual (source);
			if (isBase64)
			{
				result = ContentTransferEncoding.Base64;
				return true;
			}

			var isSevenBit = TransferEncodingNames.SevenBit.AsSpan ().SequenceEqual (source);
			if (isSevenBit)
			{
				result = ContentTransferEncoding.SevenBit;
				return true;
			}

			var isEightBit = TransferEncodingNames.EightBit.AsSpan ().SequenceEqual (source);
			if (isEightBit)
			{
				result = ContentTransferEncoding.EightBit;
				return true;
			}

			var isBinary = TransferEncodingNames.Binary.AsSpan ().SequenceEqual (source);
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
			return value switch
			{
				ContentTransferEncoding.Unspecified or // по умолчанию означает "7bit"
				ContentTransferEncoding.QuotedPrintable or
				ContentTransferEncoding.Base64 or
				ContentTransferEncoding.SevenBit => ContentTransferEncoding.SevenBit,
				ContentTransferEncoding.EightBit => ContentTransferEncoding.EightBit,
				ContentTransferEncoding.Binary => ContentTransferEncoding.Binary,
				_ => throw new NotSupportedException ("Unsupported value of ContentTransferEncoding '" + value + "'."),
			};
		}
	}
}
