namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Importance of parameter Disposition-Notification-Options header field. Определено в RFC 3798.
	/// </summary>
	internal static class ParameterImportanceNames
	{
		/// <summary>
		/// An importance of "required" indicates that interpretation of the parameter is necessary for proper generation of an MDN in response to request.
		/// Определено в RFC 3798 section 2.2.
		/// </summary>
		internal static readonly string Required = "required";

		/// <summary>
		/// An importance of "optional" indicates that an MUA that does not understand the meaning of this parameter MAY generate an MDN in response anyway,
		/// ignoring the value of the parameter.
		/// Определено в RFC 3798 section 2.2.
		/// </summary>
		internal static readonly string Optional = "optional";
	}
}
