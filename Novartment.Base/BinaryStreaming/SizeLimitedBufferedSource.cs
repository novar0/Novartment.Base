using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий указанное число байтов из другого источника данных.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({Buffer.Length}) exhausted={IsExhausted}")]
	public class SizeLimitedBufferedSource :
		IFastSkipBufferedSource
	{
		private readonly IBufferedSource _source;
		private int _countInBuffer;
		private long _countRemainder;

		/// <summary>
		/// Инициализирует новый экземпляр SizeLimitedBufferedSource получающий данные из указанного IBufferedSource
		/// разделяя его на порции указанного размера.
		/// </summary>
		/// <param name="source">Источник данных, представляющий из себя порции фиксированного размера.</param>
		/// <param name="limit">Размер порции данных.</param>
		public SizeLimitedBufferedSource(IBufferedSource source, long limit)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (limit < 0L)
			{
				throw new ArgumentOutOfRangeException(nameof(limit));
			}

			Contract.EndContractBlock();

			_source = source;

			UpdateLimits(limit);
		}

		/// <summary>
		/// Получает неиспользованный остаток лимита.
		/// </summary>
		public long UnusedSize => (long)_countInBuffer + _countRemainder;

		/// <summary>
		/// Получает буфер, в котором содержится некоторая часть данных источника.
		/// Текущая начальная позиция и количество доступных данных содержатся в свойствах Offset и Count,
		/// при этом сам буфер остаётся неизменным всё время жизни источника.
		/// </summary>
		public byte[] Buffer => _source.Buffer;

		/// <summary>
		/// Получает начальную позицию данных, доступных в Buffer.
		/// Количество данных, доступных в Buffer, содержится в Count.
		/// </summary>
		public int Offset => _source.Offset;

		/// <summary>
		/// Получает количество данных, доступных в Buffer.
		/// Начальная позиция доступных данных содержится в Offset.
		/// </summary>
		public int Count => _countInBuffer;

		/// <summary>Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.</summary>
		public bool IsExhausted => _countRemainder <= 0L;

		/// <summary>Отбрасывает (пропускает) указанное количество данных из начала буфера.</summary>
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
				_countInBuffer -= size;
			}
		}

		/// <summary>
		/// Осуществляет попытку асинхронно пропустить указанное количество данных источника, включая уже доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Размер данных для пропуска.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, результатом которой является количество пропущеных байтов данных, включая уже доступные в буфере данные.
		/// Может быть меньше, чем было указано, если источник исчерпался.
		/// Источник будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Происходит если size меньше нуля.</exception>
		public Task<long> TryFastSkipAsync (long size, CancellationToken cancellationToken)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if (size == 0L)
			{
				return Task.FromResult (0L);
			}

			var limit = (long)_countInBuffer + _countRemainder;
			var task = _source.TrySkipAsync ((size < limit) ? size : limit, cancellationToken);
			return TrySkipAsyncFinalizer ();

			async Task<long> TrySkipAsyncFinalizer ()
			{
				var skipped = await task.ConfigureAwait (false);
				UpdateLimits (limit - skipped);
				return skipped;
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
		public Task FillBufferAsync (CancellationToken cancellationToken)
		{
			if (_countRemainder <= 0)
			{
				return Task.CompletedTask;
			}

			var task = _source.FillBufferAsync (cancellationToken);
			return FillBufferAsyncFinalizer ();

			async Task FillBufferAsyncFinalizer ()
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

			if ((size <= _countInBuffer) || _source.IsExhausted || (_countRemainder <= 0))
			{
				if (size > _countInBuffer)
				{
					throw new NotEnoughDataException (size - _countInBuffer);
				}

				return Task.CompletedTask;
			}

			return EnsureBufferAsyncStateMachine (size, cancellationToken);
		}

		private async Task EnsureBufferAsyncStateMachine (int size, CancellationToken cancellationToken)
		{
			while ((size > _countInBuffer) && !_source.IsExhausted)
			{
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				UpdateLimits ((long)_countInBuffer + _countRemainder);
			}

			if (size > _countInBuffer)
			{
				throw new NotEnoughDataException (size - _countInBuffer);
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
