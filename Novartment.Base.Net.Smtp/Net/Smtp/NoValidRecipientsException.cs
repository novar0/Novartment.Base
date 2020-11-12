using System;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Отсутствуют подтверждённые получатели.
	/// </summary>
	public sealed class NoValidRecipientsException : InvalidOperationException
	{
		/// <summary>Инициализирует новый экземпляр класса NoValidRecipientsException.</summary>
		public NoValidRecipientsException ()
			: base ("No valid recipients.")
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса NoValidRecipientsException.
		/// </summary>
		/// <param name="message">Сообщение ошибки.</param>
		public NoValidRecipientsException (string message)
			: base (message)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса NoValidRecipientsException.
		/// </summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public NoValidRecipientsException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
