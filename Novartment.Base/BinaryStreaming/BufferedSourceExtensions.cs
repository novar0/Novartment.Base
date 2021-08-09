using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Extension methods for IBufferedSource.
	/// </summary>
	public static class BufferedSourceExtensions
	{
		/// <summary>
		/// Checks that the specified source is exhausted and contains no data.
		/// </summary>
		/// <param name="source">The data source to check.</param>
		/// <returns>True if the specified source is exhausted and contains no data.</returns>
		public static bool IsEmpty (this IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			return (source.Count < 1) && source.IsExhausted;
		}

		/// <summary>
		/// Asynchronously tries to skip specified amount of source data, including data already available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="source">The data source to skip data.</param>
		/// <param name="size">Size of data to skip, including data already available in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous skip operation.
		/// The result of a task will indicate the number of actually skipped bytes of data, including data already available in the buffer.
		/// It may be less than specified if the source is exhausted.
		/// Upon completion of a task, regardless of the result, the source will provide data coming right after skipped.
		/// </returns>
		/// <remarks>
		/// Work is delegated to the method SkipWihoutBufferingAsync() if source implements IFastSkipBufferedSource.
		/// </remarks>
		public static ValueTask<long> TrySkipAsync (this IBufferedSource source, long size, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (source is IFastSkipBufferedSource fastSkipSource)
			{
				return fastSkipSource.SkipWihoutBufferingAsync(size, cancellationToken);
			}

			// источник не поддерживает быстрый пропуск,
			// поэтому будем пропускать путём последовательного считывания и пропуска буфера
			var available = source.Count;
			if (size <= (long)available)
			{
				// достаточно доступных данных буфера
				source.Skip ((int)size);
				return new ValueTask<long> (size);
			}

			if (source.IsExhausted)
			{
				// источник исчерпан
				source.Skip (available);
				return new ValueTask<long> ((long)available);
			}

			return TrySkipAsyncStateMachine();

			async ValueTask<long> TrySkipAsyncStateMachine()
			{
				long skipped = 0L;
				do
				{
					// пропускаем всё что в буфере
					available = source.Count;
					source.Skip(available);
					size -= (long)available;
					skipped += (long)available;

					// заполняем буфер
					await source.LoadAsync(cancellationToken).ConfigureAwait(false);
				}
				while (!source.IsExhausted && (size > (long)source.Count));

				// пропускаем частично буфер
				var reminder = Math.Min(size, (long)source.Count);
				source.Skip((int)reminder);
				skipped += reminder;

				return skipped;
			}
		}

		/// <summary>
		/// Asynchronously skips all available source data, including data already available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="source">The data source to skip data.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous skip operation.
		/// The result of a task will indicate the number of actually skipped bytes of data, including data already available in the buffer.
		/// </returns>
		public static ValueTask<long> SkipToEndAsync (this IBufferedSource source, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source is IFastSkipBufferedSource fastSkipSource)
			{
				return fastSkipSource.SkipWihoutBufferingAsync(long.MaxValue, cancellationToken);
			}

			var available = source.Count;
			source.Skip (available);
			if (source.IsExhausted)
			{
				// источник исчерпан
				return new ValueTask<long> ((long)available);
			}

			// источник не поддерживает быстрый пропуск,
			// поэтому будем пропускать путём последовательного считывания и пропуска буфера
			return SkipToEndStateMachine ();

			async ValueTask<long> SkipToEndStateMachine ()
			{
				var skipped = (long)available;
				do
				{
					// заполняем буфер
					await source.LoadAsync (cancellationToken).ConfigureAwait (false);

					// пропускаем всё что в буфере
					available = source.Count;
					source.Skip (available);
					skipped += (long)available;
				}
				while (!source.IsExhausted);

				return skipped;
			}
		}

		/// <summary>
		/// Asynchronously reads data from the specified source into the specified region of memory.
		/// </summary>
		/// <param name="source">The data source to read from.</param>
		/// <param name="buffer">The region of memory as destination for reading data.
		/// After reading is completed, it will contain data readed from the source.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The result of a task will indicate the number of readed bytes of data.
		/// </returns>
		public static ValueTask<int> ReadAsync (this IBufferedSource source, Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (buffer.Length < 1)
			{
				return new ValueTask<int> (0);
			}

			int totalSize = 0;
			if (source.Count > 0)
			{
				// копируем то, что уже есть в буфере
				totalSize = Math.Min (buffer.Length, source.Count);
				source.BufferMemory.Slice (source.Offset, totalSize).CopyTo (buffer);
				source.Skip (totalSize);
				buffer = buffer[totalSize..];
			}

			if ((buffer.Length < 1) || source.IsExhausted)
			{
				return new ValueTask<int> (totalSize);
			}

			return ReadAsyncStateMachine ();

			async ValueTask<int> ReadAsyncStateMachine ()
			{
				do
				{
					await source.LoadAsync (cancellationToken).ConfigureAwait (false);

					if (source.Count < 1)
					{
						break; // end of stream
					}
					else
					{
						var size = Math.Min (source.Count, buffer.Length);
						source.BufferMemory.Slice (source.Offset, size).CopyTo (buffer);
						source.Skip (size);
						totalSize += size;

						if (buffer.Length <= size)
						{
							break;
						}

						buffer = buffer[size..];
					}
				}
				while (!source.IsExhausted);

				return totalSize;
			}
		}

		/// <summary>
		/// Asynchronously reads data from the specified source into the specified region of memory until specified marker appears.
		/// </summary>
		/// <param name="source">The data source to copy from.
		/// If the marker is found, then after completion, the source will point to it.
		/// If the brand is not found, then after completion the source will be exhausted.</param>
		/// <param name="marker">The byte marker indicating the end of the data to read.</param>
		/// <param name="buffer">The region of memory as destination for reading data.
		/// After reading is completed, it will contain data readed from the source.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The result of a task will indicate the number of readed bytes of data before the marker is met.
		/// If the marker is not found then all the source data will be readed.
		/// </returns>
		public static ValueTask<int> ReadToMarkerAsync (this IBufferedSource source, byte marker, Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			// проверяем то, что уже есть в буфере
			var idx = source.BufferMemory.Span.Slice (source.Offset, source.Count).IndexOf (marker);
			if (idx >= 0)
			{
				if (idx > 0)
				{
					source.BufferMemory.Slice (source.Offset, idx).CopyTo (buffer);
					source.Skip (idx);
				}

				return new ValueTask<int> (idx);
			}

			var totalOutCount = source.Count;
			if (totalOutCount > 0)
			{
				source.BufferMemory.Slice (source.Offset, totalOutCount).CopyTo (buffer);
				source.Skip (totalOutCount);
			}

			if (source.IsExhausted)
			{
				return new ValueTask<int> (totalOutCount);
			}

			buffer = buffer[totalOutCount..];

			// продолжение поиска с предварительным вызовом заполнения буфера
			return CopyToBufferUntilMarkerStateMachine ();

			async ValueTask<int> CopyToBufferUntilMarkerStateMachine ()
			{
				while (!source.IsExhausted)
				{
					cancellationToken.ThrowIfCancellationRequested ();

					await source.LoadAsync (cancellationToken).ConfigureAwait (false);

					var idx2 = source.BufferMemory.Span.Slice (source.Offset, source.Count).IndexOf (marker);
					if (idx2 >= 0)
					{
						if (idx2 > 0)
						{
							source.BufferMemory.Slice (source.Offset, idx2).CopyTo (buffer);
							source.Skip (idx2);
						}

						return totalOutCount + idx2;
					}

					var outCount = source.Count;
					if (outCount > 0)
					{
						source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
						source.Skip (outCount);
						buffer = buffer[outCount..];
						totalOutCount += outCount;
					}
				}

				// источник исчерпался или нет места в буфере, запрашивать данные больше нет смысла
				return totalOutCount;
			}
		}

		/// <summary>
		/// Asynchronously reads data from the specified source into the newly allocated region of memory.
		/// </summary>
		/// <param name="source">The data source to read from.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation,
		/// which wraps the newly allocated region of memory containing readed data.
		/// </returns>
		public static ValueTask<ReadOnlyMemory<byte>> ReadAllBytesAsync (this IBufferedSource source, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.IsExhausted)
			{
				var sizeExhausted = source.Count;
				var copy = new byte[sizeExhausted];
				if (sizeExhausted > 0)
				{
					source.BufferMemory.Slice (source.Offset, source.Count).CopyTo (copy);
					source.Skip (sizeExhausted);
				}

				return new ValueTask<ReadOnlyMemory<byte>> (copy);
			}

			return ReadAllBytesAsyncStateMachine ();

			async ValueTask<ReadOnlyMemory<byte>> ReadAllBytesAsyncStateMachine ()
			{
				var destination = new MemoryBinaryDestination ();
				while (true)
				{
					await source.LoadAsync (cancellationToken).ConfigureAwait (false);
					var available = source.Count;
					if (available <= 0)
					{
						break;
					}

					cancellationToken.ThrowIfCancellationRequested ();
					destination.Write (source.BufferMemory.Span.Slice (source.Offset, source.Count));
					source.Skip (available);
				}

				return destination.GetBuffer ();
			}
		}

		/// <summary>
		/// Asynchronously reads data from the specified source as a text in the specified encoding.
		/// </summary>
		/// <param name="source">The data source to read from.</param>
		/// <param name="encoding">The encoding applied to the data of the source.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation,
		/// which wraps the text readed from the source.
		/// </returns>
		public static async ValueTask<string> ReadAllTextAsync (this IBufferedSource source, Encoding encoding, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (encoding == null)
			{
				throw new ArgumentNullException (nameof (encoding));
			}

			// нельзя декодировать частями, потому что неизвестно сколько байт занимают отдельные символы
			var buf = await ReadAllBytesAsync (source, cancellationToken).ConfigureAwait (false);
#if NETSTANDARD2_0
			var tempBuf = new byte[buf.Length];
			buf.CopyTo (tempBuf);
			return encoding.GetString (tempBuf);
#else
			return encoding.GetString (buf.Span);
#endif
		}

		/// <summary>
		/// Asynchronously writes data from the specified source into the specified destination.
		/// </summary>
		/// <param name="source">The data source to read from.</param>
		/// <param name="destination">The binary data destination, into which data will be written.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read/write operation.
		/// The result of a task will indicate the number of bytes written to the destination.
		/// </returns>
		public static async Task<long> WriteToAsync (this IBufferedSource source, IBinaryDestination destination, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			long resultSize = 0;
			while (true)
			{
				await source.LoadAsync (cancellationToken).ConfigureAwait (false);
				var available = source.Count;
				if (available <= 0)
				{
					break;
				}

				cancellationToken.ThrowIfCancellationRequested ();
				await destination.WriteAsync (source.BufferMemory.Slice (source.Offset, available), cancellationToken).ConfigureAwait (false);
				resultSize += available;
				source.Skip (available);
			}

			return resultSize;
		}
	}
}
