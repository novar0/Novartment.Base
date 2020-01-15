using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Extension methods for implementing single-linked lists from SingleLinkedListNode class.
	/// </summary>
	public static class SingleLinkedList
	{
		/// <summary>
		/// Gets the number of nodes in the specified list.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="node">The starting node of the list from which counting begins.</param>
		/// <returns>The number of nodes in the specified list.</returns>
		public static int GetCount<T> (this SingleLinkedListNode<T> node)
		{
			int acc = 0;
			while (node != null)
			{
				acc++;
				node = node.Next;
			}

			return acc;
		}

		/// <summary>
		/// Creates a new list from the specified list with the node with the specified value added to the beginning.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="node">The starting node of the list.</param>
		/// <param name="value">The value to add to the list.</param>
		/// <returns>A new list in which a node with the specified value is added to the beginning.</returns>
		public static SingleLinkedListNode<T> AddItem<T> (this SingleLinkedListNode<T> node, T value)
		{
			return new SingleLinkedListNode<T> (value, node);
		}

		/// <summary>
		/// Creates a new list by deleting the specified start node.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="node">The starting node of the list to be deleted.</param>
		/// <returns>A new list in which the specified node is deleted.</returns>
		public static SingleLinkedListNode<T> RemoveItem<T> (this SingleLinkedListNode<T> node)
		{
			return node?.Next;
		}

		/// <summary>
		/// Returns an enumerator for the list.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="node">The starting node of the list.</param>
		/// <returns>Enumerator of values for list nodes.</returns>
		public static IEnumerator<T> GetEnumerator<T> (this SingleLinkedListNode<T> node)
		{
			return new SingleLinkedListEnumerator<T> (node);
		}

		internal sealed class SingleLinkedListEnumerator<T> :
			IEnumerator<T>
		{
			private readonly SingleLinkedListNode<T> _startingNode;
			private SingleLinkedListNode<T> _currentNode;
			private bool _started;

			internal SingleLinkedListEnumerator (SingleLinkedListNode<T> node)
			{
				_startingNode = node;
				Reset ();
			}

			/// <summary>
			/// Gets the element in the list at the current position of the enumerator.
			/// </summary>
			public T Current
			{
				get
				{
					if (!_started)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
					}

					if (_currentNode == null)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
					}

					return _currentNode.Value;
				}
			}

			object IEnumerator.Current => this.Current;

			/// <summary>
			/// Advances the enumerator to the next element of the list.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element;
			/// false if the enumerator has passed the end of the list.</returns>
			public bool MoveNext ()
			{
				if (!_started)
				{
					_started = true;
					_currentNode = _startingNode;
				}
				else
				{
					if (_currentNode == null)
					{
						return false;
					}

					_currentNode = _currentNode.Next;
				}

				return _currentNode != null;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the list.
			/// </summary>
			public void Reset ()
			{
				_currentNode = null;
				_started = false;
			}

			/// <summary>
			/// Performs resources releasing.
			/// </summary>
			public void Dispose ()
			{
			}
		}
	}
}
