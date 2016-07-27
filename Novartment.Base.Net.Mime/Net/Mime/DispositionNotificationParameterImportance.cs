
namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Важность параметра сообщения, определяющего доставку уведомления об изменении его дислокации у получателя.
	/// Определено в RFC 3798.
	/// </summary>
	public enum DispositionNotificationParameterImportance
	{
		/// <summary>Важность не указана.</summary>
		Unspecified = 0,

		/// <summary>
		/// Требуемый.
		/// Интерпретация параметра необходима для правильной генерации уведомления.
		/// Определено в RFC 3798 section 2.2 ("required").
		/// </summary>
		Required = 1,

		/// <summary>
		/// Необязательный.
		/// Почтовый агент пользователя может генерировать уведомление даже если
		/// не понимает этого параметра, игнорируя его значение.
		/// Определено в RFC 3798 section 2.2 ("optional").
		/// </summary>
		Optional = 2,
	}
}
