using System;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Неверная последовательность команд SMTP.
	/// </summary>
	public sealed class BadSequenceOfSmtpCommandsException : InvalidOperationException
	{
		/// <summary>Инициализирует новый экземпляр класса BadSequenceOfSmtpCommandsException.</summary>
		public BadSequenceOfSmtpCommandsException ()
			: base ("Bad sequence of SMTP commands.")
		{
		}

		/// <summary>Инициализирует новый экземпляр класса BadSequenceOfSmtpCommandsException.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		public BadSequenceOfSmtpCommandsException (string message)
			: base (message)
		{
		}

		/// <summary>Инициализирует новый экземпляр класса BadSequenceOfSmtpCommandsException.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public BadSequenceOfSmtpCommandsException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
