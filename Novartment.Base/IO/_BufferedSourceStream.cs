using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	public static partial class BinaryStreamingStreamExtensions
	{
		/// <content>
		/// Класс-обёртка BufferedSourceStream для представления IBufferedSource в виде Stream.
		/// </content>
		private sealed class BufferedSourceStream : Stream
		{
			private readonly IBufferedSource _source;

			internal BufferedSourceStream (IBufferedSource source)
			{
				_source = source;
			}

			public override bool CanRead => true;

			public override bool CanWrite => false;

			public override bool CanSeek => false;

			public override long Length => throw new NotSupportedException ();

			public override long Position
			{
				get => throw new NotSupportedException ();
				set => throw new NotSupportedException ();
			}

			public override long Seek (long offset, SeekOrigin origin) => throw new NotSupportedException ();

			public override void SetLength (long value) => throw new NotSupportedException ();

			public override void Write (byte[] buffer, int offset, int count) => throw new NotSupportedException ();

			public override void Flush ()
			{
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException (nameof (buffer));
				}

				if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
				{
					throw new ArgumentOutOfRangeException (nameof (offset));
				}

				if ((count < 0) || (count > (buffer.Length - offset)))
				{
					throw new ArgumentOutOfRangeException (nameof (count));
				}

				if ((count <= _source.Count) || _source.IsExhausted)
				{
					// данных в источнике достаточно, считывание не требуется
					var toCopy = Math.Min (_source.Count, count);
					if (toCopy > 0)
					{
						_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
						_source.Skip (toCopy);
					}

					return toCopy;
				}

				int resultSize = 0;
				while (count > 0)
				{
					var vTask = _source.LoadAsync (default);
					if (!vTask.IsCompletedSuccessfully)
					{
						vTask.AsTask ().GetAwaiter ().GetResult ();
					}
					if (_source.Count <= 0)
					{
						break;
					}

					var toCopy = Math.Min (_source.Count, count);
					_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
					offset += toCopy;
					count -= toCopy;
					resultSize += toCopy;
					_source.Skip (toCopy);
				}

				return resultSize;
			}

			/// <summary>
			/// Считывает байт из потока и перемещает позицию в потоке на один байт или возвращает -1, если достигнут конец потока.
			/// </summary>
			/// <returns>Байт без знака, приведенный к Int32, или значение -1, если достигнут конец потока.</returns>
			public override int ReadByte ()
			{
				if (_source.Count < 1)
				{
					var vTask = _source.LoadAsync (default);
					if (!vTask.IsCompletedSuccessfully)
					{
						vTask.AsTask ().GetAwaiter ().GetResult ();
					}
					if (_source.Count < 1)
					{
						return -1;
					}
				}

				var result = (int)_source.BufferMemory.Span[_source.Offset];
				_source.Skip (1);
				return result;
			}

			public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException (nameof (buffer));
				}

				if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
				{
					throw new ArgumentOutOfRangeException (nameof (offset));
				}

				if ((count < 0) || (count > (buffer.Length - offset)))
				{
					throw new ArgumentOutOfRangeException (nameof (count));
				}

				if ((count <= _source.Count) || _source.IsExhausted)
				{
					// данных в источнике достаточно, асинхронное обращение не требуется
					var toCopy = Math.Min (_source.Count, count);
					if (toCopy > 0)
					{
						_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
						_source.Skip (toCopy);
					}

					return Task.FromResult (toCopy);
				}

				return ReadAsyncStateMachine ();

				// асинхронный запрос к источнику пока не наберём необходимое количество данных
				async Task<int> ReadAsyncStateMachine ()
				{
					int resultSize = 0;
					while (count > 0)
					{
						if ((count > _source.Count) && !_source.IsExhausted)
						{
							await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
						}

						if (_source.Count <= 0)
						{
							break;
						}

						var toCopy = Math.Min (_source.Count, count);
						_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
						offset += toCopy;
						count -= toCopy;
						resultSize += toCopy;
						_source.Skip (toCopy);
					}

					return resultSize;
				}
			}

#if !NETSTANDARD2_0
			public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
			{
				if ((buffer.Length <= _source.Count) || _source.IsExhausted)
				{
					// данных в источнике достаточно, асинхронное обращение не требуется
					var toCopy = Math.Min (_source.Count, buffer.Length);
					if (toCopy > 0)
					{
						_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer);
						_source.Skip (toCopy);
					}

					return new ValueTask<int> (toCopy);
				}

				return ReadAsyncStateMachine ();

				// асинхронный запрос к источнику пока не наберём необходимое количество данных
				async ValueTask<int> ReadAsyncStateMachine ()
				{
					var offset = 0;
					var count = buffer.Length;
					int resultSize = 0;
					while (count > 0)
					{
						if ((count > _source.Count) && !_source.IsExhausted)
						{
							await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
						}

						if (_source.Count <= 0)
						{
							break;
						}

						var toCopy = Math.Min (_source.Count, count);
						_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer[offset..]);
						offset += toCopy;
						count -= toCopy;
						resultSize += toCopy;
						_source.Skip (toCopy);
					}

					return resultSize;
				}
			}
#endif

			public override Task CopyToAsync (Stream destination, int bufferSize, CancellationToken cancellationToken = default)
			{
				return BufferedSourceExtensions.WriteToAsync (_source, destination.AsBinaryDestination (), cancellationToken);
			}
		}
	}
}
