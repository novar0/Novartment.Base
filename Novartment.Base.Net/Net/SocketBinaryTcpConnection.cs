using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное TCP-подключение, отслеживающее полное время и время простоя
	/// с возможностью запуска TLS-подключения.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Tls",
		Justification = "'TLS' represents standard term (Transport Layer Security).")]
	public class SocketBinaryTcpConnection : BinaryTcpConnection,
		ITlsCapableConnection
	{
		private bool _authenticateAsServerClientCertificateRequired;

		/// <summary>
		/// Инициализирует новый экземпляр TlsCapableConnection на основе указанного подключенного сокета.
		/// </summary>
		/// <remarks>Created TlsCapableConnection will take ownership of the specified connectedSocket.</remarks>
		[SuppressMessage ("Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "new NetworkStream will be stored in property 'Reader' and disposed later.")]
		internal SocketBinaryTcpConnection (Socket connectedSocket, string localHostName, string remoteHostName)
			: base (
			new IPHostEndPoint ((IPEndPoint)connectedSocket.LocalEndPoint) { HostName = localHostName },
			new IPHostEndPoint ((IPEndPoint)connectedSocket.RemoteEndPoint) { HostName = remoteHostName },
			new SocketBufferedSource (connectedSocket, new byte[connectedSocket.ReceiveBufferSize]),
			new SocketBinaryDestination (connectedSocket))
		{
		}

		/// <summary>
		/// Called to authenticate the server and optionally the client in
		/// a client-server connection as an asynchronous operation.
		/// The authentication process uses the specified certificate collection and TLS protocol.
		/// </summary>
		/// <param name="clientCertificates">The X509CertificateCollection that contains client certificates.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Новое соединение, защищённое по протоколу TLS.</returns>
		public Task<ITlsConnection> StartTlsClientAsync (X509CertificateCollection clientCertificates, CancellationToken cancellationToken)
		{
			if (this.Writer is SslStream)
			{
				throw new InvalidOperationException ("TLS already started in this connection.");
			}
			if (this.RemoteEndPoint.HostName == null)
			{
				throw new InvalidOperationException ("Remote host name not specified. Host name required for starting TLS as client.");
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<ITlsConnection> (cancellationToken);
			}

			var insecureStream = new BufferedSourceBinaryDestinationStream (this.Reader, this.Writer);
			var secureStream = new SslStream (
				insecureStream,
				true,
				new RemoteCertificateValidationCallback (this.CheckAuthenticationAsClient),
				null,
				EncryptionPolicy.RequireEncryption);
			try
			{
				var task = secureStream.AuthenticateAsClientAsync (
					this.RemoteEndPoint.HostName,
					clientCertificates,
					SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, // TODO: get value from configuration
					true); // TODO: get value from configuration
				return StartTlsClientAsyncFinalizer (task, secureStream);
			}
			catch
			{
				secureStream.Dispose ();
				throw;
			}

		}

		private async Task<ITlsConnection> StartTlsClientAsyncFinalizer (Task task, SslStream stream)
		{
			try
			{
				await task.ConfigureAwait (false);
				var buf = new byte[this.Reader.Buffer.Length];
				return new SslStreamBinaryConnection (this.LocalEndPoint, this.RemoteEndPoint, stream, buf);
			}
			catch
			{
				stream.Dispose ();
				throw;
			}
		}

		/// <summary>
		/// Called to authenticate the server and optionally the client in
		/// a client-server connection using the specified certificates,
		/// requirements and security protocol as an asynchronous operation.
		/// </summary>
		/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
		/// <param name="clientCertificateRequired">A Boolean value that specifies whether the client must supply a certificate for authentication.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Новое соединение, защищённое по протоколу TLS.</returns>
		public Task<ITlsConnection> StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, CancellationToken cancellationToken)
		{
			if (serverCertificate == null)
			{
				throw new ArgumentNullException (nameof (serverCertificate));
			}
			Contract.EndContractBlock ();

			if (this.Writer is SslStream)
			{
				throw new InvalidOperationException ("TLS already started in this connection.");
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<ITlsConnection> (cancellationToken);
			}

			_authenticateAsServerClientCertificateRequired = clientCertificateRequired;
			var insecureStream = new BufferedSourceBinaryDestinationStream (this.Reader, this.Writer);
			var secureStream = new SslStream (
				insecureStream,
				true,
				new RemoteCertificateValidationCallback (this.CheckAuthenticationAsServer),
				null,
				EncryptionPolicy.RequireEncryption);
			try
			{
				var task = secureStream.AuthenticateAsServerAsync (
					serverCertificate,
					clientCertificateRequired,
					SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, // TODO: get value from configuration
					true); // TODO: get value from configuration
				return StartTlsServerAsyncFinalizer (task, secureStream);
			}
			catch
			{
				secureStream.Dispose ();
				throw;
			}
		}

		private async Task<ITlsConnection> StartTlsServerAsyncFinalizer (Task task, SslStream stream)
		{
			try
			{
				await task.ConfigureAwait (false);
				var buf = new byte[this.Reader.Buffer.Length];
				return new SslStreamBinaryConnection (this.LocalEndPoint, this.RemoteEndPoint, stream, buf);
			}
			catch
			{
				stream.Dispose ();
				throw;
			}
		}

		/// <summary>
		/// Проверяет параметры аутентификации как клиента.
		/// </summary>
		/// <param name="sender">Источник, запросивший проверку.</param>
		/// <param name="certificate">Сертификат сервера.</param>
		/// <param name="chain">Цепь построения сертификатов.</param>
		/// <param name="sslPolicyErrors">Ошибки соответствия политике сертификатов.</param>
		/// <returns>True если проверка пройдена успешно.</returns>
		protected virtual bool CheckAuthenticationAsClient (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return (sslPolicyErrors == SslPolicyErrors.None);
		}

		/// <summary>
		/// Проверяет параметры аутентификации как сервера.
		/// </summary>
		/// <param name="sender">Источник, запросивший проверку.</param>
		/// <param name="certificate">Сертификат клиента.</param>
		/// <param name="chain">Цепь построения сертификатов.</param>
		/// <param name="sslPolicyErrors">Ошибки соответствия политике сертификатов.</param>
		/// <returns>True если проверка пройдена успешно.</returns>
		protected virtual bool CheckAuthenticationAsServer (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return ((sslPolicyErrors == SslPolicyErrors.None) ||
					(!_authenticateAsServerClientCertificateRequired && (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)));
		}

		/// <summary>
		/// Асинхронно создаёт TCP-подключение к указанному URI. 
		/// </summary>
		/// <param name="remoteUri">URI куда будет произведено подключение.</param>
		/// <param name="addressFamily">Схема адресации для подключения.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой будет установленное TCP-подключение</returns>
		public static Task<ITcpConnection> CreateAsync (Uri remoteUri, AddressFamily addressFamily, CancellationToken cancellationToken)
		{
			if (remoteUri == null)
			{
				throw new ArgumentNullException (nameof (remoteUri));
			}
			var isNullOrWhiteSpace = string.IsNullOrWhiteSpace (remoteUri.Host);
			if (isNullOrWhiteSpace)
			{
				throw new ArgumentOutOfRangeException (nameof (remoteUri));
			}
			if (remoteUri.Port < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (remoteUri));
			}
			Contract.EndContractBlock ();

			var task = Dns.GetHostAddressesAsync (remoteUri.Host);
			return CreateAsyncFinalizer (task, remoteUri, addressFamily, cancellationToken);
		}

		private static async Task<ITcpConnection> CreateAsyncFinalizer (
			Task<IPAddress[]> task,
			Uri remoteUri,
			AddressFamily addressFamily,
			CancellationToken cancellationToken)
		{
			var addrs = await task.ConfigureAwait (false);
			IPAddress ipAddress = null;
			foreach (var addr in addrs)
			{
				if (addr.AddressFamily == addressFamily)
				{
					ipAddress = addr;
					break;
				}
			}
			if (ipAddress == null)
			{
				throw new InvalidOperationException (FormattableString.Invariant (
					$"Host {remoteUri.Host} does not have any IP-addresses of {addressFamily} family."));
			}

			var remoteEndpoint = new IPHostEndPoint (ipAddress, remoteUri.Port)
			{
				HostName = remoteUri.Host
			};
			return await CreateAsync (remoteEndpoint, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Асинхронно создаёт TCP-подключение к указанной конечной точке. 
		/// </summary>
		/// <param name="remoteEndpoint">Конечная точка, к которой будет создано подключение.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой будет установленное TCP-подключение</returns>
		[SuppressMessage ("Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "new TcpConnection will be returned and disposed outside.")]
		public static Task<ITcpConnection> CreateAsync (IPHostEndPoint remoteEndpoint,CancellationToken cancellationToken)
		{
			if (remoteEndpoint == null)
			{
				throw new ArgumentNullException (nameof (remoteEndpoint));
			}
			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<ITcpConnection> (cancellationToken);
			}

			var socket = new Socket (remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			IDisposable tokenRegistration = null;
			if (cancellationToken.CanBeCanceled)
			{
				tokenRegistration = cancellationToken.Register (socket.Dispose, false);
			}
			var ipProps = IPGlobalProperties.GetIPGlobalProperties ();
			var localHostFqdn = string.IsNullOrWhiteSpace (ipProps.DomainName) ?
				ipProps.HostName :
				ipProps.HostName + "." + ipProps.DomainName;
			try
			{
				var task = socket.ConnectAsync (remoteEndpoint);
				return CreateAsyncFializer (task, localHostFqdn, remoteEndpoint.HostName, socket, tokenRegistration, cancellationToken);
			}
			catch (ObjectDisposedException)
			{
				tokenRegistration?.Dispose ();
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<ITcpConnection> (cancellationToken);
				}
				throw;
			}
		}

		private static async Task<ITcpConnection> CreateAsyncFializer (Task task,
			string localHostFqdn,
			string remoteHostName,
			Socket socket,
			IDisposable disposable,
			CancellationToken cancellationToken)
		{
			try
			{
				await task.ConfigureAwait (false);
				return new SocketBinaryTcpConnection (socket, localHostFqdn, remoteHostName);
			}
			catch (ObjectDisposedException)
			{
				disposable?.Dispose ();
				cancellationToken.ThrowIfCancellationRequested ();
				throw;
			}
			finally
			{
				disposable?.Dispose ();
			}
		}
	}
}
