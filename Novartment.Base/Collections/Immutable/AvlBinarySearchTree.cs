using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// An immutable binary search tree,
	/// automatically self-balancing using the <a href="http://en.wikipedia.org/wiki/AVL_tree">AVL</a> algorithm.
	/// </summary>
	public static class AvlBinarySearchTree
	{
		/// <summary>
		/// Gets the number of nodes in the specified tree.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree from which counting begins.</param>
		/// <returns>The number of nodes in the specified tree.</returns>
		public static int GetCount<T> (this AvlBinarySearchTreeNode<T> treeNode)
		{
			return GetCountInternal (treeNode, 0);
		}

		/// <summary>
		/// Checks for a node in the tree with the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree in which to search.</param>
		/// <param name="value">The value to check for in the tree.</param>
		/// <param name="comparer">The comparer to be used when comparing values in a tree.</param>
		/// <returns>
		/// True if the node with the specified value is contained in the tree, or False if not.
		/// </returns>
		public static bool ContainsItem<T> (this AvlBinarySearchTreeNode<T> treeNode, T value, IComparer<T> comparer)
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

			do
			{
				var num = comparer.Compare (value, treeNode.Value);
				if (num == 0)
				{
					return true;
				}

				if (treeNode is not AvlBinarySearchTreeNode<T>.IntermediateNode intermediateNode)
				{
					return false;
				}

				treeNode = (num < 0) ? intermediateNode.LeftSubtree : intermediateNode.RightSubtree;
			}
			while (treeNode != null);
			return false;
		}

		/// <summary>
		/// Creates a new tree from the specified one with the addition of a node with the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value of the nodes.</typeparam>
		/// <param name="treeNode">The starting node of the tree to add to.</param>
		/// <param name="value">The value to add to the tree.</param>
		/// <param name="comparer">The comparer to be used when comparing values in a tree.</param>
		/// <returns>
		/// The starting node of the tree made from the specified one by adding the node with the specified value.
		/// Can be equal to the initial value if the node with the specified value was already in the tree and the tree did not require balancing.
		/// </returns>
		public static AvlBinarySearchTreeNode<T> AddItem<T> (
			this AvlBinarySearchTreeNode<T> treeNode,
			T value,
			IComparer<T> comparer)
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
		/// <param name="comparer">The comparer to be used when comparing values in a tree.</param>
		/// <returns>
		/// The starting node of the tree made from the specified one, removing the node with the specified value.
		/// Can be equal to the initial value if the node with the specified value was not in the tree and the tree did not require balancing.
		/// </returns>
		public static AvlBinarySearchTreeNode<T> RemoveItem<T> (
			this AvlBinarySearchTreeNode<T> treeNode,
			T value,
			IComparer<T> comparer)
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
		public static IEnumerator<T> GetEnumerator<T> (this AvlBinarySearchTreeNode<T> treeNode)
		{
			return new BinarySearchTreeEnumerator<T> (treeNode);
		}

		internal static AvlBinarySearchTreeNode<T> AddItemInternal<T> (
			this AvlBinarySearchTreeNode<T> treeNode,
			T value,
			IComparer<T> comparer,
			ref bool existsBefore)
		{
			if (treeNode == null)
			{
				return new AvlBinarySearchTreeNode<T>.EndNode (value);
			}

			var num = comparer.Compare (value, treeNode.Value);
			if (num == 0)
			{
				existsBefore = true;
				return treeNode;
			}

			if (treeNode is AvlBinarySearchTreeNode<T>.IntermediateNode intermediateNode)
			{
				if (num < 0)
				{
					var newLeftSubtree = intermediateNode.LeftSubtree.AddItemInternal (value, comparer, ref existsBefore);
					return Recreate (treeNode.Value, newLeftSubtree, intermediateNode.RightSubtree);
				}

				var newRightSubtree = intermediateNode.RightSubtree.AddItemInternal (value, comparer, ref existsBefore);
				return Recreate (treeNode.Value, intermediateNode.LeftSubtree, newRightSubtree);
			}

			return (num < 0) ?
				new AvlBinarySearchTreeNode<T>.IntermediateNode (value, null, treeNode, 2) :
				new AvlBinarySearchTreeNode<T>.IntermediateNode (value, treeNode, null, 2);
		}

		internal static AvlBinarySearchTreeNode<T> RemoveItemInternal<T> (
			this AvlBinarySearchTreeNode<T> treeNode,
			T value,
			IComparer<T> comparer,
			ref bool existsBefore)
		{
			if (treeNode == null)
			{
				return null;
			}

			var num = comparer.Compare (value, treeNode.Value);
			if (treeNode is not AvlBinarySearchTreeNode<T>.IntermediateNode node)
			{
				if (num != 0)
				{
					return treeNode;
				}

				existsBefore = true;
				return null;
			}

			var leftSubtree = node.LeftSubtree;
			var rightSubtree = node.RightSubtree;
			if (num < 0)
			{
				var newLeftSubtree = RemoveItemInternal (leftSubtree, value, comparer, ref existsBefore);
				return Recreate (node.Value, newLeftSubtree, rightSubtree);
			}

			if (num > 0)
			{
				var newRightSubtree = RemoveItemInternal (rightSubtree, value, comparer, ref existsBefore);
				return Recreate (node.Value, leftSubtree, newRightSubtree);
			}

			existsBefore = true;
			if (leftSubtree == null)
			{
				return rightSubtree;
			}

			if (rightSubtree == null)
			{
				return leftSubtree;
			}

			var tuple = CutNode (rightSubtree);
			return CreateNode (tuple.Value, leftSubtree, tuple.Node);
		}

		private static AvlBinarySearchTreeNode<T> Recreate<T> (T value, AvlBinarySearchTreeNode<T> left, AvlBinarySearchTreeNode<T> right)
		{
			var leftHeight = GetHeight (left);
			var rightHeight = GetHeight (right);

			if (rightHeight > (leftHeight + 2))
			{ // перекос направо, заодно означает что правое поддерево не является единичным узлом
				var rightInterm = (AvlBinarySearchTreeNode<T>.IntermediateNode)right;
				var rightLeft = rightInterm.LeftSubtree;
				var rightRight = rightInterm.RightSubtree;
				var rightLeftHeight = GetHeight (rightLeft);
				if (rightLeftHeight <= (leftHeight + 1))
				{
					return CreateNode (
						rightInterm.Value,
						CreateNode (value, left, rightLeft),
						rightRight);
				}

				var rightLeftInterm = (AvlBinarySearchTreeNode<T>.IntermediateNode)rightLeft;
				return CreateNode (
					rightLeftInterm.Value,
					CreateNode (value, left, rightLeftInterm.LeftSubtree),
					CreateNode (right.Value, rightLeftInterm.RightSubtree, rightRight));
			}

			if (leftHeight > (rightHeight + 2))
			{ // перекос налево, заодно означает что левое поддерево не является единичным узлом
				var leftInterm = (AvlBinarySearchTreeNode<T>.IntermediateNode)left;
				var leftLeft = leftInterm.LeftSubtree;
				var leftRight = leftInterm.RightSubtree;
				var leftRightHeight = GetHeight (leftRight);
				if (leftRightHeight <= (rightHeight + 1))
				{
					return CreateNode (
						leftInterm.Value,
						leftLeft,
						CreateNode (value, leftRight, right));
				}

				var leftRightInterm = (AvlBinarySearchTreeNode<T>.IntermediateNode)leftRight;
				return CreateNode (
					leftRightInterm.Value,
					CreateNode (left.Value, leftLeft, leftRightInterm.LeftSubtree),
					CreateNode (value, leftRightInterm.RightSubtree, right));
			}

			return CreateNode (value, left, right);
		}

		private static int GetHeight<T> (this AvlBinarySearchTreeNode<T> treeNode)
		{
			if (treeNode is AvlBinarySearchTreeNode<T>.EndNode)
			{
				return 1;
			}

			var intermediateNode = treeNode as AvlBinarySearchTreeNode<T>.IntermediateNode;
			return intermediateNode?.Height ?? 0;
		}

		private static AvlBinarySearchTreeNode<T> CreateNode<T> (T value, AvlBinarySearchTreeNode<T> left, AvlBinarySearchTreeNode<T> right)
		{
			if ((left == null) && (right == null))
			{
				return new AvlBinarySearchTreeNode<T>.EndNode (value);
			}

			var num = Math.Max (GetHeight (left), GetHeight (right));
			return new AvlBinarySearchTreeNode<T>.IntermediateNode (value, left, right, num + 1);
		}

		private static ValueAndNode<T> CutNode<T> (this AvlBinarySearchTreeNode<T> treeNode)
		{
			if (treeNode is AvlBinarySearchTreeNode<T>.EndNode)
			{
				return new ValueAndNode<T> (treeNode.Value, null);
			}

			var intermediateNode = (AvlBinarySearchTreeNode<T>.IntermediateNode)treeNode;
			var left = intermediateNode.LeftSubtree;
			var right = intermediateNode.RightSubtree;
			if (left == null)
			{
				return new ValueAndNode<T> (intermediateNode.Value, right);
			}

			var tuple = CutNode (left);
			var newNode = CreateNode (intermediateNode.Value, tuple.Node, right);
			return new ValueAndNode<T> (tuple.Value, newNode);
		}

		private static int GetCountInternal<T> (this AvlBinarySearchTreeNode<T> treeNode, int accumulator = 0)
		{
			while (true)
			{
				if (treeNode == null)
				{
					return accumulator;
				}

				if (treeNode is not AvlBinarySearchTreeNode<T>.IntermediateNode node)
				{
					return accumulator + 1;
				}

				accumulator = GetCountInternal (node.RightSubtree, accumulator + 1);
				treeNode = node.LeftSubtree;
			}
		}

		private readonly ref struct ValueAndNode<T>
		{
			internal T Value { get; }
			internal AvlBinarySearchTreeNode<T> Node { get; }

			public ValueAndNode (T value, AvlBinarySearchTreeNode<T> node)
			{
				this.Value = value;
				this.Node = node;
			}
		}

		internal sealed class BinarySearchTreeEnumerator<T> :
			IEnumerator<T>
		{
			private readonly AvlBinarySearchTreeNode<T> _startingNode;
			private SingleLinkedListNode<AvlBinarySearchTreeNode<T>> _nodesToExplore;
			private bool _started;

			internal BinarySearchTreeEnumerator (AvlBinarySearchTreeNode<T> treeNode)
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
					_nodesToExplore = Flatten (new SingleLinkedListNode<AvlBinarySearchTreeNode<T>> (_startingNode));
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

			private static SingleLinkedListNode<AvlBinarySearchTreeNode<T>> Flatten (SingleLinkedListNode<AvlBinarySearchTreeNode<T>> listNode)
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
						if (treeNode is not AvlBinarySearchTreeNode<T>.IntermediateNode intermediateTreeNode)
						{
							return listNode;
						}

						listNode = listNode.Next
							.AddItem (intermediateTreeNode.RightSubtree)
							.AddItem (new AvlBinarySearchTreeNode<T>.EndNode (treeNode.Value))
							.AddItem (intermediateTreeNode.LeftSubtree);
					}
				}

				return null;
			}
		}
	}
}
