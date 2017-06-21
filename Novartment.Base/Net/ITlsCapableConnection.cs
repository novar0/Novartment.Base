using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное TCP-подключение
	/// с возможностью перехода на уровень TLS
	/// и с отслеживанием полного времени и времени простоя.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Tls",
		Justification = "'TLS' represents standard term (Transport Layer Security).")]
	public interface ITlsCapableConnection :
		ITcpConnection
	{
		/// <summary>
		/// Called to authenticate the server and optionally the client in
		/// a client-server connection as an asynchronous operation.
		/// The authentication process uses the specified certificate collection and TLS protocol.
		/// </summary>
		/// <param name="clientCertificates">The X509CertificateCollection that contains client certificates.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Новое соединение, защищённое по протоколу TLS.</returns>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Tls",
			Justification = "'TLS' represents standard term (Transport Layer Security).")]
		Task<ITlsConnection> StartTlsClientAsync (X509CertificateCollection clientCertificates, CancellationToken cancellationToken);

		/// <summary>
		/// Called to authenticate the server and optionally the client in
		/// a client-server connection using the specified certificates,
		/// requirements and security protocol as an asynchronous operation.
		/// </summary>
		/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
		/// <param name="clientCertificateRequired">A Boolean value that specifies whether the client must supply a certificate for authentication.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Новое соединение, защищённое по протоколу TLS.</returns>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Tls",
			Justification = "'TLS' represents standard term (Transport Layer Security).")]
		Task<ITlsConnection> StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, CancellationToken cancellationToken);
	}
}
