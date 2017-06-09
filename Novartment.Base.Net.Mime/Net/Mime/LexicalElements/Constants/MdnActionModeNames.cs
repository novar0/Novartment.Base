namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Action performed by the Reporting-MUA on behalf of the user.
	/// Определено в RFC 3798 part 3.2.6.1.
	/// </summary>
	internal static class MdnActionModeNames
	{
		/// <summary>
		/// The disposition described by the disposition type was a result of an explicit instruction by the user
		/// rather than some sort of automatically performed action.
		/// </summary>
		internal static readonly string ManualAction = "manual-action";

		/// <summary>
		/// The disposition described by the disposition type was a result of an automatic action,
		/// rather than an explicit instruction by the user for this message.
		/// </summary>
		internal static readonly string AutomaticAction = "automatic-action";
	}
}
