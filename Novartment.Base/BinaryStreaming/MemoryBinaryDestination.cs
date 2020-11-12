using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base
{
	/// <summary>
	/// A binary data destination for sequential writing backed by a region of memory.
	/// As data is being written, the region of memory is replaced with another, larger size.
	/// </summary>
	/// <remarks>
	/// Similar to the library class System.IO.MemoryStream,
	/// but exposes written content as ReadOnlyMemory&lt;byte&gt;.
	/// </remarks>
	public sealed class MemoryBinaryDestination :
		IBinaryDestination
	{
		private const int MaxByteArrayLength = 0x7FFFFFC7;

		private byte[] _buffer;
		private int _position;

		/// <summary>
		/// Initializes a new instance of the MemoryBinaryDestination class.
		/// </summary>
		public MemoryBinaryDestination ()
			: this (0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MemoryBinaryDestination class which is backed by a region of memory of the specified size.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		public MemoryBinaryDestination (int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (capacity));
			}

			_buffer = capacity != 0 ? new byte[capacity] : Array.Empty<byte> ();
		}

		/// <summary>
		/// Returns the region of memory that contains the written data.
		/// </summary>
		/// <returns>The region of memory that contains the written data.</returns>
		public ReadOnlyMemory<byte> GetBuffer ()
		{
			return _buffer.AsMemory (0, _position);
		}

		/// <summary>
		/// Asynchronously writes the specified region of memory to this destination.
		/// </summary>
		/// <param name="buffer">The region of memory to write to this destination.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the write operation.</returns>
		public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			Write (buffer.Span);
			return default;
		}

		/// <summary>
		/// Writes the specified region of memory to this destination.
		/// </summary>
		/// <param name="buffer">The region of memory to write to this destination.</param>
		public void Write (ReadOnlySpan<byte> buffer)
		{
			int newPosition = _position + buffer.Length;

			if (newPosition > _buffer.Length)
			{
				IncreaseCapacity (newPosition);
			}

			buffer.CopyTo (new Span<byte> (_buffer, _position, buffer.Length));
			_position = newPosition;
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		public void SetComplete ()
		{
		}

		private void IncreaseCapacity (int requiredCapacity)
		{
			int newCapacity = Math.Max (Math.Max (256, requiredCapacity), _buffer.Length * 2);

			if ((uint)(_buffer.Length * 2) > MaxByteArrayLength)
			{
				newCapacity = requiredCapacity > MaxByteArrayLength ? requiredCapacity : MaxByteArrayLength;
			}

			if (newCapacity != _buffer.Length)
			{
				var newBuffer = new byte[newCapacity];
				if (_position > 0)
				{
					Array.Copy (_buffer, 0, newBuffer, 0, _position);
				}

				_buffer = newBuffer;
			}

			return;
		}
	}
}