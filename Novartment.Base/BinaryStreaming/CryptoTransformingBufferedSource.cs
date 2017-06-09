using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// применяющий криптографическое преобразование к данным другого источника.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({Buffer.Length}) exhausted={IsExhausted}")]
	public class CryptoTransformingBufferedSource :
		IBufferedSource
	{
		private readonly IBufferedSource _source;
		private readonly ICryptoTransform _cryptoTransform;
		private readonly int _inputMaxBlocks;
		private readonly byte[] _buffer;
		private int _offset = 0;
		private int _count = 0;
		private bool _sourceEnded = false;
		private bool _isExhausted = false;
		private byte[] _cache = null;
		private int _cacheStartOffset;
		private int _cacheEndOffset;

		/// <summary>
		/// Инициализирует новый экземпляр CryptoTransformingBufferedSource получающий данные из указанного источника
		/// и применяющий к ним указанное криптографическое преобразование.
		/// </summary>
		/// <param name="source">Источник данных, к которому будет применяться криптографическое преобразование.</param>
		/// <param name="cryptoTransform">Криптографическое преобразование, которое будет применяться к данным источника.</param>
		/// <param name="buffer">
		/// Байтовый буфер, в котором будут содержаться преобразованные данные.
		/// Должен быть достаточен по размеру,
		/// чтобы вмещать выходной блок криптографического преобразования (cryptoTransform.OutputBlockSize).
		/// </param>
		public CryptoTransformingBufferedSource (IBufferedSource source, ICryptoTransform cryptoTransform, byte[] buffer)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (cryptoTransform == null)
			{
				throw new ArgumentNullException (nameof (cryptoTransform));
			}

			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (cryptoTransform));
			}

			if (buffer.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			if (buffer.Length < cryptoTransform.OutputBlockSize)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer), FormattableString.Invariant (
					$"buffer.Length ({buffer.Length}) less than cryptoTransform.OutputBlockSize ({cryptoTransform.OutputBlockSize})."));
			}

			Contract.EndContractBlock ();

			_source = source;
			_cryptoTransform = cryptoTransform;
			_inputMaxBlocks = _source.Buffer.Length / _cryptoTransform.InputBlockSize;
			_buffer = buffer;
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

		/// <summary>Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.</summary>
		public bool IsExhausted => _isExhausted;

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
				_offset += size;
				_count -= size;
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
			if (!_isExhausted && (_count < _buffer.Length))
			{
				Defragment ();

				int sizeTransformed;
				do
				{
					sizeTransformed = await FillBufferChunkAsync (cancellationToken).ConfigureAwait (false);
				}
				while (!_isExhausted && (sizeTransformed < 1)); // повторяем пока трансформация не вернёт хотя бы один байт результата
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

			if ((size <= _count) || _isExhausted)
			{
				if (size > _count)
				{
					throw new NotEnoughDataException (size - _count);
				}

				return Task.CompletedTask;
			}

			Defragment ();

			return EnsureBufferAsyncStateMachine ();

			async Task EnsureBufferAsyncStateMachine ()
			{
				var shortage = size - _count;

				while ((shortage > 0) && !_isExhausted)
				{
					var sizeTransformed = await FillBufferChunkAsync (cancellationToken).ConfigureAwait (false);
					shortage -= sizeTransformed;
				}

				if (shortage > 0)
				{
					throw new NotEnoughDataException (shortage);
				}
			}
		}

		private Task<int> FillBufferChunkAsync (CancellationToken cancellationToken)
		{
			int sizeTransformed = LoadFromCache ();
			if (sizeTransformed > 0)
			{
				_count += sizeTransformed;
				return Task.FromResult (sizeTransformed);
			}

			var sourceAvailableSize = _source.Count;
			var sourceSizeNeeded = GetInputSizeToFillOutputSize (_buffer.Length - _offset - _count);
			var sourceSizeNeededBlockAjusted = _cryptoTransform.CanTransformMultipleBlocks ?
				sourceSizeNeeded :
				_cryptoTransform.InputBlockSize;
			if (_source.IsExhausted || (sourceSizeNeededBlockAjusted <= sourceAvailableSize))
			{
				sizeTransformed = LoadFromTransformedSource ();
				_count += sizeTransformed;
				return Task.FromResult (sizeTransformed);
			}

			var task = _source.FillBufferAsync (cancellationToken);

			return FillBufferChunkAsyncFinalizer ();

			async Task<int> FillBufferChunkAsyncFinalizer ()
			{
				await task.ConfigureAwait (false);
				if ((_source.Count < _cryptoTransform.InputBlockSize) && !_source.IsExhausted)
				{
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Source (buffer size={_source.Buffer.Length}) can't provide enough data to transform single block (size={_cryptoTransform.InputBlockSize})."));
				}

				sizeTransformed = LoadFromTransformedSource ();
				_count += sizeTransformed;
				return sizeTransformed;
			}
		}

		// Получает размер данных источника, необходимых чтобы заполнить указанное количество байтов результата трансформации.
		// availableSize - Количество байтов, доступных для результата трансформации.
		private int GetInputSizeToFillOutputSize (int availableSize)
		{
			if (_inputMaxBlocks < 1)
			{ // входной буфер меньше одного блока транформации, запрашиваем весь буфер
				return _source.Buffer.Length;
			}

			var outputAvailableBlocks = availableSize / _cryptoTransform.OutputBlockSize;

			var neededBlocks = Math.Min (outputAvailableBlocks, _inputMaxBlocks);
			if (neededBlocks < 1)
			{
				// минимум один блок
				neededBlocks = 1;
			}

			return neededBlocks * _cryptoTransform.InputBlockSize;
		}

		/// <summary>
		/// Обеспечивает чтобы данные в буфере начинались с позиции ноль.
		/// </summary>
		private void Defragment ()
		{
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
		/// Запрашивает данные из кэша.
		/// </summary>
		/// <returns>Количество байтов кэша, помещенных в выходной буфер.</returns>
		private int LoadFromCache ()
		{
			int size = 0;
			if (_cache != null)
			{
				var outputAvailableSize = _buffer.Length - _offset - _count;
				var cacheAvailableSize = _cacheEndOffset - _cacheStartOffset;
				size = Math.Min (outputAvailableSize, cacheAvailableSize);
				Array.Copy (_cache, _cacheStartOffset, _buffer, _offset + _count, size);
				_cacheStartOffset += size;
				if (_cacheStartOffset >= _cacheEndOffset)
				{
					_cache = null;
					if (_sourceEnded)
					{
						_isExhausted = true;
					}
				}
			}

			return size;
		}

		/// <summary>
		/// Запрашивает трансформированные данные источника.
		/// </summary>
		/// <returns>Количество байтов, помещенных в выходной буфер.</returns>
		private int LoadFromTransformedSource ()
		{
			var sourceAvailableSize = _source.Count;
			var outputAvailableSize = _buffer.Length - _offset - _count;
			int sizeTransformed = 0;
			if (sourceAvailableSize >= _cryptoTransform.InputBlockSize)
			{// в источнике есть как минимум один входной блок, продолжаем преобразование
				var outputAvailableBlocks = outputAvailableSize / _cryptoTransform.OutputBlockSize;
				if (outputAvailableBlocks > 0)
				{ // остаток буфера достаточен для выходного блока
					int sourceBlocksNeeded = 1;
					if (_cryptoTransform.CanTransformMultipleBlocks)
					{
						var sourceBlocksAvailable = sourceAvailableSize / _cryptoTransform.InputBlockSize;
						sourceBlocksNeeded = Math.Min (sourceBlocksAvailable, outputAvailableBlocks);
					}

					var sourceSizeNeeded = sourceBlocksNeeded * _cryptoTransform.InputBlockSize;
					sizeTransformed = _cryptoTransform.TransformBlock (
						_source.Buffer,
						_source.Offset,
						sourceSizeNeeded,
						_buffer,
						_offset + _count);
					_source.SkipBuffer (sourceSizeNeeded);
				}
				else
				{ // остаток буфера мал для выходного блока, трансформируем один блок в кэш
					var sourceSizeNeeded = _cryptoTransform.InputBlockSize;
					var cache = new byte[_cryptoTransform.OutputBlockSize];
					sizeTransformed = _cryptoTransform.TransformBlock (
						_source.Buffer,
						_source.Offset,
						sourceSizeNeeded,
						cache,
						0);
					_source.SkipBuffer (sourceSizeNeeded);
					if (sizeTransformed > outputAvailableSize)
					{ // поскольку весь буфер не влезает, сохраняем его остаток в кэше
						_cache = cache;
						_cacheStartOffset = outputAvailableSize;
						_cacheEndOffset = sizeTransformed;
						sizeTransformed = outputAvailableSize;
					}

					Array.Copy (cache, 0, _buffer, _offset + _count, sizeTransformed);
				}
			}
			else
			{// в источнике меньше чем один входной блок, завершаем преобразование
				_sourceEnded = true;
				var finalBlock = _cryptoTransform.TransformFinalBlock (_source.Buffer, _source.Offset, sourceAvailableSize);

				if (sourceAvailableSize > 0)
				{
					_source.SkipBuffer (sourceAvailableSize);
				}

				if (finalBlock.Length > outputAvailableSize)
				{ // поскольку весь буфер не влезает, сохраняем его остаток в кэше
					_cache = finalBlock;
					_cacheStartOffset = outputAvailableSize;
					_cacheEndOffset = finalBlock.Length;
					sizeTransformed = outputAvailableSize;
				}
				else
				{
					sizeTransformed = finalBlock.Length;
					_isExhausted = true;
				}

				Array.Copy (finalBlock, 0, _buffer, _offset + _count, sizeTransformed);
			}

			return sizeTransformed;
		}
	}
}
