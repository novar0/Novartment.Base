using System;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// A transformation for decoding from "Quoted-Printable" according to RFC 2045 part 6.7.
	/// </summary>
	public sealed class FromQuotedPrintableConverter :
		ISpanCryptoTransform
	{
		private readonly byte[] _buffer = new byte[3];
		private int _bufferOffset = 0;

		/// <summary>
		/// Initializes a new instance of the FromQuotedPrintableConverter class.
		/// </summary>
		public FromQuotedPrintableConverter ()
		{
		}

		/// <summary>
		///  Gets the input block size.
		/// </summary>
		public int InputBlockSize => 1;

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		public int OutputBlockSize => 1;

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
			var inputCount = inputBuffer.Length;
			var outputOffset = 0;
			var ret = 0;

			while (inputCount-- > 0)
			{
				var octet = inputBuffer[inputOffset++];

				if (_bufferOffset == 0)
				{
					if (octet == 0x3d)
					{ // '=' 0x3d
						_buffer[_bufferOffset++] = octet;
					}
					else
					{
						outputBuffer[outputOffset++] = octet;
						ret++;
					}
				}
				else
				{
					// quoted char
					_buffer[_bufferOffset++] = octet;
				}

				if (_bufferOffset == 3)
				{
					// dequote
					if ((_buffer[1] == 0x0d) && (_buffer[2] == 0x0a))
					{
						// soft newline (CarriageReturnLinefeed)
						_bufferOffset = 0;
					}
					else
					{
						if ((_buffer[1] == 0x0d) || (_buffer[1] == 0x0a))
						{
							// soft newline (CR, LF)
							if (_buffer[2] == 0x3d)
							{
								_bufferOffset = 1;
							}
							else
							{
								outputBuffer[outputOffset++] = _buffer[2];
								ret++;

								_bufferOffset = 0;
							}
						}
						else
						{
							outputBuffer[outputOffset++] = Hex.ParseByte ((char)_buffer[1], (char)_buffer[2]);
							ret++;

							_bufferOffset = 0;
						}
					}
				}
			}

			return ret;
		}

		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <returns>The computed transform.</returns>
		public ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer)
		{
			if (inputBuffer.Length < 1)
			{
				_bufferOffset = 0;
				return default;
			}

			var outputBuffer = new byte[inputBuffer.Length];
			var len = TransformBlock (inputBuffer, outputBuffer);
			_bufferOffset = 0;
			return outputBuffer.AsMemory (0, len);
		}

		/// <summary>
		/// Does nothing, since the algorithm does not take additional resources.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
