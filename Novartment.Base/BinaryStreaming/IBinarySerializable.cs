using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Сущность, пригодная для сохранения в получатель двоичных данных.
	/// </summary>
	public interface IBinarySerializable
	{
		/// <summary>
		/// Сохраняет сущность в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="destination">Получатель двоичных данных, в который будет сохранена сущность.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию сохранения.</returns>
		Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken);
	}
}