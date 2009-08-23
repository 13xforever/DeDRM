using System;
using System.Runtime.InteropServices;

namespace Win32Interop
{
	public static class Crypt32
	{
		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CryptProtectData(ref DATA_BLOB pDataIn, String szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved,
		                                            ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, CryptProtectFlags dwFlags, ref DATA_BLOB pDataOut);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, string szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved,
		                                              ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, CryptProtectFlags dwFlags, ref DATA_BLOB pDataOut);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct DATA_BLOB
		{
			public int cbData;
			public IntPtr pbData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CRYPTPROTECT_PROMPTSTRUCT
		{
			public int cbSize;
			public CryptProtectPromptFlags dwPromptFlags;
			public IntPtr hwndApp;
			public String szPrompt;
		}

		[Flags]
		public enum CryptProtectFlags
		{
			// for remote-access situations where ui is not an option
			// if UI was specified on protect or unprotect operation, the call
			// will fail and GetLastError() will indicate ERROR_PASSWORD_RESTRICTION
			CRYPTPROTECT_UI_FORBIDDEN = 0x1,

			// per machine protected data -- any user on machine where CryptProtectData
			// took place may CryptUnprotectData
			CRYPTPROTECT_LOCAL_MACHINE = 0x4,

			// force credential synchronize during CryptProtectData()
			// Synchronize is only operation that occurs during this operation
			CRYPTPROTECT_CRED_SYNC = 0x8,

			// Generate an Audit on protect and unprotect operations
			CRYPTPROTECT_AUDIT = 0x10,

			// Protect data with a non-recoverable key
			CRYPTPROTECT_NO_RECOVERY = 0x20,

			// Verify the protection of a protected blob
			CRYPTPROTECT_VERIFY_PROTECTION = 0x40
		}

		[Flags]
		public enum CryptProtectPromptFlags
		{
			// prompt on unprotect
			CRYPTPROTECT_PROMPT_ON_UNPROTECT = 0x1,

			// prompt on protect
			CRYPTPROTECT_PROMPT_ON_PROTECT = 0x2
		}
	}
}