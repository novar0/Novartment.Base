namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Defines how final attribute set is determined.
	/// </summary>
	/// <remarks>Соответствует перечислению SIATTRIBFLAGS.</remarks>
	internal enum ShellItemAttributeOptions
	{
		/// <summary>
		/// Use a bitwise AND to combine the attributes across items.
		/// </summary>
		And = 0x00000001,

		/// <summary>
		/// Use a bitwise OR to combine the attributes across items.
		/// </summary>
		Or = 0x00000002
	}
}
