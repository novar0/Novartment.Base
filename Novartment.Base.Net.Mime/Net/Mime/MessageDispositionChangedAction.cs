namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Действие, приведшее к изменению дислокации сообщения.
	/// Определено в RFC 3798 часть 3.2.6.
	/// </summary>
	public enum MessageDispositionChangedAction
	{
		/// <summary>Действие не указано.</summary>
		Unspecified = 0,

		/// <summary>
		/// Сообщение отображено по команде пользователя.
		/// Соответствует action-mode = "manual-action", disposition-type = "displayed".
		/// </summary>
		ManuallyDisplayed = 1,

		/// <summary>
		/// Сообщение удалено по команде пользователя.
		/// Соответствует action-mode = "manual-action", disposition-type = "deleted".
		/// </summary>
		ManuallyDeleted = 2,

		/// <summary>
		/// Сообщение отображено автоматически.
		/// Соответствует action-mode = "automatic-action", disposition-type = "displayed".
		/// </summary>
		AutomaticallyDisplayed = 3,

		/// <summary>Сообщение удалено автоматически.</summary>
		/// Соответствует action-mode = "automatic-action", disposition-type = "deleted".
		AutomaticallyDeleted = 4,
	}
}
