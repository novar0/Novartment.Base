using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// Трансформация для кодировки в RFC 2045 6.8 Base64 с учётом ограничения на длину строки.
	/// </summary>
	/// <remarks>
	/// От библиотечного System.Security.Cryptography.ToBase64Transform отличается тем, что:
	/// а) поддерживает преобразование нескольких блоков;
	/// б) блоком считается полная строка (требует на входе 48 байтов);
	/// в) вставляет перевод строки в конце каждого выходного блока.
	/// </remarks>
	public class ToBase64WithLineBreaksConverter :
		ICryptoTransform
	{
		[SuppressMessage (
			"Microsoft.Performance",
			"CA1802:UseLiteralsWhereAppropriate",
			Justification = "No performance gain could be achieved.")]
		private static readonly int _maxLineLen = 76;
		private readonly char[] _outArray = new char[_maxLineLen + 2];

		/// <summary>
		/// Инициализирует новый экземпляр класса ToBase64WithLineBreaksConverter.
		/// </summary>
		public ToBase64WithLineBreaksConverter()
		{
		}

		/// <summary>
		/// Получает размер входного блока.
		/// </summary>
		public int InputBlockSize => (_maxLineLen * 6) / 8;

		/// <summary>
		/// Получает размер выходного блока.
		/// </summary>
		public int OutputBlockSize => _maxLineLen + 2;

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
			var endInputOffset = inputOffset + inputCount;
			while (inputOffset < endInputOffset)
			{
				var size = Convert.ToBase64CharArray (inputBuffer, inputOffset, InputBlockSize, _outArray, 0);
				if (size != _maxLineLen)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Wrong size of chunk of base64-encoded data. Specified {size}, expected {_maxLineLen}."));
				}

				AsciiCharSet.GetBytes (_outArray, 0, size, outputBuffer, outputOffset);
				outputBuffer[outputOffset + size++] = 0x0d;
				outputBuffer[outputOffset + size++] = 0x0a;
				outputSize += size;
				inputOffset += InputBlockSize;
				outputOffset += size;
			}

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

			if (inputCount == 0)
			{
				return Array.Empty<byte> ();
			}

			var size = Convert.ToBase64CharArray (inputBuffer, inputOffset, inputCount, _outArray, 0);
			var result = new byte[size];
			AsciiCharSet.GetBytes (_outArray, 0, size, result, 0);
			return result;
		}

		/// <summary>
		/// Ничего не делает, так как алгоритм не занимает дополнительных ресурсов.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
