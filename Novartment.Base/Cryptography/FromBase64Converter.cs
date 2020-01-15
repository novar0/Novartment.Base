using System;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// A transformation for decoding from "Base64" according to RFC 2045 part 6.8.
	/// </summary>
	public sealed class FromBase64Converter :
		ISpanCryptoTransform
	{
		private readonly char[] _cache = new char[4];
		private int _cachedCount = 0;

		/// <summary>
		/// Initializes a new instance of the FromBase64Converter class.
		/// </summary>
		public FromBase64Converter ()
		{
		}

		/// <summary>
		/// Gets the input block size.
		/// </summary>
		public int InputBlockSize => 4;

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		public int OutputBlockSize => 3;

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
		/// <remarks>
		/// The return value of TransformBlock is the number of bytes returned to outputBuffer and is
		/// always &lt;= OutputBlockSize.  If CanTransformMultipleBlocks is true, then inputCount may be
		/// any positive multiple of InputBlockSize.
		/// </remarks>
		public int TransformBlock (ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			var outputOffset = 0;
			var reserved = _cache.Length; // резервируем первые 4-байта для содержимого кэша
			var buf = CreateCopyUsingBase64Alphabet (inputBuffer, reserved);
			var inputCount = buf.Length - reserved;
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
				base64chars.AsSpan (0, base64chars.Length).CopyTo (outputBuffer.Slice (outputOffset));
				return base64chars.Length;
			}
		}

		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <returns>The computed transform.</returns>
		public ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer)
		{
			var reserved = _cache.Length; // резервируем первые 4-байта для содержимого кэша
			var buf = CreateCopyUsingBase64Alphabet (inputBuffer, reserved);
			var inputCount = buf.Length + _cachedCount - reserved;
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
		/// Does nothing, since the algorithm does not take additional resources.
		/// </summary>
		public void Dispose ()
		{
		}

		private static char[] CreateCopyUsingBase64Alphabet (ReadOnlySpan<byte> buffer, int reserved)
		{
			/*
			RFC 2045 часть 6.8 правила декодирования:
			Any characters outside of the base64 alphabet are to be ignored in base64-encoded data.
			... the occurrence of any "=" characters may be taken as evidence that the end of the data has been reached
			*/

			// валидация и подсчет необходимого размера
			var count = buffer.Length;
			int base64AlphabetCharCount = 0;
			var lastChar = char.MaxValue;
			for (var index = 0; index < count; index++)
			{
				var nextChar = (char)buffer[index];
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
				var nextChar = (char)buffer[index];
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
