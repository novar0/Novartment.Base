using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a provided region of memory.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public sealed class MemoryBufferedSource :
		IFastSkipBufferedSource
	{
		/// <summary>
		/// Gets empty source.
		/// </summary>
		public static readonly IBufferedSource Empty = new MemoryBufferedSource (ReadOnlyMemory<byte>.Empty);

		private readonly ReadOnlyMemory<byte> _buffer;
		private int _offset;
		private int _count;

		/// <summary>
		/// Initializes a new instance of the MemoryBufferedSource class which provides data from the specified region of memory.
		/// </summary>
		/// <param name="buffer">The region of memory to be used as a buffer.</param>
		public MemoryBufferedSource (ReadOnlyMemory<byte> buffer)
		{
			_buffer = buffer;
			_offset = 0;
			_count = buffer.Length;
		}

		/// <summary>
		/// Gets the buffer that contains the source data.
		/// The current offset and the amount of available data are in the Offset and Count properties.
		/// The buffer remains unchanged throughout the lifetime of the source.
		/// </summary>
		public ReadOnlyMemory<byte> BufferMemory => _buffer;

		/// <summary>
		/// Gets the offset of available source data in the BufferMemory.
		/// The amount of available source data is in the Count property.
		/// </summary>
		public int Offset => _offset;

		/// <summary>
		/// Gets the amount of source data available in the BufferMemory.
		/// The offset of available source data is in the Offset property.
		/// </summary>
		public int Count => _count;

		/// <summary>Gets True, because buffer is provided when this source is created and remains unchanged.</summary>
		public bool IsExhausted => true;

		/// <summary>
		/// Skips specified amount of data from the start of available data in the buffer.
		/// Properties Offset and Count may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip from the start of available data in the buffer.
		/// Must be less than or equal to the size of available data in the buffer.</param>
		public void Skip (int size)
		{
			if ((size < 0) || (size > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size > 0)
			{
				_offset += size;
				_count -= size;
			}
		}

		/// <summary>
		/// Does nothing, because buffer is provided when this source is created and remains unchanged.
		/// </summary>
		/// <param name="cancellationToken">Not used.</param>
		/// <returns>Completed task.</returns>
		public ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			return default;
		}

		/// <summary>
		/// Does nothing, because buffer is provided when this source is created and remains unchanged.
		/// </summary>
		/// <param name="size">Amount of data required in the buffer.</param>
		/// <param name="cancellationToken">Not used.</param>
		/// <returns>Completed task.</returns>
		public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size > _count)
			{
				throw new NotEnoughDataException (size - _count);
			}

			return default;
		}

		/// <summary>
		/// Tries to skip specified amount of data available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip.</param>
		/// <param name="cancellationToken">Not used.</param>
		/// <returns>
		/// Completed task, which wraps the number of actually skipped bytes of data.
		/// Regardless of the result, the source will provide data coming right after skipped.
		/// </returns>
		public ValueTask<long> SkipWihoutBufferingAsync (long size, CancellationToken cancellationToken = default)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			var available = _count;
			if (size > (long)available)
			{
				_offset = _count = 0;
				return new ValueTask<long> ((long)available);
			}

			_offset += (int)size;
			_count -= (int)size;
			return new ValueTask<long> (size);
		}
	}
}
