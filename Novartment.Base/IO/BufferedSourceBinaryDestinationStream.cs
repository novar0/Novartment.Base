using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A stream that reads data from the data source represented by a byte buffer
	/// and transfers all written data to the binary data destination.
	/// </summary>
	public class BufferedSourceBinaryDestinationStream : Stream
	{
		private readonly IBufferedSource _source;
		private readonly IBinaryDestination _destination;

		/// <summary>
		/// Initializes a new instance of the BufferedSourceBinaryDestinationStream class
		/// that reads data from the specified data source represented by a byte buffer
		/// and transfers all written data to the specified binary data destination.
		/// </summary>
		/// <param name="source">A data source represented by a byte buffer from which the stream will read data.</param>
		/// <param name="destination">The binary data destination to which all data written to the stream will be transfered.</param>
		public BufferedSourceBinaryDestinationStream (IBufferedSource source, IBinaryDestination destination)
		{
			_source = source;
			_destination = destination;
		}

		/// <summary>Gets a value indicating that this stream supports reading.</summary>
		public override bool CanRead => true;

		/// <summary>Gets a value indicating that this stream supports writing.</summary>
		public override bool CanWrite => true;

		/// <summary>Gets a value indicating that this stream does not supports seeking.</summary>
		public override bool CanSeek => false;

		/// <summary>Not supported.</summary>
		/// <exception cref="System.NotSupportedException">
		/// Always throws because the stream not supports seeking.
		/// </exception>
		public override long Length => throw new NotSupportedException ();

		/// <summary>Not supported.</summary>
		/// <exception cref="System.NotSupportedException">
		/// Always throws because the stream not supports seeking.
		/// </exception>
		public override long Position
		{
			get => throw new NotSupportedException ();
			set => throw new NotSupportedException ();
		}

		/// <summary>Not supported.</summary>
		/// <exception cref="System.NotSupportedException">
		/// Always throws because the stream not supports seeking.
		/// </exception>
		public override long Seek (long offset, SeekOrigin origin) => throw new NotSupportedException ();

		/// <summary>Not supported.</summary>
		/// <exception cref="System.NotSupportedException">
		/// Always throws because the stream not supports seeking.
		/// </exception>
		public override void SetLength (long value) => throw new NotSupportedException ();

		/// <summary>
		/// Writes a sequence of bytes to the stream.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the stream.</param>
		/// <param name="count">The number of bytes to be written to the stream.</param>
		public override void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || ((offset + count) > buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (_destination is BinaryStreamingStreamExtensions.StreamBinaryDestination streamBinaryDestination)
			{
				streamBinaryDestination.BaseStream.Write (buffer, offset, count);
			}
			else
			{
				var vTask =_destination.WriteAsync (buffer.AsMemory (offset, count), default);
				if (!vTask.IsCompletedSuccessfully)
				{
					vTask.AsTask ().GetAwaiter ().GetResult ();
				}
			}
		}

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the stream.</param>
		/// <param name="count">The number of bytes to be written to the stream.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || ((offset + count) > buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			return (_destination is BinaryStreamingStreamExtensions.StreamBinaryDestination streamBinaryDestination) ?
				streamBinaryDestination.BaseStream.WriteAsync (buffer, offset, count, cancellationToken) :
				_destination.WriteAsync (buffer.AsMemory (offset, count), cancellationToken).AsTask ();
		}

#if !NETSTANDARD2_0

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The region of memory to write data from.</param>
		/// <param name="cancellationToken"> The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		public override ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			return (_destination is BinaryStreamingStreamExtensions.StreamBinaryDestination streamBinaryDestination) ?
				streamBinaryDestination.BaseStream.WriteAsync (buffer, cancellationToken) :
				_destination.WriteAsync (buffer, cancellationToken);
		}
