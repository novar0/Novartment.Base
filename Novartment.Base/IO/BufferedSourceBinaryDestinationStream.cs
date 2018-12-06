using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Поток, получающий данные из источника данных, представленного байтовым буфером и
	/// передающий все записываемые данные в указанный получатель двоичных данных.
	/// </summary>
	public class BufferedSourceBinaryDestinationStream : Stream
	{
		private readonly IBufferedSource _source;
		private readonly IBinaryDestination _destination;

		/// <summary>
		/// Инициализирует новый экземпляр BufferedSourceBinaryDestinationStream,
		/// получающий данные из указанного источника данных, представленного байтовым буфером и
		/// передающий все записываемые данные в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="source">Источник данных, представленный байтовым буфером, из котрого будет получать данные поток.</param>
		/// <param name="destination">Получатель двоичных данных, в который будут передаваться все записываемые в поток данные.</param>
		public BufferedSourceBinaryDestinationStream (IBufferedSource source, IBinaryDestination destination)
		{
			_source = source;
			_destination = destination;
		}

		/// <summary>Получает значение, показывающее, поддерживает ли поток возможность чтения.</summary>
		public override bool CanRead => true;

		/// <summary>Получает значение, которое показывает, поддерживает ли поток возможность записи.</summary>
		public override bool CanWrite => true;

		/// <summary>Получает значение, которое показывает, поддерживается ли в потоке возможность поиска.</summary>
		public override bool CanSeek => false;

		/// <summary>Получает длину потока в байтах.</summary>
		public override long Length => throw new NotSupportedException ();

		/// <summary>Получает или задает позицию в потоке.</summary>
		public override long Position
		{
			get => throw new NotSupportedException ();
			set => throw new NotSupportedException ();
		}

		/// <summary>
		/// Задает позицию в потоке.
		/// </summary>
		/// <param name="offset">Смещение в байтах относительно параметра origin.</param>
		/// <param name="origin">определяет точку ссылки, которая используется для получения новой позиции.</param>
		/// <returns>Новая позиция в текущем потоке.</returns>
		public override long Seek (long offset, SeekOrigin origin) => throw new NotSupportedException ();

		/// <summary>
		/// Задает длину потока.
		/// </summary>
		/// <param name="value">Необходимая длина потока в байтах.</param>
		public override void SetLength (long value) => throw new NotSupportedException ();

		/// <summary>
		/// Записывает последовательность байтов в поток и перемещает позицию в нём вперед на число записанных байтов.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="offset">Смещение байтов (начиная с нуля) в buffer, с которого начинается копирование байтов в поток.</param>
		/// <param name="count">Максимальное число байтов для записи.</param>
		public override void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || ((offset + count) > buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (_destination is StreamExtensions.StreamBinaryDestination streamBinaryDestination)
			{
				streamBinaryDestination.BaseStream.Write (buffer, offset, count);
			}
			else
			{
				_destination.WriteAsync (buffer.AsMemory (offset, count), default).Wait ();
			}
		}

		/// <summary>
		/// Асинхронно записывает последовательность байтов в поток,
		/// перемещает текущую позицию внутри потока на число записанных байтов и отслеживает запросы отмены.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="offset">Смещение байтов (начиная с нуля) в buffer, с которого начинается копирование байтов в поток.</param>
		/// <param name="count">Максимальное число байтов для записи.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || ((offset + count) > buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			return (_destination is StreamExtensions.StreamBinaryDestination streamBinaryDestination) ?
				streamBinaryDestination.BaseStream.WriteAsync (buffer, offset, count, cancellationToken) :
				_destination.WriteAsync (buffer.AsMemory (offset, count), cancellationToken);
		}

#if NETCOREAPP2_2

		/// <summary>
		/// Асинхронно записывает последовательность байтов в поток,
		/// перемещает текущую позицию внутри потока на число записанных байтов и отслеживает запросы отмены.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		public override ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			return (_destination is StreamExtensions.StreamBinaryDestination streamBinaryDestination) ?
				streamBinaryDestination.BaseStream.WriteAsync (buffer, cancellationToken) :
				new ValueTask (_destination.WriteAsync (buffer, cancellationToken));
		}
