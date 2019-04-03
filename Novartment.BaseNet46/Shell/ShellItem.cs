using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Novartment.Base.UnsafeWin32;
using static System.Linq.Enumerable;

namespace Novartment.Base.Shell
{
	/// <summary>
	/// Элемент оболочки.
	/// </summary>
	[DebuggerDisplay ("{DisplayNameRelative}")]
	public class ShellItem
	{
		private static readonly Guid _guidIShellItem = new Guid ("43826D1E-E718-42EE-BC55-A1E261C37BFE");
		private static readonly ShellPropertyKey _shellPropertyKeySize = new ShellPropertyKey (new Guid ("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 12);
		private static readonly ShellPropertyKey _shellPropertyKeyFileAttributes = new ShellPropertyKey (new Guid ("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 13);
		private static readonly ShellPropertyKey _shellPropertyKeyDateModified = new ShellPropertyKey (new Guid ("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 14);
		private static readonly ShellPropertyKey _shellPropertyKeyDateCreated = new ShellPropertyKey (new Guid ("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 15);
		private static readonly ShellPropertyKey _shellPropertyKeyDateAccessed = new ShellPropertyKey (new Guid ("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 16);
		private readonly IShellItem _nativeShellItem;

		/// <summary>
		/// Инициализирует новый экземпляр ShellItem на основе указанного COM-объекта.
		/// </summary>
		/// <param name="nativeShellItem">COM-объект, на основе которого будет создан элемент обочки.</param>
		internal ShellItem (IShellItem nativeShellItem)
		{
			if (nativeShellItem == null)
			{
				throw new ArgumentNullException (nameof (nativeShellItem));
			}

			Contract.EndContractBlock ();

			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For shell functions thread must be STA.");
			}

			_nativeShellItem = nativeShellItem;
		}

		/// <summary>Получает родительский элемент оболочки.</summary>
		public ShellItem Parent => (_nativeShellItem.GetParent (out IShellItem nativeShellItem) != 0) ?
					null :
					new ShellItem (nativeShellItem);

		/// <summary>Returns the editing name relative to the desktop.</summary>
		public string DisplayNameAbsolute
		{
			get
			{
				var hr = _nativeShellItem.GetDisplayName (ShellItemDisplayNameForm.DesktopAbsoluteEditing, out string path);
				if (hr != 0)
				{
					throw new InvalidOperationException ("IShellItem.GetDisplayName() failed.", Marshal.GetExceptionForHR (hr));
				}

				return path;
			}
		}

		/// <summary>Returns the display name relative to the parent folder.</summary>
		public string DisplayNameRelative
		{
			get
			{
				var hr = _nativeShellItem.GetDisplayName (ShellItemDisplayNameForm.Normal, out string path);
				if (hr != 0)
				{
					throw new InvalidOperationException ("IShellItem.GetDisplayName() failed.", Marshal.GetExceptionForHR (hr));
				}

				return path;
			}
		}

		/// <summary>Returns the parsing name relative to the parent folder.</summary>
		public string NameRelative
		{
			get
			{
				var hr = _nativeShellItem.GetDisplayName (ShellItemDisplayNameForm.ParentRelativeParsing, out string path);
				if (hr != 0)
				{
					throw new InvalidOperationException ("IShellItem.GetDisplayName() failed.", Marshal.GetExceptionForHR (hr));
				}

				return path;
			}
		}

		/// <summary>Returns the parsing name relative to the desktop.</summary>
		public string NameAbsolute
		{
			get
			{
				var hr = _nativeShellItem.GetDisplayName (ShellItemDisplayNameForm.DesktopAbsoluteParsing, out string path);
				if (hr != 0)
				{
					throw new InvalidOperationException ("IShellItem.GetDisplayName() failed.", Marshal.GetExceptionForHR (hr));
				}

				return path;
			}
		}

		/// <summary>Returns the item's file system path, if it has one.</summary>
		public string FileSystemPath
		{
			get
			{
				var hr = _nativeShellItem.GetDisplayName (ShellItemDisplayNameForm.FileSystemPath, out string path);
				if (hr != 0)
				{
					throw new InvalidOperationException ("IShellItem.GetDisplayName() failed.", Marshal.GetExceptionForHR (hr));
				}

				return path;
			}
		}

		/// <summary>The specified items can be copied.</summary>
		public bool CanCopy => CheckAttribute (_nativeShellItem, ShellItemAttributes.CanCopy);

		/// <summary>The specified items can be moved.</summary>
		public bool CanMove => CheckAttribute (_nativeShellItem, ShellItemAttributes.CanMove);

		/// <summary>Shortcuts can be created for the specified items.</summary>
		public bool CanLink => CheckAttribute (_nativeShellItem, ShellItemAttributes.CanLink);

		/// <summary>The specified items can be renamed.</summary>
		public bool CanRename => CheckAttribute (_nativeShellItem, ShellItemAttributes.CanRename);

		/// <summary>The specified items can be deleted.</summary>
		public bool CanDelete => CheckAttribute (_nativeShellItem, ShellItemAttributes.CanDelete);

		/// <summary>The specified folders or file objects are part of the file system
		/// that is, they are files, directories, or root directories.</summary>
		public bool IsFileSystem => CheckAttribute (_nativeShellItem, ShellItemAttributes.FileSystem);

		/// <summary>The specified items are read-only. In the case of folders,
		/// this means that new items cannot be created in those folders.</summary>
		public bool IsReadOnly => CheckAttribute (_nativeShellItem, ShellItemAttributes.ReadOnly);

		/// <summary>The item is hidden and should not be displayed unless the
		/// Show hidden files and folders option is enabled.</summary>
		public bool IsHidden => CheckAttribute (_nativeShellItem, ShellItemAttributes.Hidden);

		/// <summary>The specified items are folders.</summary>
		public bool IsFolder => CheckAttribute (_nativeShellItem, ShellItemAttributes.Folder);

		/// <summary>The specified items are on removable media or are themselves removable devices.</summary>
		public bool IsRemovable => CheckAttribute (_nativeShellItem, ShellItemAttributes.Removable);

		/// <summary>Получает размер файла в байтах.</summary>
		public long? Size
		{
			get
			{
				if (!(_nativeShellItem is IShellItem2 shellItem))
				{
					throw new NotSupportedException("Failed to get properties of shell item because it does not implements required IShellItem2 interface.");
				}

				var hr = shellItem.GetUInt64 (_shellPropertyKeySize, out ulong size);
				if (hr != 0)
				{
					return null;
				}

				return (long)size;
			}
		}

		/// <summary>Получает атрибуты файла.</summary>
		public FileAttributes? FileAttributes
		{
			get
			{
				if (!(_nativeShellItem is IShellItem2 shellItem))
				{
					throw new NotSupportedException("Failed to get properties of shell item because it does not implements required IShellItem2 interface.");
				}

				var hr = shellItem.GetInt32 (_shellPropertyKeyFileAttributes, out int attributes);
				if (hr != 0)
				{
					return null;
				}

				return (FileAttributes)attributes;
			}
		}

		/// <summary>Получает время последней операции записи в файл.</summary>
		public DateTime? DateModified
		{
			get
			{
				if (!(_nativeShellItem is IShellItem2 shellItem))
				{
					throw new NotSupportedException("Failed to get properties of shell item because it does not implements required IShellItem2 interface.");
				}

				var hr = shellItem.GetFileTime (_shellPropertyKeyDateModified, out long time);
				if (hr != 0)
				{
					return null;
				}

				return DateTime.FromFileTime (time);
			}
		}

		/// <summary>Получает время время создания файла.</summary>
		public DateTime? DateCreated
		{
			get
			{
				if (!(_nativeShellItem is IShellItem2 shellItem))
				{
					throw new NotSupportedException("Failed to get properties of shell item because it does not implements required IShellItem2 interface.");
				}

				var hr = shellItem.GetFileTime (_shellPropertyKeyDateCreated, out long time);
				if (hr != 0)
				{
					return null;
				}

				return DateTime.FromFileTime (time);
			}
		}

		/// <summary>Получает время последнего доступа к файлу.</summary>
		public DateTime? DateAccessed
		{
			get
			{
				if (!(_nativeShellItem is IShellItem2 shellItem))
				{
					throw new NotSupportedException("Failed to get properties of shell item because it does not implements required IShellItem2 interface.");
				}

				var hr = shellItem.GetFileTime (_shellPropertyKeyDateAccessed, out long time);
				if (hr != 0)
				{
					return null;
				}

				return DateTime.FromFileTime (time);
			}
		}

		/// <summary>
		/// Получает элемент оболочки в виде COM-интерфейса IShellItem.
		/// </summary>
		internal IShellItem NativeShellItem => _nativeShellItem;

		/// <summary>
		/// Получает элемент оболочки, соответствующий указанному пути к файлу.
		/// </summary>
		/// <param name="path">Полный путь к файлу.</param>
		/// <returns>Элемент оболочки, соответствующий указанному пути к файлу.</returns>
		public static ShellItem FromPath (string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException (nameof (path));
			}

			Contract.EndContractBlock ();

			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For shell functions thread must be STA.");
			}

			var guid = _guidIShellItem;
			var hr = NativeMethods.Shell32.SHCreateItemFromParsingName (
				path,
				IntPtr.Zero,
				ref guid,
				out IShellItem nativeShellItem);
			if (hr != 0)
			{
				throw new ShellException ("Failed to create shell item for path '" + path + "'.", Marshal.GetExceptionForHR (hr));
			}

			return new ShellItem (nativeShellItem);
		}

		/// <summary>
		/// Получает элемент оболочки, соответствующий указанной специально папке.
		/// </summary>
		/// <param name="specialFolder">Идентификатор специальной папки.</param>
		/// <returns>Элемент оболочки, соответствующий указанной специально папке.</returns>
		public static ShellItem FromSpecialFolder (Environment.SpecialFolder specialFolder)
		{
			var hr = NativeMethods.Shell32.SHGetFolderLocation (IntPtr.Zero, specialFolder, IntPtr.Zero, 0, out IntPtr pidlRoot);
			if (hr == -2147024894)
			{
				return null; // HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND) = 0x80070002
			}

			if (hr != 0)
			{
				throw new ShellException (
					FormattableString.Invariant (
						$"Failed to get special folder '{specialFolder}'."),
					Marshal.GetExceptionForHR (hr));
			}

			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For shell functions thread must be STA.");
			}

			hr = NativeMethods.Shell32.SHCreateShellItem (
				IntPtr.Zero,
				null,
				pidlRoot,
				out IShellItem nativeShellItem);
			if (hr != 0)
			{
				throw new ShellException (
					"Failed to create shell item for special folder '" + specialFolder + "'.",
					Marshal.GetExceptionForHR (hr));
			}

			return new ShellItem (nativeShellItem);
		}

		/// <summary>
		/// Получает массив элементов оболочки из данных объекта для обмена между приложениями.
		/// </summary>
		/// <param name="dataContainer">Объект данных объекта для обмена между приложениями.</param>
		/// <returns>Массив элементов оболочки.
		/// Первым элементом массива является родительская папка.</returns>
		/// <remarks>Массив элементов оболочки, полученный из данных объекта для обмена между приложениями.</remarks>
		public static ShellItem[] FromDataContainer (IDataContainer dataContainer)
		{
			if (dataContainer == null)
			{
				throw new ArgumentNullException (nameof (dataContainer));
			}

			Contract.EndContractBlock ();

			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For shell functions thread must be STA.");
			}

			var data = dataContainer.GetData(DataContainerFormats.ShellIdList, true);
			if (!(data is MemoryStream memStream))
			{
				return null;
			}

			var array = memStream.ToArray ();
			memStream.Close ();

			var nSize = sizeof (uint);
			var bufSize = array.Length;
			if (bufSize < (nSize + 1))
			{
				throw new FormatException (FormattableString.Invariant (
					$"Invalid CIDA structure: too small size ({bufSize}), expected min {nSize}."));
			}

			ShellItem[] result;
			var unmanagedBuffer = Marshal.AllocHGlobal (bufSize);
			try
			{
				Marshal.Copy (array, 0, unmanagedBuffer, bufSize);

				var count = (uint)Marshal.ReadInt32 (unmanagedBuffer);

				// убеждаемся что указанное количество элементов (плюс корневой) помещается в переданном буфере
				if ((count + 1) > (bufSize / nSize))
				{
					throw new FormatException (FormattableString.Invariant (
						$"Invalid CIDA structure: too small size ({bufSize}), expected {(count + 1) * nSize} for specified number of elements ({count})."));
				}

				result = new ShellItem[count + 1];
				var position = nSize;
				var offset = (uint)Marshal.ReadInt32 (unmanagedBuffer, position);
				if (offset >= bufSize)
				{
					throw new FormatException ("Invalid CIDA structure: parent element points outside.");
				}

				var pidlRoot = IntPtr.Add (unmanagedBuffer, (int)offset);

				var hr = NativeMethods.Shell32.SHCreateShellItem (IntPtr.Zero, null, pidlRoot, out IShellItem nativeShellItem);
				if (hr != 0)
				{
					throw new ShellException (
						"Failed to create shell item corresponding to the parent element of CIDA structure.",
						Marshal.GetExceptionForHR (hr));
				}

				var shellItem = new ShellItem (nativeShellItem);
				result[0] = shellItem;

				uint idx = 0;
				while (idx < count)
				{
					position += nSize;
					offset = (uint)Marshal.ReadInt32 (unmanagedBuffer, position);
					if (offset >= bufSize)
					{
						throw new FormatException (FormattableString.Invariant (
							$"Invalid CIDA structure: element #{idx + 1} points outside."));
					}

					var pidl = IntPtr.Add (unmanagedBuffer, (int)offset);
					hr = NativeMethods.Shell32.SHCreateShellItem (pidlRoot, null, pidl, out nativeShellItem);
					if (hr != 0)
					{
						throw new ShellException (
							FormattableString.Invariant (
								$"Failed to create shell item corresponding to the element #{idx + 1} of CIDA structure."),
							Marshal.GetExceptionForHR (hr));
					}

					shellItem = new ShellItem (nativeShellItem);
					result[idx + 1] = shellItem;
					idx++;
				}
			}
			finally
			{
				Marshal.FreeHGlobal (unmanagedBuffer);
			}

			return result;
		}

		/// <summary>
		/// Получает строку, представляющую объект.
		/// </summary>
		/// <returns>Строковое представление объекта.</returns>
		public override string ToString ()
		{
			return this.DisplayNameRelative;
		}

		/// <summary>
		/// Получает результат хэш-функции объекта.
		/// </summary>
		/// <returns>Хэш-код объекта.</returns>
		public override int GetHashCode ()
		{
			return this.NameAbsolute.GetHashCode ();
		}

		/// <summary>
		/// Получает перечислитель дочерних элементов.
		/// </summary>
		/// <returns>Перечислитель дочерних элементов.</returns>
		public IEnumerable<ShellItem> EnumerateItems ()
		{
			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For shell functions thread must be STA.");
			}

			var enumerable = new ShellItemsEnumerable (_nativeShellItem);

			return enumerable.Select (item => new ShellItem (item));
		}

		private static bool CheckAttribute (IShellItem nativeShellItem, ShellItemAttributes attribute)
		{
			var hr = nativeShellItem.GetAttributes (attribute, out _);
			if (hr != 0 && hr != 1)
			{
				throw new ShellException (
					FormattableString.Invariant (
						$"Failed to get attribute '{attribute}' of shell item."),
					Marshal.GetExceptionForHR (hr));
			}

			return hr == 0;
		}

		internal class ShellItemsEnumerable :
			IEnumerable<IShellItem>
		{
			private static readonly string _guidIEnumShellItems = "70629033-e363-4a28-a567-0db78006e6d7";
			private static readonly string _guidEnumItemsHandler = "94f60519-2850-4924-aa5a-d15e84868039"; // BHID_EnumItems
			private readonly IShellItem _nativeShellItem;

			internal ShellItemsEnumerable (IShellItem nativeShellItem)
			{
				_nativeShellItem = nativeShellItem;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<IShellItem> GetEnumerator ()
			{
				var guid = new Guid (_guidIEnumShellItems);
				var handler = new Guid (_guidEnumItemsHandler);
				var hr = _nativeShellItem.BindToHandler (
					IntPtr.Zero,
					ref handler,
					ref guid,
					out object enumShellItems);
				return (hr == 0) ?
					new ShellItemsEnumerator ((IEnumShellItems)enumShellItems) :
					((IEnumerable<IShellItem>)Array.Empty<IShellItem> ()).GetEnumerator ();
			}
		}
	}
}
