using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

namespace Novartment.Base.UnsafeWin32
{
	internal static partial class NativeMethods
	{
		internal static class Ole32
		{
			[DllImport ("ole32.dll")]
			internal static extern int OleInitialize (IntPtr pvReserved);

			[DllImport ("ole32.dll")]
			internal static extern void OleUninitialize ();

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport ("ole32.dll", CharSet = CharSet.Auto)]
			internal static extern int OleFlushClipboard ();

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport ("ole32.dll", CharSet = CharSet.Auto)]
			internal static extern int OleIsCurrentClipboard (IDataObject pDataObj);

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport ("ole32.dll", CharSet = CharSet.Auto)]
			internal static extern int OleGetClipboard (ref IDataObject data);

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport ("ole32.dll", CharSet = CharSet.Auto)]
			internal static extern int OleSetClipboard (IDataObject pDataObj);
		}
	}
}
