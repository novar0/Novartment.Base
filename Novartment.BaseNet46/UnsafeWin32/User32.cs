using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Novartment.Base.UnsafeWin32
{
	internal static partial class NativeMethods
	{
		internal static class User32
		{
			[SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport ("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			[return: MarshalAs (UnmanagedType.Bool)]
			internal static extern bool AddClipboardFormatListener (IntPtr hWndObserver);

			[SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport ("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			[return: MarshalAs (UnmanagedType.Bool)]
			internal static extern bool RemoveClipboardFormatListener (IntPtr hWndObserver);
		}
	}
}
