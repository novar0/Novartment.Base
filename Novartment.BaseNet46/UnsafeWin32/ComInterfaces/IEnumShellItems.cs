using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[Guid ("70629033-e363-4a28-a567-0db78006e6d7"), ComImport, InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumShellItems
	{
		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Next (uint celt, [MarshalAs (UnmanagedType.Interface)] out IShellItem rgelt, out uint pceltFetched);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Skip ([In] uint celt);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Reset ();

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Clone ([MarshalAs (UnmanagedType.Interface)] out IEnumShellItems ppenum);
	}
}
