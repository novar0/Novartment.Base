namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Form of a shell item's display name.
	/// </summary>
	/// <remarks>Соответствует перечислению SIGDN.</remarks>
	internal enum ShellItemDisplayNameForm
	{
		/// <summary>Returns the display name relative to the parent folder.</summary>
		/// <remarks>SIGDN_NORMALDISPLAY = 0 + SIGDN_NORMAL</remarks>
		Normal = 0x00000000,

#pragma warning disable SA1139 // Use literal suffix notation instead of casting

		/// <summary>Returns the parsing name relative to the parent folder.</summary>
		/// <remarks>SIGDN_PARENTRELATIVEPARSING = 1 + SIGDN_INFOLDER | SIGDN_FORPARSING</remarks>
		ParentRelativeParsing = unchecked ((int)0x80018001),

		/// <summary>Returns the parsing name relative to the desktop.</summary>
		/// <remarks>SIGDN_DESKTOPABSOLUTEPARSING = 2 + SIGDN_FORPARSING</remarks>
		DesktopAbsoluteParsing = unchecked ((int)0x80028000),

		/// <summary>Returns the editing name relative to the parent folder.</summary>
		/// <remarks>SIGDN_PARENTRELATIVEEDITING = 3 + SIGDN_INFOLDER | SIGDN_FOREDITING</remarks>
		ParentRelativeEditing = unchecked ((int)0x80031001),

		/// <summary>Returns the editing name relative to the desktop.</summary>
		/// <remarks>SIGDN_DESKTOPABSOLUTEEDITING = 4 + SIGDN_FORPARSING | SIGDN_FORADDRESSBAR</remarks>
		DesktopAbsoluteEditing = unchecked ((int)0x8004c000),

		/// <summary>Returns the item's file system path, if it has one.</summary>
		/// <remarks>SIGDN_FILESYSPATH = 5 + SIGDN_FORPARSING</remarks>
		FileSystemPath = unchecked ((int)0x80058000),

		/// <summary>Returns the item's URL, if it has one.</summary>
		/// <remarks>SIGDN_URL = 6 + SIGDN_FORPARSING</remarks>
		Url = unchecked ((int)0x80068000),

		/// <summary>Returns the path relative to the parent folder in a friendly format as displayed in an address bar.
		/// This name is suitable for display to the user.</summary>
		/// <remarks>SIGDN_PARENTRELATIVEFORADDRESSBAR = 7 + SIGDN_INFOLDER | SIGDN_FORPARSING | SIGDN_FORADDRESSBAR</remarks>
		ParentRelativeForAddressBar = unchecked ((int)0x8007c001),

		/// <summary>Returns the path relative to the parent folder.</summary>
		/// <remarks>SIGDN_PARENTRELATIVE = 8 + SIGDN_INFOLDER</remarks>
		ParentRelative = unchecked ((int)0x80080001),

		/// <summary>Returns the path relative to the parent folder suitable for display to the user.</summary>
		/// <remarks>SIGDN_PARENTRELATIVEFORU = 9 + SIGDN_INFOLDER | SHGDN_FORADDRESSBAR</remarks>
		ParentRelativeForUI = unchecked ((int)0x80094001),

#pragma warning restore SA1139 // Use literal suffix notation instead of casting
	}
}