#endif

		/// <summary>
		/// Clears all buffers for stream and causes
		/// any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush ()
		{
			if (_destination is BinaryStreamingStreamExtensions.StreamBinaryDestination streamBinaryDestination)
			{
				streamBinaryDestination.BaseStream.Flush ();
			}
		}

		/// <summary>
		/// Asynchronously clears all buffers for this stream, causes any buffered data to
		/// be written to the underlying device, and monitors cancellation requests.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		public override Task FlushAsync (CancellationToken cancellationToken = default)
		{
			return (_destination is BinaryStreamingStreamExtensions.StreamBinaryDestination streamBinaryDestination) ?
				streamBinaryDestination.BaseStream.FlushAsync () :
				Task.CompletedTask;
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>
		///  The total number of bytes read into the buffer. This can be less than the number
		///  of bytes requested if that many bytes are not currently available, or zero (0)
		///  if the end of the stream has been reached.
		/// </returns>
		public override int Read (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || (count > (buffer.Length - offset)))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			int resultSize = 0;
			var available = _source.Count;
			if ((count <= available) || _source.IsExhausted)
			{
				// данных в источнике достаточно, запрос новых данных не требуется
				var toCopy = Math.Min (available, count);
				if (toCopy > 0)
				{
					_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
					_source.Skip (toCopy);
				}

				return toCopy;
			}

			while (count > 0)
			{
				var vTask = _source.LoadAsync (default);
				if (!vTask.IsCompletedSuccessfully)
				{
					vTask.AsTask ().GetAwaiter ().GetResult ();
				}
				if (_source.Count <= 0)
				{
					break;
				}

				var toCopy = Math.Min (_source.Count, count);
				_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
				offset += toCopy;
				count -= toCopy;
				resultSize += toCopy;
				_source.Skip (toCopy);
			}

			return resultSize;
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte,
		/// or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
		public override int ReadByte ()
		{
			if (_source.Count < 1)
			{
				var vTask = _source.LoadAsync (default);
				if (!vTask.IsCompletedSuccessfully)
				{
					vTask.AsTask ().GetAwaiter ().GetResult ();
				}
				if (_source.Count < 1)
				{
					return -1;
				}
			}

			var result = (int)_source.BufferMemory.Span[_source.Offset];
			_source.Skip (1);
			return result;
		}

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The value of the TResult parameter contains the total number of bytes read into the buffer.
		/// The result value can be less than the number of bytes requested if the number of bytes currently
		/// available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || (count > (buffer.Length - offset)))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			var available = _source.Count;
			if ((count <= available) || _source.IsExhausted)
			{
				// данных в источнике достаточно, асинхронное обращение не требуется
				var toCopy = Math.Min (available, count);
				if (toCopy > 0)
				{
					_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
					_source.Skip (toCopy);
				}

				return Task.FromResult (toCopy);
			}

			return ReadAsyncStateMachine ();

			// асинхронный запрос к источнику пока не наберём необходимое количество данных
			async Task<int> ReadAsyncStateMachine ()
			{
				int resultSize = 0;
				while (count > 0)
				{
					if ((count > _source.Count) && !_source.IsExhausted)
					{
						await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
					}

					if (_source.Count <= 0)
					{
						break;
					}

					var toCopy = Math.Min (_source.Count, count);
					_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer.AsMemory (offset));
					offset += toCopy;
					count -= toCopy;
					resultSize += toCopy;
					_source.Skip (toCopy);
				}

				return resultSize;
			}
		}

#if !NETSTANDARD2_0
		/// <summary>
		/// Asynchronously reads a sequence of bytes from the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The region of memory to write the data into.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The result value contains the total number of bytes read into the buffer.
		/// It can be less than the number of bytes allocated in the buffer if that many
		/// bytes are not currently available, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		public override ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var available = _source.Count;
			if ((buffer.Length <= available) || _source.IsExhausted)
			{
				// данных в источнике достаточно, асинхронное обращение не требуется
				var toCopy = Math.Min (available, buffer.Length);
				if (toCopy > 0)
				{
					_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer);
					_source.Skip (toCopy);
				}

				return new ValueTask<int> (toCopy);
			}

			return ReadAsyncStateMachine ();

			// асинхронный запрос к источнику пока не наберём необходимое количество данных
			async ValueTask<int> ReadAsyncStateMachine ()
			{
				var offset = 0;
				var count = buffer.Length;
				int resultSize = 0;
				while (count > 0)
				{
					if ((count > _source.Count) && !_source.IsExhausted)
					{
						await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
					}

					if (_source.Count <= 0)
					{
						break;
					}

					var toCopy = Math.Min (_source.Count, count);
					_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer.Slice (offset));
					offset += toCopy;
					count -= toCopy;
					resultSize += toCopy;
					_source.Skip (toCopy);
				}

				return resultSize;
			}
		}
#endif

		/// <summary>
		/// Asynchronously reads the bytes from the stream and writes them to another stream,
		/// using a specified buffer size and cancellation token.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the stream will be copied.</param>
		/// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		public override Task CopyToAsync (Stream destination, int bufferSize, CancellationToken cancellationToken = default)
		{
			return BufferedSourceExtensions.WriteToAsync (_source, destination.AsBinaryDestination (), cancellationToken);
		}

		/// <summary>
		/// Releases the resources used by stream.
		/// </summary>
		/// <param name="disposing">
		/// True to release both managed and unmanaged resources;
		/// false to release only unmanaged resources.
		/// </param>
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing)
			{
				_destination.SetComplete ();
			}
		}
	}
}
