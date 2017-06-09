using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Novartment.Base.UnsafeWin32
{
	internal partial class NativeMethods
	{
		internal static class Crypt32
		{
			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport ("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			[return: MarshalAs (UnmanagedType.Bool)]
			internal static extern bool CryptUnprotectData (
				ref CryptBlob dataIn,
				out string dataDescription,
				ref CryptBlob optionalEntropy,
				IntPtr reserved,
				ref CryptProtectPromptParameters promptParameters,
				CryptProtectOptions flags,
				ref CryptBlob dataOut);
		}
	}
}
