using System;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Provides the text of a prompt and information about when and where that prompt
	/// is to be displayed when using the protect functions.
	/// </summary>
	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CryptProtectPromptParameters
	{
		/// <summary>The size, in bytes, of this structure.</summary>
		internal int cbSize;

		/// <summary>Flags that indicate when prompts to the user are to be displayed.</summary>
		internal CryptProtectPromptOptions dwPromptFlags;

		/// <summary>Window handle to the parent window.</summary>
		internal IntPtr hwndApp;

		/// <summary>Text of a prompt to be displayed.</summary>
		internal string szPrompt;
	}
}
