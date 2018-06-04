using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий данные последовательно из нескольких источников.
	/// </summary>
	public class AggregatingBufferedSource :
		IFastSkipBufferedSource
	{
		private readonly byte[] _buffer;
		private readonly IJobProvider<IBufferedSource, int> _sourceProvider;
		private int _offset = 0;
		private int _count = 0;
		private bool _isProviderCompleted = false;
		private JobCompletionSource<IBufferedSource, int> _currentSourceJob;

		/// <summary>
		/// Инициализирует новый экземпляр AggregatingBufferedSource использующий в качестве буфера предоставленный массив байтов и
		/// предоставляющий данные из источников указанного перечислителя.
		/// </summary>
		/// <param name="buffer">Массив байтов, который будет буфером источника.</param>
		/// <param name="sources">Перечислитель, поставляющий источники данных.</param>
		public AggregatingBufferedSource(byte[] buffer, IEnumerable<IBufferedSource> sources)
			: this(buffer, new EnumerableSourceProvider(sources))
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр AggregatingBufferedSource использующий в качестве буфера предоставленный массив байтов и
		/// предоставляющий данные из источников, поставляемых указанным поставщиком.
		/// </summary>
		/// <param name="buffer">Массив байтов, который будет буфером источника.</param>
		/// <param name="sourceProvider">
		/// Поставщик источников.
		/// Источник-маркер будет означать окончание поставки.</param>
		public AggregatingBufferedSource(byte[] buffer, IJobProvider<IBufferedSource, int> sourceProvider)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (sourceProvider == null)
			{
				throw new ArgumentNullException(nameof(sourceProvider));
			}

			Contract.EndContractBlock();

			_buffer = buffer;
			_sourceProvider = sourceProvider;
			_currentSourceJob = new JobCompletionSource<IBufferedSource, int>(ArrayBufferedSource.Empty);
		}

		/// <summary>
		/// Получает буфер, в котором содержится некоторая часть данных источника.
		/// Текущая начальная позиция и количество доступных данных содержатся в свойствах Offset и Count,
		/// при этом сам буфер остаётся неизменным всё время жизни источника.
		/// </summary>
		public byte[] Buffer => _buffer;

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
		/// Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.
		/// </summary>
		public bool IsExhausted => _isProviderCompleted && _currentSourceJob.Item.IsExhausted;

		/// <summary>
		/// Асинхронно заполняет буфер данными источника, дополняя уже доступные там данные.
		/// В результате буфер может быть заполнен не полностью если источник поставляет данные блоками, либо пуст если источник исчерпался.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.
		/// Если после завершения в Count будет ноль,
		/// то источник исчерпан и доступных данных в буфере больше не будет.</returns>
		public Task FillBufferAsync (CancellationToken cancellationToken)
		{
			Defragment ();

			if (_count >= _buffer.Length)
			{
				return Task.CompletedTask;
			}

			var task = EnsureSomethingInSourceAsync (cancellationToken);

			return FillBufferAsyncFinalizer ();

			async Task FillBufferAsyncFinalizer ()
			{
				var isSomethingInSource = await task.ConfigureAwait (false);
				if (isSomethingInSource)
				{
					FillBufferFromSource ();
				}
			}
		}

		/// <summary>
		/// Асинхронно запрашивает у источника указанное количество данных в буфере.
		/// В результате запроса в буфере может оказаться данных больше, чем запрошено.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Требуемый размер данных в буфере.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public Task EnsureBufferAsync (int size, CancellationToken cancellationToken)
		{
			if ((size < 0) || (size > this.Buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			var shortage = size - _count;

			return (shortage > 0) ? EnsureBufferAsyncStateMachine () : Task.CompletedTask;

			async Task EnsureBufferAsyncStateMachine ()
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
		/// Пропускает указанное количество данных из начала доступных данных буфера.
		/// При выполнении может измениться свойство Offset.
		/// </summary>
		/// <param name="size">Размер данных для пропуска в начале доступных данных буфера.
		/// Должен быть меньше чем размер доступных в буфере данных.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Происходит если size меньше нуля или больше размера доступных в буфере данных.
		/// </exception>
		public void SkipBuffer (int size)
		{
			if ((size < 0) || (size > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if (size > 0)
			{
				_offset += size;
				_count -= size;
			}
		}

		/// <summary>
		/// Пытается асинхронно пропустить указанное количество данных источника, включая доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Количество байтов данных для пропуска, включая доступные в буфере данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, результатом которой является количество пропущеных байтов данных, включая доступные в буфере данные.
		/// Может быть меньше, чем было указано, если источник исчерпался.
		/// После завершения задачи, независимо от её результата, источник будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		public Task<long> TryFastSkipAsync (long size, CancellationToken cancellationToken)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			// достаточно доступных данных буфера
			if (size <= (long)_count)
			{
				SkipBuffer ((int)size);
				return Task.FromResult (size);
			}

			return TrySkipAsyncStateMachine ();

			async Task<long> TrySkipAsyncStateMachine ()
			{
				var available = _count;

				long skipped = available;

				// пропускаем весь буфер
				size -= (long)available;
				SkipBuffer (available);

				bool isJobSetted;
				do
				{
					var currentSourceSkipped = await _currentSourceJob.Item.TrySkipAsync (size, cancellationToken).ConfigureAwait (false);
					size -= currentSourceSkipped;
					skipped += currentSourceSkipped;
					CheckIfSourceExhausted ();
					if ((size <= 0) || _isProviderCompleted)
					{
						break;
					}

					var newJob = await _sourceProvider.TakeJobAsync (cancellationToken).ConfigureAwait (false);
					isJobSetted = SetNewJob (newJob);
				}
				while (isJobSetted);

				return skipped;
			}
		}

		private async Task<bool> EnsureSomethingInSourceAsync (CancellationToken cancellationToken)
		{
			while (_currentSourceJob.Item.Count < 1)
			{
				await _currentSourceJob.Item.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				if (_currentSourceJob.Item.Count < 1)
				{
					CheckIfSourceExhausted ();
					if (_isProviderCompleted)
					{
						return false;
					}

					var newJob = await _sourceProvider.TakeJobAsync (cancellationToken).ConfigureAwait (false);
					var isJobSetted = SetNewJob (newJob);
					if (!isJobSetted)
					{
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
			var size = Math.Min (_buffer.Length - _count, _currentSourceJob.Item.Count);
			if (size > 0)
			{
				Array.Copy (_currentSourceJob.Item.Buffer, _currentSourceJob.Item.Offset, _buffer, _offset + _count, size);
				_currentSourceJob.Item.SkipBuffer (size);
				_count += size;
				if (_currentSourceJob.Item.IsExhausted && (_currentSourceJob.Item.Count < 1))
				{
					_currentSourceJob.TrySetResult (0);
				}
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
					Array.Copy (_buffer, _offset, _buffer, 0, _count);
				}

				_offset = 0;
			}
		}

		/// <summary>
		/// Устанавливает новое задание-источник.
		/// </summary>
		/// <param name="newJob">Новое задание-источник.</param>
		/// <returns>Признак успешной установки нового задания-источника.</returns>
		private bool SetNewJob (JobCompletionSource<IBufferedSource, int> newJob)
		{
			if (newJob == null)
			{
				throw new InvalidOperationException ("Contract violation: IJobProvider.TakeJobAsync() returned null.");
			}

			if (newJob.IsMarker)
			{
				_isProviderCompleted = true;
				newJob.TrySetResult (0);
				return false;
			}

			_currentSourceJob = newJob;
			CheckIfSourceExhausted ();
			return true;
		}

		/// <summary>
		/// Проверяет, не закончился ли текущий источник и устанавливает соответственно признак выполнения задания.
		/// </summary>
		private void CheckIfSourceExhausted ()
		{
			if (_currentSourceJob.Item.IsExhausted && (_currentSourceJob.Item.Count < 1))
			{
				_currentSourceJob.TrySetResult (0);
			}
		}

		internal class EnumerableSourceProvider : IJobProvider<IBufferedSource, int>
		{
			private readonly IEnumerator<IBufferedSource> _enumerator;
			private bool _enumerationEnded = false;

			internal EnumerableSourceProvider(IEnumerable<IBufferedSource> sources)
			{
				if (sources == null)
				{
					throw new ArgumentNullException(nameof(sources));
				}

				Contract.EndContractBlock();

				_enumerator = sources.GetEnumerator();
			}

			public Task<JobCompletionSource<IBufferedSource, int>> TakeJobAsync(CancellationToken cancellationToken)
			{
				if (!_enumerationEnded)
				{
					_enumerationEnded = !_enumerator.MoveNext();
					if (_enumerationEnded)
					{
						_enumerator.Dispose();
					}
				}

				var job = _enumerationEnded ?
					JobCompletionSourceMarker.Create<IBufferedSource, int> () :
					new JobCompletionSource<IBufferedSource, int>(_enumerator.Current);
				return Task.FromResult(job);
			}
		}
	}
}
