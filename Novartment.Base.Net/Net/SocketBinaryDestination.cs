using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Получатель двоичных данных для последовательной записи в сокет.
	/// </summary>
	public sealed class SocketBinaryDestination :
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
			_socket = socket ?? throw new ArgumentNullException (nameof (socket));
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
		/// Asynchronously writes specified region of memory to this destination.
		/// </summary>
		/// <param name="buffer">The region of memory to write to this destination.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the write operation.</returns>
		public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (_isCompleted)
			{
				throw new InvalidOperationException ("Can not write to completed socket destination.");
			}

#if NETSTANDARD2_0
			return new ValueTask (_socket.SendAsync (new ArraySegment<byte> (buffer.ToArray (), 0, buffer.Length), SocketFlags.None));
#else
			var vTask = _socket.SendAsync (buffer, SocketFlags.None, cancellationToken);
			return (vTask.IsCompletedSuccessfully) ?
				default :
				new ValueTask (vTask.AsTask ());
#endif
		}
	}
}
