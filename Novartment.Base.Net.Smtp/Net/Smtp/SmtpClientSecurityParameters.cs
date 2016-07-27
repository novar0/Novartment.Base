using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Параметры безопасности клиента SMTP.
	/// </summary>
	public sealed class SmtpClientSecurityParameters
	{
		/// <summary>
		/// Получает признак использования шифрования.
		/// </summary>
		public bool EncryptionRequired { get; }

		/// <summary>
		/// Получает поставщик данных аутентификации клиента.
		/// </summary>
		public ICredentialsByHost ClientCredentials { get; }

		/// <summary>
		/// Получает коллекцию сертификатов клиента.
		/// </summary>
		public X509CertificateCollection ClientCertificates { get; }

		/// <summary>
		/// Получает параметры, не требующие использования механизмов безопасности.
		/// </summary>
		public static SmtpClientSecurityParameters AllowNoSecurity { get; } =
			new SmtpClientSecurityParameters (false,  null, null);

		/// <summary>
		/// Получает параметры, требующие использование шифрования.
		/// </summary>
		public static SmtpClientSecurityParameters RequireEncryption { get; } =
			new SmtpClientSecurityParameters (true, null, null);

		// приватный конструктор принуждает создавать экземпляры с помощью статических методов
		private SmtpClientSecurityParameters (
			bool requireEncryption,
			ICredentialsByHost credentials,
			X509CertificateCollection clientCertificates)
		{
			this.EncryptionRequired = requireEncryption;
			this.ClientCredentials = credentials;
			this.ClientCertificates = clientCertificates;
		}

		/// <summary>
		/// Получает параметры, требующие использование шифрования и идентификации клиента с помощью указанного набора сертификатов.
		/// </summary>
		/// <param name="clientCertificates">Сертификаты клиента, используемые при установке защищённого соединения.</param>
		/// <returns>Параметры безопасности клиента SMTP,
		/// требующие использование шифрования и идентификации клиента с помощью указанного набора сертификатов.</returns>
		public static SmtpClientSecurityParameters RequireEncryptionUseClientCertificate (X509CertificateCollection clientCertificates)
		{
			if (clientCertificates == null)
			{
				throw new ArgumentNullException (nameof (clientCertificates));
			}
			if (clientCertificates.Count < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (clientCertificates));
			}
			Contract.EndContractBlock ();

			return new SmtpClientSecurityParameters (true, null, clientCertificates);
		}

		/// <summary>
		/// Получает параметры,
		/// требующие использование шифрования с последующей аутентификацией указанным поставщиком данных аутентификации.
		/// </summary>
		/// <param name="credentials">Поставщк данных аутентификации.</param>
		/// <returns>Параметры безопасности клиента SMTP,
		/// требующие использование шифрования с последующей аутентификацией указанным поставщиком данных аутентификации.</returns>
		public static SmtpClientSecurityParameters RequireEncryptionUseCredentials (ICredentialsByHost credentials)
		{
			if (credentials == null)
			{
				throw new ArgumentNullException (nameof (credentials));
			}
			Contract.EndContractBlock ();

			return new SmtpClientSecurityParameters (true, credentials, null);
		}
	}
}
