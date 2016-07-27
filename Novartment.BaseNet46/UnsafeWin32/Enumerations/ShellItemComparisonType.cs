
namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Determines how to compare two shell items.
	/// </summary>
	/// <remarks>Соответствует перечислению SICHINTF.</remarks>
	internal enum ShellItemComparisonType
	{
		/// <summary>Comparison is based on the display in a folder view.</summary>
		Display = 0x00000000,

		/// <summary>Comparison is based on a canonical name.</summary>
		Canonical = 0x10000000,

		/// <summary>If the Shell items are not the same, test the file system paths.</summary>
		TestFilesysPathIfNotEqual = 0x20000000,

		/// <summary>Exact comparison of two instances of a Shell item.</summary>
		AllFields = unchecked ((int)0x80000000)
	}
}
