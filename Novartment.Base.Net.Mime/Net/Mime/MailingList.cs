using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Список рассылки согласно RFC 2369 и RFC 2919.
	/// </summary>
	public class MailingList
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса MailingList.
		/// </summary>
		public MailingList ()
		{
		}

		/// <summary>Gets or sets mailing list identifier. "List-ID" field defined in RFC 2919.</summary>
		public string Id { get; set; }

		/// <summary>Gets or sets mailing list identifier description. "List-ID" field defined in RFC 2919.</summary>
		public string Description { get; set; }

		/// <summary>Gets or sets URL of mailing list archive. "List-Archive" field defined in RFC 2369.</summary>
		public IAdjustableList<string> ArchiveCommands { get; } = new ArrayList<string> ();

		/// <summary>URL for mailing list information. "List-Help" field defined in RFC 2369.</summary>
		public IAdjustableList<string> HelpCommands { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets URL for mailing list owner's mailbox. "List-Owner" field defined in RFC 2369.</summary>
		public IAdjustableList<string> OwnerCommands { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets URL for mailing list posting. "List-Post" field defined in RFC 2369.</summary>
		public IAdjustableList<string> PostCommands { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets URL for mailing list subscription. "List-Subscribe" field defined in RFC 2369.</summary>
		public IAdjustableList<string> SubscribeCommands { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets URL for mailing list unsubscription. "List-Unsubscribe" field defined in RFC 2369.</summary>
		public IAdjustableList<string> UnsubscribeCommands { get; } = new ArrayList<string> ();
	}
}