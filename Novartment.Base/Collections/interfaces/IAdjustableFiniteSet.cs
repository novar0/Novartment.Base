namespace Novartment.Base.Collections
{
	// The library class System.Collections.Generic.ISet<T> is not suitable for use because it is redundant.
	// All its methods (except Add):
	// 1. Cannot be implemented inside the class more efficiently than outside in extension methods.
	// 2. Not suitable for concurent access.

	/// <summary>
	/// The finite set of unique elements with the ability to enumerate, verify belonging, clean, add and remove elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>
	/// The typical representatives - System.Collections.Generic.SortedSet and System.Collections.Generic.HashSet.
	/// </remarks>
	public interface IAdjustableFiniteSet<T> :
		IReadOnlyFiniteSet<T>,
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Removes the element from the set.
		/// </summary>
		/// <param name="item">The element to remove.</param>
		void Remove (T item);
	}
}
