using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Базовый класс - источник данных, представленный байтовым буфером,
	/// предоставляющий данные другого источника данных,
	/// разделяя их на части по результатам вызова метода.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({Buffer.Length}) exhausted={IsExhausted}")]
	public abstract class EvaluatorPartitionedBufferedSourceBase :
		IPartitionedBufferedSource
	{
		private readonly IBufferedSource _source;
		private int _partValidatedLength = 0;

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
		public int Count => _partValidatedLength;

		/// <summary>Получает признак исчерпания одной части источника.
		/// Возвращает True если источник больше не поставляет данных части.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.</summary>
		public bool IsExhausted =>
			this.IsEndOfPartFound ||
			(_source.IsExhausted && (_partValidatedLength >= _source.Count)); // проверен весь остаток источника

		/// <summary>
		/// В наследованном классе возвращает признак того, что в буфере содержится конец части.
		/// </summary>
		protected abstract bool IsEndOfPartFound { get; }

		/// <summary>
		/// В наследованном классе возвращает размер эпилога части,
		/// то есть порции, которая будет пропущена при переходе на следующую часть.
		/// </summary>
		protected abstract int PartEpilogueSize { get; }

		/// <summary>
		/// В наследованном классе проверяет данные на принадлежность к одной части.
		/// Также обновляет свойства IsEndOfPartFound и PartEpilogueSize.
		/// </summary>
		/// <param name="validatedPartLength">
		/// Размер уже проверенных данных,
		/// которые указаны как принадлежащие одной части в предыдущих вызовах.
		/// </param>
		/// <returns>Размер данных в буфере, которые принадлежат одной части.</returns>
		protected abstract int ValidatePartData (int validatedPartLength);

		/// <summary>
		/// Инициализирует новый экземпляр EvaluatorPartitionedBufferedSourceBase получающий данные из указанного IBufferedSource
		/// разделяя его по результатам вызова указанной функции.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		protected EvaluatorPartitionedBufferedSourceBase (IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			_source = source;
		}

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
				_partValidatedLength -= size;
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
		public async Task FillBufferAsync (CancellationToken cancellationToken)
		{
			if (!this.IsEndOfPartFound)
			{
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				_partValidatedLength = ValidatePartData (_partValidatedLength);
			}
		}

		/// <summary>
		/// Асинхронно запрашивает у источника указанное количество данных в буфере.
		/// В результате запроса в буфере может оказаться данных больше, чем запрошено.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Требуемый размер данных в буфере.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		public Task EnsureBufferAsync (int size, CancellationToken cancellationToken)
		{
			if ((size < 0) || (size > this.Buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}
			Contract.EndContractBlock ();

			if ((size <= _partValidatedLength) || this.IsEndOfPartFound || _source.IsExhausted)
			{
				if (size > _partValidatedLength)
				{
					throw new NotEnoughDataException (size - _partValidatedLength);
				}
				return Task.CompletedTask;
			}
			return EnsureBufferAsyncStateMachine (size, cancellationToken);
		}

		private async Task EnsureBufferAsyncStateMachine (int size, CancellationToken cancellationToken)
		{
			while ((size > _partValidatedLength) && !_source.IsExhausted)
			{
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				_partValidatedLength = ValidatePartData (_partValidatedLength);
			}
			if (size > _partValidatedLength)
			{
				throw new NotEnoughDataException (size - _partValidatedLength);
			}
		}

		/// <summary>
		/// Пытается асинхронно пропустить все данные источника, принадлежащие текущей части,
		/// чтобы стали доступны данные следующей части.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является
		/// True если разделитель найден и пропущен,
		/// либо False если источник исчерпался и разделитель не найден.
		/// </returns>
		public async Task<bool> TrySkipPartAsync (CancellationToken cancellationToken)
		{
			if (_source.IsExhausted && (_source.Count < 1))
			{
				// источник пуст
				return false;
			}

			// необходимо найти конец части если еще не найден
			while (!this.IsEndOfPartFound)
			{
				// пропускаем проверенные данные
				if (_partValidatedLength > 0)
				{
					SkipBuffer (_partValidatedLength);
				}
				await FillBufferAsync (cancellationToken).ConfigureAwait (false);
				if ((_partValidatedLength <= 0) && !this.IsEndOfPartFound)
				{
					// в полном буфере не найдено ни подходящих данных, ни полного разделителя/эпилога
					// означает что разделитель не вместился в буфер
					throw new InvalidOperationException ("Buffer insufficient for detecting end of part.");
				}
			}

			// Пропускает разделитель (и всё до него) когда он найден.
			var sizeToSkip = _partValidatedLength + this.PartEpilogueSize;
			if (sizeToSkip > 0)
			{
				_source.SkipBuffer (sizeToSkip);
				_partValidatedLength = 0;
			}

			_partValidatedLength = ValidatePartData (_partValidatedLength);

			return true;
		}
	}
}
