
namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Action performed by the Reporting-MUA on behalf of the user.
	/// Определено в RFC 3798 part 3.2.6.1.
	/// </summary>
	internal static class MdnSendingModeNames
	{
		/// <summary>
		/// The user explicitly gave permission for this particular MDN to be sent.
		/// </summary>
		internal static readonly string MdnSentManually = "MDN-sent-manually";

		/// <summary>
		/// The MDN was sent because the MUA had previously been configured to do so automatically.
		/// </summary>
		internal static readonly string MdnSentAutomatically = "MDN-sent-automatically";
	}
}
