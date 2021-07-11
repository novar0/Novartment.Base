using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A channel of binary data, represented by a byte buffer,
	/// that allows to write data simultaneously and independently of reading.
	/// Binary data is interpreted as a continuous stream, regardless of the number and size of written portions.
	/// </summary>
	/// <remarks>
	/// Reading and writing are blocked depending on the buffer fill.
	/// New (written) data becomes available in the buffer only after a request in the EnsureAvailableAsync() and LoadAsync() methods.
	/// Similar to the library classes System.Threading.Channels.Channel and System.Threading.Tasks.Dataflow.BufferBlock,
	/// but not for fixed elements, but for arbitrary sequences of bytes.
	/// </remarks>
	public sealed class BufferedChannel :
		IFastSkipBufferedSource,
		IBinaryDestination
	{
		private readonly object _integrityLocker = new ();
		private readonly Memory<byte> _buffer;
		private int _offset = 0;
		private int _count = 0;

		// Задача, завершение которой означает, что с момента последнего запроса данных при отсутствии отложенных данных записи,
		// произошла запись новых данных.
		// Завершённая заменяется на новую при любом запросе данных если отсутствуют отложенные данные записи.
		private TaskCompletionSource<int> _pendingDataArrival = new ();

		// Задача, завершение которой означает, что с момента последней записи для которой не хватило места в буфере,
		// произошло освобождение места в буфере и копирование туда всех ожидающих данных.
		// Заменяется на новую при записи, для которой не хватило места в буфере.
		private TaskCompletionSource<int> _pendingDataConsumption = new ();

		// следующие поля требуют синхронизации (через _integrityLocker) ЛЮБОГО доступа (в том числе при чтении)
		// потому что могут изменяться конкурентно
		private ReadOnlyMemory<byte> _pendingData = default;
		private int _destinationReservedCount = 0;
		private int _destinationTailOffset;
		private bool _isCompleted = false;
		private long _sizeToSkipOnWrite = 0L;

		/// <summary>
		/// Initializes a new instance of the BufferedChannel class
		/// that uses a specified buffer for the data.
		/// </summary>
		/// <param name="buffer">The region of memory that will be used as a buffer for channel data.</param>
		public BufferedChannel (Memory<byte> buffer)
		{
			if (buffer.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			Contract.EndContractBlock ();

			_buffer = buffer;
			_pendingDataConsumption.SetResult (0);
		}

		/// <summary>
		/// Gets the buffer that contains some of the channel data.
		/// The current offset and the amount of available data are in the Offset and Count properties.
		/// The buffer remains unchanged throughout the lifetime of the channel.
		/// </summary>
		public ReadOnlyMemory<byte> BufferMemory => _buffer;

		/// <summary>
		/// Gets the offset of available channel data in BufferMemory.
		/// The amount of available channel data is in the Count property.
		/// </summary>
		public int Offset => _offset;

		/// <summary>
		/// Gets the amount of channel data available in the BufferMemory.
		/// The offset of available channel data is in the Offset property.
		/// </summary>
		public int Count => _count;

		/// <summary>
		/// Gets a value indicating whether the channel is exhausted.
		/// Returns True if the channel no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		public bool IsExhausted
		{
			get
			{
				lock (_integrityLocker)
				{
					return _isCompleted &&
						_pendingDataArrival.Task.IsCompleted &&
						(_destinationReservedCount < 1) &&
						(_pendingData.Length < 1);
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
			if ((size < 0) || (size > _count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			_offset += size;
			_count -= size;
		}

		/// <summary>
		/// Asynchronously tries to skip specified amount of channel data, including data already available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip, including data already available in the buffer.</param>
		/// <param name="cancellationToken">
		/// The cancellation request token is not used.
		/// To unblock the pending skip operation, call the SetComplete() method.
		/// </param>
		/// <returns>
		/// A task that represents the asynchronous skip operation.
		/// The result of a task will indicate the number of actually skipped bytes of data, including data already available in the buffer.
		/// It may be less than specified if the channel is completed.
		/// Upon completion of a task, regardless of the result, the channel will provide data coming right after skipped.
		/// </returns>
		public ValueTask<long> SkipWihoutBufferingAsync (long size, CancellationToken cancellationToken = default)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			long skipped = 0L;

			// этап 1: пропускаем текущие данные в буфере
			var chunkSize = Math.Min (size, (long)_count);
			skipped += chunkSize;
			size -= chunkSize;
			_offset += (int)chunkSize;
			_count -= (int)chunkSize;
			if (size < 1L)
			{
				return new ValueTask<long> (skipped);
			}

			// этап 2: пропускаем зарезервированные и отложенные данные
			lock (_integrityLocker)
			{
				// тут будет _count == 0
				chunkSize = AcceptReservedAndPendingData (size);
				size -= chunkSize;
				skipped += chunkSize;
				if (_isCompleted || (size < 1L))
				{
					return new ValueTask<long> (skipped);
				}

				_sizeToSkipOnWrite = size;
			}

			// этап 3: ждём запись
			// здесь мы имеем пустой буфер, отсутствие зарезервированных и отложенных данных
			// в методе записи сразу будет пропущено указанное в _sizeToSkipOnWrite количество
			return TrySkipAsyncStateMachine ();

			async ValueTask<long> TrySkipAsyncStateMachine ()
			{
				while (true)
				{
					await _pendingDataArrival.Task.ConfigureAwait (false);
					lock (_integrityLocker)
					{
						skipped += size - _sizeToSkipOnWrite;
						size = _sizeToSkipOnWrite;
						if (_isCompleted || (size < 1L))
						{
							return skipped;
						}
					}
				}
			}
		}

		/// <summary>
		/// Asynchronously requests the source to load more data in the buffer.
		/// As a result, the buffer may not be completely filled if the source supplies data in blocks,
		/// or it may be empty if the source is exhausted.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation request token is not used.
		/// To unblock the pending fill buffer operation, call the SetComplete() method.
		/// </param>
		/// <returns>A task that represents the asynchronous fill operation.
		/// If Count property equals zero after completion,
		/// this means that the channel is exhausted and there will be no more data in the buffer.</returns>
		/// <remarks>
		/// The size of the data portions available in the buffer does not depend on the size of the portions written in the channel.
		/// All the data written in the channel for which there was enough space in the buffer will be merged and available as one portion.
		/// </remarks>
		public async ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			await _pendingDataArrival.Task.ConfigureAwait (false);
			lock (_integrityLocker)
			{
				AcceptReservedAndPendingData (0L);
			}
		}

		/// <summary>
		/// Asynchronously requests the channel to provide the specified amount of data in the buffer.
		/// As a result, there may be more data in the buffer than requested.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Amount of data required in the buffer.</param>
		/// <param name="cancellationToken">
		/// The cancellation request token is not used.
		/// To unblock the pending fill buffer operation, call the SetComplete() method.
		/// </param>
		/// <returns>A task that represents the operation.</returns>
		public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			return size <= _count ? default : EnsureBufferAsyncStateMachine ();

			async ValueTask EnsureBufferAsyncStateMachine ()
			{
				// ждём записи в канал, пока не наберём необходимое количество данных
				bool isExhausted;
				do
				{
					cancellationToken.ThrowIfCancellationRequested ();
					lock (_integrityLocker)
					{
						// ждать не надо если...
						if (_pendingDataArrival.Task.IsCompleted)
						{
							// данные уже поступили либо уже никогда не поступят
							AcceptReservedAndPendingData (0L);
							isExhausted = _isCompleted &&
								(_destinationReservedCount < 1) &&
								(_pendingData.Length < 1);
							continue;
						}
					}

					await _pendingDataArrival.Task.ConfigureAwait (false);
					lock (_integrityLocker)
					{
						AcceptReservedAndPendingData (0L);
						isExhausted = _isCompleted &&
							_pendingDataArrival.Task.IsCompleted &&
							(_destinationReservedCount < 1) &&
							(_pendingData.Length < 1);
					}
				}
				while ((size > _count) && !isExhausted);

				if (size > _count)
				{
					throw new NotEnoughDataException (size - _count);
				}
			}
		}

		/// <summary>
		/// Asynchronously writes specified region of memory to this channel.
		/// Запись можно вызывать одновременно и независимо от операций чтения.
		/// </summary>
		/// <param name="buffer">The region of memory to write to this destination.</param>
		/// <param name="cancellationToken">
		/// The cancellation request token is not used.
		/// To unblock the pending write operation, call the SkipWihoutBufferingAsync() method and specify a size of at least buffer.Length.
		/// </param>
		/// <returns>A task that represents the write operation.</returns>
		/// <remarks>
		/// The written data will not be available in the buffer immediately,
		/// but only upon request in EnsureAvailableAsync(), LoadAsync() and SkipWihoutBufferingAsync() methods.
		/// Information about individual portions of written data is not kept,
		/// the channel provides all the accumulated data for reading in arbitrary parts,
		/// depending on the availability of space in the buffer.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// The channel is marked as completed.
		/// </exception>
		public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var offset = 0;
			var count = buffer.Length;
			lock (_integrityLocker)
			{
				if (_isCompleted)
				{
					throw new InvalidOperationException ("Channel completed. New data can not be written.");
				}

				// если ожидается пропуск данных, то сразу пропускаем
				if (_sizeToSkipOnWrite > 0L)
				{
					var chunkSize = Math.Min (_sizeToSkipOnWrite, (long)count);
					offset += (int)chunkSize;
					count -= (int)chunkSize;
					_sizeToSkipOnWrite -= chunkSize;
				}

				ValueTask result;
				if (count < 1)
				{
					result = default;
				}
				else
				{
					var isDataProcessed = ReserveData (buffer.Slice (offset, count));
					if (isDataProcessed)
					{
						result = default;
					}
					else
					{
						_pendingDataConsumption = new TaskCompletionSource<int> ();
						result = new ValueTask (_pendingDataConsumption.Task);
					}
				}

				_pendingDataArrival.TrySetResult (0);
				return result;
			}
		}

		/// <summary>
		/// Mark the channel as being complete, meaning no more items will be written to it.
		/// After that, there will be no more waiting new data in LoadAsync() and EnsureAvailableAsync() methods, and Write() method will throw an exception.
		/// </summary>
		/// <remarks>
		/// The IsExhausted property may not be set immediately,
		/// but only after freeing up space in the buffer to place all the data waiting to be written to the channel.
		/// </remarks>
		public void SetComplete ()
		{
			lock (_integrityLocker)
			{
				_isCompleted = true;
				_pendingDataArrival.TrySetResult (0);
			}
		}

		// Делает доступными в буфере все зарезервированные и отложенные при записи данные.
		// Метод вызывается конкурентно, поэтому требуется внешняя синхронизация.
		private long AcceptReservedAndPendingData (long sizeToSkip)
		{
			long skipped = 0L;
			var isDataPending = AcceptReservedData ();
			if (isDataPending)
			{
				if ((sizeToSkip > 0L) && (_count > 0))
				{
					var size = Math.Min (sizeToSkip, (long)_count);
					skipped += size;
					_offset += (int)size;
					_count -= (int)size;
					sizeToSkip -= size;
				}

				if (_offset > 0)
				{
					if (_count > 0)
					{
						// После вызова AcceptReservedData() в буфере гарантировано не будет зарезервированных данных,
						// поэтому можно копировать данные без риска их порчи.
						_buffer.Slice (_offset, _count).CopyTo (_buffer);
					}

					_offset = 0;
				}

				_destinationTailOffset = _count;

				if ((sizeToSkip > 0L) && (_pendingData.Length > 0))
				{
					var size = Math.Min (sizeToSkip, (long)_pendingData.Length);
					skipped += size;
					_pendingData = _pendingData[(int)size..];
				}

				var isDataProcessed = ReserveData (_pendingData);
				if (isDataProcessed)
				{
					_pendingDataConsumption.TrySetResult (0);
				}

				AcceptReservedData ();
			}

			return skipped;
		}

		// Метод вызывается конкурентно, поэтому требуется внешняя синхронизация.
		// Возвращает true если остались неиспользованные предложенные для записи данные.
		private bool AcceptReservedData ()
		{
			// утверждаем данные, ранее зарезервированные в основном буфере
			if (_destinationReservedCount > 0)
			{
				_count += _destinationReservedCount;
				_destinationTailOffset = _offset + _count;
				_destinationReservedCount = 0;
			}

			if (_pendingData.Length < 1)
			{
				// записанные данные закончились
				if (_pendingDataArrival.Task.IsCompleted && !_isCompleted)
				{
					_pendingDataArrival = new TaskCompletionSource<int> ();
				}

				return false;
			}

			return true;
		}

		// Метод вызывается конкурентно, поэтому требуется внешняя синхронизация.
		// Возвращает true если все входные данные обработаны и их можно освобождать.
		private bool ReserveData (ReadOnlyMemory<byte> source)
		{
			// резервируем сколько влезет в конец главного буфера
			var chunkSize = Math.Min (source.Length, _buffer.Length - _destinationTailOffset - _destinationReservedCount);
			source.Slice (0, chunkSize).CopyTo (_buffer[(_destinationTailOffset + _destinationReservedCount)..]);
			_destinationReservedCount += chunkSize;

			if (chunkSize >= source.Length)
			{
				// освобождаем ссылки на сохранённый буфер
				_pendingData = default;
				return true;
			}

			// то, что не влезло, откладываем
			_pendingData = source[chunkSize..];
			return false;
		}
	}
}
