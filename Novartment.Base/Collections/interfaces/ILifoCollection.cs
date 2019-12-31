namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection that supports adding elements,
	/// enumerating the elements in the order they were added,
	/// and getting/removing the last of the added elements (stack semantics).
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>The typical representative - System.Collections.Generic.Stack.</remarks>
	public interface ILifoCollection<T> :
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Tries to get the last item in a collection.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a collection, if collection was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the collection was not empty; otherwise, False.</returns>
		bool TryPeekLast (out T item);

		/// <summary>
		/// Tries to get and remove the last item in a collection.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a collection, if collection was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the collection was not empty; otherwise, False.</returns>
		bool TryTakeLast (out T item);
	}
}
