using System;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// A transformation for encoding to "Base64" according to RFC 2045 part 6.8,
	/// taking into account string length restrictions.
	/// </summary>
	/// <remarks>
	/// Differences from the library System.Security.Cryptography.ToBase64Transform:
	/// 1) supports the transformation of several blocks;
	/// 2) a block is a complete line (requires 48 bytes at the input);
	/// 3) inserts a line feed at the end of each output block.
	/// </remarks>
	public sealed class ToBase64WithLineBreaksConverter :
		ISpanCryptoTransform
	{
		private const int MaxLineLen = 76;
		private readonly char[] _outArray = new char[MaxLineLen + 2];

		/// <summary>
		/// Initializes a new instance of the ToBase64WithLineBreaksConverter class.
		/// </summary>
		public ToBase64WithLineBreaksConverter ()
		{
		}

		/// <summary>
		///  Gets the input block size.
		/// </summary>
		public int InputBlockSize => (MaxLineLen * 6) / 8;

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		public int OutputBlockSize => MaxLineLen + 2;

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

				AsciiCharSet.GetBytes (_outArray.AsSpan (0, size), outputBuffer[outputOffset..]);
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
		/// Does nothing, since the algorithm does not take additional resources.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
