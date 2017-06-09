using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Novartment.Base.UnsafeWin32;

namespace Novartment.Base
{
	/// <summary>
	/// Обёртка для вызова Win32 функций CryptProtectData/CryptUnprotectData.
	/// </summary>
	public static class DataEncryptionService
	{
		/// <summary>Calls Win32 CryptUnprotectData to decrypt ciphertext bytes.</summary>
		/// <param name="value">Encrypted data.</param>
		/// <returns>Decrypted data bytes.</returns>
		/// <remarks>
		/// When decrypting data, it is not necessary to specify which
		/// type of encryption key to use: user-specific or
		/// machine-specific; DPAPI will figure it out by looking at
		/// the signature of encrypted data.
		/// </remarks>
		public static byte[] Decrypt (byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			// Create BLOBs to hold data.
			var plainTextBlob = default (CryptBlob);
			var cipherTextBlob = default (CryptBlob);
			var entropyBlob = default (CryptBlob);

			// We only need prompt structure because it is a required parameter.
			var prompt = default (CryptProtectPromptParameters);
			InitPrompt (ref prompt);

			try
			{
				// Convert ciphertext bytes into a BLOB structure.
				InitBLOB (value, ref cipherTextBlob);

				// Convert entropy bytes into a BLOB structure.
				InitBLOB (null, ref entropyBlob);

				// Call DPAPI to decrypt data.
				var success = NativeMethods.Crypt32.CryptUnprotectData (
					ref cipherTextBlob,
					out string description,
					ref entropyBlob,
					IntPtr.Zero,
					ref prompt,
					CryptProtectOptions.UIForbidden,
					ref plainTextBlob);

				// Check the result.
				if (!success)
				{
					// If operation failed, retrieve last Win32 error.
					var errCode = Marshal.GetLastWin32Error ();
					var excpt = new InvalidOperationException ("Win32 Error " + errCode);
					if (errCode == -2146893813/*NTE_BAD_KEY_STATE 0x8009000B*/)
					{
						throw new CryptWrongCredentialsException (excpt);
					}
					else
					{
						throw excpt;
					}
				}

				// Allocate memory to hold plaintext.
				var plainTextBytes = new byte[plainTextBlob.CountBytesData];

				// Copy ciphertext from the BLOB to a byte array.
				Marshal.Copy (
					plainTextBlob.PointerBytesData,
					plainTextBytes,
					0,
					plainTextBlob.CountBytesData);

				// Return the result.
				return plainTextBytes;
			}

			// Free all memory allocated for BLOBs.
			finally
			{
				if (plainTextBlob.PointerBytesData != IntPtr.Zero)
				{
					Marshal.FreeHGlobal (plainTextBlob.PointerBytesData);
				}

				if (cipherTextBlob.PointerBytesData != IntPtr.Zero)
				{
					Marshal.FreeHGlobal (cipherTextBlob.PointerBytesData);
				}

				if (entropyBlob.PointerBytesData != IntPtr.Zero)
				{
					Marshal.FreeHGlobal (entropyBlob.PointerBytesData);
				}
			}
		}

		/// <summary>
		/// Initializes empty prompt structure.
		/// </summary>
		/// <param name="ps">
		/// Prompt parameter (which we do not actually need).
		/// </param>
		private static void InitPrompt (ref CryptProtectPromptParameters ps)
		{
			ps.Size = Marshal.SizeOf (typeof (CryptProtectPromptParameters));
			ps.PromptFlags = CryptProtectPromptOptions.None;
			ps.WindowHandle = IntPtr.Zero;
			ps.Prompt = null;
		}

		/// <summary>
		/// Initializes a BLOB structure from a byte array.
		/// </summary>
		/// <param name="data">
		/// Original data in a byte array format.
		/// </param>
		/// <param name="blob">
		/// Returned blob structure.
		/// </param>
		private static void InitBLOB (byte[] data, ref CryptBlob blob)
		{
			// Use empty array for null parameter.
			data = data ?? Array.Empty<byte> ();

			// Allocate memory for the BLOB data.
			blob.PointerBytesData = Marshal.AllocHGlobal (data.Length);

			// Specify number of bytes in the BLOB.
			blob.CountBytesData = data.Length;

			// Copy data from original source to the BLOB structure.
			Marshal.Copy (data, 0, blob.PointerBytesData, data.Length);
		}
	}
}
