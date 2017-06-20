using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class SocketTcpListener :
		ITcpListener,
		IDisposable
	{
		private readonly IPEndPoint _localEP;
		private Socket _socket;
		private bool _active;

		/// <summary>
		/// Инициализирует новый экземпляр SocketTcpListener для прослушивания на указанной конечной точке
		/// и создающий объект-подключение используя указанную фабрику.
		/// </summary>
		/// <param name="localEP">Конечная точка, на которой будет осуществляться прослушивание.</param>
		public SocketTcpListener (IPEndPoint localEP)
		{
			if (localEP == null)
			{
				throw new ArgumentNullException (nameof (localEP));
			}

			Contract.EndContractBlock ();

			_localEP = localEP;
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
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1716:IdentifiersShouldNotMatchKeywords",
			MessageId = "Stop",
			Justification = "No other name could be applied because base System.Net.Sockets.TcpListener have method 'Stop()'.")]
		public void Stop ()
		{
			_socket.Dispose ();
			_active = false;
			_socket = new Socket (_localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Асинхронно получает подключение, полученное прослушивателем.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Установленное TCP-подключение.</returns>
		[SuppressMessage (
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "new TcpConnection will be returned and disposed outside.")]
		public Task<ITcpConnection> AcceptTcpClientAsync (CancellationToken cancellationToken)
		{
			if (!_active)
			{
				throw new InvalidOperationException ("stopped");
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
				return AcceptTcpClientAsyncFinalizer ();
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

			async Task<ITcpConnection> AcceptTcpClientAsyncFinalizer ()
			{
				try
				{
					var connectedSocket = await task.ConfigureAwait (false);
					return new SocketBinaryTcpConnection (connectedSocket, localHostFqdn, null);
				}
				catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
				{
					throw new TaskCanceledException (task);
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
		[SuppressMessage (
			"Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type.")]
		[SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			_socket.Dispose ();
		}
	}
}
