using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Reflection;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Sorted set of unique values for concurrent access based on the binary search tree built on hash functions of elements.
	/// </summary>
	/// <typeparam name="T">
	/// The type of the elements.
	/// Must support atomic assignment, that is, be a reference type
	/// or value type no larger than the size of a pointer on the executing platform.
	/// </typeparam>
	/// <remarks>
	/// To construct the tree, it is not the values of the elements that are used, but their hash code.
	/// If there is a value comparer for elements, then ConcurrentTreeSet will work faster with the same functionality.
	/// No synchronization is required for concurrent access.
	/// Does not implement ICollection interface due to incompatibility of its contract with concurrent access.
	/// </remarks>
	public sealed class ConcurrentHashTreeSet<T> :
		IAdjustableFiniteSet<T>
	{
		/*
		для обеспечения конкурентного доступа, всё состояние хранится в одном поле _startNode, а любое его изменение выглядит так:
		SpinWait spinWait = default;
		while (true)
		{
		  var state1 = _startNode;
		  var newState = SomeOperation (state1);
		  var state2 = Interlocked.CompareExchange (ref _startNode, newState, state1);
		  if (state1 == state2)
		  {
		    return;
		  }
		  spinWait.SpinOnce ();
		}
		*/

		private readonly IEqualityComparer<T> _comparer;
		private AvlBinarySearchHashTreeNode<T> _startNode; // null является корректным значением, означает пустое дерево

		/// <summary>
		/// Initializes a new instance of the ConcurrentHashTreeSet class that is empty and uses a specified value comparer.
		/// </summary>
		/// <param name="comparer">
		/// The comparer to be used when comparing values in a set. Specify null-reference to use default comparer for type T.
		/// </param>
		public ConcurrentHashTreeSet (IEqualityComparer<T> comparer = null)
		{
			var isAtomicallyAssignable = ReflectionService.IsAtomicallyAssignable (typeof (T));
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		/// <summary>
		/// Gets the comparer used when comparing values of a set.
		/// </summary>
		public IEqualityComparer<T> Comparer => _comparer;

		/// <summary>
		/// Gets a value indicating whether the set is empty (contains no elements).
		/// </summary>
		public bool IsEmpty => _startNode == null;

		/// <summary>
		/// Gets the number of elements in the set.
		/// </summary>
		/// <remarks>To check for an empty set, use the IsEmpty property.</remarks>
		public int Count => _startNode.GetCount ();

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
		public void Add (T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var oldState = _startNode;
				var newState = oldState.AddItem (item, _comparer);

				// заменяем состояние если оно не изменилось с момента вызова
				var testState = Interlocked.CompareExchange (ref _startNode, newState, oldState);
				if (testState == oldState)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>Removes all items from the set.</summary>
		public void Clear ()
		{
			_startNode = null;
		}

		/// <summary>
		/// Removes the element from the set.
		/// </summary>
		/// <param name="item">The element to remove.</param>
		public void Remove (T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var oldState = _startNode;
				var newState = oldState.RemoveItem (item, _comparer);

				// заменяем состояние если оно не изменилось с момента вызова
				var testState = Interlocked.CompareExchange (ref _startNode, newState, oldState);
				if (testState == oldState)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Returns an enumerator for the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}

		/// <summary>
		/// Returns an enumerator for the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}
	}
}
