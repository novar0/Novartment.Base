using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

// RFC 5321 part 3.3:
// There are three steps to SMTP mail transactions.
// The transaction starts with a MAIL command that gives the sender identification.
// A series of one or more RCPT commands follows, giving the receiver information.
// Then, a DATA command initiates transfer of the mail data and is terminated by the "end of mail" data indicator,
// which also confirms the transaction.
namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Транзакция, представляющая собой передачу почтового сообщения.
	/// </summary>
	/// <remarks>
	/// Порядок вызова методов: один StartAsync(), потом несколько TryAddRecipientAsync() и в конце TransferDataAndFinishAsync().
	/// Транзакция будет отменена если любой метод вызовет исключение.
	/// При отмене транзакции никакая часть сообщения не будет доставлена ни одному из получателей.
	/// </remarks>
	public interface IMailDataTransferTransaction :
		IDisposable
	{
		/// <summary>
		/// Асинхронно начинает почтовую транзакцию используя указанный адрес возврата.
		/// </summary>
		/// <param name="returnPath">Mailbox, which can be used to report errors. Specify null if return-reporting not allowed.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Task, representing operation.</returns>
		/// <exception cref="Novartment.Base.Net.Smtp.UnacceptableSmtpMailboxException">
		/// Происходит если указанный returnPath не подходит для транзакции.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// Происходит если транзакция уже начата или если принимающая сторона не может начать транзакцию.
		/// </exception>
		Task StartAsync (AddrSpec returnPath, CancellationToken cancellationToken);

		/// <summary>
		/// Асинхронно добавляет указанного получателя в начатую почтовую транзакцию.
		/// </summary>
		/// <param name="recipient">Получатель почты, передаваемой в транзакции.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, представляющая собой добавление получателя. Результатом задачи будет одно из значений RecipientAcceptanceState,
		/// соответствующее успешности добавления получателя.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		/// Происходит если транзакция не начата.
		/// </exception>
		Task<RecipientAcceptanceState> TryAddRecipientAsync (AddrSpec recipient, CancellationToken cancellationToken);

		/// <summary>
		/// В начатой почтовой транзакции асинхронно передаёт указанные данные и завершает её.
		/// </summary>
		/// <param name="data">Источник данных, передаваемых в транзакции.</param>
		/// <param name="exactSize">Точный размер данных транзакции. Укажите отрицательное значение если размер заранее неизвестен.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая собой передачу данных и завершение транзакции.</returns>
		/// <exception cref="Novartment.Base.Net.Smtp.NoValidRecipientsException">
		/// Происходит если не указаны получатели транзакции.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// Происходит если транзакция не начата.
		/// </exception>
		Task TransferDataAndFinishAsync (IBufferedSource data, long exactSize, CancellationToken cancellationToken);
	}
}
