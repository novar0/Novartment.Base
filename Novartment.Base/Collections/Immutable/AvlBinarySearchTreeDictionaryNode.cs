using System;
using System.Diagnostics;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Элемент словаря на базе двоичного дерева поиска.
	/// </summary>
	/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
	/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
	/// <remarks>Значение null является корректным и означает пустой словарь.</remarks>
	public class AvlBinarySearchTreeDictionaryNode<TKey, TValue>
		: IValueHolder<TValue>
	{
		private readonly TKey _key;
		private TValue _value;

		private AvlBinarySearchTreeDictionaryNode(TKey key, TValue value)
		{
			_key = key;
			_value = value;
		}

		/// <summary>
		/// Получает ключ элемента.
		/// </summary>
		public TKey Key => _key;

		/// <summary>
		/// Получает или устанавливает значение элемента.
		/// </summary>
		public TValue Value
		{
			get => _value;

			set
			{
				_value = value;
			}
		}

		[DebuggerTypeProxy (typeof (AvlBinarySearchTreeDictionaryNode<,>.DebugView))]
		internal class IntermediateNode : AvlBinarySearchTreeDictionaryNode<TKey, TValue>
		{
			private readonly AvlBinarySearchTreeDictionaryNode<TKey, TValue> _leftSubtree;
			private readonly AvlBinarySearchTreeDictionaryNode<TKey, TValue> _rightSubtree;
			private readonly int _height;

			internal IntermediateNode (
				TKey key,
				TValue value,
				AvlBinarySearchTreeDictionaryNode<TKey, TValue> leftSubtree,
				AvlBinarySearchTreeDictionaryNode<TKey, TValue> rightSubtree,
				int height)
				: base (key, value)
			{
				_leftSubtree = leftSubtree;
				_rightSubtree = rightSubtree;
				_height = height;
			}

			internal int Height => _height;

			internal AvlBinarySearchTreeDictionaryNode<TKey, TValue> LeftSubtree => _leftSubtree;

			internal AvlBinarySearchTreeDictionaryNode<TKey, TValue> RightSubtree => _rightSubtree;
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
