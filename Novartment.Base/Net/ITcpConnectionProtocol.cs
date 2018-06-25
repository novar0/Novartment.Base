using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Протокол, осуществляющий обработку потокового подклюючения.
	/// </summary>
	public interface ITcpConnectionProtocol
	{
		/// <summary>
		/// Запускает обработку указанного подключения.
		/// </summary>
		/// <param name="connection">Потоковое подключение.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая обработку подключения.</returns>
		/// <exception cref="Novartment.Base.Net.UnrecoverableProtocolException">
		/// Происходит когда в протоколе возникла неустранимое противоречие, делающее его дальнейшую работу невозможным.
		/// Настоятельно рекомендуется закрыть соединение.
		/// </exception>
		Task StartAsync (ITcpConnection connection, CancellationToken cancellationToken = default);
	}
}
