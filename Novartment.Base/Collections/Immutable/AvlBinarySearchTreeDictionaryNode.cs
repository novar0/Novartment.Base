using System;
using System.Diagnostics;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Dictionary node based on a binary search tree.
	/// </summary>
	/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
	/// <remarks>
	/// A null-value is correct and means an empty dictionary.
	/// Semantically equivalent to System.Collections.Generic.KeyValuePair, but is a reference type to guarantee atomicity of assignments.
	/// </remarks>
	public class AvlBinarySearchTreeDictionaryNode<TKey, TValue>
		: IValueHolder<TValue>
	{
		private AvlBinarySearchTreeDictionaryNode (TKey key, TValue value)
		{
			this.Key = key;
			this.Value = value;
		}

		/// <summary>
		/// Gets the key of the node.
		/// </summary>
		public TKey Key { get; }

		/// <summary>
		/// Gets or sets the value of the node.
		/// </summary>
		public TValue Value { get; set; }

		[DebuggerTypeProxy (typeof (AvlBinarySearchTreeDictionaryNode<,>.DebugView))]
		internal class IntermediateNode : AvlBinarySearchTreeDictionaryNode<TKey, TValue>
		{
			internal IntermediateNode (
				TKey key,
				TValue value,
				AvlBinarySearchTreeDictionaryNode<TKey, TValue> leftSubtree,
				AvlBinarySearchTreeDictionaryNode<TKey, TValue> rightSubtree,
				int height)
				: base (key, value)
			{
				this.LeftSubtree = leftSubtree;
				this.RightSubtree = rightSubtree;
				this.Height = height;
			}

			internal int Height { get; }

			internal AvlBinarySearchTreeDictionaryNode<TKey, TValue> LeftSubtree { get; }

			internal AvlBinarySearchTreeDictionaryNode<TKey, TValue> RightSubtree { get; }
		}

		[DebuggerDisplay ("Key={Key}, Value = {Value}")]
		internal class EndNode : AvlBinarySearchTreeDictionaryNode<TKey, TValue>
		{
			internal EndNode (TKey key, TValue value)
				: base (key, value)
			{
			}
		}

		internal sealed class DebugView
		{
			private readonly IntermediateNode _node;

			public DebugView (IntermediateNode node)
			{
				_node = node;
			}

			public Tuple<TKey, TValue>[] LeftSubtree
			{
				get
				{
					if (_node.LeftSubtree == null)
					{
						return null;
					}

					var items = new Tuple<TKey, TValue>[_node.LeftSubtree.GetCount ()];
					int i = 0;
					using (var enumerator = _node.LeftSubtree.GetEnumerator ())
					{
						while (enumerator.MoveNext ())
						{
							items[i++] = Tuple.Create (enumerator.Current.Key, enumerator.Current.Value);
						}
					}

					return items;
				}
			}

			public TKey Key => _node.Key;

			public TValue Value => _node.Value;

			public Tuple<TKey, TValue>[] RightSubtree
			{
				get
				{
					if (_node.RightSubtree == null)
					{
						return null;
					}

					var items = new Tuple<TKey, TValue>[_node.RightSubtree.GetCount ()];
					int i = 0;
					using (var enumerator = _node.RightSubtree.GetEnumerator ())
					{
						while (enumerator.MoveNext ())
						{
							items[i++] = Tuple.Create (enumerator.Current.Key, enumerator.Current.Value);
						}
					}

					return items;
				}
			}
		}
	}
}
