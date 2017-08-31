using System;
using System.Diagnostics;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Узел двоичного дерева поиска, дополнительно к значению содержащий хэш-функцию значения.
	/// </summary>
	/// <typeparam name="T">Тип значения узла.</typeparam>
	/// <remarks>Значение null является корректным и означает пустое дерево.</remarks>
	[DebuggerDisplay ("Value = {Value}, Height = {Height}")]
	public class AvlBinarySearchHashTreeNode<T> :
		IValueHolder<T>
	{
		private readonly int _hash;
		private readonly T _value;

		private AvlBinarySearchHashTreeNode (int hash, T value)
		{
			_hash = hash;
			_value = value;
		}

		/// <summary>
		/// Получает значение узла.
		/// </summary>
		public T Value => _value;

		/// <summary>
		/// Получает хэш узла.
		/// </summary>
		public int Hash => _hash;

		internal virtual int Height => 1;

		internal static AvlBinarySearchHashTreeNode<T> Create (int hash, T value, AvlBinarySearchHashTreeNode<T> left, AvlBinarySearchHashTreeNode<T> right)
		{
			if ((left == null) && (right == null))
			{
				return new AvlBinarySearchHashTreeNode<T> (hash, value);
			}

			var leftHeight = (left == null) ? 0 : left.Height;
			var rightHeight = (right == null) ? 0 : right.Height;
			var newHeight = Math.Max (leftHeight, rightHeight);
			if ((right == null) || (hash != right.Hash))
			{
				newHeight++; // глубина увеличивается только если не совпадают хэши
			}

			return new IntermediateNode (hash, value, left, right, newHeight);
		}

		[DebuggerTypeProxy (typeof (AvlBinarySearchHashTreeNode<>.DebugView))]
		internal class IntermediateNode : AvlBinarySearchHashTreeNode<T>
		{
			private readonly AvlBinarySearchHashTreeNode<T> _leftSubtree;
			private readonly AvlBinarySearchHashTreeNode<T> _rightSubtree;
			private readonly int _height;

			internal IntermediateNode(int hash, T value, AvlBinarySearchHashTreeNode<T> leftSubtree, AvlBinarySearchHashTreeNode<T> rightSubtree, int height)
				: base(hash, value)
			{
				_leftSubtree = leftSubtree;
				_rightSubtree = rightSubtree;
				_height = height;
			}

			internal AvlBinarySearchHashTreeNode<T> LeftSubtree => _leftSubtree;

			internal AvlBinarySearchHashTreeNode<T> RightSubtree => _rightSubtree;

			internal override int Height => _height;
		}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
		internal sealed class DebugView
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
		{
			private readonly IntermediateNode _node;

			public DebugView (IntermediateNode node)
			{
				_node = node;
			}

			public T[] LeftSubtree
			{
				get
				{
					if (_node.LeftSubtree == null)
					{
						return null;
					}

					var items = new T[_node.LeftSubtree.GetCount ()];
					int i = 0;
					using (var enumerator = _node.LeftSubtree.GetEnumerator ())
					{
						while (enumerator.MoveNext ())
						{
							items[i++] = enumerator.Current;
						}
					}

					return items;
				}
			}

			public T NodeValue => _node.Value;

			public T[] RightSubtree
			{
				get
				{
					if (_node.RightSubtree == null)
					{
						return null;
					}

					var items = new T[_node.RightSubtree.GetCount ()];
					var i = 0;
					using (var enumerator = _node.RightSubtree.GetEnumerator ())
					{
						while (enumerator.MoveNext ())
						{
							items[i++] = enumerator.Current;
						}
					}

					return items;
				}
			}
		}
	}
}
