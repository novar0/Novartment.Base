using System;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Опции шифрования и дешифрации данных.
	/// </summary>
	/// <remarks>Значение используется в параметре dwFlags функций CryptProtectData и CryptUnprotectData.</remarks>
	[Flags]
	internal enum CryptProtectOptions : int
	{
		None = 0,

		/// <summary>UI Forbidden. For remote-access situations where ui is not an option.
		/// When this flag is set and a UI is specified for either the protect or unprotect operation, the operation fails.</summary>
		/// <remarks>Константа CRYPTPROTECT_UI_FORBIDDEN.</remarks>
		UIForbidden = 1,

		/// <summary>Associates the data encrypted with the current computer instead of with an individual user.</summary>
		/// <remarks>Константа CRYPTPROTECT_LOCAL_MACHINE.</remarks>
		LocalMachine = 4,

		/// <summary>Force credential synchronize during protect data.
		/// Synchronize is only operation that occurs during this operation.</summary>
		/// <remarks>Константа CRYPTPROTECT_CRED_SYNC.</remarks>
		CredentialSynchronize = 8,

		/// <summary>Generate an audit on protect and unprotect operations.</summary>
		/// <remarks>Константа CRYPTPROTECT_AUDIT.</remarks>
		GenerateAudit = 0x10,

		/// <summary>Protect data with a non-recoverable key.</summary>
		/// <remarks>Константа CRYPTPROTECT_NO_RECOVERY.</remarks>
		NoRecovery = 0x20,

		/// <summary>Verify the protection of a protected blob.</summary>
		/// <remarks>Константа CRYPTPROTECT_VERIFY_PROTECTION.</remarks>
		VerifyProtection = 0x40,

		/// <summary>Regenerate the local machine protection.</summary>
		/// <remarks>Константа CRYPTPROTECT_CRED_REGENERATE.</remarks>
		CredentialRegenerate = 0x80,
	}
}
