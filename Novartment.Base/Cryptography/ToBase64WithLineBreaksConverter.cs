using System;
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
	public sealed class ToBase64WithLineBreaksConverter :
		ISpanCryptoTransform
	{
		private const int MaxLineLen = 76;
		private readonly char[] _outArray = new char[MaxLineLen + 2];

		/// <summary>
		/// Инициализирует новый экземпляр класса ToBase64WithLineBreaksConverter.
		/// </summary>
		public ToBase64WithLineBreaksConverter()
		{
		}

		/// <summary>
		/// Получает размер входного блока.
		/// </summary>
		public int InputBlockSize => (MaxLineLen * 6) / 8;

		/// <summary>
		/// Получает размер выходного блока.
		/// </summary>
		public int OutputBlockSize => MaxLineLen + 2;

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
		/// <param name="outputBuffer">Выходной массив, в который записывается результат преобразования.</param>
		/// <returns>Число записанных байтов.</returns>
		public int TransformBlock (ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			var inputOffset = 0;
			var outputOffset = 0;
			int outputSize = 0;
			while (inputOffset < inputBuffer.Length)
			{
#if NETSTANDARD2_0
				var tempBuf = new byte[this.InputBlockSize];
				inputBuffer.Slice (inputOffset, this.InputBlockSize).CopyTo (tempBuf);
				var size = Convert.ToBase64CharArray (tempBuf, 0, tempBuf.Length, _outArray, 0, Base64FormattingOptions.None);
#else
				Convert.TryToBase64Chars (inputBuffer.Slice (inputOffset, this.InputBlockSize), _outArray, out int size, Base64FormattingOptions.None);
#endif
				if (size != MaxLineLen)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Wrong size of chunk of base64-encoded data. Specified {size}, expected {MaxLineLen}."));
				}

				AsciiCharSet.GetBytes (_outArray.AsSpan (0, size), outputBuffer.Slice (outputOffset));
				outputBuffer[outputOffset + size++] = 0x0d;
				outputBuffer[outputOffset + size++] = 0x0a;
				outputSize += size;
				inputOffset += this.InputBlockSize;
				outputOffset += size;
			}

			return outputSize;
		}

		/// <summary>
		/// Преобразует заданную область заданного массива байтов.
		/// </summary>
		/// <param name="inputBuffer">Входные данные, для которых вычисляется преобразование.</param>
		/// <returns>Вычисленное преобразование.</returns>
		public ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer)
		{
			if (inputBuffer.Length < 1)
			{
				return Array.Empty<byte> ();
			}

#if NETSTANDARD2_0
			var tempBuf = new byte[inputBuffer.Length];
			inputBuffer.CopyTo (tempBuf);
			var size = Convert.ToBase64CharArray (tempBuf, 0, tempBuf.Length, _outArray, 0, Base64FormattingOptions.None);
#else
			Convert.TryToBase64Chars (inputBuffer, _outArray, out int size, Base64FormattingOptions.None);
#endif
			var result = new byte[size];
			AsciiCharSet.GetBytes (_outArray.AsSpan (0, size), result);
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
