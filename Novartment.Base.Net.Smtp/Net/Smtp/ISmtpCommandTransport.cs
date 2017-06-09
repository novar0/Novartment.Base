using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Транспорт, доставляющий SmtpCommand-ы и ответы на них
	/// с возможностью организации безопасной доставки.
	/// </summary>
	internal interface ISmtpCommandTransport
	{
		/// <summary>
		/// Получает признак использования безопасной доставки.
		/// </summary>
		bool TlsEstablished { get; }

		/// <summary>
		/// Получает сертификат пункта доставки.
		/// </summary>
		X509Certificate RemoteCertificate { get; }

		/// <summary>
		/// Получает команду с указанием ожидаемого режима.
		/// </summary>
		/// <param name="expectedInputType">Режим ожидаемых команд.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task<SmtpCommand> ReceiveCommandAsync (SmtpCommand.ExpectedInputType expectedInputType, CancellationToken cancellationToken);

		/// <summary>
		/// Получает ответ на команду.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task<SmtpReply> ReceiveReplyAsync (CancellationToken cancellationToken);

		/// <summary>
		/// Посылает команду.
		/// </summary>
		/// <param name="command">Команда для отправки.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task SendCommandAsync (SmtpCommand command, CancellationToken cancellationToken);

		/// <summary>
		/// Посылает ответ на команду.
		/// </summary>
		/// <param name="reply">Ответ для отправки.</param>
		/// <param name="canBeGrouped">Признак возможности группировки с предыдущей командой.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task SendReplyAsync (SmtpReply reply, bool canBeGrouped, CancellationToken cancellationToken);

		/// <summary>
		/// Посылает двоичные данные. Используется после команды отправки двоичных данных.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task SendBinaryAsync (IBufferedSource source, CancellationToken cancellationToken);

		/// <summary>
		/// Запускает режим безопасной доставки в режиме клиента.
		/// </summary>
		/// <param name="clientCertificates">Сертификат клиента.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task StartTlsClientAsync (X509CertificateCollection clientCertificates);

		/// <summary>
		/// Запускает режим безопасной доставки в режиме сервера.
		/// </summary>
		/// <param name="serverCertificate">Сертификат сервера.</param>
		/// <param name="clientCertificateRequired">Признак обязательности клиентского сертификата.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		Task StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired);
	}
}
