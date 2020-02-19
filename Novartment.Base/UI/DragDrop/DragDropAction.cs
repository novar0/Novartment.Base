namespace Novartment.Base.UI
{
	/// <summary>
	/// Specifies how and if a drag-and-drop operation should continue.
	/// </summary>
	/// <remarks>
	/// The value is returned when calling the IDropSource::QueryContinueDrag() COM interface.
	/// Analog of System.Windows.DragAction and System.Windows.Forms.DragAction.
	/// Created so that there is no binding to PresentationCore.dll or System.Windows.Forms.dll.
	/// </remarks>
	public enum DragDropAction : int
	{
		/// <summary>The operation will continue.</summary>
		/// <remarks>The S_OK Win32 constant.</remarks>
		Continue = 0,

		/// <summary>The operation will stop with a drop.</summary>
		/// <remarks>The DRAGDROP_S_DROP Win32 constant.</remarks>
		Drop = 262400,

		/// <summary>The operation is canceled with no drop message.</summary>
		/// <remarks>The DRAGDROP_S_CANCEL Win32 constant.</remarks>
		Cancel = 262401,
	}
}
