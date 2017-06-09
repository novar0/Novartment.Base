﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <content>
	/// Класс-обёртка BufferedSourceStream для представления IBufferedSource в виде Stream.
	/// </content>
	public static partial class StreamExtensions
	{
		private class BufferedSourceStream : Stream
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

			internal IBufferedSource BaseBufferedSource => _source;

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

				Contract.EndContractBlock ();

				if ((count <= _source.Count) || _source.IsExhausted)
				{
					// данных в источнике достаточно, считывание не требуется
					var toCopy = Math.Min (_source.Count, count);
					if (toCopy > 0)
					{
						Array.Copy (_source.Buffer, _source.Offset, buffer, offset, toCopy);
						_source.SkipBuffer (toCopy);
					}

					return toCopy;
				}

				int resultSize = 0;
				while (count > 0)
				{
					_source.FillBufferAsync (CancellationToken.None).Wait ();
					if (_source.Count <= 0)
					{
						break;
					}

					var toCopy = Math.Min (_source.Count, count);
					Array.Copy (_source.Buffer, _source.Offset, buffer, offset, toCopy);
					offset += toCopy;
					count -= toCopy;
					resultSize += toCopy;
					_source.SkipBuffer (toCopy);
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
					_source.FillBufferAsync (CancellationToken.None).Wait ();
					if (_source.Count < 1)
					{
						return -1;
					}
				}

				var result = (int)_source.Buffer[_source.Offset];
				_source.SkipBuffer (1);
				return result;
			}

			public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
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

				Contract.EndContractBlock ();

				if ((count <= _source.Count) || _source.IsExhausted)
				{
					// данных в источнике достаточно, асинхронное обращение не требуется
					var toCopy = Math.Min (_source.Count, count);
					if (toCopy > 0)
					{
						Array.Copy (_source.Buffer, _source.Offset, buffer, offset, toCopy);
						_source.SkipBuffer (toCopy);
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
							await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
						}

						if (_source.Count <= 0)
						{
							break;
						}

						var toCopy = Math.Min (_source.Count, count);
						Array.Copy (_source.Buffer, _source.Offset, buffer, offset, toCopy);
						offset += toCopy;
						count -= toCopy;
						resultSize += toCopy;
						_source.SkipBuffer (toCopy);
					}

					return resultSize;
				}
			}

			public override Task CopyToAsync (Stream destination, int bufferSize, CancellationToken cancellationToken)
			{
				return BufferedSourceExtensions.WriteToAsync (_source, destination.AsBinaryDestination (), cancellationToken);
			}
		}
	}
}
