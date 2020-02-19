using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Specifies the current state of the modifier keys (SHIFT, CTRL, and ALT), as well as the state of the mouse buttons.
	/// This enumeration allows a bitwise combination of its member values.
	/// </summary>
	/// <remarks>
	/// The value is passed in parameters when calling the IDropTarget and IDropSource COM interfaces.
	/// Analog of System.Windows.DragDropKeyStates.
	/// Created so that there is no binding to PresentationCore.dll.
	/// </remarks>
	[Flags]
	public enum DragDropKeyStates : int
	{
		/// <summary>No modifier keys or mouse buttons are pressed.</summary>
		None = 0,

		/// <summary>The left mouse button is pressed.</summary>
		/// <remarks>The MK_LBUTTON Win32 constant.</remarks>
		LeftMouseButton = 1,

		/// <summary>The right mouse button is pressed.</summary>
		/// <remarks>The MK_RBUTTON Win32 constant.</remarks>
		RightMouseButton = 2,

		/// <summary>The shift (SHIFT) key is pressed.</summary>
		/// <remarks>The MK_SHIFT Win32 constant.</remarks>
		ShiftKey = 4,

		/// <summary>The control (CTRL) key is pressed.</summary>
		/// <remarks>The MK_CONTROL Win32 constant.</remarks>
		ControlKey = 8,

		/// <summary>The middle mouse button is pressed.</summary>
		/// <remarks>The MK_MBUTTON Win32 constant.</remarks>
		MiddleMouseButton = 16,

		/// <summary>The ALT key is pressed.</summary>
		/// <remarks>The MK_ALT Win32 constant.</remarks>
		AltKey = 32,
	}
}
