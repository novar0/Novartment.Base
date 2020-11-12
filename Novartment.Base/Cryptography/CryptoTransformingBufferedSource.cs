using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a byte buffer,
	/// that applies cryptographic transformation to data from another source.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public class CryptoTransformingBufferedSource :
		IBufferedSource
	{
		private readonly IBufferedSource _source;
		private readonly ISpanCryptoTransform _cryptoTransform;
		private readonly int _inputMaxBlocks;
		private readonly Memory<byte> _buffer;
		private int _offset = 0;
		private int _count = 0;
		private bool _sourceEnded = false;
		private bool _isExhausted = false;
		private ReadOnlyMemory<byte> _cache = null;
		private int _cacheStartOffset;
		private int _cacheEndOffset;

		/// <summary>
		/// Initializes a new instance of the CryptoTransformingBufferedSource class,
		/// which will fetch data from the specified data source and apply the specified cryptographic transformation to them.
		/// </summary>
		/// <param name="source">The data source to which cryptographic transformation will be applied.</param>
		/// <param name="cryptoTransform">The cryptographic transformation, which will be applied to data.</param>
		/// <param name="buffer">
		/// A byte buffer that will contain transformed data.
		/// Must be large enough to accommodate the cryptographic transformation output unit  (cryptoTransform.OutputBlockSize).
		/// </param>
		public CryptoTransformingBufferedSource (IBufferedSource source, ISpanCryptoTransform cryptoTransform, Memory<byte> buffer)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (cryptoTransform == null)
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
			_inputMaxBlocks = _source.BufferMemory.Length / _cryptoTransform.InputBlockSize;
			_buffer = buffer;
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
		public bool IsExhausted => _isExhausted;

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

			Contract.EndContractBlock ();

			if (size > 0)
			{
				_offset += size;
				_count -= size;
			}
		}

		/// <summary>
		/// Asynchronously requests the source to load more data in the buffer.
		/// As a result, the buffer may not be completely filled if the source supplies data in blocks,
		/// or it may be empty if the source is exhausted.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous fill operation.
		/// If Count property equals zero after completion,
		/// this means that the source is exhausted and there will be no more data in the buffer.</returns>
		public async ValueTask LoadAsync (CancellationToken cancellationToken = default)
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

			Contract.EndContractBlock ();

			if ((size <= _count) || _isExhausted)
			{
				if (size > _count)
				{
					throw new NotEnoughDataException (size - _count);
				}

				return default;
			}

			Defragment ();

			return EnsureBufferAsyncStateMachine ();

			async ValueTask EnsureBufferAsyncStateMachine ()
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

		private ValueTask<int> FillBufferChunkAsync (CancellationToken cancellationToken)
		{
			int sizeTransformed = LoadFromCache ();
			if (sizeTransformed > 0)
			{
				_count += sizeTransformed;
				return new ValueTask<int> (sizeTransformed);
			}

			var sourceAvailableSize = _source.Count;
			var sourceSizeNeeded = GetInputSizeToFillOutputSize (_buffer.Length - _offset - _count);
			var sourceSizeNeededBlockAjusted = _cryptoTransform.CanTransformMultipleBlocks ?
				sourceSizeNeeded :
				_cryptoTransform.InputBlockSize;
			if (_source.IsExhausted || (sourceAvailableSize >= sourceSizeNeededBlockAjusted))
			{
				sizeTransformed = LoadFromTransformedSource ();
				_count += sizeTransformed;
				return new ValueTask<int> (sizeTransformed);
			}

			return FillBufferChunkAsyncFinalizer ();

			async ValueTask<int> FillBufferChunkAsyncFinalizer ()
			{
				// запрашивает данные до тех пор, пока не наберём на входной блок или источник не закончится
				while (true)
				{
					var oldCount = _source.Count;
					await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
					var newCount = _source.Count;
					if ((newCount >= _cryptoTransform.InputBlockSize) || _source.IsExhausted)
					{
						sizeTransformed = LoadFromTransformedSource ();
						_count += sizeTransformed;
						return sizeTransformed;
					}
					if (newCount <= oldCount)
					{
						throw new InvalidOperationException (FormattableString.Invariant (
							$"Source (buffer size={_source.BufferMemory.Length}) is not exhausted, but can't provide enough data to transform single block (size={_cryptoTransform.InputBlockSize})."));
					}
				}
			}
		}

		// Получает размер данных источника, необходимых чтобы заполнить указанное количество байтов результата трансформации.
		// availableSize - Количество байтов, доступных для результата трансформации.
		private int GetInputSizeToFillOutputSize (int availableSize)
		{
			if (_inputMaxBlocks < 1)
			{ // входной буфер меньше одного блока транформации, запрашиваем весь буфер
				return _source.BufferMemory.Length;
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
					_buffer.Slice (_offset, _count).CopyTo (_buffer);
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
			if (_cache.Length > 0)
			{
				var outputAvailableSize = _buffer.Length - _offset - _count;
				var cacheAvailableSize = _cacheEndOffset - _cacheStartOffset;
				size = Math.Min (outputAvailableSize, cacheAvailableSize);
				_cache.Slice (_cacheStartOffset, size).CopyTo (_buffer[(_offset + _count)..]);
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
			int sizeTransformed;
			if (sourceAvailableSize >= _cryptoTransform.InputBlockSize)
			{
				// в источнике есть как минимум один входной блок, продолжаем преобразование
				var outputAvailableBlocks = outputAvailableSize / _cryptoTransform.OutputBlockSize;
				if (outputAvailableBlocks > 0)
				{
					// остаток буфера достаточен для выходного блока
					int sourceBlocksNeeded = 1;
					if (_cryptoTransform.CanTransformMultipleBlocks)
					{
						var sourceBlocksAvailable = sourceAvailableSize / _cryptoTransform.InputBlockSize;
						sourceBlocksNeeded = Math.Min (sourceBlocksAvailable, outputAvailableBlocks);
					}

					var sourceSizeNeeded = sourceBlocksNeeded * _cryptoTransform.InputBlockSize;
					sizeTransformed = _cryptoTransform.TransformBlock (
						_source.BufferMemory.Span.Slice (_source.Offset, sourceSizeNeeded),
						_buffer.Span[(_offset + _count)..]);
					_source.Skip (sourceSizeNeeded);
				}
				else
				{
					// остаток буфера мал для выходного блока, трансформируем один блок в кэш
					var sourceSizeNeeded = _cryptoTransform.InputBlockSize;
					var cache = new byte[_cryptoTransform.OutputBlockSize];
					sizeTransformed = _cryptoTransform.TransformBlock (
						_source.BufferMemory.Span.Slice (_source.Offset, sourceSizeNeeded),
						cache.AsSpan ());
					_source.Skip (sourceSizeNeeded);
					if (sizeTransformed > outputAvailableSize)
					{ // поскольку весь буфер не влезает, сохраняем его остаток в кэше
						_cache = cache;
						_cacheStartOffset = outputAvailableSize;
						_cacheEndOffset = sizeTransformed;
						sizeTransformed = outputAvailableSize;
					}

					cache.AsSpan (0, sizeTransformed).CopyTo (_buffer.Span[(_offset + _count)..]);
				}
			}
			else
			{
				// в источнике меньше чем один входной блок, завершаем преобразование
				_sourceEnded = true;
				var finalBlock = _cryptoTransform.TransformFinalBlock (_source.BufferMemory.Span.Slice (_source.Offset, _source.Count));

				if (sourceAvailableSize > 0)
				{
					_source.Skip (sourceAvailableSize);
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

				finalBlock.Slice (0, sizeTransformed).CopyTo (_buffer[(_offset + _count)..]);
			}

			return sizeTransformed;
		}
	}
}
