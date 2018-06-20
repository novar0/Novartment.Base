using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Test
{
	// имитирует источник данных большого размера без выделения ресурсов
	// заполняет буфер путем вызова указанной функции, в которую передается абсолютная позиция в источнике
	public class BigBufferedSourceMock :
		IFastSkipBufferedSource
	{
		private readonly Memory<byte> _buffer;
		private readonly long _size = 0L;
		private readonly Func<long, byte> _dataFunction;
		private long _position = 0;
		private int _offset = 0;
		private int _count = 0;
		private bool _isExhausted = false;

		public BigBufferedSourceMock (long size, int bufferSize, Func<long, byte> dataFunction)
		{
			if (bufferSize < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (bufferSize));
			}

			_size = size;
			_buffer = new byte[bufferSize];
			_dataFunction = dataFunction;
		}

		public ReadOnlyMemory<byte> BufferMemory => _buffer;

		public int Offset => _offset;

		public int Count => _count;

		public bool IsExhausted => _isExhausted;

		public void SkipBuffer (int size)
		{
			if ((size < 0) || (size > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size > 0)
			{
				_offset += size;
				_count -= size;
			}
		}

		public Task FillBufferAsync (CancellationToken cancellationToken)
		{
			if (!_isExhausted)
			{
				if (_offset > 0)
				{
					// сдвигаем в начало данные буфера
					if (_count > 0)
					{
						_buffer.Slice (_offset, _count).CopyTo (_buffer);
					}

					_offset = 0;
				}

				int i = 0;
				var span = _buffer.Span;
				while (((_offset + _count + i) < _buffer.Length) && ((_position + i) < _size))
				{
					span[_offset + _count + i] = _dataFunction.Invoke (_position + i);
					i++;
				}

				if ((_position + i) >= _size)
				{
					_isExhausted = true;
				}

				_count += i;
				_position += i;
			}

			return Task.CompletedTask;
		}

		public Task EnsureBufferAsync (int size, CancellationToken cancellationToken)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size > 0)
			{
				var available = _count;
				var shortage = size - available;

				// данных в буфере достаточно или запрашивать их безполезно
				if ((shortage > 0) && !_isExhausted)
				{
					// сдвигаем в начало данные буфера
					if (_offset > 0)
					{
						if (available > 0)
						{
							_buffer.Slice (_offset, available).CopyTo (_buffer);
						}

						_offset = 0;
					}

					int i = 0;
					var span = _buffer.Span;
					while ((i < shortage) && ((_position + i) < _size))
					{
						span[_offset + _count + i] = _dataFunction.Invoke (_position + i);
						i++;
					}

					if ((_position + i) >= _size)
					{
						_isExhausted = true;
					}

					_count += i;
					_position += i;
				}

				if (shortage > 0)
				{
					throw new NotEnoughDataException (shortage);
				}
			}

			return Task.CompletedTask;
		}

		public Task<long> TryFastSkipAsync (long size, CancellationToken cancellationToken)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			long skipped = 0L;
			var availableBuffer = _count;
			if (size <= (long)availableBuffer)
			{
				_offset += (int)size;
				return Task.FromResult (size);
			}

			if (availableBuffer > 0)
			{
				size -= availableBuffer;
				skipped += availableBuffer;
				_count = 0;
			}

			var availableSource = _size - _position;
			if (size > availableSource)
			{
				_position = _size;
				skipped += availableSource;
				_isExhausted = true;
			}
			else
			{
				_position += size;
				skipped += size;
			}

			return Task.FromResult (skipped);
		}
	}
}
