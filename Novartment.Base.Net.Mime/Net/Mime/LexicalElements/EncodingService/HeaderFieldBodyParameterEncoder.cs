using System;
using System.Buffers;
using System.Globalization;
using System.Text;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Строковое значение, позволяющее получать информацию об отдельных сегментах, разделённых пробелами.
	/// </summary>
	internal class HeaderFieldBodyParameterEncoder
	{
		private readonly string _name;
		private readonly byte[] _value;
		private readonly EstimatingEncoderChunk[] _valueSegments;
		private readonly IEstimatingEncoder _extendedParameterEncoder;
		private int _currentSegmentNumber = 0;

		private HeaderFieldBodyParameterEncoder (string parameterName, byte[] parameterValue, EstimatingEncoderChunk[] valueParts, IEstimatingEncoder extendedParameterEncoder)
		{
			_name = parameterName;
			_value = parameterValue;
			_valueSegments = valueParts;
			_extendedParameterEncoder = extendedParameterEncoder;
		}

		/// <summary>
		/// Разбирает указанный текст на отдельные слова.
		/// Каждое слово содержит хотя бы один непробельный символ.
		/// Каждое слово кроме первого начинается с пробельного символа.
		/// </summary>
		/// <param name="parameterName">Имя параметра.</param>
		/// <param name="parameterValue">Текст для разбивки на части.</param>
		internal static HeaderFieldBodyParameterEncoder Parse (string parameterName, string parameterValue)
		{
			/*
			RFC 2231 часть 4.1:
			1. Language and character set information only appear at the beginning of a given parameter value.
			2. Continuations do not provide a facility for using more than one character set or language in the same parameter value.
			3. A value presented using multiple continuations may contain a mixture of encoded and unencoded segments.
			4. The first segment of a continuation MUST be encoded if language and character set information are given.
			5. If the first segment of a continued parameter value is encoded the language and character set field delimiters MUST be present even when the fields are left blank.
			*/

			var bytes = Encoding.UTF8.GetBytes (parameterValue);

			var isEncodingNeeded = !AsciiCharSet.IsAllOfClass (parameterValue, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);

			var extendedParameterEncoder = new HeaderFieldBodyExtendedParameterValueEstimatingEncoder (Encoding.UTF8);
			var encodersOne = ReadOnlyList.Repeat<IEstimatingEncoder> (extendedParameterEncoder, 1);
			var encodersAll = new ReadOnlyArray<IEstimatingEncoder> (new IEstimatingEncoder[]
			{
				new AsciiCharClassEstimatingEncoder (AsciiCharClasses.Token),
				new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace),
				extendedParameterEncoder,
			});
			var extra = " *=;".Length + parameterName.Length;
			var valueParts = EstimatingEncoder.CutBySize (
				segmentNumber => (isEncodingNeeded && (segmentNumber == 0)) ?
					encodersOne :
					encodersAll, // если нужна кодировка то первый кусок принудительно делаем в виде 'extended-value'
				bytes,
				HeaderEncoder.MaxLineLengthRecommended,
				(segmentNumber, encoder) => extra + ((segmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (segmentNumber))) + ((encoder is HeaderFieldBodyExtendedParameterValueEstimatingEncoder) ? 1 : 0));

			var data = new HeaderFieldBodyParameterEncoder (parameterName, bytes, valueParts, extendedParameterEncoder);
			return data;
		}

		/// <summary>
		/// Получает следующий сегмент из последовательности, образующей исходную строку.
		/// Сегмент закодирован для использования в виде значения параметра поля заголовка и пригоден для фолдинга с другими сегментами.
		/// Сегменты выбираются из исходной строки по следующим правилам:
		/// 1. Объединяет несколько слов если первое и последнее слово требуют кодирования (для экономии на накладных расходах кодирования).
		/// 2. Имеет суммарную длину не более HeaderEncoder.MaxLineLengthRequired (для читабельности результата).
		/// </summary>
		/// <returns>Следующий сегмент из последовательности, образующей исходную строку. Если вся исходная строка уже выдана, то null.</returns>
		internal string GetNextSegment ()
		{
			if (_currentSegmentNumber >= _valueSegments.Length)
			{
				return null;
			}

			var isSingleUnencodedPart = (_valueSegments.Length < 2) && !(_valueSegments[0].Encoder is HeaderFieldBodyExtendedParameterValueEstimatingEncoder);

			var outBytesRequiredCapacity = isSingleUnencodedPart ?
					_name.Length + 2 + (_valueSegments[_currentSegmentNumber].Count * 2) :
					_name.Length + 4 + ((_currentSegmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (_currentSegmentNumber))) + (_valueSegments[_currentSegmentNumber].Count * 4);
			var outBytes = ArrayPool<byte>.Shared.Rent (outBytesRequiredCapacity);

			AsciiCharSet.GetBytes (_name.AsSpan (), outBytes);
			var outBytesPos = _name.Length;
			int bytesProduced;
			string result;

			if (isSingleUnencodedPart)
			{
				// значение из одной части не требующей кодирования (возможно квотирование)
				outBytes[outBytesPos++] = (byte)'=';
				(bytesProduced, _) = _valueSegments[0].Encoder.Encode (
					_value,
					_valueSegments[0].Offset,
					_valueSegments[0].Count,
					outBytes,
					outBytesPos,
					outBytesRequiredCapacity - outBytesPos,
					0,
					false);
				outBytesPos += bytesProduced;
			}
			else
			{
				// значение из многих частей либо требует кодирования
				outBytes[outBytesPos++] = (byte)'*';
				var idxStr = _currentSegmentNumber.ToString (CultureInfo.InvariantCulture);
				AsciiCharSet.GetBytes (idxStr.AsSpan (), outBytes.AsSpan (outBytesPos));
				outBytesPos += idxStr.Length;
				if (_valueSegments[_currentSegmentNumber].Encoder == _extendedParameterEncoder)
				{
					outBytes[outBytesPos++] = (byte)'*';
				}

				outBytes[outBytesPos++] = (byte)'=';
				(bytesProduced, _) = _valueSegments[_currentSegmentNumber].Encoder.Encode (
					_value,
					_valueSegments[_currentSegmentNumber].Offset,
					_valueSegments[_currentSegmentNumber].Count,
					outBytes,
					outBytesPos,
					outBytesRequiredCapacity - outBytesPos,
					_currentSegmentNumber,
					_currentSegmentNumber < (_valueSegments.Length - 1));
				outBytesPos += bytesProduced;
				if (_currentSegmentNumber != (_valueSegments.Length - 1))
				{
					outBytes[outBytesPos++] = (byte)';';
				}
			}

			_currentSegmentNumber++;
			result = AsciiCharSet.GetString (outBytes.AsSpan (0, outBytesPos));
			ArrayPool<byte>.Shared.Return (outBytes);
			return result;
		}
	}
}
