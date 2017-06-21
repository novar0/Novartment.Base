using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Прослушиватель TCP-подключений.
	/// </summary>
	/// <remarks>
	/// Извлечён из библиотечного System.Net.Sockets.TcpListener. Отличия только в методе AcceptTcpClientAsync():
	/// 1. Для реализации отмены прослушивания принимает параметр CancellationToken.
	/// 2. Для реализации IoC возвращает ITcpConnection вместо библиотечного TcpClient.
	/// </remarks>
	public interface ITcpListener
	{
		/// <summary>
		/// Получает конечнную точку, на которой производится прослушивание.
		/// </summary>
		IPEndPoint LocalEndpoint { get; }

		/// <summary>
		/// Запускает прослушивание.
		/// </summary>
		void Start ();

		/// <summary>
		/// Останавливает прослушивание.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1716:IdentifiersShouldNotMatchKeywords",
			MessageId = "Stop",
			Justification = "No other name could be applied because base System.Net.Sockets.TcpListener have method 'Stop()'.")]
		void Stop ();

		/// <summary>
		/// Асинхронно получает подключение, полученное прослушивателем.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Установленное TCP-подключение.</returns>
		Task<ITcpConnection> AcceptTcpClientAsync (CancellationToken cancellationToken);
	}
}
