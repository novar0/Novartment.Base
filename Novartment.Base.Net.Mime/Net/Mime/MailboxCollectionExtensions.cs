using System;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Методы расширения для коллекции Mailbox.
	/// </summary>
	public static class MailboxCollectionExtensions
	{
		/// <summary>
		/// Добавляет к коллекции почтовых ящиков указанный адрес с указанным именем.
		/// </summary>
		/// <param name="collection">Коллекция почтовых ящиков.</param>
		/// <param name="address">Строковое представление адреса почтового ящика.</param>
		/// <param name="displayName">Имя почтового ящика. Может быть не указано (значение null).</param>
		/// <returns>Созданный почтовый ящик.</returns>
		public static Mailbox Add (this IAdjustableCollection<Mailbox> collection, string address, string displayName = null)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			if (address == null)
			{
				throw new ArgumentNullException (nameof (address));
			}

			Contract.EndContractBlock ();

			var mailBox = new Mailbox (AddrSpec.Parse (address), displayName);
			collection.Add (mailBox);

			return mailBox;
		}

		/// <summary>
		/// Добавляет к коллекции почтовых ящиков указанный адрес с указанным именем.
		/// </summary>
		/// <param name="collection">Коллекция почтовых ящиков.</param>
		/// <param name="address">Адрес почтового ящика.</param>
		/// <param name="displayName">Имя почтового ящика. Может быть не указано (значение null).</param>
		/// <returns>Созданный почтовый ящик.</returns>
		public static Mailbox Add (this IAdjustableCollection<Mailbox> collection, AddrSpec address, string displayName = null)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			if (address == null)
			{
				throw new ArgumentNullException (nameof (address));
			}

			Contract.EndContractBlock ();

			var mailBox = new Mailbox (address, displayName);
			collection.Add (mailBox);

			return mailBox;
		}
	}
}
