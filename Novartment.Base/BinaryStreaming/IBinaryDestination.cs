using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Получатель двоичных данных для последовательной записи.
	/// </summary>
	public interface IBinaryDestination
	{
		/// <summary>
		/// Асинхронно записывает в получатель указанный сегмент массива байтов.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="offset">Смещение байтов (начиная с нуля) в buffer, с которого начинается копирование байтов.</param>
		/// <param name="count">Число байтов для записи.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken);

		/// <summary>
		/// Указывает что запись окончена.
		/// </summary>
		void SetComplete ();
	}
}
