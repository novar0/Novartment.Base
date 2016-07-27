using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[Guid ("7E9FB0D3-919F-4307-AB2E-9B1860310C93"), ComImport, InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellItem2
	{
		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int BindToHandler (
			[In] IntPtr pbc,
			[In] ref Guid bhid,
			[In] ref Guid riid,
			[Out, MarshalAs (UnmanagedType.Interface)] out object ppv);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetParent ([MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetDisplayName (
			[In] ShellItemDisplayNameForm sigdnName,
			out IntPtr ppszName);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetAttributes ([In] ShellItemAttributes sfgaoMask, out int psfgaoAttribs);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Compare (
			[In, MarshalAs (UnmanagedType.Interface)] IShellItem psi,
			[In] ShellItemComparisonType hint,
			out int piOrder);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
		int GetPropertyStore (
			[In] PropertyStoreOptions flags,
			[In] ref Guid riid,
			[Out, MarshalAs (UnmanagedType.Interface)] out object/*IPropertyStore*/ ppv);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPropertyStoreWithCreateObject ([In] PropertyStoreOptions flags, [In, MarshalAs (UnmanagedType.IUnknown)] object punkCreateObject, [In] ref Guid riid, out IntPtr ppv);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPropertyStoreForKeys ([In] ref ShellPropertyKey rgKeys, [In] uint cKeys, [In] PropertyStoreOptions flags, [In] ref Guid riid, [Out, MarshalAs (UnmanagedType.IUnknown)] out object/*IPropertyStore*/ ppv);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPropertyDescriptionList ([In] ref ShellPropertyKey keyType, [In] ref Guid riid, out IntPtr ppv);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Update ([In, MarshalAs (UnmanagedType.Interface)] object/*IBindCtx*/ pbc);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetProperty ([In] ref ShellPropertyKey key, [Out] IntPtr/*PropVariant*/ ppropvar);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetCLSID ([In] ref ShellPropertyKey key, out Guid pclsid);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetFileTime ([In] ref ShellPropertyKey key, out long pft);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetInt32 ([In] ref ShellPropertyKey key, out int pi);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetString ([In] ref ShellPropertyKey key, [MarshalAs (UnmanagedType.LPWStr)] out string ppsz);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetUInt32 ([In] ref ShellPropertyKey key, out uint pui);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetUInt64 ([In] ref ShellPropertyKey key, out ulong pull);

		[PreserveSig, MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetBool ([In] ref ShellPropertyKey key, out int pf);
	}
}
