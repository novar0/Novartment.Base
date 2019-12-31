using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection that supports enumeration, cleaning, and adding elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>
	/// Does not supports deletion of elements because that will require a search, which can lead to large implicit costs in most common collection types.
	/// </remarks>
	public interface IAdjustableCollection<T> :
		IReadOnlyCollection<T>
	{
		/// <summary>
		/// Adds an element to the collection.
		/// </summary>
		/// <param name="item">The element to add to the collection</param>
		void Add (T item);

		/// <summary>Removes all items from the collection.</summary>
		void Clear ();
	}
}
