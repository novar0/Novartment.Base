using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct ShellFilterSpecification
	{
		[MarshalAs (UnmanagedType.LPWStr)] public string Name;
		[MarshalAs (UnmanagedType.LPWStr)] public string Spec;
	}
}
