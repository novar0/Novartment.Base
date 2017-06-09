using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
#pragma warning disable SA1600 // Elements must be documented
	[ComImport]
	[Guid ("42f85136-db7e-439c-85f1-e4075d135fc8")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IFileOpenDialog
	{
		// Defined on IModalWindow - repeated here due to requirements of COM interop layer.
		[PreserveSig]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Show ([In] IntPtr parent);

		// Defined on IFileDialog - repeated here due to requirements of COM interop layer.
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileTypes ([In] uint cFileTypes, [In] ref ShellFilterSpecification rgFilterSpec);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileTypeIndex ([In] uint iFileType);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		uint GetFileTypeIndex ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		uint Advise ([In, MarshalAs (UnmanagedType.Interface)] IFileDialogEvents pfde);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Unadvise ([In] uint dwCookie);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetOptions ([In] ShellFileOpenOptions fos);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		ShellFileOpenOptions GetOptions ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetDefaultFolder ([In, MarshalAs (UnmanagedType.Interface)] IShellItem psi);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFolder ([In, MarshalAs (UnmanagedType.Interface)] IShellItem psi);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs (UnmanagedType.Interface)]
		IShellItem GetFolder ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs (UnmanagedType.Interface)]
		IShellItem GetCurrentSelection ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileName ([In, MarshalAs (UnmanagedType.LPWStr)] string pszName);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs (UnmanagedType.LPWStr)]
		string GetFileName ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetTitle ([In, MarshalAs (UnmanagedType.LPWStr)] string pszTitle);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetOkButtonLabel ([In, MarshalAs (UnmanagedType.LPWStr)] string pszText);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileNameLabel ([In, MarshalAs (UnmanagedType.LPWStr)] string pszLabel);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs (UnmanagedType.Interface)]
		IShellItem GetResult ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AddPlace ([In, MarshalAs (UnmanagedType.Interface)] IShellItem psi, ShellFileDialogAddPlacePosition fdap);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetDefaultExtension ([In, MarshalAs (UnmanagedType.LPWStr)] string pszDefaultExtension);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Close ([MarshalAs (UnmanagedType.Error)] int hr);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetClientGuid ([In] ref Guid guid);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ClearClientData ();

		// Not supported: IShellItemFilter is not defined, converting to IntPtr.
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFilter ([MarshalAs (UnmanagedType.Interface)] IntPtr pFilter);

		// Defined by IFileOpenDialog.
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs (UnmanagedType.Interface)]
		IShellItemArray GetResults ();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs (UnmanagedType.Interface)]
		IShellItemArray GetSelectedItems ();
	}
#pragma warning restore SA1600 // Elements must be documented
}
