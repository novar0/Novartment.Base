using System;
using System.Diagnostics.Contracts;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Сигнализатор для подачи сигналов между компонентами, использующий именованые каналы.
	/// </summary>
	public class NamedPipeSignaler :
		ISignaler
	{
		private readonly string _pipeName;
		private int _started = 0;

		/// <summary>
		/// Инициализирует новый экземпляр NamedPipeSignaler использующий указанное имя канала.
		/// </summary>
		/// <param name="pipeName">Имя канала. Должно быть уникально среди связывающихся компонентов.</param>
		public NamedPipeSignaler (string pipeName)
		{
			var isNullOrWhiteSpace = string.IsNullOrWhiteSpace (pipeName);
			if (isNullOrWhiteSpace)
			{
				throw new ArgumentNullException (nameof (pipeName));
			}

			Contract.EndContractBlock ();

			_pipeName = pipeName;
		}

		/// <summary>Запуск ожидания приёма сигнала.</summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая собой процесс ожидания.</returns>
		public async Task WaitForSignalAsync (CancellationToken cancellationToken = default)
		{
			var oldValue = Interlocked.CompareExchange (ref _started, 1, 0);
			if (oldValue != 0)
			{
				throw new InvalidOperationException ("Listening already started, second instance not supported.");
			}

			try
			{
				using var pipeServer = new NamedPipeServerStream (_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				await pipeServer.WaitForConnectionAsync (cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				_started = 0;
			}
		}

		/// <summary>Посылка сигнала.</summary>
		/// <param name="millisecondsTimeout">Максимальное время (в миллисекундах) отведённое на отсылку сигнала.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public async Task SendSignalAsync (int millisecondsTimeout, CancellationToken cancellationToken = default)
		{
			using var client = new NamedPipeClientStream (".", _pipeName, PipeDirection.Out);
			// это послужит сигналом серверу, ждущему подключения
			await client.ConnectAsync (millisecondsTimeout, cancellationToken).ConfigureAwait (false);
		}
	}
}
