using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Узел двоичного дерева поиска.
	/// </summary>
	/// <typeparam name="T">Тип значения узла.</typeparam>
	/// <remarks>Значение null является корректным и означает пустое дерево.</remarks>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Avl",
		Justification = "'AVL-tree' represents standard term.")]
	public class AvlBinarySearchTreeNode<T> :
		IValueHolder<T>
	{
		private readonly T _value;

		/// <summary>
		/// Получает значение узла.
		/// </summary>
		public T Value => _value;

		private AvlBinarySearchTreeNode (T value)
		{
			_value = value;
		}

		[DebuggerTypeProxy (typeof (AvlBinarySearchTreeNode<>._DebugView))]
		internal class IntermediateNode : AvlBinarySearchTreeNode<T>
		{
			private readonly AvlBinarySearchTreeNode<T> _leftSubtree;
			private readonly AvlBinarySearchTreeNode<T> _rightSubtree;

			private readonly int _height;

			internal int Height => _height;
			internal AvlBinarySearchTreeNode<T> LeftSubtree => _leftSubtree;
			internal AvlBinarySearchTreeNode<T> RightSubtree => _rightSubtree;

			internal IntermediateNode (T value, AvlBinarySearchTreeNode<T> leftSubtree, AvlBinarySearchTreeNode<T> rightSubtree, int height)
				: base (value)
			{
				_leftSubtree = leftSubtree;
				_rightSubtree = rightSubtree;
				_height = height;
			}
		}

		[DebuggerDisplay ("Value = {Value}")]
		internal class EndNode : AvlBinarySearchTreeNode<T>
		{
			internal EndNode (T value)
				: base (value)
			{
			}
		}

		#region class _DebugView

		internal sealed class _DebugView
		{
			private readonly IntermediateNode _node;

			public _DebugView (IntermediateNode node)
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
					int i = 0;
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

		#endregion
	}
}
