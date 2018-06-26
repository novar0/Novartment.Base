using System;
using System.Buffers;
using System.Globalization;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Строковое значение, позволяющее получать информацию об отдельных сегментах, разделённых пробелами.
	/// </summary>
	internal class HeaderFieldBodyParameterEncoder
	{
		/*
		RFC 2231 часть 4.1:
		1. Language and character set information only appear at the beginning of a given parameter value.
		2. Continuations do not provide a facility for using more than one character set or language in the same parameter value.
		3. A value presented using multiple continuations may contain a mixture of encoded and unencoded segments.
		4. The first segment of a continuation MUST be encoded if language and character set information are given.
		5. If the first segment of a continued parameter value is encoded the language and character set field delimiters MUST be present even when the fields are left blank.
		*/

		/// <summary>
		/// Получает следующий сегмент из последовательности, образующей исходную строку.
		/// Сегмент закодирован для использования в виде значения параметра поля заголовка и пригоден для фолдинга с другими сегментами.
		/// Сегменты выбираются из исходной строки по следующим правилам:
		/// 1. Объединяет несколько слов если первое и последнее слово требуют кодирования (для экономии на накладных расходах кодирования).
		/// 2. Имеет суммарную длину не более HeaderEncoder.MaxLineLengthRequired (для читабельности результата).
		/// </summary>
		/// <returns>Следующий сегмент из последовательности, образующей исходную строку. Если вся исходная строка уже выдана, то null.</returns>
		internal static string GetNextSegment (string name, byte[] value, EstimatingEncoderChunk[] valueSegments, ref int currentSegmentNumber)
		{
			if (currentSegmentNumber >= valueSegments.Length)
			{
				return null;
			}

			var isSingleUnencodedPart = (valueSegments.Length < 2) && !(valueSegments[0].Encoder is HeaderFieldBodyExtendedParameterValueEstimatingEncoder);

			var outBytesRequiredCapacity = isSingleUnencodedPart ?
					name.Length + 2 + (valueSegments[currentSegmentNumber].Count * 2) :
					name.Length + 4 + ((currentSegmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (currentSegmentNumber))) + (valueSegments[currentSegmentNumber].Count * 4);
			var outBytes = ArrayPool<byte>.Shared.Rent (outBytesRequiredCapacity);

			AsciiCharSet.GetBytes (name.AsSpan (), outBytes);
			var outBytesPos = name.Length;
			int bytesProduced;
			string result;

			if (isSingleUnencodedPart)
			{
				// значение из одной части не требующей кодирования (возможно квотирование)
				outBytes[outBytesPos++] = (byte)'=';
				(bytesProduced, _) = valueSegments[0].Encoder.Encode (
					source: value,
					offset: valueSegments[0].Offset,
					count: valueSegments[0].Count,
					destination: outBytes,
					outOffset: outBytesPos,
					maxOutCount: outBytesRequiredCapacity - outBytesPos,
					segmentNumber: 0,
					isLastSegment: false);
				outBytesPos += bytesProduced;
			}
			else
			{
				// значение из многих частей либо требует кодирования
				outBytes[outBytesPos++] = (byte)'*';
				var idxStr = currentSegmentNumber.ToString (CultureInfo.InvariantCulture);
				AsciiCharSet.GetBytes (idxStr.AsSpan (), outBytes.AsSpan (outBytesPos));
				outBytesPos += idxStr.Length;
				if (valueSegments[currentSegmentNumber].Encoder is HeaderFieldBodyExtendedParameterValueEstimatingEncoder)
				{
					outBytes[outBytesPos++] = (byte)'*';
				}

				outBytes[outBytesPos++] = (byte)'=';
				(bytesProduced, _) = valueSegments[currentSegmentNumber].Encoder.Encode (
					source: value,
					offset: valueSegments[currentSegmentNumber].Offset,
					count: valueSegments[currentSegmentNumber].Count,
					destination: outBytes,
					outOffset: outBytesPos,
					maxOutCount: outBytesRequiredCapacity - outBytesPos,
					segmentNumber: currentSegmentNumber,
					isLastSegment: currentSegmentNumber < (valueSegments.Length - 1));
				outBytesPos += bytesProduced;
				if (currentSegmentNumber != (valueSegments.Length - 1))
				{
					outBytes[outBytesPos++] = (byte)';';
				}
			}

			currentSegmentNumber++;
			result = AsciiCharSet.GetString (outBytes.AsSpan (0, outBytesPos));
			ArrayPool<byte>.Shared.Return (outBytes);
			return result;
		}
	}
}
