using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных для последовательного чтения, представленный байтовым буфером,
	/// представляющий собой отдельные части.
	/// Предоставляет данные в пределах одной части и возможность перейти на следуюущую часть.
	/// </summary>
	/// <remarks>
	/// Меняется семантика наследованного свойства IsExhausted.
	/// Теперь оно означает исчерпание одной части,
	/// что не исключает перехода на следующую.
	/// </remarks>
	public interface IPartitionedBufferedSource :
		IBufferedSource
	{
		/// <summary>
		/// Пытается асинхронно пропустить все данные источника, принадлежащие текущей части,
		/// чтобы стали доступны данные следующей части.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является
		/// True если источник переключился на следующую часть,
		/// либо False если источник исчерпался.
		/// </returns>
		Task<bool> TrySkipPartAsync (CancellationToken cancellationToken = default);
	}
}
