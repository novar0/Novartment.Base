namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Результат попытки доставки сообщения адресату.
	/// Определено в RFC 3464 часть 2.3.
	/// </summary>
	public enum DeliveryAttemptResult
	{
		/// <summary>Результат не указан.</summary>
		Unspecified = 0,

		/// <summary>Сообщение не может быть доставлено получателю ("failed").</summary>
		Failed = 1,

		/// <summary>Почтовый агент пока что не смог доставить сообщение,
		/// но будет повторять попытки сделать это ("delayed").</summary>
		Delayed = 2,

		/// <summary>Сообщение было успешно доставлено адресату ("delivered").</summary>
		Delivered = 3,

		/// <summary>Сообщение было ретранслировано в окружение которое
		/// не несёт ответственности за создание уведомлений о статусе доставки сообщения ("relayed").</summary>
		Relayed = 4,

		/// <summary>Сообщение было успешно доставлено адресату и перенаправлено дальше
		/// множеству дополнительных адресатов ("expanded").</summary>
		Expanded = 5
	}
}
