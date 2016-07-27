
namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Response to the sharing violation.
	/// </summary>
	/// <remarks>Соответствует перечислению FDE_SHAREVIOLATION_RESPONSE.</remarks>
	internal enum ShellFileDialogEventResponse
	{
		/// <summary>
		/// The application has not handled the event.
		/// The dialog displays a UI that indicates that the file is in use and a different file must be chosen.
		/// </summary>
		Default = 0x00000000,

		/// <summary>
		/// The application has determined that the file should be returned from the dialog.
		/// </summary>
		Accept = 0x00000001,

		/// <summary>
		/// The application has determined that the file should not be returned from the dialog.
		/// </summary>
		Refuse = 0x00000002
	}
}
