using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a byte buffer.
	/// </summary>
	[ContractClass (typeof (IBufferedSourceContracts))]
	public interface IBufferedSource
	{
		/// <summary>
		/// Gets the buffer that contains some of the source data.
		/// The current offset and the amount of available data are in the Offset and Count properties.
		/// The buffer remains unchanged throughout the lifetime of the source.
		/// </summary>
		ReadOnlyMemory<byte> BufferMemory { get; }

		/// <summary>
		/// Gets the offset of available source data in the BufferMemory.
		/// The amount of available source data is in the Count property.
		/// </summary>
		int Offset { get; }

		/// <summary>
		/// Gets the amount of source data available in the BufferMemory.
		/// The offset of available source data is in the Offset property.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Gets a value indicating whether the source is exhausted.
		/// Returns True if the source no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		bool IsExhausted { get; }

		/// <summary>
		/// Asynchronously requests the source to load more data in the buffer.
		/// As a result, the buffer may not be completely filled if the source supplies data in blocks,
		/// or it may be empty if the source is exhausted.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous load operation.
		/// If Count property equals zero after completion,
		/// this means that the source is exhausted and there will be no more data in the buffer.</returns>
		ValueTask LoadAsync (CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously requests the source to load the specified amount of data in the buffer.
		/// As a result, there may be more data in the buffer than requested.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Amount of data required in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Specified size less than zero or more than size of the buffer.
		/// </exception>
		/// <exception cref="Novartment.Base.BinaryStreaming.NotEnoughDataException">
		/// The source can not provide specified amount of data.
		/// </exception>
		ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default);

		/// <summary>
		/// Skips specified amount of available data in the buffer.
		/// Properties Offset and Count may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip from the start of available data in the buffer.
		/// Must be less than or equal to the size of available data in the buffer.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Specified size less than zero or more than size of the buffer.
		/// </exception>
		void Skip (int size);
	}

	/// <summary>
	/// Metadata for contracts of IBufferedSource.
	/// </summary>
	[ContractClassFor (typeof (IBufferedSource))]
	internal abstract class IBufferedSourceContracts :
		IBufferedSource
	{
		private IBufferedSourceContracts ()
		{
		}

		public ReadOnlyMemory<byte> BufferMemory => default;

		public int Offset => 0;

		public int Count => 0;

		public bool IsExhausted => false;

		public ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			Contract.Ensures (this.BufferMemory.Equals (Contract.OldValue (this.BufferMemory)));
			Contract.Ensures ((this.Count > 0) || this.IsExhausted);
			Contract.EndContractBlock ();
			return default;
		}

		public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default)
		{
			Contract.Requires (size >= 0);
			Contract.Requires (size <= this.BufferMemory.Length);

			Contract.Ensures (this.BufferMemory.Equals (Contract.OldValue (this.BufferMemory)));
			Contract.EndContractBlock ();
			return default;
		}

		public void Skip (int size)
		{
			Contract.Requires ((size >= 0) && (size <= this.Count));

			Contract.Ensures (this.BufferMemory.Equals (Contract.OldValue (this.BufferMemory)));
			Contract.Ensures ((this.Offset + this.Count) == (Contract.OldValue (this.Offset) + Contract.OldValue (this.Count)));
			Contract.Ensures (this.IsExhausted == Contract.OldValue (this.IsExhausted));
			Contract.EndContractBlock ();
		}
	}
}
