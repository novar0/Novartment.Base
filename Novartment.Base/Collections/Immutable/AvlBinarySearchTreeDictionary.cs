using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// An immutable (by composition of elements) dictionay based on binary search tree,
	/// automatically self-balancing using the <a href="http://en.wikipedia.org/wiki/AVL_tree">AVL</a> algorithm.
	/// </summary>
	/// <remarks>Synonyms: map, associative array, symbol table, dictionary.</remarks>
	public static class AvlBinarySearchTreeDictionary
	{
		/// <summary>
		/// Gets the number of nodes in the specified tree.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="treeNode">The starting node of the dictionay from which counting begins.</param>
		/// <returns>The number of nodes in the specified dictionay.</returns>
		public static int GetCount<TKey, TValue> (this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode)
		{
			return GetCountInternal (treeNode, 0);
		}

		/// <summary>
		/// Determines whether the dictionary contains an element that has the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="treeNode">The starting node of the dictionary tree where the search is performed.</param>
		/// <param name="key">The key to locate.</param>
		/// <param name="comparer">The comparer to be used when comparing keys in the dictionary.</param>
		/// <returns>
		/// True if the dictionary contains an node that has the specified key;
		/// otherwise, False.
		/// </returns>
		public static bool ContainsKey<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode,
			TKey key,
			IComparer<TKey> comparer)
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
				var num = comparer.Compare (key, treeNode.Key);
				if (num == 0)
				{
					return true;
				}

				if (treeNode is not AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateNode)
				{
					return false;
				}

				treeNode = (num < 0) ? intermediateNode.LeftSubtree : intermediateNode.RightSubtree;
			}
			while (treeNode != null);
			return false;
		}

		/// <summary>
		/// Gets the value that is associated with the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="treeNode">The starting node of the dictionary tree where the value is searched.</param>
		/// <param name="key">The key to locate.</param>
		/// <param name="comparer">The comparer to be used when comparing keys in the dictionary.</param>
		/// <param name="value">
		/// When this method returns, the value associated with the specified key, if the key is found;
		/// otherwise, the default value for the type of the value parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// True if the dictionary contains an node that has the specified key; otherwise, false.
		/// </returns>
		public static bool TryGetValue<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode,
			TKey key,
			IComparer<TKey> comparer,
			out TValue value)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			if (treeNode == null)
			{
				value = default;
				return false;
			}

			do
			{
				var num = comparer.Compare (key, treeNode.Key);
				if (num == 0)
				{
					value = treeNode.Value;
					return true;
				}

				if (treeNode is not AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateNode)
				{
					value = default;
					return false;
				}

				treeNode = (num < 0) ? intermediateNode.LeftSubtree : intermediateNode.RightSubtree;
			}
			while (treeNode != null);
			value = default;
			return false;
		}

		/// <summary>
		/// Creates a new dictionary from the specified one by setting the specified value in the element with the specified key,
		/// adding a new element if necessary.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="treeNode">The starting node of the dictionary tree where the value is set.</param>
		/// <param name="key">The key for setting the value in the dictionary.</param>
		/// <param name="value">The value to set in the dictionary.</param>
		/// <param name="comparer">The comparer to be used when comparing keys in the dictionary.</param>
		/// <returns>
		/// The starting node of the dictionary tree made from the specified one by setting the specified value and possibly adding a new element.
		/// Can be equal to the initial value if the node with the specified key was already in the tree and the tree did not require balancing.
		/// </returns>
		public static AvlBinarySearchTreeDictionaryNode<TKey, TValue> SetValue<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode,
			TKey key,
			TValue value,
			IComparer<TKey> comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			bool existsBefore = false;
			return SetValueInternal (treeNode, key, value, comparer, ref existsBefore);
		}

		/// <summary>
		/// Creates a new dictionary from the specified one by deleting the element with the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="treeNode">The starting node of the dictionary tree to delete from.</param>
		/// <param name="key">The key to delete from the dictionary.</param>
		/// <param name="comparer">The comparer to be used when comparing keys in the dictionary.</param>
		/// <returns>
		/// The starting node of the dictionary tree made from the specified one, removing the node with the specified key.
		/// Can be equal to the initial value if the node with the specified key was not in the tree and the tree did not require balancing.
		/// </returns>
		public static AvlBinarySearchTreeDictionaryNode<TKey, TValue> RemoveKey<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode,
			TKey key,
			IComparer<TKey> comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			bool existsBefore = false;
			return RemoveKeyInternal (treeNode, key, comparer, ref existsBefore);
		}

		/// <summary>
		/// Returns an enumerator for the dictionary tree nodes.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="treeNode">The starting node of the dictionary tree where the enumeration is performed.</param>
		/// <returns>Enumerator of values for dictionary tree nodes.</returns>
		public static IEnumerator<AvlBinarySearchTreeDictionaryNode<TKey, TValue>> GetEnumerator<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode)
		{
			return new BinarySearchTreeEnumerator<TKey, TValue> (treeNode);
		}

		internal static AvlBinarySearchTreeDictionaryNode<TKey, TValue> SetValueInternal<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode,
			TKey key,
			TValue value,
			IComparer<TKey> comparer,
			ref bool existsBefore)
		{
			if (treeNode == null)
			{
				return new AvlBinarySearchTreeDictionaryNode<TKey, TValue>.EndNode (key, value);
			}

			var num = comparer.Compare (key, treeNode.Key);
			if (num == 0)
			{
				treeNode.Value = value;
				existsBefore = true;
				return treeNode;
			}

			if (treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateNode)
			{
				if (num < 0)
				{
					var newLeftSubtree = intermediateNode.LeftSubtree.SetValueInternal (key, value, comparer, ref existsBefore);
					return Recreate (treeNode.Key, treeNode.Value, newLeftSubtree, intermediateNode.RightSubtree);
				}

				var newRightSubtree = intermediateNode.RightSubtree.SetValueInternal (key, value, comparer, ref existsBefore);
				return Recreate (treeNode.Key, treeNode.Value, intermediateNode.LeftSubtree, newRightSubtree);
			}

			return (num < 0) ?
				new AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode (key, value, null, treeNode, 2) :
				new AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode (key, value, treeNode, null, 2);
		}

		internal static AvlBinarySearchTreeDictionaryNode<TKey, TValue> RemoveKeyInternal<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode,
			TKey key,
			IComparer<TKey> comparer,
			ref bool existsBefore)
		{
			if (treeNode == null)
			{
				return null;
			}

			var num = comparer.Compare (key, treeNode.Key);
			if (treeNode is not AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode node)
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
				var newLeftSubtree = RemoveKeyInternal (leftSubtree, key, comparer, ref existsBefore);
				return Recreate (node.Key, node.Value, newLeftSubtree, rightSubtree);
			}

			if (num > 0)
			{
				var newRightSubtree = RemoveKeyInternal (rightSubtree, key, comparer, ref existsBefore);
				return Recreate (node.Key, node.Value, leftSubtree, newRightSubtree);
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
			return CreateNode (tuple.Key, tuple.Value, leftSubtree, tuple.Node);
		}

		private static int GetCountInternal<TKey, TValue> (this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode, int accumulator = 0)
		{
			while (true)
			{
				if (treeNode == null)
				{
					return accumulator;
				}

				if (treeNode is not AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode node)
				{
					return accumulator + 1;
				}

				accumulator = GetCountInternal (node.RightSubtree, accumulator + 1);
				treeNode = node.LeftSubtree;
			}
		}

		private static AvlBinarySearchTreeDictionaryNode<TKey, TValue> Recreate<TKey, TValue> (
			TKey key,
			TValue value,
			AvlBinarySearchTreeDictionaryNode<TKey, TValue> left,
			AvlBinarySearchTreeDictionaryNode<TKey, TValue> right)
		{
			var leftHeight = GetHeight (left);
			var rightHeight = GetHeight (right);

			if (rightHeight > (leftHeight + 2))
			{ // перекос направо, заодно означает что правое поддерево не является единичным узлом
				var rightInterm = (AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode)right;
				var rightLeft = rightInterm.LeftSubtree;
				var rightRight = rightInterm.RightSubtree;
				var rightLeftHeight = GetHeight (rightLeft);
				if (rightLeftHeight <= (leftHeight + 1))
				{
					return CreateNode (
						rightInterm.Key,
						rightInterm.Value,
						CreateNode (key, value, left, rightLeft),
						rightRight);
				}

				var rightLeftInterm = (AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode)rightLeft;
				return CreateNode (
					rightLeftInterm.Key,
					rightLeftInterm.Value,
					CreateNode (key, value, left, rightLeftInterm.LeftSubtree),
					CreateNode (right.Key, right.Value, rightLeftInterm.RightSubtree, rightRight));
			}

			if (leftHeight > (rightHeight + 2))
			{ // перекос налево, заодно означает что левое поддерево не является единичным узлом
				var leftInterm = (AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode)left;
				var leftLeft = leftInterm.LeftSubtree;
				var leftRight = leftInterm.RightSubtree;
				var leftRightHeight = GetHeight (leftRight);
				if (leftRightHeight <= (rightHeight + 1))
				{
					return CreateNode (
						leftInterm.Key,
						leftInterm.Value,
						leftLeft,
						CreateNode (key, value, leftRight, right));
				}

				var leftRightInterm = (AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode)leftRight;
				return CreateNode (
					leftRightInterm.Key,
					leftRightInterm.Value,
					CreateNode (left.Key, left.Value, leftLeft, leftRightInterm.LeftSubtree),
					CreateNode (key, value, leftRightInterm.RightSubtree, right));
			}

			return CreateNode (key, value, left, right);
		}

		private static int GetHeight<TKey, TValue> (this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode)
		{
			if (treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.EndNode)
			{
				return 1;
			}

			var intermediateNode = treeNode as AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode;
			return intermediateNode?.Height ?? 0;
		}

		private static AvlBinarySearchTreeDictionaryNode<TKey, TValue> CreateNode<TKey, TValue> (
			TKey key,
			TValue value,
			AvlBinarySearchTreeDictionaryNode<TKey, TValue> left,
			AvlBinarySearchTreeDictionaryNode<TKey, TValue> right)
		{
			if ((left == null) && (right == null))
			{
				return new AvlBinarySearchTreeDictionaryNode<TKey, TValue>.EndNode (key, value);
			}

			var num = Math.Max (GetHeight (left), GetHeight (right));
			return new AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode (key, value, left, right, num + 1);
		}

		private static KeyValueAndNode<TKey, TValue> CutNode<TKey, TValue> (
			this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode)
		{
			if (treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.EndNode)
			{
				return new KeyValueAndNode<TKey, TValue> (treeNode.Key, treeNode.Value, null);
			}

			var intermediateNode = (AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode)treeNode;
			var left = intermediateNode.LeftSubtree;
			var right = intermediateNode.RightSubtree;
			if (left == null)
			{
				return new KeyValueAndNode<TKey, TValue> (intermediateNode.Key, intermediateNode.Value, right);
			}

			var tuple = CutNode (left);
			var newNode = CreateNode (intermediateNode.Key, intermediateNode.Value, tuple.Node, right);
			return new KeyValueAndNode<TKey, TValue> (tuple.Key, tuple.Value, newNode);
		}

		private readonly ref struct KeyValueAndNode<TKey, TValue>
		{
			internal TKey Key { get; }
			internal TValue Value { get; }
			internal AvlBinarySearchTreeDictionaryNode<TKey, TValue> Node { get; }

			public KeyValueAndNode (TKey key, TValue value, AvlBinarySearchTreeDictionaryNode<TKey, TValue> node)
			{
				this.Key = key;
				this.Value = value;
				this.Node = node;
			}
		}

		internal sealed class BinarySearchTreeEnumerator<TKey, TValue> :
			IEnumerator<AvlBinarySearchTreeDictionaryNode<TKey, TValue>>
		{
			private readonly AvlBinarySearchTreeDictionaryNode<TKey, TValue> _startingNode;
			private SingleLinkedListNode<AvlBinarySearchTreeDictionaryNode<TKey, TValue>> _nodesToExplore;
			private bool _started;

			internal BinarySearchTreeEnumerator (AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode)
			{
				_startingNode = treeNode;
				Reset ();
			}

			/// <summary>
			/// Gets the element in the tree at the current position of the enumerator.
			/// </summary>
			public AvlBinarySearchTreeDictionaryNode<TKey, TValue> Current
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

					return _nodesToExplore.Value;
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
					_nodesToExplore = Flatten (new SingleLinkedListNode<AvlBinarySearchTreeDictionaryNode<TKey, TValue>> (_startingNode));
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

			private static SingleLinkedListNode<AvlBinarySearchTreeDictionaryNode<TKey, TValue>> Flatten (SingleLinkedListNode<AvlBinarySearchTreeDictionaryNode<TKey, TValue>> listNode)
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
						if (treeNode is not AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateTreeNode)
						{
							return listNode;
						}

						listNode = listNode.Next
							.AddItem (intermediateTreeNode.RightSubtree)
							.AddItem (new AvlBinarySearchTreeDictionaryNode<TKey, TValue>.EndNode (treeNode.Key, treeNode.Value))
							.AddItem (intermediateTreeNode.LeftSubtree);
					}
				}

				return null;
			}
		}
	}
}
