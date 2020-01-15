using System;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// A transformation for encoding to "Quoted-Printable" according to RFC 2045 part 6.7,
	/// taking into account string length restrictions.
	/// </summary>
	public sealed class ToQuotedPrintableWithLineBreaksConverter :
		ISpanCryptoTransform
	{
		private const int MaximumLineLen = 75; // по стандарту предел 76 печатных символов, но чтобы начать новую строку надо добавлять символ '='
		private const int MaximumSourceToSurelyFitMaximumLineLen = 25;
		private readonly bool _isText;

		private readonly byte[] _outArray = new byte[(MaximumLineLen * 2) + 3];
		private readonly int[] _outArraySizes = new int[(MaximumLineLen * 2) + 3];

		private int _outArrayOffset;
		private int _outArrayCount;

		private int _outArraySizesOffset;
		private int _outArraySizesCount;

		/// <summary>
		/// Initializes a new instance of the ToQuotedPrintableWithLineBreaksConverter class that is uses the specified text handling mode.
		/// </summary>
		/// <param name="isText">
		/// The value indicating whether to keep without transformation the line feed characters found in the source data.
		/// </param>
		public ToQuotedPrintableWithLineBreaksConverter (bool isText = false)
		{
			_isText = isText;
		}

		/// <summary>
		/// Gets the input block size.
		/// </summary>
		public int InputBlockSize => MaximumSourceToSurelyFitMaximumLineLen;

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		public int OutputBlockSize => MaximumLineLen + 3;

		/// <summary>
		/// Gets a value indicating that TransformBlock() can accept any number
		/// of whole blocks, not just a single block.
		/// </summary>
		public bool CanTransformMultipleBlocks => true;

		/// <summary>
		/// Gets a value indicating that after a call to TransformFinalBlock() the transform
		/// resets its internal state to its initial configuration and can
		/// be used to perform another encryption/decryption.
		/// </summary>
		public bool CanReuseTransform => true;

		/// <summary>
		/// Transforms the specified region of the input byte array and copies the resulting
		/// transform to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="outputBuffer">The output to which to write the transform.</param>
		/// <returns>The number of bytes written.</returns>
		public int TransformBlock (ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			var inputOffset = 0;
			var outputOffset = 0;
			int outputSize = 0;
			var inputEndOffset = inputBuffer.Length;
			do
			{
				// сканируем буфер до тех пор пока он не кончится
				int totalSize = 0; // суммарный размер всех элементов до текущего
				int idx = 0; // номер элемента в буфере
				while (idx < _outArraySizesCount)
				{
					// встретился перевод строки, выводим всё до него
					if (_outArraySizes[idx] == 2)
					{
						totalSize += _outArraySizes[idx];
						_outArray.AsSpan (0, totalSize).CopyTo (outputBuffer.Slice (outputOffset));
						outputSize += totalSize;
						outputOffset += totalSize;
						_outArrayCount -= totalSize;
						_outArrayOffset -= totalSize;
						_outArraySizesCount -= idx + 1;
						_outArraySizesOffset -= idx + 1;

						// сдвигаем остаток буфера outArray в начало
						if (_outArrayCount > 0)
						{
							Array.Copy (_outArray, totalSize, _outArray, 0, _outArrayCount);
						}

						// сдвигаем остаток буфера outArraySizes в начало
						if (_outArraySizesCount > 0)
						{
							Array.Copy (_outArraySizes, idx + 1, _outArraySizes, 0, _outArraySizesCount);
						}

						// начнём сначала сканирование буфера
						totalSize = 0;
						idx = 0;
					}
					else
					{
						// данных хватает на целую строку, выводим её
						if ((totalSize + _outArraySizes[idx]) > MaximumLineLen)
						{
							_outArray.AsSpan (0, totalSize).CopyTo (outputBuffer.Slice (outputOffset));
							outputSize += totalSize;
							outputOffset += totalSize;
							_outArrayCount -= totalSize;
							_outArrayOffset -= totalSize;
							_outArraySizesCount -= idx;
							_outArraySizesOffset -= idx;
							outputBuffer[outputOffset++] = 0x3d;
							outputSize++;
							outputBuffer[outputOffset++] = 0x0d;
							outputSize++;
							outputBuffer[outputOffset++] = 0x0a;
							outputSize++;

							// сдвигаем остаток буфера outArray в начало
							if (_outArrayCount > 0)
							{
								Array.Copy (_outArray, totalSize, _outArray, 0, _outArrayCount);
							}

							// сдвигаем остаток буфера outArraySizes в начало
							if (_outArraySizesCount > 0)
							{
								Array.Copy (_outArraySizes, idx, _outArraySizes, 0, _outArraySizesCount);
							}

							// начнём сначала сканирование буфера
							totalSize = 0;
							idx = 0;
						}
						else
						{
							// идём дальше по элементам буфера, суммируя их размер
							totalSize += _outArraySizes[idx++];
						}
					}
				}

				// сюда попали только когда в буфере недостаточно данных для вывода строки, поэтому добавляем данные в буфер
				var inputSize = Math.Min (MaximumSourceToSurelyFitMaximumLineLen, inputEndOffset - inputOffset);
				FillOutBuffer (inputBuffer.Slice (inputOffset, inputSize));
				inputOffset += inputSize;
			}
			while (inputOffset < inputEndOffset);

			return outputSize;
		}

		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <returns>The computed transform.</returns>
		public ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer)
		{
			var result = new byte[MaximumLineLen + _outArray.Length];
			int resultOffset = 0;
			if (inputBuffer.Length > 0)
			{
				// трансформируем входные данные (они добавятся к уже накопленным в буфере)
				resultOffset = TransformBlock (inputBuffer, result);
			}

			// забираем остаток из буфера
			Array.Copy (_outArray, 0, result, resultOffset, _outArrayCount);
			var resultMem = result.AsMemory (0, resultOffset + _outArrayCount);
			ResetBuffer ();

			return resultMem;
		}

		/// <summary>
		/// Does nothing, since the algorithm does not take additional resources.
		/// </summary>
		public void Dispose ()
		{
		}

		private void ResetBuffer ()
		{
			_outArraySizesOffset = 0;
			_outArraySizesCount = 0;

			_outArrayOffset = 0;
			_outArrayCount = 0;
		}

		private void FillOutBuffer (ReadOnlySpan<byte> inArray)
		{
			var offsetIn = 0;
			var offsetInEnd = inArray.Length;
			var hexOctets = Hex.OctetsUpper.Span;
			while (offsetIn < offsetInEnd)
			{
				var octet = inArray[offsetIn++];

				// в текстовом режиме встретился перевод строки
				if (_isText && (offsetIn < offsetInEnd))
				{
					if ((octet == 0x0d) && (inArray[offsetIn] == 0x0a))
					{
						// если перед ним был пробел или таб, то надо его закодировать
						if (_outArrayOffset > 0)
						{
							var prevOctet = _outArray[_outArrayOffset - 1];
							if ((prevOctet == 0x09) || (prevOctet == 0x20))
							{
								// откатываемся на предыдущую позицию
								_outArrayOffset--;
								_outArraySizesOffset--;
								_outArraySizesCount--;
								_outArrayCount--;

								// вписываем закодированный вариант предыдущего символа
								_outArray[_outArrayOffset++] = 0x3d; // =
								_outArray[_outArrayOffset++] = (byte)hexOctets[prevOctet][0];
								_outArray[_outArrayOffset++] = (byte)hexOctets[prevOctet][1];
								_outArraySizes[_outArraySizesOffset++] = 3;
								_outArraySizesCount++;
								_outArrayCount += 3;
							}
						}

						offsetIn++;
						_outArray[_outArrayOffset++] = 0x0d;
						_outArray[_outArrayOffset++] = 0x0a;
						_outArrayCount += 2;
						_outArraySizes[_outArraySizesOffset++] = 2;
						_outArraySizesCount++;
						continue;
					}
				}

				// printable char (except '=' 0x3d)
				if (((octet >= 0x21) && (octet <= 0x3c)) ||
					((octet >= 0x3e) && (octet <= 0x7e)) ||
					(_isText && ((octet == 0x09) || (octet == 0x20))))
				{
					_outArray[_outArrayOffset++] = octet;
					_outArraySizes[_outArraySizesOffset++] = 1;
					_outArraySizesCount++;
					_outArrayCount++;
				}
				else
				{
					// '=' 0x3d or non printable char
					_outArray[_outArrayOffset++] = 0x3d;
					_outArray[_outArrayOffset++] = (byte)hexOctets[octet][0];
					_outArray[_outArrayOffset++] = (byte)hexOctets[octet][1];
					_outArraySizes[_outArraySizesOffset++] = 3;
					_outArraySizesCount++;
					_outArrayCount += 3;
				}
			}
		}
	}
}
