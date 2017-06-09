namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// MIME content disposition type names. Определено в RFC 2183.
	/// </summary>
	internal static class DispositionTypeNames
	{
		/// <summary>
		/// A bodypart should be marked `inline' if it is intended to be displayed automatically upon display of the message.
		/// Inline bodyparts should be presented in the order in which they occur, subject to the normal semantics of multipart messages.
		/// </summary>
		internal static readonly string Inline = "inline";

		/// <summary>
		/// Bodyparts can be designated `attachment' to indicate that they are separate from the main body of the mail message,
		/// and that their display should not be automatic, but contingent upon some further action of the user.
		/// </summary>
		internal static readonly string Attachment = "attachment";

		/// <summary>
		/// Bodyparts represents fields to be supplied by the user who fills out the form. Определено в RFC 2388.
		/// </summary>
		internal static readonly string FormData = "form-data";
	}
}
