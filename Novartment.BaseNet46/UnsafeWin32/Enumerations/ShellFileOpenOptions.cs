using System;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Flags to control the behavior of the file dialog.
	/// </summary>
	/// <remarks>Соответсвтует перечислению FILEOPENDIALOGOPTIONS.</remarks>
	[Flags]
	internal enum ShellFileOpenOptions
	{
		/// <summary>When saving a file, prompt before overwriting an existing file of the same name. This is a default value for the Save dialog.</summary>
		/// <remarks>FOS_OVERWRITEPROMPT</remarks>
		OverwritePrompt = 0x00000002,

		/// <summary>In the save dialog, only allow the user to choose a file that has one of the file name extensions specified.</summary>
		/// <remarks>FOS_STRICTFILETYPES</remarks>
		StrictFileTypes = 0x00000004,

		/// <summary>Present the Open dialog offering a choice of folders rather than files.</summary>
		/// <remarks>FOS_PICKFOLDER</remarks>
		PickFolders = 0x00000020,

		/// <summary>Ensures that returned items are file system items.</summary>
		/// <remarks>FOS_FORCEFILESYSTEM</remarks>
		ForceFileSystem = 0x00000040, // Ensure that items returned are filesystem items.

		/// <summary>Enables the user to choose any item in the Shell namespace, not just those with STREAM or FILESYSTEM attributes.</summary>
		/// <remarks>FOS_ALLNONSTORAGEITEMS</remarks>
		AllNonStorageItems = 0x00000080, // Allow choosing items that have no storage.

		/// <summary>Do not check for situations that would prevent an application from opening the selected file, such as sharing violations or access denied errors.</summary>
		/// <remarks>FOS_NOVALIDATE</remarks>
		NoValidate = 0x00000100,

		/// <summary>Enables the user to select multiple items in the open dialog.</summary>
		/// <remarks>FOS_ALLOWMULTISELECT</remarks>
		AllowMultipleSelection = 0x00000200,

		/// <summary>The item returned must be in an existing folder. This is a default value.</summary>
		/// <remarks>FOS_PATHMUSTEXIST</remarks>
		PathMustExist = 0x00000800,

		/// <summary>The item returned must exist. This is a default value for the Open dialog.</summary>
		/// <remarks>FOS_FILEMUSTEXIST</remarks>
		FileMustExist = 0x00001000,

		/// <summary>Prompt for creation if the item returned in the save dialog does not exist. Note that this does not actually create the item.</summary>
		/// <remarks>FOS_CREATEPROMPT</remarks>
		CreatePrompt = 0x00002000,

		/// <summary>In the case of a sharing violation when an application is opening a file, call the application back through OnShareViolation for guidance.
		/// This flag is overridden by NoValidate.</summary>
		/// <remarks>FOS_SHAREAWARE</remarks>
		ShareAware = 0x00004000,

		/// <summary>Do not return read-only items. This is a default value for the Save dialog.</summary>
		/// <remarks>FOS_NOREADONLYRETURN</remarks>
		NoReadOnlyReturn = 0x00008000,

		/// <summary>Do not test creation of the item returned in the save dialog.
		/// If this flag is not set, the calling application must handle errors such as denial of access discovered in the creation test.</summary>
		/// <remarks>FOS_NOTESTFILECREATE</remarks>
		NoTestFileCreate = 0x00010000,

		/// <summary>Hide the list of places from which the user has recently opened or saved items.</summary>
		/// <remarks>FOS_HIDEMRUPLACES</remarks>
		HideMostRecentlyUsedPlaces = 0x00020000,

		/// <summary>Hide items shown by default in the view's navigation pane.</summary>
		/// <remarks>FOS_HIDEPINNEDPLACES</remarks>
		HidePinnedPlaces = 0x00040000,

		/// <summary>Shortcuts should not be treated as their target items, allowing an application to open a .lnk file.</summary>
		/// <remarks>FOS_NODEREFERENCELINKS</remarks>
		NoDereferenceLinks = 0x00100000,

		/// <summary>Do not add the item being opened or saved to the recent documents list.</summary>
		/// <remarks>FOS_DONTADDTORECENT</remarks>
		DoNotAddToRecent = 0x02000000,

		/// <summary>Show hidden and system items.</summary>
		/// <remarks>FOS_FORCESHOWHIDDEN</remarks>
		ForceShowHidden = 0x10000000,

		/// <summary>Indicates to the Save As dialog box that it should open in expanded mode.</summary>
		/// <remarks>FOS_DEFAULTNOMINIMODE</remarks>
		DefaultNoMiniMode = 0x20000000,

		/// <summary>Indicates to the Open dialog box that the preview pane should always be displayed.</summary>
		/// <remarks>FOS_FORCEPREVIEWPANEON</remarks>
		ForcePreviewPane = 0x40000000
	}
}
