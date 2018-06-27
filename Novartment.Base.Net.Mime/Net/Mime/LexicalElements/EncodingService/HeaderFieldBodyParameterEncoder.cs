﻿using System;
using System.Buffers;
using System.Globalization;
using System.Text;
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

		private static readonly HeaderFieldBodyExtendedParameterValueEstimatingEncoder ExtendedParameterEncoder = new HeaderFieldBodyExtendedParameterValueEstimatingEncoder (Encoding.UTF8);
		private static readonly AsciiCharClassEstimatingEncoder AsciiEncoder = new AsciiCharClassEstimatingEncoder (AsciiCharClasses.Token);
		private static readonly QuotedStringEstimatingEncoder QuotedStringEncoder = new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);

		/// <summary>
		/// Получает следующий сегмент из последовательности, образующей исходную строку.
		/// Сегмент закодирован для использования в виде значения параметра поля заголовка и пригоден для фолдинга с другими сегментами.
		/// Сегменты выбираются из исходной строки по следующим правилам:
		/// 1. Объединяет несколько слов если первое и последнее слово требуют кодирования (для экономии на накладных расходах кодирования).
		/// 2. Имеет суммарную длину не более HeaderEncoder.MaxLineLengthRequired (для читабельности результата).
		/// </summary>
		/// <returns>Следующий сегмент из последовательности, образующей исходную строку. Если вся исходная строка уже выдана, то null.</returns>
		internal static string GetNextSegment (string name, byte[] value, int segmentNumber, ref int pos)
		{
			if (pos >= value.Length)
			{
				return null;
			}

			var segment = GetNextChunk (name.Length, value, segmentNumber, ref pos);
			var isLastSegment = pos >= value.Length;
			var isSingleUnencodedPart = (segmentNumber == 0) && isLastSegment && !(segment.Encoder == ExtendedParameterEncoder);

			var outBytesRequiredCapacity = isSingleUnencodedPart ?
					name.Length + 2 + (segment.Count * 2) :
					name.Length + 4 + ((segmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (segmentNumber))) + (segment.Count * 4);
			var outBytes = ArrayPool<byte>.Shared.Rent (outBytesRequiredCapacity);

			AsciiCharSet.GetBytes (name.AsSpan (), outBytes);
			var outBytesPos = name.Length;
			int bytesProduced;
			string result;

			if (isSingleUnencodedPart)
			{
				// значение из одной части не требующей кодирования (возможно квотирование)
				outBytes[outBytesPos++] = (byte)'=';
				(bytesProduced, _) = segment.Encoder.Encode (
					source: value,
					offset: segment.Offset,
					count: segment.Count,
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
				var idxStr = segmentNumber.ToString (CultureInfo.InvariantCulture);
				AsciiCharSet.GetBytes (idxStr.AsSpan (), outBytes.AsSpan (outBytesPos));
				outBytesPos += idxStr.Length;
				if (segment.Encoder is HeaderFieldBodyExtendedParameterValueEstimatingEncoder)
				{
					outBytes[outBytesPos++] = (byte)'*';
				}

				outBytes[outBytesPos++] = (byte)'=';
				(bytesProduced, _) = segment.Encoder.Encode (
					source: value,
					offset: segment.Offset,
					count: segment.Count,
					destination: outBytes,
					outOffset: outBytesPos,
					maxOutCount: outBytesRequiredCapacity - outBytesPos,
					segmentNumber: segmentNumber,
					isLastSegment: isLastSegment);
				outBytesPos += bytesProduced;
				if (!isLastSegment)
				{
					outBytes[outBytesPos++] = (byte)';';
				}
			}

			segmentNumber++;
			result = AsciiCharSet.GetString (outBytes.AsSpan (0, outBytesPos));
			ArrayPool<byte>.Shared.Return (outBytes);
			return result;
		}

		private static EstimatingEncoderChunk GetNextChunk (int nameLength, byte[] source, int segmentNumber, ref int pos)
		{
			// TODO: реализовать поддержку смещения и кол-ва в source
			var segmentNumberChars = (segmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (segmentNumber));
			var maxOutCount = HeaderEncoder.MaxLineLengthRecommended - nameLength - " *=;".Length - segmentNumberChars;

			EstimatingEncoderChunk result;

			// кодироващики в порядке приоритета от лучших к худшим
			var encoder1 = AsciiEncoder;
			var encoder2 = QuotedStringEncoder;
			var encoder3 = ExtendedParameterEncoder;
			if (segmentNumber == 0)
			{
				// проверяем что все символы ASCII
				var subPos = 0;
				while (subPos < source.Length)
				{
					var octet = source[subPos];
					if ((octet >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[octet] & (short)(AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace)) == 0))
					{
						// значение, требующее кодирования или первый сегмент многосегментного значения
						// должны использовать только ExtendedParameterEncoder
						var (_, bytesConsumed) = encoder3.Estimate (source, pos, source.Length - pos, maxOutCount - 1, segmentNumber, true);
						result = new EstimatingEncoderChunk (encoder3, pos, bytesConsumed);
						pos += bytesConsumed;
						return result;
					}

					subPos++;
				}
			}

			var (_, bytesConsumed1) = encoder1.Estimate (source, pos, source.Length - pos, maxOutCount, segmentNumber, true);
			var (_, bytesConsumed2) = encoder2.Estimate (source, pos, source.Length - pos, maxOutCount, segmentNumber, true);
			var (_, bytesConsumed3) = encoder3.Estimate (source, pos, source.Length - pos, maxOutCount - 1, segmentNumber, true);

			if (bytesConsumed2 >= bytesConsumed3)
			{
				if (bytesConsumed1 >= bytesConsumed2)
				{
					result = new EstimatingEncoderChunk (encoder1, pos, bytesConsumed1);
					pos += bytesConsumed1;
				}
				else
				{
					result = new EstimatingEncoderChunk (encoder2, pos, bytesConsumed2);
					pos += bytesConsumed2;
				}
			}
			else
			{
				if (bytesConsumed1 >= bytesConsumed3)
				{
					result = new EstimatingEncoderChunk (encoder1, pos, bytesConsumed1);
					pos += bytesConsumed1;
				}
				else
				{
					result = new EstimatingEncoderChunk (encoder3, pos, bytesConsumed3);
					pos += bytesConsumed3;
				}
			}

			return result;
		}
	}
}
