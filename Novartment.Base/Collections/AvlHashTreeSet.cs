using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Sorted set of unique values based on the binary search tree built on hash functions of elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>
	/// To construct the tree, it is not the values of the elements that are used, but their hash code.
	/// If there is a value comparer for elements, then AvlTreeSet will work faster with the same functionality.
	/// The tree may contain duplicates (hashes, but not values), since a hash function can generate matching hashes for different values.
	/// В дереве могут содержаться дупликаты (хэшей, но не значений), так как хэш-функция может порождать совпадающие хэши для разных значений.
	/// </remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public sealed class AvlHashTreeSet<T> :
		IAdjustableFiniteSet<T>
	{
		private readonly IEqualityComparer<T> _comparer;
		private AvlBinarySearchHashTreeNode<T> _startNode = null; // null является корректным значением, означает пустое дерево
		private int _count = 0;

		/// <summary>
		/// Initializes a new instance of the AvlHashTreeSet class that is empty and uses a specified value comparer.
		/// </summary>
		/// <param name="comparer">
		/// The comparer to be used when comparing values in a set. Specify null-reference to use default comparer for type T.
		/// </param>
		public AvlHashTreeSet (IEqualityComparer<T> comparer = null)
		{
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		/// <summary>
		/// Initializes a new instance of the AvlTreeSet class that contains specified binary search tree nodes
		/// and uses a specified value comparer.
		/// </summary>
		/// <param name="startNode">The initial node of the binary search tree that will become the contents of the set.</param>
		/// <param name="comparer">
		/// The comparer to be used when comparing values in a set. Specify null-reference to use default comparer for type T.
		/// </param>
		public AvlHashTreeSet (AvlBinarySearchHashTreeNode<T> startNode, IEqualityComparer<T> comparer = null)
		{
			_startNode = startNode;
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		/// <summary>
		/// Gets the comparer used when comparing values of a set.
		/// </summary>
		public IEqualityComparer<T> Comparer => _comparer;

		/// <summary>
		/// Gets the number of elements in the set.
		/// </summary>
		public int Count => _count;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => (_startNode != null) ?
			$"<{typeof(T).Name}> Count={_count} StartNode={_startNode.Value}" :
			$"<{typeof(T).Name}> empty";

		/// <summary>
		/// Checks if the specified value belongs to the set.
		/// </summary>
		/// <param name="item">The value to check for belonging to the set.</param>
		/// <returns>
		/// True if the specified value belongs to the set, or False if it does not.
		/// </returns>
		public bool Contains (T item)
		{
			return _startNode.ContainsItem (item, _comparer);
		}

		/// <summary>
		/// Adds an element to the set.
		/// </summary>
		/// <param name="item">The element to add to the set.</param>
		/// <returns>True if the element is added to the set, False if the set already had an element with this value.</returns>
		public bool Add (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.AddItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (!existsBefore)
			{
				_count++;
			}

			return !existsBefore;
		}

		/// <summary>
		/// Adds an element to the set.
		/// </summary>
		/// <param name="item">The element to add to the set.</param>
		void IAdjustableCollection<T>.Add (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.AddItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (!existsBefore)
			{
				_count++;
			}
		}

		/// <summary>Removes all items from the set.</summary>
		public void Clear ()
		{
			_startNode = null;
			_count = 0;
		}

		/// <summary>
		/// Removes the element from the set.
		/// </summary>
		/// <param name="item">The element to remove.</param>
		/// <returns>True if the element is removed from the set, False if there was no element with the same value in the set.</returns>
		public bool Remove (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.RemoveItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (existsBefore)
			{
				_count--;
			}

			return existsBefore;
		}

		/// <summary>
		/// Removes the element from the set.
		/// </summary>
		/// <param name="item">The element to remove.</param>
		void IAdjustableFiniteSet<T>.Remove (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.RemoveItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (existsBefore)
			{
				_count--;
			}
		}

		/// <summary>
		/// Returns an enumerator for the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}

		/// <summary>
		/// Returns an enumerator for the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}
	}
}
