
namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Action performed by the Reporting-MUA on behalf of the user.
	/// Определено в RFC 3798 part 3.2.6.2.
	/// </summary>
	internal static class MdnDispositionTypeNames
	{
		/// <summary>
		/// The message has been displayed by the MUA to someone reading the recipient's mailbox.
		/// There is no guarantee that the content has been read or understood.
		/// </summary>
		internal static readonly string Displayed = "displayed";

		/// <summary>
		/// The message has been deleted. The recipient may or may not have seen the message.
		/// The recipient might "undelete" the message at a later time and read the message.
		/// </summary>
		internal static readonly string Deleted = "deleted";
	}
}
