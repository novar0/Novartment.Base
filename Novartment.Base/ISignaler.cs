using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base
{
	/// <summary>
	/// Простой сигнализатор для подачи сигналов между компонентами.
	/// </summary>
	public interface ISignaler
	{
		/// <summary>Ожидание приёма сигнала.</summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены ожидания.</param>
		/// <returns>Задача, представляющая собой процесс ожидания.</returns>
		Task WaitForSignalAsync (CancellationToken cancellationToken);

		/// <summary>Посылка сигнала.</summary>
		/// <param name="millisecondsTimeout">Максимальное время (в миллисекундах) отведённое на отсылку сигнала.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены ожидания.</param>
		Task SendSignalAsync (int millisecondsTimeout, CancellationToken cancellationToken);
	}
}
