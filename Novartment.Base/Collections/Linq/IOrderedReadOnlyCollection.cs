using System;
using System.Collections.Generic;
using System.Linq;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Represents a sorted collection.
	/// </summary>
	/// <typeparam name="TElement">The type of the elements.</typeparam>
	public interface IOrderedReadOnlyCollection<TElement> :
		IOrderedEnumerable<TElement>,
		IReadOnlyCollection<TElement>
	{
		/// <summary>
		/// Performs a subsequent ordering on the elements of a collection according to a key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key produced by keySelector.</typeparam>
		/// <param name="keySelector">The function used to extract the key for each element.</param>
		/// <param name="comparer">The comparer used to compare keys for placement in the returned sequence.</param>
		/// <param name="descending">True to sort the elements in descending order; False to sort the elements in ascending order.</param>
		/// <returns>An collection whose elements are sorted according to a key.</returns>
		IOrderedReadOnlyCollection<TElement> CreateOrderedReadOnlyCollection<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
	}
}
