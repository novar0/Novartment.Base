using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Specifies the effects of a drag-and-drop operation.
	/// This enumeration allows a bitwise combination of its member values.
	/// </summary>
	/// <remarks>
	/// Corresponds to Win32 constants DROPEFFECT_*.
	/// The value is passed in parameters when calling the IDropTarget and IDropSource COM interfaces.
	/// Analog of System.Windows.DragDropEffects and System.Windows.Forms.DragDropEffects.
	/// Created so that there is no binding to PresentationCore.dll or System.Windows.Forms.dll.
	/// </remarks>
	[Flags]
	public enum DragDropEffects : int
	{
		/// <summary>The drop target does not accept the data.</summary>
		/// <remarks>The DROPEFFECT_NONE Win32 constant.</remarks>
		None = 0,

		/// <summary>The data from the drag source is copied to the drop target.</summary>
		/// <remarks>The DROPEFFECT_COPY Win32 constant.</remarks>
		Copy = 1,

		/// <summary>The data from the drag source is moved to the drop target.</summary>
		/// <remarks>The DROPEFFECT_MOVE Win32 constant.</remarks>
		Move = 2,

		/// <summary>The data from the drag source is linked to the drop target.</summary>
		/// <remarks>The DROPEFFECT_LINK Win32 constant.</remarks>
		Link = 4,

		/// <summary>The target can be scrolled while dragging to locate a drop position that is not currently visible in the target.</summary>
		/// <remarks>The DROPEFFECT_SCROLL Win32 constant.</remarks>
		Scroll = -2147483648, // 0x80000000,

		/// <summary>The combination of the Copy, Move, and Scroll effects.</summary>
		All = -2147483641, // 0x80000007
	}
}
