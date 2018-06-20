using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <content>
	/// Класс-обёртка StreamBufferedSource для представления Stream в виде IFastSkipBufferedSource.
	/// </content>
	// TODO: переделать чтобы _buffer был Memory<byte>
	public static partial class StreamExtensions
	{
		private class StreamBufferedSource :
			IFastSkipBufferedSource
		{
			private readonly Stream _stream;
			private readonly byte[] _buffer;
			private int _offset = 0;
			private int _count = 0;
			private bool _streamEnded = false;

			internal StreamBufferedSource(Stream readableStream, byte[] buffer)
			{
				_stream = readableStream;
				_buffer = buffer;
			}

			public ReadOnlyMemory<byte> BufferMemory => _buffer;

			public int Offset => _offset;

			public int Count => _count;

			public bool IsExhausted => _streamEnded;

			public void SkipBuffer (int size)
			{
				if ((size < 0) || (size > _count))
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

			public Task<long> TryFastSkipAsync (long size, CancellationToken cancellationToken)
			{
				if (size < 0L)
				{
					throw new ArgumentOutOfRangeException (nameof (size));
				}

				Contract.EndContractBlock ();

				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<long> (cancellationToken);
				}

				var available = _count;

				// достаточно доступных данных буфера
				if (size <= (long)available)
				{
					_offset += (int)size;
					_count -= (int)size;
					return Task.FromResult (size);
				}

				long skipped = available;

				// пропускаем весь буфер
				size -= (long)available;
				_offset = _count = 0;

				// поток поддерживает позиционирование, сразу переходим на нужную позицию
				if (_stream.CanSeek)
				{
					try
					{
						var streamAvailable = _stream.Length - _stream.Position;
						_streamEnded = size >= streamAvailable;

						// если выходим за пределы размера потока то ставим указатель на конец
						if (size > streamAvailable)
						{
							_stream.Seek (0, SeekOrigin.End);
							skipped += streamAvailable;
							return Task.FromResult (skipped);
						}

						_stream.Seek (size, SeekOrigin.Current);
						skipped += size;
						return Task.FromResult (skipped);
					}

					// на случай если свойство Length, свойство Position или метод Seek() не поддерживаются потоком
					// будет использован метод последовательного чтения далее
					catch (NotSupportedException)
					{
					}
				}

				// поток не поддерживает позиционирование, читаем данные в буфер пока не считаем нужное количество
				return TrySkipAsyncStateMachine ();

				async Task<long> TrySkipAsyncStateMachine ()
				{
					int readed = 0;
					do
					{
						size -= (long)readed;
						skipped += (long)readed;
						readed = await _stream.ReadAsync (_buffer, 0, _buffer.Length, cancellationToken).ConfigureAwait (false);
						if (readed < 1)
						{
							_streamEnded = true;
							return skipped;
						}
					}
					while (size > (long)readed);

					// делаем доступным остаток буфера
					skipped += size;
					_offset = (int)size;
					_count = readed - (int)size;
					return skipped;
				}
			}

			public Task FillBufferAsync (CancellationToken cancellationToken)
			{
				if (_streamEnded || (_count >= _buffer.Length))
				{
					return Task.CompletedTask;
				}

				Defragment ();

				var task = _stream.ReadAsync (
					_buffer,
					_offset + _count,
					_buffer.Length - _offset - _count,
					cancellationToken);
				return FillBufferAsyncFinalizer ();

				async Task FillBufferAsyncFinalizer ()
				{
					var readed = await task.ConfigureAwait (false);
					_count += readed;
					if (readed < 1)
					{
						_streamEnded = true;
					}
				}
			}

			public Task EnsureBufferAsync (int size, CancellationToken cancellationToken)
			{
				if ((size < 0) || (size > _buffer.Length))
				{
					throw new ArgumentOutOfRangeException (nameof (size));
				}

				Contract.EndContractBlock ();

				if ((size <= _count) || _streamEnded)
				{
					if (size > _count)
					{
						throw new NotEnoughDataException (size - _count);
					}

					return Task.CompletedTask;
				}

				Defragment ();

				return EnsureBufferAsyncAsyncStateMachine ();

				// запускаем асинхронное чтение источника пока не наберём необходимое количество данных
				async Task EnsureBufferAsyncAsyncStateMachine ()
				{
					var available = _count;
					var shortage = size - available;
					while ((shortage > 0) && !_streamEnded)
					{
						var readed = await _stream.ReadAsync (
							_buffer,
							_offset + _count,
							_buffer.Length - _offset - _count,
							cancellationToken).ConfigureAwait (false);
						shortage -= readed;
						if (readed > 0)
						{
							_count += readed;
						}
						else
						{
							_streamEnded = true;
						}
					}

					if (shortage > 0)
					{
						throw new NotEnoughDataException (shortage);
					}
				}
			}

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
		}
	}
}
