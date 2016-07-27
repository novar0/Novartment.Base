using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Novartment.Base.UnsafeWin32
{
	internal static partial class NativeMethods
	{
		internal static class Shell32
		{
			[SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport ("shell32")]
			internal static extern int SHGetFolderLocation (
				IntPtr hwndOwner,
				Environment.SpecialFolder nFolder,
				IntPtr hToken,
				uint dwReserved,
				out IntPtr ppidl);

			[DllImport ("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern int SHCreateShellItem (
				IntPtr pidlParent,
				[In, MarshalAs (UnmanagedType.Interface)] IShellItem psfParent,
				IntPtr pidl,
				[MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi
			);

			[DllImport ("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern int SHCreateItemFromParsingName (
				[MarshalAs (UnmanagedType.LPWStr)] string path,
				IntPtr pbc,
				ref Guid riid,
				[MarshalAs (UnmanagedType.Interface)] out IShellItem shellItem);
		}
	}
}
