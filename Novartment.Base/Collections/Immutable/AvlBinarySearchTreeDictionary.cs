using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Неизменяемый (по составу элементов) словарь на базе двоичного дерева поиска,
	/// автоматически балансирующегося по алгоритму <a href="http://en.wikipedia.org/wiki/AVL_tree">АВЛ</a>.
	/// </summary>
	/// <remarks>Синонимы: ассоциативный массив, словарь, map, associative array, symbol table, dictionary.</remarks>
	public static class AvlBinarySearchTreeDictionary
	{
		/// <summary>
		/// Получает количество элементов в словаре.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
		/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
		/// <param name="treeNode">Начальный узел дерева словаря, в котором производится подсчёт.</param>
		/// <returns>Количество элементов в словаре.</returns>
		public static int GetCount<TKey, TValue> (this AvlBinarySearchTreeDictionaryNode<TKey, TValue> treeNode)
		{
			return GetCountInternal (treeNode, 0);
		}

		/// <summary>
		/// Проверяет наличие в словаре элемента с указанным ключом.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
		/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
		/// <param name="treeNode">Начальный узел дерева словаря, в котором производится поиск.</param>
		/// <param name="key">Ключ для проверки наличия элемента в словаре.</param>
		/// <param name="comparer">Реализация IComparer&lt;TKey&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <returns>
		/// True если элемент с указанным ключом содержится в словаре,
		/// либо False если нет.
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

				if (!(treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateNode))
				{
					return false;
				}

				treeNode = (num < 0) ? intermediateNode.LeftSubtree : intermediateNode.RightSubtree;
			}
			while (treeNode != null);
			return false;
		}

		/// <summary>
		/// Пытается получить из словаря значение элемента с указанным ключом.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
		/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
		/// <param name="treeNode">Начальный узел дерева словаря, в котором производится запрос значения.</param>
		/// <param name="key">Ключ для получения значения элемента в словаре.</param>
		/// <param name="comparer">Реализация IComparer&lt;TKey&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <param name="value">Возвращаемое значение, связанное с указанном ключом, если он найден;
		/// в противном случае — значение по умолчанию для данного типа параметра value.
		/// Этот параметр передается неинициализированным.</param>
		/// <returns>
		/// Значение true, если словарь содержит элемент с указанным ключом;
		/// в противном случае — значение false.
		/// </returns>
		public static bool TryGetItem<TKey, TValue> (
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

				if (!(treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateNode))
				{
					value = default(TValue);
					return false;
				}

				treeNode = (num < 0) ? intermediateNode.LeftSubtree : intermediateNode.RightSubtree;
			}
			while (treeNode != null);
			value = default;
			return false;
		}

		/// <summary>
		/// Создаёт новый словарь из указанного путём установки указанного значения в элементе с указанным ключом,
		/// добавляя новый элемент при необходимости.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
		/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
		/// <param name="treeNode">Начальный узел дерева словаря, в котором производится установка значения.</param>
		/// <param name="key">Ключ для установки значения в словаре.</param>
		/// <param name="value">Значение для установки в словаре.</param>
		/// <param name="comparer">Реализация IComparer&lt;TKey&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <returns>
		/// Начальный узел дерева словаря, полученного из указанного установкой указанного значения и, возможно, добавлением нового элемента.
		/// Может совпадать с начальным значением если элемента с указанным ключом в нём уже был и оно не требовало балансировки.
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
		/// Создаёт новый словарь из указанного путём удалением элемента с указанным ключом.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
		/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
		/// <param name="treeNode">Начальный узел дерева словаря, из которого производится удаление.</param>
		/// <param name="key">Ключ для удаления из словаря.</param>
		/// <param name="comparer">Реализация IComparer&lt;TKey&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <returns>
		/// Начальный узел дерева словаря, полученного из указанного с удалением элемента с указанным ключом.
		/// Может совпадать с начальным значением если элемента с указанным ключом в нём не было и оно не требовало балансировки.
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
		/// Получает перечислитель элементов словаря.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа элементов словаря.</typeparam>
		/// <typeparam name="TValue">Тип значений элементов словаря.</typeparam>
		/// <param name="treeNode">Начальный узел дерева словаря, в котором производится перечисление.</param>
		/// <returns>Перечислитель элементов словаря.</returns>
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
			if (!(treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode node))
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

				if (!(treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode node))
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
				return new KeyValueAndNode<TKey, TValue>
				{
					Key = treeNode.Key,
					Value = treeNode.Value,
					Node = null,
				};
			}

			var intermediateNode = (AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode)treeNode;
			var left = intermediateNode.LeftSubtree;
			var right = intermediateNode.RightSubtree;
			if (left == null)
			{
				return new KeyValueAndNode<TKey, TValue>
				{
					Key = intermediateNode.Key,
					Value = intermediateNode.Value,
					Node = right,
				};
			}

			var tuple = CutNode (left);
			var newNode = CreateNode (intermediateNode.Key, intermediateNode.Value, tuple.Node, right);
			return new KeyValueAndNode<TKey, TValue> ()
			{
				Key = tuple.Key,
				Value = tuple.Value,
				Node = newNode,
			};
		}

		internal struct KeyValueAndNode<TKey, TValue>
		{
			internal TKey Key;
			internal TValue Value;
			internal AvlBinarySearchTreeDictionaryNode<TKey, TValue> Node;
		}

		internal sealed class BinarySearchTreeEnumerator<TKey, TValue> : IEnumerator<AvlBinarySearchTreeDictionaryNode<TKey, TValue>>
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
			/// Получает текущий элемент перечислителя.
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
			/// Перемещает перечислитель к следующему элементу строки.
			/// </summary>
			/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
			/// false, если перечислитель достиг конца.</returns>
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
			/// Возвращает перечислитель в исходное положение.
			/// </summary>
			public void Reset ()
			{
				_nodesToExplore = null;
				_started = false;
			}

			/// <summary>
			/// Ничего не делает.
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
						if (!(treeNode is AvlBinarySearchTreeDictionaryNode<TKey, TValue>.IntermediateNode intermediateTreeNode))
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
