using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных для последовательного чтения, представленный байтовым буфером и
	/// поддерживающий быстрый пропуск данных.
	/// </summary>
	public interface IFastSkipBufferedSource :
		IBufferedSource
	{
		/// <summary>
		/// Пытается асинхронно пропустить указанное количество данных источника, включая доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Количество байтов данных для пропуска, включая доступные в буфере данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, результатом которой является количество пропущеных байтов данных, включая доступные в буфере данные.
		/// Может быть меньше, чем было указано, если источник исчерпался.
		/// После завершения задачи, независимо от её результата, источник будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		Task<long> TryFastSkipAsync (long size, CancellationToken cancellationToken = default);
	}
}
