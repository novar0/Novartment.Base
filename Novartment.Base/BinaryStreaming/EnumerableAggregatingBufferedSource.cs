using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Collections;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a byte buffer,
	/// that represents data from collection of other sources.
	/// </summary>
	public sealed class EnumerableAggregatingBufferedSource
		: IFastSkipBufferedSource
	{
		private readonly Memory<byte> _buffer;
		private int _offset = 0;
		private int _count = 0;
		private readonly IAsyncEnumerator<IBufferedSource> _sourceProvider;
		private bool _isProviderCompleted = false;
		private IBufferedSource _currentSource;
		// единый токен отмены для всех запросов к поставщику источников, потому что интерефейс IAsyncEnumerable не предусматривает отдельные токены на каждую операцию
		private readonly CancellationTokenSource _sourceProviderCTS = null;

		/// <summary>
		/// Initializes a new instance of the EnumerableAggregatingBufferedSource class
		/// that uses specified buffer for the data and represents data from the specified collection of other sources.
		/// </summary>
		/// <param name="buffer">The region of memory that will be used as a buffer for source data.</param>
		/// <param name="sources">The enumerator of collection of sources.</param>
		public EnumerableAggregatingBufferedSource (Memory<byte> buffer, IEnumerable<IBufferedSource> sources)
		{
			if (sources == null)
			{
				throw new ArgumentNullException (nameof (sources));
			}

			_buffer = buffer;
			_currentSource = MemoryBufferedSource.Empty;
			_sourceProvider = sources.AsAsyncEnumerable ().GetAsyncEnumerator ();
		}

		/// <summary>
		/// Initializes a new instance of the EnumerableAggregatingBufferedSource class
		/// that uses specified buffer for the data and represents data from the specified collection of other sources.
		/// </summary>
		/// <param name="buffer">The region of memory that will be used as a buffer for source data.</param>
		/// <param name="sources">The enumerator of collection of sources.</param>
		public EnumerableAggregatingBufferedSource (Memory<byte> buffer, IAsyncEnumerable<IBufferedSource> sources)
		{
			if (sources == null)
			{
				throw new ArgumentNullException (nameof (sources));
			}

			_buffer = buffer;
			_currentSource = MemoryBufferedSource.Empty;
			_sourceProviderCTS = new CancellationTokenSource ();
			_sourceProvider = sources.GetAsyncEnumerator (_sourceProviderCTS.Token);
		}

		/// <summary>
		/// Gets the buffer that contains some of the source data.
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

		/// <summary>
		/// Gets a value indicating whether the source is exhausted.
		/// Returns True if the source no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		public bool IsExhausted => _isProviderCompleted && _currentSource.IsExhausted;

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
		public async ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			Defragment ();

			if (_count >= _buffer.Length)
			{
				return;
			}

			var isSomethingInSource = await EnsureSomethingInSourceAsync (cancellationToken).ConfigureAwait (false);
			if (isSomethingInSource)
			{
				FillBufferFromSource ();
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

			var shortage = size - _count;

			return (shortage > 0) ? EnsureBufferAsyncStateMachine () : default;

			async ValueTask EnsureBufferAsyncStateMachine ()
			{
				Defragment ();

				// запускаем чтение потока пока не наберём необходимое количество данных
				while (shortage > 0)
				{
					var isSomethingInSource = await EnsureSomethingInSourceAsync (cancellationToken).ConfigureAwait (false);
					if (!isSomethingInSource)
					{
						break;
					}

					shortage -= FillBufferFromSource ();
				}

				if (shortage > 0)
				{
					throw new NotEnoughDataException (shortage);
				}
			}
		}

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

			// достаточно доступных данных буфера
			if (size <= (long)_count)
			{
				Skip ((int)size);
				return new ValueTask<long> (size);
			}

			return TrySkipAsyncStateMachine ();

			async ValueTask<long> TrySkipAsyncStateMachine ()
			{
				var available = _count;

				long skipped = available;

				// пропускаем весь буфер
				size -= (long)available;
				Skip (available);

				do
				{
					// TODO: вызывать SkipWihoutBufferingAsync() если источником поддерживается IFastSkipBufferedSource
					var currentSourceSkipped = await _currentSource.TrySkipAsync (size, cancellationToken).ConfigureAwait (false);
					size -= currentSourceSkipped;
					skipped += currentSourceSkipped;
					if ((size <= 0) || _isProviderCompleted)
					{
						break;
					}

					_isProviderCompleted = !await MoveToNextSource (cancellationToken).ConfigureAwait (false);
				}
				while (!_isProviderCompleted);

				return skipped;
			}
		}

		// Устанавливает новое задание-источник.
		// <returns>Признак успешной установки нового задания-источника.</returns>
		private async ValueTask<bool> MoveToNextSource (CancellationToken cancellationToken)
		{
			if (_isProviderCompleted)
			{
				return false;
			}

			var disposable = ((_sourceProviderCTS != null) && cancellationToken.CanBeCanceled) ?
				cancellationToken.Register (_sourceProviderCTS.Cancel, false) :
				(IDisposable)null;
			var success = await _sourceProvider.MoveNextAsync ().ConfigureAwait (false);
			disposable?.Dispose ();
			if (!success)
			{
				_isProviderCompleted = true;
				await _sourceProvider.DisposeAsync ().ConfigureAwait (false);
				_sourceProviderCTS?.Dispose ();
				return false;
			}

			if (_sourceProvider.Current == null)
			{
				throw new InvalidOperationException ("Contract violation: null-source.");
			}

			_currentSource = _sourceProvider.Current;
			return true;
		}

		private async ValueTask<bool> EnsureSomethingInSourceAsync (CancellationToken cancellationToken)
		{
			while (_currentSource.Count < 1)
			{
				await _currentSource.LoadAsync (cancellationToken).ConfigureAwait (false);
				if (_currentSource.Count < 1)
				{
					if (_isProviderCompleted)
					{
						return false;
					}

					var isSourceSetted = await MoveToNextSource (cancellationToken).ConfigureAwait (false);
					if (!isSourceSetted)
					{
						_isProviderCompleted = true;
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Заполняет буфер данными из текущего источника.
		/// </summary>
		/// <returns>Размер доступных в текущем источнике данных.</returns>
		private int FillBufferFromSource ()
		{
			var size = Math.Min (_buffer.Length - _count, _currentSource.Count);
			if (size > 0)
			{
				_currentSource.BufferMemory.Slice (_currentSource.Offset, size).CopyTo (_buffer[(_offset + _count)..]);
				_currentSource.Skip (size);
				_count += size;
			}

			return size;
		}

		/// <summary>
		/// Переносит текущие данные буфера в начало.
		/// </summary>
		private void Defragment ()
		{
			// сдвигаем в начало данные буфера
			if (_offset > 0)
			{
				if (_count > 0)
				{
					_buffer.Slice (_offset, _count).CopyTo (_buffer);
				}

				_offset = 0;
			}
		}
	}
}
