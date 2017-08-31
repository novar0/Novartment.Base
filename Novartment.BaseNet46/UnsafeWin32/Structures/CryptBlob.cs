using System;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Arbitrary array of bytes.
	/// </summary>
	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CryptBlob
	{
		/// <summary>Count, in bytes, of data.</summary>
		internal int CountBytesData;

		/// <summary>A pointer to the data buffer.</summary>
		internal IntPtr PointerBytesData;
	}
}
