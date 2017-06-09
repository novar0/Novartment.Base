using System;
using System.Runtime.InteropServices;
using Novartment.Base.UnsafeWin32;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Диалог, запрашивающий у пользователя папку оболочки.
	/// </summary>
	public sealed class FolderBrowserDialog : Microsoft.Win32.CommonDialog
	{
		private const int _errorCancelledCode = unchecked ((int)0x800704C7);
		private FolderBrowserDialogTypeFilter _filter;
		private Shell.ShellItem _selectedFolder;

		/// <summary>
		/// Инициализирует новый экземпляр класса FolderBrowserDialog.
		/// </summary>
		public FolderBrowserDialog ()
		{
		}

		/// <summary>
		/// Получает или устанавливает папку по-умолчанию,
		/// с которой будет начат выбор.
		/// </summary>
		public Shell.ShellItem DefaultFolder { get; set; }

		/// <summary>
		/// Получает выбранную пользователем папку оболочки.
		/// </summary>
		public Shell.ShellItem SelectedFolder => _selectedFolder;

		/// <summary>
		/// Получает или устанавливает заголовок диалога.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Получает или устанавливает тип доступных для выбора папок.
		/// </summary>
		public FolderBrowserDialogTypeFilter Filter
		{
			get => _filter;

			set
			{
				_filter = value;
			}
		}

		/// <summary>
		/// Сбрасывает свойства в значения по-умолчанию.
		/// </summary>
		public override void Reset ()
		{
			_filter = FolderBrowserDialogTypeFilter.All;
			this.Title = null;
			this.DefaultFolder = null;
			_selectedFolder = null;
		}

		/// <summary>
		/// Отображает диалог выбора папки оболочки.
		/// </summary>
		/// <param name="hwndOwner">Handle to the window that owns the dialog box.</param>
		/// <returns>
		/// If the user clicks the OK button of the dialog that is displayed, true is returned; otherwise, false.
		/// </returns>
		protected override bool RunDialog (IntPtr hwndOwner)
		{
			_selectedFolder = null;
			var dialog = (IFileOpenDialog)new FileOpenDialog ();
			if (this.DefaultFolder != null)
			{
				dialog.SetDefaultFolder (this.DefaultFolder.NativeShellItem);
			}

			if (this.Title != null)
			{
				dialog.SetTitle (this.Title);
			}

			var dialogOptions = ShellFileOpenOptions.PickFolders;
			switch (_filter)
			{
				case FolderBrowserDialogTypeFilter.All:
					dialogOptions |= ShellFileOpenOptions.AllNonStorageItems;
					break;
				case FolderBrowserDialogTypeFilter.FileSystem:
					dialogOptions |= ShellFileOpenOptions.ForceFileSystem;
					break;
			}

			dialog.SetOptions (dialogOptions);

			var hr = dialog.Show (hwndOwner);
			if (hr == _errorCancelledCode)
			{
				return false;
			}

			if (hr != 0)
			{
				throw new Shell.ShellException ("IFileOpenDialog.Show()", Marshal.GetExceptionForHR (hr));
			}

			var nativeShellItem = dialog.GetResult ();
			_selectedFolder = new Shell.ShellItem (nativeShellItem);
			return true;
		}
	}
}
