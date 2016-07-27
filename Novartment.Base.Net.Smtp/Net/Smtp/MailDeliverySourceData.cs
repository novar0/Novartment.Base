using System.Security.Cryptography.X509Certificates;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Данные об источнике отправляющем почту.
	/// </summary>
	public class MailDeliverySourceData
	{
		/// <summary>
		/// Получает конечную точку подключения источника.
		/// </summary>
		public IPHostEndPoint EndPoint { get; }

		/// <summary>
		/// Получает сертификат источника, либо null если сертификат не предоставлен.
		/// </summary>
		public X509Certificate Certificate { get; }

		/// <summary>
		/// Получает объект, представляющий аутентифицированного пользователя источника, либо null если пользователь не аутентифицирован.
		/// </summary>
		public object AuthenticatedUser { get; }

		/// <summary>
		/// Инициализирует новый экземпляр MailDeliverySourceData, содержащий указанную конечную точку подключения, сертификат и пользователя.
		/// </summary>
		/// <param name="endPoint">Конечная точка подключения источника.</param>
		/// <param name="certificate">Сертификат источника, либо null если сертификат не предоставлен.</param>
		/// <param name="authenticatedUser">Объект, представляющий аутентифицированного пользователя источника,
		///  либо null если пользователь не аутентифицирован.</param>
		public MailDeliverySourceData (IPHostEndPoint endPoint, X509Certificate certificate, object authenticatedUser)
		{
			this.EndPoint = endPoint;
			this.Certificate = certificate;
			this.AuthenticatedUser = authenticatedUser;
		}
	}
}
