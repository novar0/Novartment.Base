using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// An established TCP connection with the ability to switch to the TLS layer
	/// and monitoring of idle time and total time.
	/// </summary>
	public interface ITlsCapableConnection :
		ITcpConnection
	{
		/// <summary>
		/// Called to authenticate the server and optionally the client in
		/// a client-server connection as an asynchronous operation.
		/// The authentication process uses the specified certificate collection and TLS protocol.
		/// </summary>
		/// <param name="clientCertificates">The X509CertificateCollection that contains client certificates.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the transition operation.
		/// The result value contains the new TLS-connection.
		/// </returns>
		Task<ITlsConnection> StartTlsClientAsync (X509CertificateCollection clientCertificates, CancellationToken cancellationToken = default);

		/// <summary>
		/// Called to authenticate the server and optionally the client in
		/// a client-server connection using the specified certificates,
		/// requirements and security protocol as an asynchronous operation.
		/// </summary>
		/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
		/// <param name="clientCertificateRequired">A Boolean value that specifies whether the client must supply a certificate for authentication.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the transition operation.
		/// The result value contains the new TLS-connection.
		/// </returns>
		Task<ITlsConnection> StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, CancellationToken cancellationToken = default);
	}
}
