using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection whose elements can be listed, retrieved, set and deleted by position number.
	/// Also supports adding new elements to the end or to the specified position.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>The typical representative - System.Collections.Generic.List.</remarks>
	public interface IAdjustableList<T> :
		IReadOnlyList<T>,
		IAdjustableCollection<T>,
		IFifoCollection<T>,
		ILifoCollection<T>
	{
		/// <summary>
		/// Gets or sets the element at the specified index in list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get.</param>
		new T this[int index] { get; set; }

		/// <summary>
		/// Inserts an element into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The element to insert. The value can be null for reference types.</param>
		void Insert (int index, T item);

		/// <summary>
		/// Inserts a specified number of the elements into the list at the specified index.
		/// The inserted elements will have a default value.
		/// </summary>
		/// <param name="index"> The zero-based index at which the new elements should be inserted.</param>
		/// <param name="count">The number of elements to insert.</param>
		void InsertRange (int index, int count);

		/// <summary>
		/// Removes the element at the specified index of the list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		void RemoveAt (int index);

		/// <summary>
		/// Removes a range of elements from the list.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove.</param>
		void RemoveRange (int index, int count);
	}
}
