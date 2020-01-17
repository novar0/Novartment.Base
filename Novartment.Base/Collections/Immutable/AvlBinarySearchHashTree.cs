using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// An immutable binary search tree,
	/// built on hash functions of elements and
	/// automatically self-balancing using the <a href="http://en.wikipedia.org/wiki/AVL_tree">AVL</a> algorithm.
	/// </summary>
	/// <remarks>
	/// Unlike AvlBinarySearchTree, duplicates are allowed (hash function collisions).
	/// </remarks>
	public static class AvlBinarySearchHashTree
	{
		/// <summary>
		/// Gets the number of nodes in the specified tree.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree from which counting begins.</param>
		/// <returns>The number of nodes in the specified tree.</returns>
		public static int GetCount<T> (this AvlBinarySearchHashTreeNode<T> treeNode)
		{
			return GetCountInternal (treeNode, 0);
		}

		/// <summary>
		/// Checks for a node in the tree with the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree in which to search.</param>
		/// <param name="value">The value to check for in the tree.</param>
		/// <param name="comparer">The equality comparer to be used when comparing values in a tree.</param>
		/// <returns>
		/// True if the node with the specified value is contained in the tree, or False if not.
		/// </returns>
		public static bool ContainsItem<T> (this AvlBinarySearchHashTreeNode<T> treeNode, T value, IEqualityComparer<T> comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			if (treeNode == null)
			{
				return false;
			}

			var hash = comparer.GetHashCode (value);
			while ((hash != treeNode.Hash) || !comparer.Equals (value, treeNode.Value))
			{
				if (!(treeNode is AvlBinarySearchHashTreeNode<T>.IntermediateNode intermediateNode))
				{
					return false; // end node
				}

				treeNode = (hash < treeNode.Hash) ? intermediateNode.LeftSubtree : intermediateNode.RightSubtree;
				if (treeNode == null)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Creates a new tree from the specified one with the addition of a node with the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree to add to.</param>
		/// <param name="value">The value to add to the tree.</param>
		/// <param name="comparer">The equality comparer to be used when comparing values in a tree.</param>
		/// <returns>
		/// The starting node of the tree made from the specified one by adding the node with the specified value.
		/// Can be equal to the initial value if the node with the specified value was already in the tree and the tree did not require balancing.
		/// </returns>
		public static AvlBinarySearchHashTreeNode<T> AddItem<T> (
			this AvlBinarySearchHashTreeNode<T> treeNode,
			T value,
			IEqualityComparer<T> comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			bool existsBefore = false;
			return AddItemInternal (treeNode, value, comparer, ref existsBefore);
		}

		/// <summary>
		/// Creates a new tree from the specified one, removing the node with the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree to be deleted from.</param>
		/// <param name="value">The value to remove from the tree.</param>
		/// <param name="comparer">The equality comparer to be used when comparing values in a tree.</param>
		/// <returns>
		/// The starting node of the tree made from the specified one, removing the node with the specified value.
		/// Can be equal to the initial value if the node with the specified value was not in the tree and the tree did not require balancing.
		/// </returns>
		public static AvlBinarySearchHashTreeNode<T> RemoveItem<T> (
			this AvlBinarySearchHashTreeNode<T> treeNode,
			T value,
			IEqualityComparer<T> comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			bool existsBefore = false;
			return RemoveItemInternal (treeNode, value, comparer, ref existsBefore);
		}

		/// <summary>
		/// Returns an enumerator for the tree.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree.</param>
		/// <returns>Enumerator of values for tree nodes.</returns>
		public static IEnumerator<T> GetEnumerator<T> (this AvlBinarySearchHashTreeNode<T> treeNode)
		{
			return new BinarySearchHashTreeEnumerator<T> (treeNode);
		}

		internal static AvlBinarySearchHashTreeNode<T> AddItemInternal<T> (
			this AvlBinarySearchHashTreeNode<T> treeNode,
			T value,
			IEqualityComparer<T> comparer,
			ref bool existsBefore)
		{
			var hash = comparer.GetHashCode (value);

			if (treeNode == null)
			{
				return AvlBinarySearchHashTreeNode<T>.Create (hash, value, null, null);
			}

			var isValueEqualsTreeNode = (hash == treeNode.Hash) && comparer.Equals (value, treeNode.Value);
			if (isValueEqualsTreeNode)
			{
				existsBefore = true;
				return treeNode;
			}

			AvlBinarySearchHashTreeNode<T> result;
			if (treeNode is AvlBinarySearchHashTreeNode<T>.IntermediateNode intermediateNode)
			{ // промежуточный узел
				var leftSubtree = intermediateNode.LeftSubtree;
				var rightSubtree = intermediateNode.RightSubtree;
				if (hash < treeNode.Hash)
				{
					var newLeftSubtree = leftSubtree.AddItemInternal (value, comparer, ref existsBefore);
					result = Recreate (treeNode.Hash, treeNode.Value, newLeftSubtree, rightSubtree);
				}
				else
				{
					var newRightSubtree = rightSubtree.AddItemInternal (value, comparer, ref existsBefore);
					result = Recreate (treeNode.Hash, treeNode.Value, leftSubtree, newRightSubtree);
				}
			}
			else
			{ // крайний узел
				if (hash < treeNode.Hash)
				{
					result = AvlBinarySearchHashTreeNode<T>.Create (hash, value, null, treeNode);
				}
				else
				{
					result = hash == treeNode.Hash ?
						AvlBinarySearchHashTreeNode<T>.Create (treeNode.Hash, treeNode.Value, null, AvlBinarySearchHashTreeNode<T>.Create (hash, value, null, null)) :
						AvlBinarySearchHashTreeNode<T>.Create (hash, value, treeNode, null);
				}
			}

			return result;
		}

		internal static AvlBinarySearchHashTreeNode<T> RemoveItemInternal<T> (
			this AvlBinarySearchHashTreeNode<T> treeNode,
			T value,
			IEqualityComparer<T> comparer,
			ref bool existsBefore)
		{
			if (treeNode == null)
			{
				return null;
			}

			var hash = comparer.GetHashCode (value);
			if (treeNode is AvlBinarySearchHashTreeNode<T>.IntermediateNode intermediateNode)
			{ // промежуточный узел
				var leftSubtree = intermediateNode.LeftSubtree;
				var rightSubtree = intermediateNode.RightSubtree;
				var isValueEuqalsTreeNode = (hash == treeNode.Hash) && comparer.Equals (value, treeNode.Value);
				if (isValueEuqalsTreeNode)
				{
					existsBefore = true;
					if (leftSubtree == null)
					{
						return rightSubtree;
					}

					return rightSubtree == null ?
						leftSubtree :
						AvlBinarySearchHashTreeNode<T>.Create (rightSubtree.Hash, rightSubtree.Value, leftSubtree, CutNode (rightSubtree));
				}

				if (hash < treeNode.Hash)
				{
					var newLeftSubtree = leftSubtree.RemoveItemInternal (value, comparer, ref existsBefore);
					return Recreate (treeNode.Hash, treeNode.Value, newLeftSubtree, rightSubtree);
				}

				var newRightSubtree = rightSubtree.RemoveItemInternal (value, comparer, ref existsBefore);
				return Recreate (treeNode.Hash, treeNode.Value, leftSubtree, newRightSubtree);
			}

			// крайний узел
			var isValueEqualsTreeNode = (hash == treeNode.Hash) && comparer.Equals (value, treeNode.Value);
			if (isValueEqualsTreeNode)
			{
				existsBefore = true;
				return null; // удалили последний узел, возвращаем пустое дерево
			}

			return treeNode;
		}

		private static AvlBinarySearchHashTreeNode<T> Recreate<T> (int hash, T value, AvlBinarySearchHashTreeNode<T> left, AvlBinarySearchHashTreeNode<T> right)
		{
			var leftHeight = (left == null) ? 0 : left.Height;
			var rightHeight = (right == null) ? 0 : right.Height;

			if (rightHeight > (leftHeight + 2))
			{ // перекос направо, заодно означает что правое поддерево является промежуточным узлом
				var rightInterm = (AvlBinarySearchHashTreeNode<T>.IntermediateNode)right;
				var rightLeft = rightInterm.LeftSubtree;
				var rightRight = rightInterm.RightSubtree;
				if (((rightLeft == null) ? 0 : rightLeft.Height) <= (leftHeight + 1))
				{
					return AvlBinarySearchHashTreeNode<T>.Create (
						right.Hash,
						right.Value,
						AvlBinarySearchHashTreeNode<T>.Create (hash, value, left, rightLeft),
						rightRight);
				}

				var rightLeftInterm = (AvlBinarySearchHashTreeNode<T>.IntermediateNode)rightLeft;
				return AvlBinarySearchHashTreeNode<T>.Create (
					rightLeftInterm.Hash,
					rightLeftInterm.Value,
					AvlBinarySearchHashTreeNode<T>.Create (hash, value, left, rightLeftInterm.LeftSubtree),
					AvlBinarySearchHashTreeNode<T>.Create (right.Hash, right.Value, rightLeftInterm.RightSubtree, rightRight));
			}

			if (leftHeight > (rightHeight + 2))
			{ // перекос налево, заодно означает что левое поддерево является промежуточным узлом
				var leftInterm = (AvlBinarySearchHashTreeNode<T>.IntermediateNode)left;
				var leftLeft = leftInterm.LeftSubtree;
				var leftRight = leftInterm.RightSubtree;
				if (((leftRight == null) ? 0 : leftRight.Height) <= (rightHeight + 1))
				{
					return AvlBinarySearchHashTreeNode<T>.Create (
						left.Hash,
						left.Value,
						leftLeft,
						AvlBinarySearchHashTreeNode<T>.Create (hash, value, leftRight, right));
				}

				var leftRightInterm = (AvlBinarySearchHashTreeNode<T>.IntermediateNode)leftRight;
				return AvlBinarySearchHashTreeNode<T>.Create (
					leftRightInterm.Hash,
					leftRightInterm.Value,
					AvlBinarySearchHashTreeNode<T>.Create (left.Hash, left.Value, leftLeft, leftRightInterm.LeftSubtree),
					AvlBinarySearchHashTreeNode<T>.Create (hash, value, leftRightInterm.RightSubtree, right));
			}

			return AvlBinarySearchHashTreeNode<T>.Create (hash, value, left, right);
		}

		private static AvlBinarySearchHashTreeNode<T> CutNode<T> (this AvlBinarySearchHashTreeNode<T> treeNode)
		{
			if (!(treeNode is AvlBinarySearchHashTreeNode<T>.IntermediateNode intermediateNode))
			{
				return null;
			}

			var left = intermediateNode.LeftSubtree;
			var right = intermediateNode.RightSubtree;
			return left == null ?
				right :
				AvlBinarySearchHashTreeNode<T>.Create (treeNode.Hash, treeNode.Value, CutNode (left), right);
		}

		private static int GetCountInternal<T> (this AvlBinarySearchHashTreeNode<T> treeNode, int accumulator)
		{
			while (true)
			{
				if (treeNode == null)
				{
					return accumulator;
				}

				if (!(treeNode is AvlBinarySearchHashTreeNode<T>.IntermediateNode node))
				{
					return accumulator + 1;
				}

				accumulator = GetCountInternal (node.RightSubtree, accumulator + 1);
				treeNode = node.LeftSubtree;
			}
		}

		internal sealed class BinarySearchHashTreeEnumerator<T> :
			IEnumerator<T>
		{
			private readonly AvlBinarySearchHashTreeNode<T> _startingNode;
			private SingleLinkedListNode<AvlBinarySearchHashTreeNode<T>> _nodesToExplore;
			private bool _started;

			internal BinarySearchHashTreeEnumerator (AvlBinarySearchHashTreeNode<T> treeNode)
			{
				_startingNode = treeNode;
				Reset ();
			}

			/// <summary>
			/// Gets the element in the tree at the current position of the enumerator.
			/// </summary>
			public T Current
			{
				get
				{
					if (!_started)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
					}

					if (_nodesToExplore == null)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
					}

					return _nodesToExplore.Value.Value;
				}
			}

			object IEnumerator.Current => this.Current;

			/// <summary>
			/// Advances the enumerator to the next element of the tree.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element;
			/// false if the enumerator has passed the end of the tree.</returns>
			public bool MoveNext ()
			{
				if (!_started)
				{
					_started = true;
					_nodesToExplore = Flatten (new SingleLinkedListNode<AvlBinarySearchHashTreeNode<T>> (_startingNode));
				}
				else
				{
					if (_nodesToExplore == null)
					{
						return false;
					}

					_nodesToExplore = Flatten (_nodesToExplore.Next);
				}

				return _nodesToExplore != null;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the tree.
			/// </summary>
			public void Reset ()
			{
				_nodesToExplore = null;
				_started = false;
			}

			/// <summary>
			/// Performs resources releasing.
			/// </summary>
			public void Dispose ()
			{
			}

			private static SingleLinkedListNode<AvlBinarySearchHashTreeNode<T>> Flatten (SingleLinkedListNode<AvlBinarySearchHashTreeNode<T>> listNode)
			{
				while (listNode != null)
				{
					var treeNode = listNode.Value;
					if (treeNode == null)
					{
						listNode = listNode.Next;
					}
					else
					{
						if (!(treeNode is AvlBinarySearchHashTreeNode<T>.IntermediateNode intermediateTreeNode))
						{
							return listNode;
						}

						listNode = listNode.Next
							.AddItem (intermediateTreeNode.RightSubtree)
							.AddItem (AvlBinarySearchHashTreeNode<T>.Create (treeNode.Hash, treeNode.Value, null, null))
							.AddItem (intermediateTreeNode.LeftSubtree);
					}
				}

				return null;
			}
		}
	}
}
