using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности согласно RFC 2045.
	/// </summary>
	public interface IEntityBody :
		IBinarySerializable
	{
		/// <summary>Получает кодировку передачи содержимого тела сущности.</summary>
		ContentTransferEncoding TransferEncoding { get; }

		/// <summary>
		/// Очищает тело сущности.
		/// </summary>
		void Clear ();

		/// <summary>
		/// Загружает тело сущности из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных, содержащий тело сущности.</param>
		/// <param name="subBodyFactory">Фабрика, позволяющая создавать тело вложенных сущностей с указанными параметрами.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию загрузки.</returns>
		Task LoadAsync (
			IBufferedSource source,
			Func<EssentialContentProperties, IEntityBody> subBodyFactory,
			CancellationToken cancellationToken = default);
	}
}
