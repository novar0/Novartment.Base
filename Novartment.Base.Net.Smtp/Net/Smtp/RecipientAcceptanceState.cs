
namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Результат добавления получателя к списку получателей.
	/// </summary>
	public enum RecipientAcceptanceState : int
	{
		/// <summary>Получатель ещё не добавлялся.</summary>
		Unknown = 0,

		/// <summary>Получатель добавлен успешно.</summary>
		Success = 1,

		/// <summary>Получатель не добавлен из-за переполнения списка получателей.</summary>
		FailureTooManyRecipients = 2,

		/// <summary>Получатель не добавлен из-за его временной недоступности.</summary>
		FailureMailboxTemporarilyUnavailable = 3,

		/// <summary>Получатель не добавлен из-за его недоступности.</summary>
		FailureMailboxUnavailable = 4
	}
}
