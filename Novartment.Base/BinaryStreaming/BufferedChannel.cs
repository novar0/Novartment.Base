using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Канал двоичных данных, представленный байтовым буфером,
	/// позволяющий вести запись данных одновременно и независимо от их чтения.
	/// Двоичные данные интерпретируются как непрерывный поток, независимо от количества и размера записываемых порций.
	/// </summary>
	/// <remarks>
	/// Чтение и запись данных откладываются в зависимости от заполнения буфера.
	/// Новые (записанные) данные становятся доступны в буфере только после запроса
	/// в методах EnsureBuffer() и FillBuffer().
	/// Аналогичен библиотечным классам System.Threading.Channels.Channel и System.Threading.Tasks.Dataflow.BufferBlock, но не для фиксированных элементов,
	/// а для произвольных последовательностей байтов.
	/// </remarks>
	public class BufferedChannel :
		IFastSkipBufferedSource,
		IBinaryDestination
	{
		private readonly object _integrityLocker = new object ();
		private readonly Memory<byte> _buffer;
		private int _offset = 0;
		private int _count = 0;

		// Задача, завершение которой означает, что с момента последнего запроса данных при отсутствии отложенных данных записи,
		// произошла запись новых данных.
		// Завершённая заменяется на новую при любом запросе данных если отсутствуют отложенные данные записи.
		private TaskCompletionSource<int> _pendingDataArrival = new TaskCompletionSource<int> ();

		// Задача, завершение которой означает, что с момента последней записи для которой не хватило места в буфере,
		// произошло освобождение места в буфере и копирование туда всех ожидающих данных.
		// Заменяется на новую при записи, для которой не хватило места в буфере.
		private TaskCompletionSource<int> _pendingDataConsumption = new TaskCompletionSource<int> ();

		// следующие поля требуют синхронизации (через _integrityLocker) ЛЮБОГО доступа (в том числе при чтении)
		// потому что могут изменяться конкурентно
		private ReadOnlyMemory<byte> _pendingData = default;
		private int _destinationReservedCount = 0;
		private int _destinationTailOffset;
		private bool _isCompleted = false;
		private long _sizeToSkipOnWrite = 0L;

		/// <summary>
		/// Инициализирует новый экземпляр BufferedChannel,
		/// использующий указанный буфер для записи данных.
		/// </summary>
		/// <param name="buffer">Байтовый буфер, в котором будут содержаться записанные в канал данные.</param>
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
		/// Получает буфер, в котором содержится некоторая часть данных источника.
		/// Текущая начальная позиция и количество доступных данных содержатся в свойствах Offset и Count,
		/// при этом сам буфер остаётся неизменным всё время жизни источника.
		/// </summary>
		public ReadOnlyMemory<byte> BufferMemory => _buffer;

		/// <summary>
		/// Получает начальную позицию данных, доступных в Buffer.
		/// Количество данных, доступных в Buffer, содержится в Count.
		/// </summary>
		public int Offset => _offset;

		/// <summary>
		/// Получает количество данных, доступных в Buffer.
		/// Начальная позиция доступных данных содержится в Offset.
		/// </summary>
		public int Count => _count;

		/// <summary>
		/// Получает признак исчерпания канала.
		/// Возвращает True если канал больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.
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

		/// <summary>Отбрасывает (пропускает) указанное количество данных из начала буфера канала.</summary>
		/// <param name="size">Размер данных для пропуска в начале буфера.
		/// Должен быть меньше чем размер данных в буфере.</param>
		public void SkipBuffer (int size)
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
		/// Пытается асинхронно пропустить указанное количество данных канала, включая доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Количество байтов данных для пропуска, включая доступные в буфере данные.</param>
		/// <param name="cancellationToken">
		/// Токен для отслеживания запросов отмены не используется.
		/// Для отмены ожидания новых данных вызывайте метод SetComplete().
		/// </param>
		/// <returns>
		/// Задача, результатом которой является количество пропущенных байтов данных, включая доступные в буфере данные.
		/// Может быть меньше, чем было указано, если канал исчерпан.
		/// После завершения задачи, независимо от её результата, канал будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		public ValueTask<long> TryFastSkipAsync (long size, CancellationToken cancellationToken = default)
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
		/// Асинхронно заполняет буфер данными канала, дополняя уже доступные там данные.
		/// В результате буфер может быть заполнен не полностью если канал поставляет данные блоками, либо пуст если канал исчерпан.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="cancellationToken">
		/// Токен для отслеживания запросов отмены не используется.
		/// Для отмены ожидания новых данных вызывайте метод SetComplete().
		/// </param>
		/// <returns>Задача, представляющая операцию.
		/// Если после завершения в Count будет ноль,
		/// то канал исчерпан и доступных данных в буфере больше не будет.</returns>
		/// <remarks>
		/// Размер получаемых в буфере порций данных не зависит от размера порций, записанных в канал.
		/// Будут объеденены и доступны как одна порция все записанные в канал данные,
		/// для которых хватило места в буфере.
		/// </remarks>
		public async ValueTask FillBufferAsync (CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested ();

			await _pendingDataArrival.Task.ConfigureAwait (false);
			lock (_integrityLocker)
			{
				AcceptReservedAndPendingData (0L);
			}
		}

		/// <summary>
		/// Асинхронно запрашивает у канала указанное количество данных в буфере.
		/// В результате запроса в буфере может оказаться данных больше, чем запрошено.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Требуемый размер данных в буфере.</param>
		/// <param name="cancellationToken">
		/// Токен для отслеживания запросов отмены не используется.
		/// Для отмены ожидания новых данных вызывайте метод SetComplete().
		/// </param>
		/// <returns>Задача, представляющая операцию.</returns>
		public ValueTask EnsureBufferAsync (int size, CancellationToken cancellationToken = default)
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
		/// Асинхронно записывает последовательность байтов в канал.
		/// Запись можно вызывать одновременно и независимо от операций чтения.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="cancellationToken">
		/// Токен для отслеживания запросов отмены не используется.
		/// Для отмены ожидания вызывайте метод TrySkip() указав размер не менее чем count.
		/// </param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		/// <remarks>
		/// Записанные данные будут доступны для чтения не сразу,
		/// а только по запросу из методов EnsureBuffer(), FillBuffer() и TrySkip().
		/// Информация об отдельных порциях записанных данных не сохраняется,
		/// канал предоставляет все накопленные данные для чтения произвольными частями в зависимости от наличия места в буфере.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// Возникает если предоставление данных в канал окончено.
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
		/// Сигнализирует, что запись данных в канал окончена.
		/// После этого в методах FillBuffer() и EnsureBuffer() не будет происходить ожидание новых данных,
		/// а метод Write() будет вызывать исключение.
		/// </summary>
		/// <remarks>
		/// Свойство IsExhausted может быть установлено не сразу,
		/// а только после освобождения места в буфере для помещения туда всех данных, ожидающих записи в канал.
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
					_pendingData = _pendingData.Slice ((int)size);
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
			source.Slice (0, chunkSize).CopyTo (_buffer.Slice (_destinationTailOffset + _destinationReservedCount));
			_destinationReservedCount += chunkSize;

			if (chunkSize >= source.Length)
			{
				// освобождаем ссылки на сохранённый буфер
				_pendingData = default;
				return true;
			}

			// то, что не влезло, откладываем
			_pendingData = source.Slice (chunkSize);
			return false;
		}
	}
}
