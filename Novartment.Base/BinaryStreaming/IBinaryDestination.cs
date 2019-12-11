using System;
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
		/// Асинхронно записывает в получатель указанный диапазон байтов.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

		/// <summary>
		/// Указывает что запись окончена.
		/// </summary>
		void SetComplete ();
	}
}
