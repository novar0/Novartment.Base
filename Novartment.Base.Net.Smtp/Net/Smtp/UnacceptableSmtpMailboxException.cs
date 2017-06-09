using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Недопустимый почтовый ящик.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Usage",
		"CA2237:MarkISerializableTypesWithSerializable",
		Justification = "In portable projects this class would not be ISerializable")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with custom message not allowed.")]
	public class UnacceptableSmtpMailboxException : InvalidOperationException
	{
		/// <summary>Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException.</summary>
		public UnacceptableSmtpMailboxException ()
			: base ("Mailbox not allowed.")
		{
		}

		/// <summary>Инициализирует новый экземпляр класса UnacceptableSmtpMailboxException с указанным почтовым ящиком.</summary>
		/// <param name="mailbox">Почтовый ящик.</param>
		public UnacceptableSmtpMailboxException (AddrSpec mailbox)
			: base ("Mailbox " + MailboxToAngleString (mailbox) + " not allowed.")
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
