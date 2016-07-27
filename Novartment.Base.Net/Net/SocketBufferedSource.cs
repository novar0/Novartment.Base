using System;
using System.Net.Sockets;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий данные, считанные из указанного сокета.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({Buffer.Length}) exhausted={IsExhausted}")]
	public class SocketBufferedSource :
		IBufferedSource
	{
		private readonly Socket _socket;
		private readonly byte[] _buffer;
		private int _offset = 0;
		private int _count = 0;
		private bool _socketClosed = false;

		/// <summary>
		/// Получает буфер, в котором содержится некоторая часть данных сокета.
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

		/// <summary>Получает признак исчерпания сокета.
		/// Возвращает True если сокет больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.</summary>
		public bool IsExhausted => _socketClosed;

		/// <summary>
		/// Инициализирует новый экземпляр SocketBufferedSource получающий данные из указанного сокета
		/// используя указанный буфер.
		/// </summary>
		/// <param name="socket">Исходный сокет для чтения данных.</param>
		/// <param name="buffer">Байтовый буфер, в котором будут содержаться считанные из сокета данные.</param>
		public SocketBufferedSource (Socket socket, byte[] buffer)
		{
			if (socket == null)
			{
				throw new ArgumentNullException (nameof (socket));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}
			if (buffer.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}
			Contract.EndContractBlock ();

			_socket = socket;
			_buffer = buffer;
		}

		/// <summary>Отбрасывает (пропускает) указанное количество данных из начала буфера.</summary>
		/// <param name="size">Размер данных для пропуска в начале буфера.
		/// Должен быть меньше чем размер данных в буфере.</param>
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

		/// <summary>
		/// Асинхронно заполняет буфер данными источника, дополняя уже доступные там данные.
		/// В результате буфер может быть заполнен не полностью если источник поставляет данные блоками, либо пуст если источник исчерпался.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.
		/// Если после завершения в Count будет ноль,
		/// то источник исчерпан и доступных данных в буфере больше не будет.</returns>
		/// <remarks>
		/// Параметр cancellationToken не используется для отмены уже запущенного чтения сокета,
		/// потому что чтение сокета вообще не поддерживает отмену.
		/// Для отмены чтения используйте Socket.Close().
		/// </remarks>
		public Task FillBufferAsync (CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			if (_socketClosed || (_count >= _buffer.Length))
			{
				return Task.CompletedTask;
			}

			Defragment ();

			var bufSegment = new ArraySegment<byte> (_buffer, _offset + _count, _buffer.Length - _offset - _count);
			var task = _socket.ReceiveAsync (bufSegment, SocketFlags.None);
			return FillBufferAsyncFinalizer (task);
		}

		private async Task FillBufferAsyncFinalizer (Task<int> task)
		{
			var readed = await task.ConfigureAwait (false);
			if (readed > 0)
			{
				AcceptChunk (readed);
			}
			else
			{
				SetStreamEnded ();
			}
		}

		/// <summary>
		/// Асинхронно запрашивает у источника указанное количество данных в буфере.
		/// В результате запроса в буфере может оказаться данных больше, чем запрошено.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Требуемый размер данных в буфере.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <remarks>
		/// Параметр cancellationToken не используется для отмены уже запущенного чтения сокета,
		/// потому что чтение сокета вообще не поддерживает отмену.
		/// Для отмены чтения используйте Socket.Close().
		/// </remarks>
		public Task EnsureBufferAsync (int size, CancellationToken cancellationToken)
		{
			if ((size < 0) || (size > _buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}
			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}
			if ((size <= _count) || _socketClosed)
			{
				if (size > _count)
				{
					throw new NotEnoughDataException (size - _count);
				}
				return Task.CompletedTask;
			}
			Defragment ();
			return EnsureBufferAsyncStateMachine (size, cancellationToken);
		}

		// запускаем асинхронное чтение источника пока не наберём необходимое количество данных
		private async Task EnsureBufferAsyncStateMachine (int size, CancellationToken cancellationToken)
		{
			var available = _count;
			var shortage = size - available;
			while ((shortage > 0) && !_socketClosed)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				var bufSegment = new ArraySegment<byte> (_buffer, _offset + _count, _buffer.Length - _offset - _count);
				var readed = await _socket.ReceiveAsync (bufSegment, SocketFlags.None).ConfigureAwait (false);
				shortage -= readed;
				if (readed > 0)
				{
					AcceptChunk (readed);
				}
				else
				{
					SetStreamEnded ();
				}
			}
			if (shortage > 0)
			{
				throw new NotEnoughDataException (shortage);
			}
		}

		/// <summary>
		/// Принимает в буфер указанное количество считанных из сокета данных.
		/// Добавленные данные должны располагаться в буфере начиная с позиции Count.
		/// </summary>
		/// <param name="count">Количество байтов, добавленных в буфер.</param>
		protected void AcceptChunk (int count)
		{
			_count += count;
		}

		/// <summary>
		/// Устанавливет признак исчерпания сокета.
		/// </summary>
		protected void SetStreamEnded ()
		{
			_socketClosed = true;
		}

		/// <summary>
		/// Обеспечивает чтобы данные в буфере начинались с позиции ноль.
		/// </summary>
		protected void Defragment ()
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
