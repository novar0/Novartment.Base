using System;
using System.Collections.Generic;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Почтовое сообщение.
	/// </summary>
	/// <typeparam name="TMailbox">Тип почтовых ящиков, указываемых в параметрах сообщения.</typeparam>
	public interface IMailMessage<out TMailbox> :
		IBinarySerializable
	{
		/// <summary>Получает дату формирования сообщения.</summary>
		DateTimeOffset? OriginationDate { get; }

		/// <summary>Получает тему сообщения.</summary>
		string Subject { get; }

		/// <summary>Получает коллекцию почтовых ящиков авторов сообщения.</summary>
		IReadOnlyList<TMailbox> Originators { get; }

		/// <summary>Получает коллекцию почтовых ящиков получателей сообщения.</summary>
		IReadOnlyList<TMailbox> Recipients { get; }

		/// <summary>Получает кодировку передачи содержимого, требуемую для передачи сообщения.</summary>
		ContentTransferEncoding TransferEncoding { get; }
	}
}
