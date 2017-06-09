namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тип значения поля уведомления.
	/// Определено в RFC 3464 часть 2.1.2.
	/// </summary>
	public enum NotificationFieldValueKind
	{
		/// <summary>Тип не указан.</summary>
		Unspecified = 0,

		/// <summary>Mailbox address.</summary>
		Mailbox = 1,

		/// <summary>Status code.</summary>
		Status = 2,

		/// <summary>Name of the host.</summary>
		Host = 3
	}
}
