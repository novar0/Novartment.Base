using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Smtp
{
#pragma warning disable CA1032 // Implement standard exception constructors
	/// <summary>
	/// Недопустимый почтовый ящик.
	/// </summary>
	public class UnacceptableSmtpMailboxException : InvalidOperationException
#pragma warning restore CA1032 // Implement standard exception constructors
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException с указанным почтовым ящиком.
		/// </summary>
		/// <param name="mailbox">Почтовый ящик.</param>
		public UnacceptableSmtpMailboxException (AddrSpec mailbox)
			: base ("Mailbox not allowed.")
		{
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			this.Mailbox = mailbox;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException с указанным сообщением и почтовым ящиком.
		/// </summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="mailbox">Почтовый ящик.</param>
		public UnacceptableSmtpMailboxException (string message, AddrSpec mailbox)
			: base (message)
		{
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			this.Mailbox = mailbox;
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
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			this.Mailbox = mailbox;
		}

		/// <summary>Получает почтовый ящик, вызвавший исключение.</summary>
		public AddrSpec Mailbox { get; }

		private static string MailboxToAngleString (AddrSpec mailbox)
		{
			return mailbox?.ToAngleString () ?? "<null>";
		}
	}
}
