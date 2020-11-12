using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий данные, считанные из указанного сокета.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public sealed class SocketBufferedSource :
		IBufferedSource
	{
		private readonly Socket _socket;
		private readonly Memory<byte> _buffer;
		private int _offset = 0;
		private int _count = 0;
		private bool _socketClosed = false;

		/// <summary>
		/// Инициализирует новый экземпляр SocketBufferedSource получающий данные из указанного сокета
		/// используя указанный буфер.
		/// </summary>
		/// <param name="socket">Исходный сокет для чтения данных.</param>
		/// <param name="buffer">Байтовый буфер, в котором будут содержаться считанные из сокета данные.</param>
		public SocketBufferedSource (Socket socket, Memory<byte> buffer)
		{
			if (socket == null)
			{
				throw new ArgumentNullException (nameof (socket));
			}

			if (buffer.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			Contract.EndContractBlock ();

			_socket = socket;
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
		public bool IsExhausted => _socketClosed;

		/// <summary>
		/// Skips specified amount of available data in the buffer.
		/// Properties Offset and Count may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip from the start of available data in the buffer.
		/// Must be less than or equal to the size of available data in the buffer.</param>
		public void Skip (int size)
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
		/// Asynchronously requests the source to load more data in the buffer.
		/// As a result, the buffer may not be completely filled if the source supplies data in blocks,
		/// or it may be empty if the source is exhausted.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous fill operation.
		/// If Count property equals zero after completion,
		/// this means that the source is exhausted and there will be no more data in the buffer.</returns>
		public ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return new ValueTask (Task.FromCanceled (cancellationToken));
			}

			if (_socketClosed || (_count >= _buffer.Length))
			{
				return default;
			}

			Defragment ();


#if NETSTANDARD2_0
			var bufSegment = new ArraySegment<byte> (_buffer.ToArray (), _offset + _count, _buffer.Length - _offset - _count);
			return FillBufferAsyncFinalizer (new ValueTask<int> (_socket.ReceiveAsync (bufSegment, SocketFlags.None)));
#else
			return FillBufferAsyncFinalizer (_socket.ReceiveAsync (_buffer.Slice (_offset + _count, _buffer.Length - _offset - _count), SocketFlags.None, cancellationToken));
#endif

			async ValueTask FillBufferAsyncFinalizer (ValueTask<int> task)
			{
				int readed;
#if NETSTANDARD2_0
				try
				{
					readed = await task.ConfigureAwait (false);
				}
				catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
				{
					// Когда операции с сокетами не поддерживают отмену, в случае отмены обычно сокет просто закрывают,
					// что вызывает ObjectDisposedException.
					// В таком случае мы проглатываем ObjectDisposedException и генерируем вместо него TaskCanceledException.
					throw new TaskCanceledException (task.AsTask ());
				}
#else
				readed = await task.ConfigureAwait (false);
#endif

				if (readed > 0)
				{
					_count += readed;
				}
				else
				{
					_socketClosed = true;
				}
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
			if ((size < 0) || (size > _buffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return new ValueTask (Task.FromCanceled (cancellationToken));
			}

			if ((size <= _count) || _socketClosed)
			{
				if (size > _count)
				{
					throw new NotEnoughDataException (size - _count);
				}

				return default;
			}

			Defragment ();
			return EnsureBufferAsyncStateMachine ();

			// запускаем асинхронное чтение источника пока не наберём необходимое количество данных
			async ValueTask EnsureBufferAsyncStateMachine ()
			{
				var available = _count;
				var shortage = size - available;
				while ((shortage > 0) && !_socketClosed)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					int readed;
#if NETSTANDARD2_0
					// Операции с сокетами не поддерживают отмену, поэтому в случае отмены обычно сокет просто закрывают,
					// что вызывает ObjectDisposedException.
					var bufSegment = new ArraySegment<byte> (_buffer.ToArray (), _offset + _count, _buffer.Length - _offset - _count);
					var task = _socket.ReceiveAsync (bufSegment, SocketFlags.None);
					try
					{
						readed = await task.ConfigureAwait (false);
					}
					catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
					{
						// сокет просто закрыт как единственно возможный метод прерывания чтения
						// проглатываем ObjectDisposedException и генерируем вместо него TaskCanceledException
						throw new TaskCanceledException (task);
					}
#else
					readed = await _socket.ReceiveAsync (_buffer.Slice (_offset + _count, _buffer.Length - _offset - _count), SocketFlags.None, cancellationToken).ConfigureAwait (false);
#endif

					shortage -= readed;
					if (readed > 0)
					{
						_count += readed;
					}
					else
					{
						_socketClosed = true;
					}
				}

				if (shortage > 0)
				{
					throw new NotEnoughDataException (shortage);
				}
			}
		}

		/// <summary>
		/// Обеспечивает чтобы данные в буфере начинались с позиции ноль.
		/// </summary>
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
