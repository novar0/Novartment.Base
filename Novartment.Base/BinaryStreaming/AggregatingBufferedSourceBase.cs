using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Базовый класса для создания источников данных (представленных байтовым буфером),
	/// предоставляющих данные последовательно из нескольких источников.
	/// При наследовании необходимо реализовать метод MoveToNextSource(),
	/// в котором будет установлено свойство CurrentSource.
	/// </summary>
	public abstract class AggregatingBufferedSourceBase :
		IFastSkipBufferedSource
	{
		private readonly Memory<byte> _buffer;
		private int _offset = 0;
		private int _count = 0;
		private bool _isProviderCompleted = false;
		private IBufferedSource _currentSource;

		/// <summary>
		/// Инициализирует новый экземпляр AggregatingBufferedSourceBase использующий в качестве буфера предоставленный массив байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов, который будет буфером источника.</param>
		public AggregatingBufferedSourceBase (Memory<byte> buffer)
		{
			_buffer = buffer;
			_currentSource = MemoryBufferedSource.Empty;
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
		/// Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.
		/// </summary>
		public bool IsExhausted => _isProviderCompleted && _currentSource.IsExhausted;

		/// <summary>
		/// Для наследованных классов получает или устанавливает текущий источник данных.
		/// </summary>
		protected IBufferedSource CurrentSource
		{
			get
			{
				return _currentSource;
			}
			set
			{
				_currentSource = value;
				OnSourceConsumed ();
			}
		}

		/// <summary>
		/// Асинхронно заполняет буфер данными источника, дополняя уже доступные там данные.
		/// В результате буфер может быть заполнен не полностью если источник поставляет данные блоками, либо пуст если источник исчерпался.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.
		/// Если после завершения в Count будет ноль,
		/// то источник исчерпан и доступных данных в буфере больше не будет.</returns>
		public ValueTask FillBufferAsync (CancellationToken cancellationToken = default)
		{
			Defragment ();

			if (_count >= _buffer.Length)
			{
				return default;
			}

			return FillBufferAsyncFinalizer (EnsureSomethingInSourceAsync (cancellationToken));

			async ValueTask FillBufferAsyncFinalizer (ValueTask<bool> task)
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
		public ValueTask EnsureBufferAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

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
		/// Задача, результатом которой является количество пропущенных байтов данных, включая доступные в буфере данные.
		/// Может быть меньше, чем было указано, если источник исчерпался.
		/// После завершения задачи, независимо от её результата, источник будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		public ValueTask<long> TryFastSkipAsync (long size, CancellationToken cancellationToken = default)
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
				return new ValueTask<long> (size);
			}

			return TrySkipAsyncStateMachine ();

			async ValueTask<long> TrySkipAsyncStateMachine ()
			{
				var available = _count;

				long skipped = available;

				// пропускаем весь буфер
				size -= (long)available;
				SkipBuffer (available);

				do
				{
					// TODO: вызывать TryFastSkipAsync() если источником поддерживается IFastSkipBufferedSource
					var currentSourceSkipped = await _currentSource.TrySkipAsync (size, cancellationToken).ConfigureAwait (false);
					size -= currentSourceSkipped;
					skipped += currentSourceSkipped;
					OnSourceConsumed ();
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

		/// <summary>
		/// В наследованных классах производит переход на новое задание-источник.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Признак успешной установки нового задания-источника.</returns>
		protected abstract ValueTask<bool> MoveToNextSource (CancellationToken cancellationToken);

		/// <summary>
		/// В наследованных классах уведомляет о потреблении некоторой части текущего задания-источника.
		/// </summary>
		protected virtual void OnSourceConsumed ()
		{
		}

		private async ValueTask<bool> EnsureSomethingInSourceAsync (CancellationToken cancellationToken)
		{
			while (_currentSource.Count < 1)
			{
				await _currentSource.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				if (_currentSource.Count < 1)
				{
					OnSourceConsumed ();
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
				_currentSource.BufferMemory.Slice (_currentSource.Offset, size).CopyTo (_buffer.Slice (_offset + _count));
				_currentSource.SkipBuffer (size);
				_count += size;
				OnSourceConsumed ();
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
