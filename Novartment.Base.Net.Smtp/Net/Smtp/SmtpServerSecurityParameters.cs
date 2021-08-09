using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Параметры безопасности сервера SMTP.
	/// </summary>
	public sealed class SmtpServerSecurityParameters
	{
		// приватный конструктор принуждает создавать экземпляры с помощью статических методов
		private SmtpServerSecurityParameters (
			X509Certificate serverCertificate,
			bool clientCertificateRequired,
			Func<string, string, Task<object>> clientAuthenticator)
		{
			this.ServerCertificate = serverCertificate;
			this.ClientCertificateRequired = clientCertificateRequired;
			this.ClientAuthenticator = clientAuthenticator;
		}

		/// <summary>
		/// Получает параметры без использования механизмов безопасности.
		/// </summary>
		public static SmtpServerSecurityParameters NoSecurity { get; } = new SmtpServerSecurityParameters (null, false, null);

		/// <summary>
		/// Получает сертификат сервера. Null означает что шифрование не используется и сервер себя не идентифицирует.
		/// </summary>
		public X509Certificate ServerCertificate { get; }

		/// <summary>
		/// Получает признак обязательности предъявления сертификата клиентом.
		/// </summary>
		public bool ClientCertificateRequired { get; }

		/// <summary>
		/// Получает функцию, асинхронно аутентифицирующую подключающихся клиентов по указанному имени и паролю.
		/// Null означает что у клиентов не запрашивается аутентификация.
		/// </summary>
		public Func<string, string, Task<object>> ClientAuthenticator { get; }

		/// <summary>
		/// Получает параметры, требующие использование шифрования и идентификации сервера с помощью указанного сертификата.
		/// </summary>
		/// <param name="serverCertificate">Сертификат сервера, используемый для идентификации сервера и шифрования.</param>
		/// <returns>Параметры безопасности сервера SMTP,
		/// требующие использование шифрования и идентификации клиента с помощью указанного набора сертификатов.</returns>
		public static SmtpServerSecurityParameters UseServerCertificate (X509Certificate serverCertificate)
		{
			if (serverCertificate == null)
			{
				throw new ArgumentNullException (nameof (serverCertificate));
			}

			return new SmtpServerSecurityParameters (serverCertificate, false, null);
		}

		/// <summary>
		/// Получает параметры, требующие использование шифрования, идентификации сервера с помощью указанного сертификата
		/// и предъявления сертификата клиентом.
		/// </summary>
		/// <param name="serverCertificate">Сертификат сервера, используемый для идентификации сервера и шифрования.</param>
		/// <returns>Параметры безопасности сервера SMTP,
		/// требующие использование шифрования, идентификации сервера с помощью указанного сертификата
		/// и предъявления сертификата клиентом.</returns>
		public static SmtpServerSecurityParameters UseServerAndRequireClientCertificate (X509Certificate serverCertificate)
		{
			if (serverCertificate == null)
			{
				throw new ArgumentNullException (nameof (serverCertificate));
			}

			return new SmtpServerSecurityParameters (serverCertificate, true, null);
		}

		/// <summary>
		/// Получает параметры, требующие использование шифрования, идентификации сервера с помощью указанного сертификата
		/// и аутентификации клиента указанным поставщиком данных аутентификации.
		/// </summary>
		/// <param name="serverCertificate">Сертификат сервера, используемый для идентификации сервера и шифрования.</param>
		/// <param name="clientAuthenticator">Функция, асинхронно аутентифицирующая пользователя по указанному имени и паролю,
		/// например Microsoft.AspNet.Identity.UserManager.FindAsync().</param>
		/// <returns>Параметры безопасности сервера SMTP,
		/// требующие использование шифрования, идентификации сервера с помощью указанного сертификата
		/// и аутентификации клиента указанным поставщиком данных аутентификации.</returns>
		public static SmtpServerSecurityParameters UseServerCertificateAndClientAuthenticator (
			X509Certificate serverCertificate,
			Func<string, string, Task<object>> clientAuthenticator)
		{
			if (serverCertificate == null)
			{
				throw new ArgumentNullException (nameof (serverCertificate));
			}

			if (clientAuthenticator == null)
			{
				throw new ArgumentNullException (nameof (clientAuthenticator));
			}

			return new SmtpServerSecurityParameters (serverCertificate, false, clientAuthenticator);
		}
	}
}
