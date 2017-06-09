namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тип значения поля уведомления.
	/// Определено в RFC 3464 часть 2.1.2.
	/// </summary>
	internal static class NotificationFieldValueTypeNames
	{
		/// <summary>Mailbox address.</summary>
		internal static readonly string Rfc822 = "rfc822";

		/// <summary>Status code.</summary>
		internal static readonly string Smtp = "smtp";

		/// <summary>Name of the host.</summary>
		internal static readonly string Dns = "dns";
	}
}
