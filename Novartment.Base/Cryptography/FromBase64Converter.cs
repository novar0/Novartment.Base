using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// Трансформация для раскодирования из "Base64" согласно RFC 2045 часть 6.8.
	/// </summary>
	public sealed class FromBase64Converter :
		ICryptoTransform
	{
		private char[] _cache = new char[4];
		private int _cachedCount = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса FromBase64Converter.
		/// </summary>
		public FromBase64Converter()
		{
		}

		/// <summary>
		/// Получает размер входного блока.
		/// </summary>
		public int InputBlockSize => 4;

		/// <summary>
		/// Получает размер выходного блока.
		/// </summary>
		public int OutputBlockSize => 3;

		/// <summary>
		/// Получает значение, указывающее на возможность преобразования нескольких блоков.
		/// </summary>
		public bool CanTransformMultipleBlocks => true;

		/// <summary>
		/// Получает значение, указывающее на возможность повторного использования текущего преобразования.
		/// </summary>
		public bool CanReuseTransform => true;

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

			var reserved = _cache.Length; // резервируем первые 4-байта для содержимого кэша
			var buf = CreateCopyUsingBase64Alphabet (inputBuffer, inputOffset, inputCount, reserved);
			inputCount = buf.Length - reserved;
			var totalSize = inputCount + _cachedCount;
			if (totalSize < 4)
			{ // все уходит в кэш, ничего не возвращаем
				Array.Copy (buf, reserved, _cache, _cachedCount, inputCount);
				_cachedCount += inputCount;
				return 0;
			}
			else
			{
				var bufStart = reserved - _cachedCount;
				var bufLen = totalSize & -4; // -4 = 0xFFFFFFFC установлены все биты кроме младших двух

				// добавляем содержимое кэша в начало буфера
				Array.Copy (_cache, 0, buf, reserved - _cachedCount, _cachedCount);

				// остаток входных данных помещаем в кэш
				_cachedCount = totalSize & 3;
				Array.Copy (buf, bufStart + bufLen, _cache, 0, _cachedCount);

				var base64chars = Convert.FromBase64CharArray (buf, bufStart, bufLen);
				Array.Copy (base64chars, 0, outputBuffer, outputOffset, base64chars.Length);
				return base64chars.Length;
			}
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

			var reserved = _cache.Length; // резервируем первые 4-байта для содержимого кэша
			var buf = CreateCopyUsingBase64Alphabet (inputBuffer, inputOffset, inputCount, reserved);
			inputCount = buf.Length + _cachedCount - reserved;
			if ((inputCount & 3) != 0)
			{
				throw new FormatException ("Base64-encoded data is not multiple of 4.");
			}

			if (inputCount < 1)
			{
				return Array.Empty<byte> ();
			}

			// добавляем содержимое кэша в начало буфера
			Array.Copy (_cache, 0, buf, reserved - _cachedCount, _cachedCount);
			var result = Convert.FromBase64CharArray (buf, reserved - _cachedCount, inputCount);
			_cachedCount = 0;
			return result;
		}

		/// <summary>
		/// Ничего не делает, так как алгоритм не занимает дополнительных ресурсов.
		/// </summary>
		public void Dispose ()
		{
		}

		private static char[] CreateCopyUsingBase64Alphabet (byte[] buffer, int offset, int count, int reserved)
		{
			// RFC 2045 часть 6.8 правила декодирования:
			// Any characters outside of the base64 alphabet are to be ignored in base64-encoded data.
			// ... the occurrence of any "=" characters may be taken as evidence that the end of the data has been reached

			// валидация и подсчет необходимого размера
			int base64AlphabetCharCount = 0;
			var lastChar = char.MaxValue;
			for (var index = 0; index < count; index++)
			{
				var nextChar = (char)buffer[offset + index];
				var isNextCharWhiteSpace = char.IsWhiteSpace (nextChar);
				if (!isNextCharWhiteSpace)
				{
					if ((nextChar != '=') && (lastChar == '='))
					{
						throw new FormatException ("Base64-coded value contains ending marker '=' not in end position.");
					}

					var isNextCharBase64 = (nextChar == '=') ||
						AsciiCharSet.IsCharOfClass (nextChar, AsciiCharClasses.Base64Alphabet);
					if (isNextCharBase64)
					{
						base64AlphabetCharCount++;
					}

					lastChar = nextChar;
				}
			}

			var result = new char[reserved + base64AlphabetCharCount];

			// копирование только нужных символов
			var idx = reserved;
			for (var index = 0; index < count; ++index)
			{
				var nextChar = (char)buffer[offset + index];
				var isNextCharBase64 = (nextChar == '=') ||
					AsciiCharSet.IsCharOfClass (nextChar, AsciiCharClasses.Base64Alphabet);
				if (isNextCharBase64)
				{
					result[idx++] = nextChar;
				}
			}

			return result;
		}
	}
}
