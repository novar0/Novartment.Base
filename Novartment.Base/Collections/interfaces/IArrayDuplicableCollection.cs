using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection that supports enumeration and copying all elements to an array.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	public interface IArrayDuplicableCollection<T> : IReadOnlyCollection<T>
	{
		/// <summary>
		/// Copies the collection to a one-dimensional array,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <remarks>Corresponds to the System.Collections.ICollection.CopyTo() and System.Array.CopyTo().</remarks>
		void CopyTo (T[] array, int arrayIndex);
	}
}
