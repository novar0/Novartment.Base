using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a byte buffer,
	/// that provides a specified number of bytes from another data source.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public sealed class SizeLimitedBufferedSource :
		IFastSkipBufferedSource
	{
		private readonly IBufferedSource _source;
		private int _countInBuffer;
		private long _countRemainder;

		/// <summary>
		/// Initializes a new instance of the SizeLimitedBufferedSource class,
		/// receiving data from the specified data source and limiting it to the specified size.
		/// </summary>
		/// <param name="source">A data source to relay.</param>
		/// <param name="limit">A number of bytes that limits the data source.</param>
		public SizeLimitedBufferedSource (IBufferedSource source, long limit)
		{
			if (limit < 0L)
			{
				throw new ArgumentOutOfRangeException(nameof(limit));
			}

			_source = source ?? throw new ArgumentNullException (nameof (source));

			UpdateLimits (limit);
		}

		/// <summary>
		/// Gets the unused limit remainder.
		/// </summary>
		public long UnusedSize => (long)_countInBuffer + _countRemainder;

		/// <summary>
		/// Gets the buffer that contains some of the source data.
		/// The current offset and the amount of available data are in the Offset and Count properties.
		/// The buffer remains unchanged throughout the lifetime of the source.
		/// </summary>
		public ReadOnlyMemory<byte> BufferMemory => _source.BufferMemory;

		/// <summary>
		/// Gets the offset of available source data in the BufferMemory.
		/// The amount of available source data is in the Count property.
		/// </summary>
		public int Offset => _source.Offset;

		/// <summary>
		/// Gets the amount of source data available in the BufferMemory.
		/// The offset of available source data is in the Offset property.
		/// </summary>
		public int Count => _countInBuffer;

		/// <summary>
		/// Gets a value indicating whether the source is exhausted.
		/// Returns True if the source no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		public bool IsExhausted => _countRemainder <= 0L;

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
				_source.Skip (size);
				_countInBuffer -= size;
			}
		}

		/// <summary>
		/// Asynchronously tries to skip specified amount of source data, including data already available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip, including data already available in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous skip operation.
		/// The result of a task will indicate the number of actually skipped bytes of data, including data already available in the buffer.
		/// It may be less than specified if the source is exhausted.
		/// Upon completion of a task, regardless of the result, the source will provide data coming right after skipped.
		/// </returns>
		public ValueTask<long> SkipWihoutBufferingAsync (long size, CancellationToken cancellationToken = default)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size == 0L)
			{
				return new ValueTask<long> (0L);
			}

			var limit = (long)_countInBuffer + _countRemainder;
			var task = _source.TrySkipAsync ((size < limit) ? size : limit, cancellationToken);
			return TrySkipAsyncFinalizer ();

			async ValueTask<long> TrySkipAsyncFinalizer ()
			{
				var skipped = await task.ConfigureAwait (false);
				UpdateLimits (limit - skipped);
				return skipped;
			}
		}

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
		public ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			if (_countRemainder <= 0)
			{
				return default;
			}

			var task = _source.LoadAsync (cancellationToken);

			return FillBufferAsyncFinalizer ();

			async ValueTask FillBufferAsyncFinalizer ()
			{
				await task.ConfigureAwait (false);
				if ((_source.Count < 1) && (_countInBuffer < 1))
				{
					throw new NotEnoughDataException (
						"Source exhausted before reaching specified limit.",
						_countRemainder);
				}

				UpdateLimits ((long)_countInBuffer + _countRemainder);
			}
		}

		/// <summary>
		/// Asynchronously requests the source to load the specified amount of data in the buffer.
		/// As a result, there may be more data in the buffer than requested.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Amount of data required in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if ((size <= _countInBuffer) || _source.IsExhausted || (_countRemainder <= 0))
			{
				if (size > _countInBuffer)
				{
					throw new NotEnoughDataException (size - _countInBuffer);
				}

				return default;
			}

			return EnsureBufferAsyncStateMachine ();

			async ValueTask EnsureBufferAsyncStateMachine ()
			{
				while ((size > _countInBuffer) && !_source.IsExhausted)
				{
					await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
					UpdateLimits ((long)_countInBuffer + _countRemainder);
				}

				if (size > _countInBuffer)
				{
					throw new NotEnoughDataException (size - _countInBuffer);
				}
			}
		}

		// Обновляет границы данных.
		private void UpdateLimits (long limit)
		{
			_countRemainder = limit - (long)_source.Count;

			if (_countRemainder > 0)
			{
				_countInBuffer = _source.Count;
			}
			else
			{
				_countRemainder = 0L;
				_countInBuffer = (int)limit;
			}
		}
	}
}
