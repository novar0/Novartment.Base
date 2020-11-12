using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[Guid ("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
	[ComImport]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItem
	{
		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int BindToHandler (
			[In] IntPtr pbc,
			[In] ref Guid bhid,
			[In] ref Guid riid,
			[Out, MarshalAs (UnmanagedType.Interface)] out object ppv);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetParent ([MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetDisplayName (
			[In] ShellItemDisplayNameForm sigdnName,
			[Out, MarshalAs (UnmanagedType.LPWStr)] out string ppszName);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetAttributes ([In] ShellItemAttributes sfgaoMask, out int psfgaoAttribs);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Compare (
			[In, MarshalAs (UnmanagedType.Interface)] IShellItem psi,
			[In] ShellItemComparisonType hint,
			out int piOrder);
	}
}
