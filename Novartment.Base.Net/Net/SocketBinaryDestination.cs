using System;
using System.Net.Sockets;
using System.Diagnostics.Contracts;
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
		/// <param name="socket"></param>
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
		/// <param name="offset">Смещение байтов (начиная с нуля) в buffer, с которого начинается копирование байтов.</param>
		/// <param name="count">Число байтов для записи.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
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

			if (_isCompleted)
			{
				throw new InvalidOperationException ("Can not write to completed socket destination.");
			}

			return _socket.SendAsync (new ArraySegment<byte> (buffer, offset, count), SocketFlags.None);
		}
	}
}
