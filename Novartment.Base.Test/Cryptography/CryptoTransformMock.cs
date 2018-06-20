using System;

namespace Novartment.Base.Test
{
	/// <summary>
	/// Крипто-преобразование, которое напрямую копирует входные данные в выходные с инвертированием каждого байта,
	/// используя указанные размеры входного и выходного блока,
	/// а также задерживающее (кэширующее) указанное количество входных блоков.
	/// В случае если выходной блок меньше входного, то данные каждого входного блока обрезаются.
	/// В случае если выходной блок больше входного, то данные каждого входного блока повторяются.
	/// </summary>
	internal class CryptoTransformMock : ISpanCryptoTransform
	{
		private readonly bool _canTransformMultipleBlocks;
		private readonly int _inputBlockSize;
		private readonly int _outputBlockSize;
		private readonly int _cacheBlocksLimit;
		private readonly byte[] _cache;
		private int _cachedBlocks = 0;

		public CryptoTransformMock (int inputBlockSize, int outputBlockSize, int inputCacheBlocks, bool canTransformMultipleBlocks)
		{
			if (inputBlockSize < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (inputBlockSize));
			}

			if (outputBlockSize < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (inputBlockSize));
			}

			if (inputCacheBlocks < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (inputCacheBlocks));
			}

			_inputBlockSize = inputBlockSize;
			_outputBlockSize = outputBlockSize;
			_cacheBlocksLimit = inputCacheBlocks;
			_cache = new byte[_cacheBlocksLimit * inputBlockSize];
			_canTransformMultipleBlocks = canTransformMultipleBlocks;
		}

		public bool CanReuseTransform => true;

		public bool CanTransformMultipleBlocks => _canTransformMultipleBlocks;

		public int InputBlockSize => _inputBlockSize;

		public int OutputBlockSize => _outputBlockSize;

		/// <summary>
		/// Ничего не делает.
		/// </summary>
		public void Dispose ()
		{
		}

		public int TransformBlock (ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			var inputCount = inputBuffer.Length;
			var outputOffset = 0;
			if (!_canTransformMultipleBlocks && (inputCount > _inputBlockSize))
			{
				throw new ArgumentOutOfRangeException (nameof (inputBuffer));
			}

			if ((inputCount % _inputBlockSize) != 0)
			{
				throw new InvalidOperationException ("inputCount must be multiple of InputBlockSize");
			}

			var inputBlocks = inputCount / _inputBlockSize;
			var outputBlocks = inputBlocks;
			if ((inputBlocks * _outputBlockSize) > (outputBuffer.Length - outputOffset))
			{
				throw new InvalidOperationException ("available size of outputBuffer must hold all the blocks specified in inputCount");
			}

			int result;

			// все влезет в кэш, на выход - ничего
			if ((_cachedBlocks + inputBlocks) <= _cacheBlocksLimit)
			{
				inputBuffer.Slice (0, inputCount).CopyTo (_cache.AsSpan (_cachedBlocks * _inputBlockSize));
				_cachedBlocks += inputBlocks;
				result = 0;
			}
			else
			{
				// заполняем начало выхода тем что в кэше
				if (_cachedBlocks > 0)
				{
					var blocksFromCache = Math.Min (_cachedBlocks, inputBlocks);

					// преобразуем часть кэша в выход
					Transform (_cache, outputBuffer.Slice (outputOffset), blocksFromCache);

					// сдвигаем остаток кэша в начало
					Array.Copy (_cache, blocksFromCache * _inputBlockSize, _cache, 0, (_cachedBlocks - blocksFromCache) * _inputBlockSize);
					_cachedBlocks -= blocksFromCache;
					outputOffset += blocksFromCache * _outputBlockSize;
					outputBlocks -= blocksFromCache;
				}

				// заполняем оставшийся выход из входа
				Transform (inputBuffer, outputBuffer.Slice (outputOffset), outputBlocks);
				var inputOffset = outputBlocks * _inputBlockSize;

				// остаток входа сохраняем в кэш
				inputBuffer.Slice (inputOffset, (inputBlocks - outputBlocks) * _inputBlockSize).CopyTo (_cache.AsSpan (_cachedBlocks * _inputBlockSize));
				_cachedBlocks += inputBlocks - outputBlocks;
				result = inputBlocks * _outputBlockSize;
			}

			// Trace.WriteLine (string.Format ("---TransformBlock: result = {0}", result));
			return result;
		}

		public ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer)
		{
			var inputOffset = 0;
			var inputCount = inputBuffer.Length;
			var inputBlocks = inputCount / _inputBlockSize;
			var inputReminder = inputCount - (inputBlocks * _inputBlockSize);
			var outputReminder = Math.Min (inputReminder, _outputBlockSize);
			var result = new byte[((_cachedBlocks + inputBlocks) * _outputBlockSize) + outputReminder];

			// данные из кэша
			Transform (_cache, result, _cachedBlocks);
			var outputOffset = _cachedBlocks * _outputBlockSize;

			// данные входа кратно блокам
			Transform (inputBuffer.Slice (inputOffset), result.AsSpan (outputOffset), inputBlocks);
			inputOffset += inputBlocks * _inputBlockSize;
			outputOffset += inputBlocks * _outputBlockSize;

			// оставшиеся данные
			for (int j = 0; j < outputReminder; j++)
			{
				result[outputOffset + j] = (byte)~inputBuffer[inputOffset + (j % inputReminder)];
			}

			// Trace.WriteLine (string.Format ("---TransformFinalBlock: result.Length = {0}", result.Length));
			return result;
		}

		private void Transform (ReadOnlySpan<byte> input, Span<byte> output, int nBlocks)
		{
			for (int i = 0; i < nBlocks; i++)
			{
				for (int j = 0; j < _outputBlockSize; j++)
				{
					var inIndex = (i * _inputBlockSize) + (j % _inputBlockSize);
					var outIndex = (i * _outputBlockSize) + j;
					output[outIndex] = (byte)~input[inIndex];
				}
			}
		}
	}
}
