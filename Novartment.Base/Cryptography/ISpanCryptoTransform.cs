using System;

namespace Novartment.Base
{
	/// <summary>
	/// Defines the basic operations of cryptographic transformations.
	/// </summary>
	/// <remarks>
	/// An analog of System.Security.Cryptography.ICryptoTransform adapted to use Span and Memory.
	/// </remarks>
	public interface ISpanCryptoTransform :
		IDisposable
	{
		/// <summary>
		/// Gets the input block size.
		/// </summary>
		int InputBlockSize { get; }

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		int OutputBlockSize { get; }

		/// <summary>
		/// Gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		/// <remarks>
		/// CanTransformMultipleBlocks == true implies that TransformBlock() can accept any number
		/// of whole blocks, not just a single block.
		/// If CanTransformMultipleBlocks is false, you have to feed blocks one at a time.
		/// </remarks>
		bool CanTransformMultipleBlocks { get; }

		/// <summary>
		/// Gets a value indicating whether the current transform can be reused.
		/// </summary>
		/// <remarks>
		/// If CanReuseTransform is true, then after a call to TransformFinalBlock() the transform
		/// resets its internal state to its initial configuration (with Key and IV loaded) and can
		/// be used to perform another encryption/decryption.
		/// </remarks>
		bool CanReuseTransform { get; }

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
		int TransformBlock (ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer);

		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <returns>The computed transform.</returns>
		/// <remarks>
		/// Special function for transforming the last block or partial block in the stream.  The
		/// return value is an array containing the remaining transformed bytes.
		/// We return a new array here because the amount of information we send back at the end could
		/// be larger than a single block once padding is accounted for.
		/// </remarks>
		ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer);
	}
}
