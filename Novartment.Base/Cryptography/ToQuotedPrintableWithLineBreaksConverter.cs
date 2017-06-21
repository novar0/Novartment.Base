﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// Трансформация для кодировки в RFC 2045 6.7 Quoted-Printable с учётом ограничения на длину строки.
	/// </summary>
	public class ToQuotedPrintableWithLineBreaksConverter :
		ICryptoTransform
	{
		[SuppressMessage (
			"Microsoft.Performance",
			"CA1802:UseLiteralsWhereAppropriate",
			Justification = "No performance gain could be achieved.")]
		private static readonly int _maxLineLen = 75; // по стандарту предел 76 печатных символов, но чтобы начать новую строку надо добавлять символ '='
		[SuppressMessage (
			"Microsoft.Performance",
			"CA1802:UseLiteralsWhereAppropriate",
			Justification = "No performance gain could be achieved.")]
		private static readonly int _inputBlockSize = 25;
		private readonly bool _isText;

		private readonly byte[] _outArray = new byte[(_maxLineLen * 2) + 3];
		private readonly int[] _outArraySizes = new int[(_maxLineLen * 2) + 3];

		private int _outArrayOffset;
		private int _outArrayCount;

		private int _outArraySizesOffset;
		private int _outArraySizesCount;

		/// <summary>
		/// Инициализирует новый экземпляр класса ToQuotedPrintableWithLineBreaksConverter с указанным режимом обработки текста.
		/// </summary>
		/// <param name="isText">
		/// Укажите True чтобы сохранять без трансформации встречающиеся в исходных данных символы перевода строки.
		/// </param>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public ToQuotedPrintableWithLineBreaksConverter (bool isText = false)
		{
			_isText = isText;
		}

		/// <summary>
		/// Получает размер входного блока.
		/// </summary>
		public int InputBlockSize => _inputBlockSize;

		/// <summary>
		/// Получает размер выходного блока.
		/// </summary>
		public int OutputBlockSize => _maxLineLen + 3;

		/// <summary>
		/// Получает значение, указывающее на возможность повторного использования текущего преобразования.
		/// </summary>
		public bool CanReuseTransform => true;

		/// <summary>
		/// Получает значение, указывающее на возможность преобразования нескольких блоков.
		/// </summary>
		public bool CanTransformMultipleBlocks => true;

		/// <summary>
		/// Преобразует заданную область входного массива байтов и копирует результат в заданную область выходного массива байтов.
		/// </summary>
		/// <param name="inputBuffer">Входные данные, для которых вычисляется преобразование.</param>
		/// <param name="inputOffset">Смещение во входном массиве байтов, начиная с которого следует использовать данные.</param>
		/// <param name="inputCount">Число байтов во входном массиве для использования в качестве данных.</param>
		/// <param name="outputBuffer">Выходной массив, в который записывается результат преобразования.</param>
		/// <param name="outputOffset">Смещение в выходном массиве байтов, начиная с которого следует записывать данные.</param>
		/// <returns>Число записанных байтов.</returns>
		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (inputBuffer == null)
			{
				throw new ArgumentNullException (nameof (inputBuffer));
			}

			if ((inputOffset < 0) || (inputOffset > inputBuffer.Length) || ((inputOffset == inputBuffer.Length) && (inputCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (inputOffset));
			}

			if ((inputCount < 0) || ((inputOffset + inputCount) > inputBuffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (inputCount));
			}

			if (outputBuffer == null)
			{
				throw new ArgumentNullException (nameof (outputBuffer));
			}

			if ((outputOffset < 0) || (outputOffset > outputBuffer.Length) || ((outputOffset == outputBuffer.Length) && (inputCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (outputOffset));
			}

			Contract.EndContractBlock ();

			int outputSize = 0;
			var inputEndOffset = inputOffset + inputCount;
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
						Array.Copy (_outArray, 0, outputBuffer, outputOffset, totalSize);
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
						if ((totalSize + _outArraySizes[idx]) > _maxLineLen)
						{
							Array.Copy (_outArray, 0, outputBuffer, outputOffset, totalSize);
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
				var inputSize = Math.Min (_inputBlockSize, inputEndOffset - inputOffset);
				FillOutBuffer (inputBuffer, inputOffset, inputSize);
				inputOffset += inputSize;
			}
			while (inputOffset < inputEndOffset);

			return outputSize;
		}

		/// <summary>
		/// Преобразует заданную область заданного массива байтов.
		/// </summary>
		/// <param name="inputBuffer">Входные данные, для которых вычисляется преобразование.</param>
		/// <param name="inputOffset">Смещение в массиве байтов, начиная с которого следует использовать данные.</param>
		/// <param name="inputCount">Число байтов в массиве для использования в качестве данных.</param>
		/// <returns>Вычисленное преобразование.</returns>
		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
			{
				throw new ArgumentNullException (nameof (inputBuffer));
			}

			if ((inputOffset < 0) || (inputOffset > inputBuffer.Length) || ((inputOffset == inputBuffer.Length) && (inputCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (inputOffset));
			}

			if ((inputCount < 0) || ((inputOffset + inputCount) > inputBuffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (inputCount));
			}

			Contract.EndContractBlock ();

			var result = new byte[_maxLineLen + _outArray.Length];
			int resultOffset = 0;
			if (inputCount > 0)
			{
				// трансформируем входные данные (они добавятся к уже накопленным в буфере)
				resultOffset = TransformBlock (inputBuffer, inputOffset, inputCount, result, 0);
			}

			// забираем остаток из буфера
			Array.Copy (_outArray, 0, result, resultOffset, _outArrayCount);
			Array.Resize (ref result, resultOffset + _outArrayCount);

			ResetBuffer ();

			return result;
		}

		/// <summary>
		/// Ничего не делает, так как алгоритм не занимает дополнительных ресурсов.
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

		private void FillOutBuffer (byte[] inArray, int offsetIn, int length)
		{
			var offsetInEnd = offsetIn + length;
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
								_outArray[_outArrayOffset++] = (byte)Hex.OctetsUpper[prevOctet][0];
								_outArray[_outArrayOffset++] = (byte)Hex.OctetsUpper[prevOctet][1];
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
					_outArray[_outArrayOffset++] = (byte)Hex.OctetsUpper[octet][0];
					_outArray[_outArrayOffset++] = (byte)Hex.OctetsUpper[octet][1];
					_outArraySizes[_outArraySizesOffset++] = 3;
					_outArraySizesCount++;
					_outArrayCount += 3;
				}
			}
		}
	}
}
