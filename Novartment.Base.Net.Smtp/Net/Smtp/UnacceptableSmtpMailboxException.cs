using System;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Недопустимый почтовый ящик.
	/// </summary>
	public sealed class UnacceptableSmtpMailboxException : InvalidOperationException
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException с указанным почтовым ящиком.
		/// </summary>
		/// <param name="mailbox">Почтовый ящик.</param>
		public UnacceptableSmtpMailboxException (AddrSpec mailbox)
			: base ("Mailbox not allowed.")
		{
			this.Mailbox = mailbox ?? throw new ArgumentNullException (nameof (mailbox));
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException с указанным сообщением и почтовым ящиком.
		/// </summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="mailbox">Почтовый ящик.</param>
		public UnacceptableSmtpMailboxException (string message, AddrSpec mailbox)
			: base (message)
		{
			this.Mailbox = mailbox ?? throw new ArgumentNullException (nameof (mailbox));
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException с указанным сообщением, почтовым ящиком
		/// и предшествующим исключением.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="mailbox">Почтовый ящик.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public UnacceptableSmtpMailboxException (string message, AddrSpec mailbox, Exception innerException)
			: base (message, innerException)
		{
			this.Mailbox = mailbox ?? throw new ArgumentNullException (nameof (mailbox));
		}

		/// <summary>Получает почтовый ящик, вызвавший исключение.</summary>
		public AddrSpec Mailbox { get; }
	}
}
