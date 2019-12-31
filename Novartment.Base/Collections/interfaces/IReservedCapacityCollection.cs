namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection that supports the enumeration and addition of elements,
	/// as well as managing the reservation of space for them.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	public interface IReservedCapacityCollection<T> :
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Reserves space for the specified total number of elements.
		/// </summary>
		/// <param name="min">
		/// Minimum required capacity including items already in the collection.
		/// </param>
		/// <remarks>
		/// Corresponds to setting Capacity property of classes System.Collections.ArrayList and System.Collections.Generic.List&lt;T&gt;.
		/// </remarks>
		void EnsureCapacity (int min);

		/// <summary>
		/// Eliminates the collection of reserved items.
		/// </summary>
		/// <remarks>
		/// Corresponds to TrimExcess() method of classes in the  System.Collections.Generic namespace:
		/// List&lt;T&gt;, Stack&lt;T&gt;, Queue&lt;T&gt;, HashSet&lt;T&gt;.
		/// </remarks>
		void TrimExcess ();
	}
}
