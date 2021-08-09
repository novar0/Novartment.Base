using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Прослушиватель TCP-подключений на основе сокетов.
	/// </summary>
	public sealed class SocketTcpListener :
		ITcpListener,
		IDisposable
	{
		private readonly IPEndPoint _localEP;
		private Socket _socket;
		private bool _active = false;

		/// <summary>
		/// Инициализирует новый экземпляр SocketTcpListener для прослушивания на указанной конечной точке.
		/// </summary>
		/// <param name="localEP">Конечная точка, на которой будет осуществляться прослушивание.</param>
		public SocketTcpListener (IPEndPoint localEP)
		{
			_localEP = localEP ?? throw new ArgumentNullException (nameof (localEP));
			_socket = new Socket (_localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Получает конечнную точку, на которой производится прослушивание.
		/// </summary>
		public IPEndPoint LocalEndpoint => _localEP;

		/// <summary>
		/// Запускает прослушивание.
		/// </summary>
		public void Start ()
		{
			if (!_active)
			{
				_socket.Bind (_localEP);
				try
				{
					_socket.Listen (int.MaxValue);
				}
				catch (SocketException)
				{
					Stop ();
					throw;
				}

				_active = true;
			}
		}

		/// <summary>
		/// Останавливает прослушивание.
		/// </summary>
		public void Stop ()
		{
			if (_active)
			{
				_socket.Dispose ();
				_active = false;
				_socket = new Socket (_localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			}
		}

		/// <summary>
		/// Асинхронно получает подключение, полученное прослушивателем.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Установленное TCP-подключение.</returns>
		public Task<ITcpConnection> AcceptTcpClientAsync (CancellationToken cancellationToken = default)
		{
			if (!_active)
			{
				throw new InvalidOperationException ("Can not accept connections in «stopped» state.");
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<ITcpConnection> (cancellationToken);
			}

			var ipProps = IPGlobalProperties.GetIPGlobalProperties ();
			var localHostFqdn = string.IsNullOrWhiteSpace (ipProps.DomainName) ?
				ipProps.HostName :
				ipProps.HostName + "." + ipProps.DomainName;

			// ввиду того, что Socket.AcceptAsync() не поддерживает отмену, применяем обходной путь:
			// регистрируем Socket.Close() на отмену, при этом Socket.AcceptAsync() бросит исключение ObjectDisposedException
			// что и будет для нас означать отмену
			IDisposable tokenRegistration = null;
			if (cancellationToken.CanBeCanceled)
			{
				tokenRegistration = cancellationToken.Register (Stop, false);
			}

			Task<Socket> task;
			try
			{
				task = _socket.AcceptAsync ();
				return AcceptTcpClientAsyncFinalizer (task);
			}
			catch (Exception excpt)
			{
				tokenRegistration?.Dispose ();
				if ((excpt is ObjectDisposedException) && cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<ITcpConnection> (cancellationToken);
				}

				throw;
			}

			async Task<ITcpConnection> AcceptTcpClientAsyncFinalizer (Task<Socket> runningTask)
			{
				try
				{
					var connectedSocket = await runningTask.ConfigureAwait (false);
					return new SocketBinaryTcpConnection (connectedSocket, localHostFqdn, null);
				}
				catch (SocketException) when (cancellationToken.IsCancellationRequested)
				{
					throw new TaskCanceledException (runningTask);
				}
				catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
				{
					throw new TaskCanceledException (runningTask);
				}
				finally
				{
					tokenRegistration?.Dispose ();
				}
			}
		}

		/// <summary>
		/// Освобождает все используемые ресурсы.
		/// </summary>
		public void Dispose ()
		{
			_socket.Dispose ();
		}
	}
}
