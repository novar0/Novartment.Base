using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
	/// <summary>
	/// Установленное защищённое TCP-подключение на основе сокетов с отслеживанием полного времени и времени простоя.
	/// </summary>
	internal sealed class SslStreamBinaryConnection : BinaryTcpConnection,
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
		ITlsConnection
	{
		private readonly SslStream _secureStream;

		public SslStreamBinaryConnection (IPHostEndPoint localEndPoint, IPHostEndPoint remoteEndPoint, SslStream secureStream, byte[] buffer)
			: base (
				localEndPoint,
				remoteEndPoint,
				GetBufferedSource (secureStream, buffer),
				GetBinaryDestination (secureStream))
		{
			_secureStream = secureStream;
		}

		/// <summary>
		/// Получает сертификат, представляющий клиента. Null если сертификат не предоставлен.
		/// </summary>
		public X509Certificate LocalCertificate => _secureStream.LocalCertificate;

		/// <summary>
		/// Получает сертификат, представляющий сервер. Null если сертификат не предоставлен.
		/// </summary>
		public X509Certificate RemoteCertificate => _secureStream.RemoteCertificate;

		/// <summary>
		/// Gets a value that indicates the security protocol used to authenticate this connection.
		/// </summary>
		public SslProtocols TlsProtocol => _secureStream.SslProtocol;

		/// <summary>
		/// Gets a value that identifies the bulk encryption algorithm used by this connection.
		/// </summary>
		public CipherAlgorithmType CipherAlgorithm => _secureStream.CipherAlgorithm;

		/// <summary>
		/// Gets a value that identifies the strength of the cipher algorithm used by this SslStream.
		/// </summary>
		public int CipherStrength => _secureStream.CipherStrength;

		/// <summary>
		/// Gets the algorithm used for generating message authentication codes (MACs).
		/// </summary>
		public HashAlgorithmType HashAlgorithm => _secureStream.HashAlgorithm;

		/// <summary>
		/// Gets a value that identifies the strength of the hash algorithm used by this instance.
		/// </summary>
		public int HashStrength => _secureStream.HashStrength;

		/// <summary>
		/// Gets the key exchange algorithm used by this SslStream.
		/// </summary>
		public ExchangeAlgorithmType KeyExchangeAlgorithm => _secureStream.KeyExchangeAlgorithm;

		/// <summary>
		/// Gets a value that identifies the strength of the key exchange algorithm used by this instance.
		/// </summary>
		public int KeyExchangeStrength => _secureStream.KeyExchangeStrength;

		private static IBufferedSource GetBufferedSource (SslStream stream, byte[] buffer)
		{
			if (stream == null)
			{
				throw new ArgumentNullException (nameof (stream));
			}

			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			return BinaryStreamingStreamExtensions.AsBufferedSource (stream, buffer);
		}

		private static IBinaryDestination GetBinaryDestination (SslStream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException (nameof (stream));
			}

			return BinaryStreamingStreamExtensions.AsBinaryDestination (stream);
		}
	}
}
