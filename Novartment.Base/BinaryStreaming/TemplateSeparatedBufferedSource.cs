using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий данные другого источника данных,
	/// разделяя их на части по указанному образцу-разделителю.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public class TemplateSeparatedBufferedSource :
		IPartitionedBufferedSource
	{
		private readonly IBufferedSource _source;
		private readonly byte[] _template;
		private readonly bool _throwIfNoSeparatorFound;
		private int _foundTemplateOffset;
		private int _foundTemplateLength = 0;

		/// <summary>
		/// Инициализирует новый экземпляр TemplateSeparatedBufferedSource предоставляющий данные указанного источника данных,
		/// разделяя их на части по указанному образцу-разделителю.
		/// </summary>
		/// <param name="source">Источник данных, данные которого разделены на части указанным образцом-разделителем.</param>
		/// <param name="separator">Образец-разделитель, разделяющий источник на отдельные части.</param>
		/// <param name="throwIfNoSeparatorFound">
		/// Признак, приводящий к бросанию исключения при чтении/пропуске в случае когда образец-разделитель не найден до исчерпания source.
		/// </param>
		public TemplateSeparatedBufferedSource (IBufferedSource source, byte[] separator, bool throwIfNoSeparatorFound)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (separator == null)
			{
				throw new ArgumentNullException (nameof (separator));
			}

			if ((separator.Length < 1) || (separator.Length > source.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (separator));
			}

			Contract.EndContractBlock ();

			_source = source;
			_template = separator;
			_throwIfNoSeparatorFound = throwIfNoSeparatorFound;
			SearchBuffer (true);
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
		public int Count => _foundTemplateOffset - _source.Offset;

		/// <summary>Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.</summary>
		public bool IsExhausted =>
				(!_throwIfNoSeparatorFound && _source.IsExhausted) || (_foundTemplateLength >= _template.Length);

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
			if (_foundTemplateLength >= _template.Length)
			{
				// further reading limited by found separator
				return;
			}

			bool isSourceExhausted;
			var prevOffset = _source.Offset;
			try
			{
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// признак изменения позиции данных в буфере.
				// данные, доступные ранее, могут передвинуться, но не могут измениться (могут лишь добавиться новые)
				var resetSearch = prevOffset != _source.Offset;
				isSourceExhausted = SearchBuffer (resetSearch);
			}

			if (isSourceExhausted && _throwIfNoSeparatorFound)
			{
				throw new NotEnoughDataException (
					"Source exhausted, but separator not found.",
					_template.Length - _foundTemplateLength);
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
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if ((size <= (_foundTemplateOffset - _source.Offset)) || _source.IsExhausted || (_foundTemplateLength >= _template.Length))
			{
				if (size > (_foundTemplateOffset - _source.Offset))
				{
					throw new NotEnoughDataException (size - (_foundTemplateOffset - _source.Offset));
				}

				return Task.CompletedTask;
			}

			return EnsureBufferAsyncStateMachine();

			async Task EnsureBufferAsyncStateMachine()
			{
				while ((size > (_foundTemplateOffset - _source.Offset)) && !_source.IsExhausted)
				{
					var prevOffset = _source.Offset;
					await _source.FillBufferAsync(cancellationToken).ConfigureAwait(false);
					var resetSearch = prevOffset != _source.Offset; // изменилась позиция данных в буфере. при этом сами данные, доступные ранее, не могут измениться, могут лишь добавиться новые
					SearchBuffer(resetSearch);
				}

				if (size > (_foundTemplateOffset - _source.Offset))
				{
					throw new NotEnoughDataException(size - (_foundTemplateOffset - _source.Offset));
				}
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
			int sizeToSkip;

			// find separator if not already found
			while (_foundTemplateLength < _template.Length)
			{
				sizeToSkip = _foundTemplateOffset - _source.Offset;
				if (sizeToSkip > 0)
				{
					SkipBuffer (sizeToSkip); // skip all data to found separator
				}

				var prevOffset = _source.Offset;
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				var resetSearch = prevOffset != _source.Offset; // изменилась позиция данных в буфере. при этом сами данные, доступные ранее, не могут измениться, могут лишь добавиться новые
				var isSourceExhausted = SearchBuffer (resetSearch);
				if (isSourceExhausted)
				{
					// разделитель не найден, источник ичерпался
					if (_source.Count > 0)
					{
						_source.SkipBuffer (_source.Count);
					}

					return false;
				}
			}

			// jump over found separator
			sizeToSkip = _foundTemplateOffset - _source.Offset + _foundTemplateLength;
			_source.SkipBuffer (sizeToSkip);
			SearchBuffer (true);

			return true;
		}

		/// <summary>
		/// Ищет шаблон в текущем буфере.
		/// </summary>
		/// <param name="resetFromStart">Признак сброса поиска.
		/// Если True, то поиск начнется с начала данных буфера,
		/// иначе продолжится с последней позиции где искали в предыдущий раз.</param>
		/// <returns>Если True, то шаблон уже никогда не будет найден (источник исчерпался) и запускать поиск повторно не нужно.</returns>
		private bool SearchBuffer (bool resetFromStart)
		{
			if (resetFromStart)
			{
				_foundTemplateOffset = _source.Offset;
				_foundTemplateLength = 0;
			}

			var buf = _source.BufferMemory.Span;
			while (((_foundTemplateOffset + _foundTemplateLength) < (_source.Offset + _source.Count)) && (_foundTemplateLength < _template.Length))
			{
				if (buf[_foundTemplateOffset + _foundTemplateLength] == _template[_foundTemplateLength])
				{
					_foundTemplateLength++;
				}
				else
				{
					_foundTemplateOffset++;
					_foundTemplateLength = 0;
				}
			}

			// no more data from source, separator not found
			if (_source.IsExhausted && (_foundTemplateLength < _template.Length))
			{ // stops searching
				_foundTemplateOffset = _source.Offset + _source.Count;
				_foundTemplateLength = 0;
				return true;
			}

			return false;
		}
	}
}
