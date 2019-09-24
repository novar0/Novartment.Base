using System;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Получатель двоичных данных для последовательной записи в сокет.
	/// </summary>
	public class SocketBinaryDestination :
		IBinaryDestination
	{
		private readonly Socket _socket;
		private bool _isCompleted = false;

		/// <summary>
		/// Инициализирует новый экземпляр SocketBinaryDestination для записи в указанный сокет.
		/// </summary>
		/// <param name="socket">Сокет, в который будет производиться запись.</param>
		public SocketBinaryDestination (Socket socket)
		{
			_socket = socket;
		}

		/// <summary>
		/// Получает сокет, в который осуществляется запись.
		/// </summary>
		public Socket BaseSocket => _socket;

		/// <summary>
		/// Указывает что запись окончена.
		/// </summary>
		public void SetComplete ()
		{
			_isCompleted = true;
		}

		/// <summary>
		/// Асинхронно записывает в получатель указанный сегмент массива байтов.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		public Task WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (_isCompleted)
			{
				throw new InvalidOperationException ("Can not write to completed socket destination.");
			}

#if NETSTANDARD2_1
			return _socket.SendAsync (buffer, SocketFlags.None, cancellationToken).AsTask ();
#else
			// TODO: как то переделать чтобы не создавать временный массив и ArraySegment
			return _socket.SendAsync (new ArraySegment<byte> (buffer.ToArray (), 0, buffer.Length), SocketFlags.None);
#endif
		}
	}
}
