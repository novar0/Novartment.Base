using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct ShellFilterSpecification
	{
#pragma warning disable CA1051 // Do not declare visible instance fields
		[MarshalAs (UnmanagedType.LPWStr)]
		public string Name;

		[MarshalAs (UnmanagedType.LPWStr)]
		public string Spec;
#pragma warning restore CA1051 // Do not declare visible instance fields
	}
}
