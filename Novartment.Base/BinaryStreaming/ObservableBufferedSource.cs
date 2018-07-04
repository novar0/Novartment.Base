using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Ретранслятор, дублирующий асинхронный источник, представленный байтовым буфером,
	/// отправляющий уведомления о потреблённых из источника данных.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public class ObservableBufferedSource :
		IFastSkipBufferedSource
	{
		private readonly IBufferedSource _source;
		private readonly IProgress<long> _progress;

		/// <summary>
		/// Инициализирует новый экземпляр ObservableBufferedSource,
		/// который будет ретранслировать указанный источник.
		/// </summary>
		/// <param name="source">Источник данных, который будет ретранслироваться.</param>
		/// <param name="progress">Объект, который будет получать уведомления о потреблении данных источника.</param>
		public ObservableBufferedSource (IBufferedSource source, IProgress<long> progress)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (progress == null)
			{
				throw new ArgumentNullException (nameof (progress));
			}

			Contract.EndContractBlock ();

			_source = source;
			_progress = progress;
		}

		/// <summary>
		/// Получает буфер, в котором содержится некоторая часть данных источника.
		/// Текущая начальная позиция и количество доступных данных содержатся в свойствах Offset и Count,
		/// при этом сам буфер остаётся неизменным всё время жизни источника.
		/// </summary>
		public ReadOnlyMemory<byte> BufferMemory => _source.BufferMemory;

		/// <summary>
		/// Получает начальную позицию данных, доступных в Buffer.
		/// Количество данных, доступных в Buffer, содержится в Count.
		/// </summary>
		public int Offset => _source.Offset;

		/// <summary>
		/// Получает количество данных, доступных в Buffer.
		/// Начальная позиция доступных данных содержится в Offset.
		/// </summary>
		public int Count => _source.Count;

		/// <summary>
		/// Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.
		/// </summary>
		public bool IsExhausted => _source.IsExhausted;

		/// <summary>
		/// Отбрасывает (пропускает) указанное количество данных из начала буфера.
		/// При выполнении может измениться свойство Offset.
		/// </summary>
		/// <param name="size">Размер данных для пропуска в начале буфера.
		/// Должен быть меньше чем размер данных в буфере.</param>
		public void SkipBuffer (int size)
		{
			if ((size < 0) || (size > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if (size > 0)
			{
				_source.SkipBuffer (size);
				_progress.Report (size);
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
		public Task FillBufferAsync (CancellationToken cancellationToken = default)
		{
			return _source.FillBufferAsync (cancellationToken);
		}

		/// <summary>
		/// Асинхронно запрашивает у источника указанное количество данных в буфере.
		/// В результате запроса в буфере может оказаться данных больше, чем запрошено.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Требуемый размер данных в буфере.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public Task EnsureBufferAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			return size == 0 ? Task.FromResult (0) : _source.EnsureBufferAsync (size, cancellationToken);
		}

		/// <summary>
		/// Осуществляет попытку асинхронно пропустить указанное количество данных источника, включая уже доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Размер данных для пропуска.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, результатом которой является количество пропущенных байтов данных, включая уже доступные в буфере данные.
		/// Может быть меньше, чем было указано, если источник исчерпался.
		/// Источник будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		public Task<long> TryFastSkipAsync (long size, CancellationToken cancellationToken = default)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			Task<long> task;
			if (_source is IFastSkipBufferedSource fastSkipSource)
			{
				task = fastSkipSource.TryFastSkipAsync (size, cancellationToken);
				return TryFastSkipAsyncFinalizer ();
			}

			// источник не поддерживает быстрый пропуск,
			// поэтому будем пропускать путём последовательного считывания и пропуска буфера
			var available = _source.Count;
			if (size <= (long)available)
			{
				// достаточно доступных данных буфера
				_source.SkipBuffer ((int)size);
				_progress.Report (size);
				return Task.FromResult (size);
			}

			if (_source.IsExhausted)
			{
				// источник исчерпан
				_source.SkipBuffer (available);
				_progress.Report (available);
				return Task.FromResult ((long)available);
			}

			return TrySkipAsyncStateMachine ();

			async Task<long> TrySkipAsyncStateMachine ()
			{
				long skipped = 0L;
				do
				{
					// пропускаем всё что в буфере
					available = _source.Count;
					_source.SkipBuffer (available);
					_progress.Report (available);
					size -= (long)available;
					skipped += (long)available;

					// заполняем буфер
					await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				}
				while (!_source.IsExhausted && (size > (long)_source.Count));

				// пропускаем частично буфер
				var reminder = (int)Math.Min (size, (long)_source.Count);
				_source.SkipBuffer (reminder);
				_progress.Report (reminder);
				skipped += reminder;

				return skipped;
			}

			async Task<long> TryFastSkipAsyncFinalizer ()
			{
				size = await task.ConfigureAwait (false);
				_progress.Report (size);
				return size;
			}
		}
	}
}
