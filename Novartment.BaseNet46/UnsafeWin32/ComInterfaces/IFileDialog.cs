using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[ComImport, Guid ("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IFileDialog
	{
		// Defined on IModalWindow - repeated here due to requirements of COM interop layer.
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime),
		PreserveSig]
		int Show ([In] IntPtr parent);

		// IFileDialog-Specific interface members.

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileTypes (
			[In] uint cFileTypes,
			[In, MarshalAs (UnmanagedType.LPArray)] ShellFilterSpecification[] rgFilterSpec);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileTypeIndex ([In] uint iFileType);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetFileTypeIndex (out uint piFileType);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Advise (
			[In, MarshalAs (UnmanagedType.Interface)] IFileDialogEvents pfde,
			out uint pdwCookie);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Unadvise ([In] uint dwCookie);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetOptions ([In] ShellFileOpenOptions fos);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetOptions (out ShellFileOpenOptions pfos);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetDefaultFolder ([In, MarshalAs (UnmanagedType.Interface)] IShellItem psi);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFolder ([In, MarshalAs (UnmanagedType.Interface)] IShellItem psi);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetFolder ([MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetCurrentSelection ([MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileName ([In, MarshalAs (UnmanagedType.LPWStr)] string pszName);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetFileName ([MarshalAs (UnmanagedType.LPWStr)] out string pszName);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetTitle ([In, MarshalAs (UnmanagedType.LPWStr)] string pszTitle);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetOkButtonLabel ([In, MarshalAs (UnmanagedType.LPWStr)] string pszText);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetFileNameLabel ([In, MarshalAs (UnmanagedType.LPWStr)] string pszLabel);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetResult ([MarshalAs (UnmanagedType.Interface)] out IShellItem ppsi);

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
	}
}
