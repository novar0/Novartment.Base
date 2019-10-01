using System;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Attributes that can be retrieved on an shell item (file or folder) or set of shell items.
	/// </summary>
	/// <remarks>Соответствует перечислению SFGAO.</remarks>
	[Flags]
	internal enum ShellItemAttributes
	{
		/// <summary>
		/// The specified items can be copied.
		/// </summary>
		CanCopy = 0x00000001,

		/// <summary>
		/// The specified items can be moved.
		/// </summary>
		CanMove = 0x00000002,

		/// <summary>
		/// Shortcuts can be created for the specified items.
		/// </summary>
		CanLink = 0x00000004,

		/// <summary>
		/// The specified items can be bound to an IStorage interface.
		/// </summary>
		Storage = 0x00000008,

		/// <summary>
		/// The specified items can be renamed.
		/// </summary>
		CanRename = 0x00000010,

		/// <summary>
		/// The specified items can be deleted.
		/// </summary>
		CanDelete = 0x00000020,

		/// <summary>
		/// The specified items have property sheets.
		/// </summary>
		HasPropertySheet = 0x00000040,

		/// <summary>
		/// The specified items are drop targets.
		/// </summary>
		DropTarget = 0x00000100,

		/// <summary>
		/// This flag is a mask for the capability flags.
		/// </summary>
		CapabilityMask = 0x00000177,

		/// <summary>
		/// Windows 7 and later. The specified items are system items.
		/// </summary>
		System = 0x00001000,

		/// <summary>
		/// The specified items are encrypted.
		/// </summary>
		Encrypted = 0x00002000,

		/// <summary>
		/// Indicates that accessing the object through IStream or other storage interfaces,
		/// is a slow operation.
		/// </summary>
		IsSlow = 0x00004000,

		/// <summary>
		/// The specified items are ghosted icons.
		/// </summary>
		Ghosted = 0x00008000,

		/// <summary>
		/// The specified items are shortcuts.
		/// </summary>
		Link = 0x00010000,

		/// <summary>
		/// The specified folder objects are shared.
		/// </summary>
		Share = 0x00020000,

		/// <summary>
		/// The specified items are read-only. In the case of folders, this means
		/// that new items cannot be created in those folders.
		/// </summary>
		ReadOnly = 0x00040000,

		/// <summary>
		/// The item is hidden and should not be displayed unless the
		/// Show hidden files and folders option is enabled.
		/// </summary>
		Hidden = 0x00080000,

		/// <summary>
		/// This flag is a mask for the display attributes.
		/// </summary>
		DisplayAttributeMask = 0x000FC000,

		/// <summary>
		/// The specified folders contain one or more file system folders.
		/// </summary>
		FileSystemAncestor = 0x10000000,

		/// <summary>
		/// The specified items are folders.
		/// </summary>
		Folder = 0x20000000,

		/// <summary>
		/// The specified folders or file objects are part of the file system
		/// that is, they are files, directories, or root directories.
		/// </summary>
		FileSystem = 0x40000000,

#pragma warning disable SA1139 // Use literal suffix notation instead of casting

		/// <summary>
		/// The specified folders have subfolders.
		/// </summary>
		HasSubFolder = unchecked ((int)0x80000000),

		/// <summary>
		/// This flag is a mask for the contents attributes.
		/// </summary>
		ContentsMask = unchecked ((int)0x80000000),

#pragma warning restore SA1139 // Use literal suffix notation instead of casting

		/// <summary>
		/// When specified as input, instructs the folder to validate that the items exist.
		/// When used with the file system folder, instructs the folder to discard cached
		/// properties that may have accumulated for the specified items.
		/// </summary>
		Validate = 0x01000000,

		/// <summary>
		/// The specified items are on removable media or are themselves removable devices.
		/// </summary>
		Removable = 0x02000000,

		/// <summary>
		/// The specified items are compressed.
		/// </summary>
		Compressed = 0x04000000,

		/// <summary>
		/// The specified items can be browsed in place.
		/// </summary>
		Browsable = 0x08000000,

		/// <summary>
		/// The items are nonenumerated items.
		/// </summary>
		Nonenumerated = 0x00100000,

		/// <summary>
		/// The objects contain new content.
		/// </summary>
		NewContent = 0x00200000,

		/// <summary>
		/// It is possible to create monikers for the specified file objects or folders.
		/// </summary>
		CanMoniker = 0x00400000,

		/// <summary>
		/// Indicates that the item has a stream associated with it.
		/// </summary>
		Stream = 0x00400000,

		/// <summary>
		/// Children of this item are accessible through IStream or IStorage.
		/// </summary>
		StorageAncestor = 0x00800000,

		/// <summary>
		/// This flag is a mask for the storage capability attributes.
		/// </summary>
		StorageCapabilityMask = 0x70C50008,

#pragma warning disable SA1139 // Use literal suffix notation instead of casting
		/// <summary>
		/// Mask used to remove certain values that are considered
		/// to cause slow calculations or lack context.
		/// </summary>
		PkeyMask = unchecked ((int)0x81044000),
#pragma warning restore SA1139 // Use literal suffix notation instead of casting
	}
}
