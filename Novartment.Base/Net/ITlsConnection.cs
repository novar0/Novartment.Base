using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное TLS-подключение,
	/// с отслеживанием полного времени и времени простоя.
	/// </summary>
	public interface ITlsConnection :
		ITcpConnection
	{
		/// <summary>
		/// Получает сертификат локальной стороны подключения. Null если сертификат не предоставлен.
		/// </summary>
		X509Certificate LocalCertificate { get; }

		/// <summary>
		/// Получает сертификат удалённой стороны подключения. Null если сертификат не предоставлен.
		/// </summary>
		X509Certificate RemoteCertificate { get; }

		/// <summary>
		/// Gets a value that indicates the security protocol used to authenticate this connection.
		/// </summary>
		SslProtocols TlsProtocol { get; }

		/// <summary>
		/// Gets a value that identifies the bulk encryption algorithm used by this connection.
		/// </summary>
		CipherAlgorithmType CipherAlgorithm { get; }

		/// <summary>
		/// Gets a value that identifies the strength of the cipher algorithm used by this SslStream.
		/// </summary>
		int CipherStrength { get; }

		/// <summary>
		/// Gets the algorithm used for generating message authentication codes (MACs).
		/// </summary>
		HashAlgorithmType HashAlgorithm { get; }

		/// <summary>
		/// Gets a value that identifies the strength of the hash algorithm used by this instance.
		/// </summary>
		int HashStrength { get; }

		/// <summary>
		/// Gets the key exchange algorithm used by this SslStream.
		/// </summary>
		ExchangeAlgorithmType KeyExchangeAlgorithm { get; }

		/// <summary>
		/// Gets a value that identifies the strength of the key exchange algorithm used by this instance.
		/// </summary>
		int KeyExchangeStrength { get; }
	}
}