#endif

		/// <summary>
		/// Очищает все буферы данного потока и вызывает запись данных буферов в базовое устройство.
		/// </summary>
		public override void Flush ()
		{
			if (_destination is StreamExtensions.StreamBinaryDestination streamBinaryDestination)
			{
				streamBinaryDestination.BaseStream.Flush ();
			}
		}

		/// <summary>
		/// Асинхронно очищает все буферы для этого потока и вызывает запись всех буферизованных данных в базовое устройство.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию очистки.</returns>
		public override Task FlushAsync (CancellationToken cancellationToken = default)
		{
			return (_destination is StreamExtensions.StreamBinaryDestination streamBinaryDestination) ?
				streamBinaryDestination.BaseStream.FlushAsync () :
				Task.CompletedTask;
		}

		/// <summary>
		/// Считывает последовательность байтов из потока и перемещает позицию в потоке на число считанных байтов.
		/// </summary>
		/// <param name="buffer">Буфер, в который записываются данные.</param>
		/// <param name="offset">Смещение байтов в buffer, с которого начинается запись данных из потока.</param>
		/// <param name="count">Максимальное число байтов, предназначенных для чтения.</param>
		/// <returns>Общее количество байтов, считанных в буфер.Это число может быть меньше количества запрошенных байтов, если столько байтов в настоящее время недоступно, а также равняться нулю (0), если был достигнут конец потока.</returns>
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

			int resultSize = 0;
			var available = _source.Count;
			if ((count <= available) || _source.IsExhausted)
			{
				// данных в источнике достаточно, запрос новых данных не требуется
				var toCopy = Math.Min (available, count);
				if (toCopy > 0)
				{
					_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
					_source.SkipBuffer (toCopy);
				}

				return toCopy;
			}

			while (count > 0)
			{
				_source.FillBufferAsync (default).Wait ();
				if (_source.Count <= 0)
				{
					break;
				}

				var toCopy = Math.Min (_source.Count, count);
				_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
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
				_source.FillBufferAsync (default).Wait ();
				if (_source.Count < 1)
				{
					return -1;
				}
			}

			var result = (int)_source.BufferMemory.Span[_source.Offset];
			_source.SkipBuffer (1);
			return result;
		}

		/// <summary>
		/// Асинхронно считывает последовательность байтов из потока,
		/// перемещает позицию в потоке на число считанных байтов и отслеживает запросы отмены.
		/// </summary>
		/// <param name="buffer">Буфер, в который считывается данные.</param>
		/// <param name="offset">Смещение байтов в buffer, с которого начинается cчитывание данных из потока.</param>
		/// <param name="count">Максимальное число байтов, предназначенных для чтения.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является общее число байтов, считанных в буфер.
		/// Значение результата может быть меньше запрошенного числа байтов,
		/// если число доступных в данный момент байтов меньше запрошенного числа,
		/// или результат может быть равен 0 (нулю), если был достигнут конец потока.</returns>
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

			Contract.EndContractBlock ();

			var available = _source.Count;
			if ((count <= available) || _source.IsExhausted)
			{
				// данных в источнике достаточно, асинхронное обращение не требуется
				var toCopy = Math.Min (available, count);
				if (toCopy > 0)
				{
					_source.BufferMemory.Span.Slice (_source.Offset, toCopy).CopyTo (buffer.AsSpan (offset));
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
					_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer.AsMemory (offset));
					offset += toCopy;
					count -= toCopy;
					resultSize += toCopy;
					_source.SkipBuffer (toCopy);
				}

				return resultSize;
			}
		}

#if NETCOREAPP2_2
		/// <summary>
		/// Асинхронно считывает последовательность байтов из потока,
		/// перемещает позицию в потоке на число считанных байтов и отслеживает запросы отмены.
		/// </summary>
		/// <param name="buffer">Буфер, в который считывается данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является общее число байтов, считанных в буфер.
		/// Значение результата может быть меньше размера буфера,
		/// если число доступных в данный момент байтов меньше запрошенного числа,
		/// или результат может быть равен 0 (нулю), если был достигнут конец потока.</returns>
		public override ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var available = _source.Count;
			if ((buffer.Length <= available) || _source.IsExhausted)
			{
				// данных в источнике достаточно, асинхронное обращение не требуется
				var toCopy = Math.Min (available, buffer.Length);
				if (toCopy > 0)
				{
					_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer);
					_source.SkipBuffer (toCopy);
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
						await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
					}

					if (_source.Count <= 0)
					{
						break;
					}

					var toCopy = Math.Min (_source.Count, count);
					_source.BufferMemory.Slice (_source.Offset, toCopy).CopyTo (buffer.Slice (offset));
					offset += toCopy;
					count -= toCopy;
					resultSize += toCopy;
					_source.SkipBuffer (toCopy);
				}

				return resultSize;
			}
		}
#endif

		/// <summary>
		/// Асинхронно считывает байты из потока и записывает их в другой поток, используя указанный размер буфера и токен отмены.
		/// </summary>
		/// <param name="destination">Поток, в который будет скопировано содержимое текущего потока.</param>
		/// <param name="bufferSize">Размер (в байтах) буфера. Это значение должно быть больше нуля.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию копирования.</returns>
		public override Task CopyToAsync (Stream destination, int bufferSize, CancellationToken cancellationToken = default)
		{
			return BufferedSourceExtensions.WriteToAsync (_source, destination.AsBinaryDestination (), cancellationToken);
		}

		/// <summary>
		/// Освобождает неуправляемые ресурсы, используемые объектом, а при необходимости освобождает также управляемые ресурсы.
		/// </summary>
		/// <param name="disposing">Значение true позволяет освободить управляемые и неуправляемые ресурсы;
		/// значение false позволяет освободить только неуправляемые ресурсы.</param>
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing)
			{
				_destination.SetComplete ();
			}
		}
	}
}
