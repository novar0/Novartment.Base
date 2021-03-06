using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[ComImport]
	[Guid ("B63EA76D-1F85-456F-A19C-48159EFA858B")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItemArray
	{
		// Not supported: IBindCtx.
		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int BindToHandler (
			[In, MarshalAs (UnmanagedType.Interface)] IntPtr pbc,
			[In] ref Guid rbhid,
			[In] ref Guid riid,
			out IntPtr ppvOut);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetPropertyStore (
			[In] int flags,
			[In] ref Guid riid,
			out IntPtr ppv);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetPropertyDescriptionList (
			[In] ref ShellPropertyKey keyType,
			[In] ref Guid riid,
			out IntPtr ppv);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetAttributes (
			[In] ShellItemAttributeOption dwAttribFlags,
			[In] ShellItemAttributes sfgaoMask,
			out ShellItemAttributes psfgaoAttribs);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetCount (out uint pdwNumItems);

		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetItemAt (
			[In] uint dwIndex,
			[MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi);

		// Not supported: IEnumShellItems (will use GetCount and GetItemAt instead).
		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int EnumItems ([MarshalAs (UnmanagedType.Interface)] out IntPtr ppenumShellItems);
	}
}
