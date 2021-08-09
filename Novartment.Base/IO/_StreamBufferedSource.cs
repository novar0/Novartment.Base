using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	public static partial class BinaryStreamingStreamExtensions
	{
		/// <content>
		/// Класс-обёртка StreamBufferedSource для представления Stream в виде IFastSkipBufferedSource.
		/// </content>
		private sealed class StreamBufferedSource :
			IFastSkipBufferedSource
		{
			private readonly Stream _stream;
			private readonly Memory<byte> _buffer;
			private int _offset = 0;
			private int _count = 0;
			private bool _streamEnded = false;

			internal StreamBufferedSource (Stream readableStream, Memory<byte> buffer)
			{
				_stream = readableStream;
				_buffer = buffer;
			}

			public ReadOnlyMemory<byte> BufferMemory => _buffer;

			public int Offset => _offset;

			public int Count => _count;

			public bool IsExhausted => _streamEnded;

			public void Skip (int size)
			{
				if ((size < 0) || (size > _count))
				{
					throw new ArgumentOutOfRangeException (nameof (size));
				}

				if (size > 0)
				{
					_offset += size;
					_count -= size;
				}
			}

			public ValueTask<long> SkipWihoutBufferingAsync (long size, CancellationToken cancellationToken = default)
			{
				if (size < 0L)
				{
					throw new ArgumentOutOfRangeException (nameof (size));
				}

				if (cancellationToken.IsCancellationRequested)
				{
					return new ValueTask<long> (Task.FromCanceled<long> (cancellationToken));
				}

				var available = _count;

				// достаточно доступных данных буфера
				if (size <= (long)available)
				{
					_offset += (int)size;
					_count -= (int)size;
					return new ValueTask<long> (size);
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
							return new ValueTask<long> (skipped);
						}

						_stream.Seek (size, SeekOrigin.Current);
						skipped += size;
						return new ValueTask<long> (skipped);
					}

					// на случай если свойство Length, свойство Position или метод Seek() не поддерживаются потоком
					// будет использован метод последовательного чтения далее
					catch (NotSupportedException)
					{
					}
				}

				// поток не поддерживает позиционирование, читаем данные в буфер пока не считаем нужное количество
				return TrySkipAsyncStateMachine ();

				async ValueTask<long> TrySkipAsyncStateMachine ()
				{
					int readed = 0;
					do
					{
						size -= (long)readed;
						skipped += (long)readed;
#if NETSTANDARD2_0
						readed = await _stream.ReadAsync (_buffer.ToArray (), 0, _buffer.Length, cancellationToken).ConfigureAwait (false);
#else
						readed = await _stream.ReadAsync (_buffer, cancellationToken).ConfigureAwait (false);
#endif
						if (readed < 1)
						{
							_streamEnded = true;
							return skipped;
						}
					}
					while (size > (long)readed);

					CheckIfStreamEnded ();

					// делаем доступным остаток буфера
					skipped += size;
					_offset = (int)size;
					_count = readed - (int)size;
					return skipped;
				}
			}

			public async ValueTask LoadAsync (CancellationToken cancellationToken = default)
			{
				if (_streamEnded || (_count >= _buffer.Length))
				{
					return;
				}

				Defragment ();

#if NETSTANDARD2_0
				var readed = await _stream.ReadAsync (
					_buffer.ToArray (),
					_offset + _count,
					_buffer.Length - _offset - _count,
					cancellationToken).ConfigureAwait (false);
#else
				var readed = await _stream.ReadAsync (
					_buffer.Slice (_offset + _count, _buffer.Length - _offset - _count),
					cancellationToken).ConfigureAwait (false);
#endif

				_count += readed;
				if (readed < 1)
				{
					_streamEnded = true;
				}
				else
				{
					CheckIfStreamEnded ();
				}
			}

			public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default)
			{
				if ((size < 0) || (size > _buffer.Length))
				{
					throw new ArgumentOutOfRangeException (nameof (size));
				}

				if ((size <= _count) || _streamEnded)
				{
					if (size > _count)
					{
						throw new NotEnoughDataException (size - _count);
					}

					return default;
				}

				Defragment ();

				return EnsureBufferAsyncAsyncStateMachine ();

				// запускаем асинхронное чтение источника пока не наберём необходимое количество данных
				async ValueTask EnsureBufferAsyncAsyncStateMachine ()
				{
					var available = _count;
					var shortage = size - available;
					while ((shortage > 0) && !_streamEnded)
					{
#if NETSTANDARD2_0
						var readed = await _stream.ReadAsync (
							_buffer.ToArray (),
							_offset + _count,
							_buffer.Length - _offset - _count,
							cancellationToken).ConfigureAwait (false);
#else
						var readed = await _stream.ReadAsync (
							_buffer.Slice (_offset + _count, _buffer.Length - _offset - _count),
							cancellationToken).ConfigureAwait (false);
#endif
						shortage -= readed;
						if (readed > 0)
						{
							_count += readed;
							CheckIfStreamEnded ();
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

			private void CheckIfStreamEnded ()
			{
				if (_stream.CanSeek && (_stream.Position >= _stream.Length))
				{
					_streamEnded = true;
				}
			}

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
}
