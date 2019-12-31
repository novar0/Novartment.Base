using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// The finite set of unique elements with the ability to enumerate and verify belonging.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	public interface IReadOnlyFiniteSet<T> :
		IReadOnlyCollection<T>
	{
		/// <summary>
		/// Checks if the specified value belongs to the set.
		/// </summary>
		/// <param name="item">The value to check for belonging to the set.</param>
		/// <returns>
		/// True if the specified value belongs to the set, or False if it does not.
		/// </returns>
		/// <remarks>
		/// Corresponds to the System.Collections.Generic.ICollection&lt;&gt;.Contains() method.
		/// </remarks>
		bool Contains (T item);
	}
}
