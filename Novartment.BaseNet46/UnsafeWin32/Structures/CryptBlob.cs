using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Arbitrary array of bytes.
	/// </summary>
	[SuppressMessage ("Microsoft.Design",
		"CA1049:TypesThatOwnNativeResourcesShouldBeDisposable",
		Justification = "Memeber pbData is never assigned. The struct only passing back and forth between C# and unmanaged code."),
	StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CryptBlob
	{
		/// <summary>Count, in bytes, of data.</summary>
		internal int cbData;

		/// <summary>A pointer to the data buffer.</summary>
		[SuppressMessage ("Microsoft.Reliability",
			"CA2006:UseSafeHandleToEncapsulateNativeResources",
			Justification = "Memeber pbData is never assigned. The struct only passing back and forth between C# and unmanaged code.")]
		internal IntPtr pbData;
	}
}
