namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection that supports adding elements,
	/// enumerating the elements in the order they were added,
	/// and getting/removing the first of the added elements (queue semantics).
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>The typical representative - System.Collections.Generic.Queue.</remarks>
	public interface IFifoCollection<T> :
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Tries to get the first item in a collection.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a collection, if collection was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the collection was not empty; otherwise, False.</returns>
		bool TryPeekFirst (out T item);

		/// <summary>
		/// Tries to get and remove the first item in a collection.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a collection, if collection was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the collection was not empty; otherwise, False.</returns>
		bool TryTakeFirst (out T item);
	}
}
