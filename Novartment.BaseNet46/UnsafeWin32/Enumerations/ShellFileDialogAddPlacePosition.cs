namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Specifies where the item is placed within the list.
	/// </summary>
	/// <remarks>Соответствует перечислению FDAP.</remarks>
	internal enum ShellFileDialogAddPlacePosition
	{
		/// <summary>The place is added to the bottom of the default list.</summary>
		Bottom = 0x00000000,

		/// <summary>The place is added to the top of the default list.</summary>
		Top = 0x00000001,
	}
}
