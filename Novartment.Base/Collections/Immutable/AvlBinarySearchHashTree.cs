using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Неизменяемое двоичное дерево поиска,
	/// построенное на хэш-функциях элементов.
	/// автоматически балансирующееся по алгоритму <a href="http://en.wikipedia.org/wiki/AVL_tree">АВЛ</a>.
	/// </summary>
	/// <remarks>
	/// В отличии от BinarySearchTree допускаются повторы (коллизии хэш-функции).
	/// </remarks>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Avl",
		Justification = "'AVL-tree' represents standard term.")]
	public static class AvlBinarySearchHashTree
	{
		/// <summary>
		/// Получает количество узлов в дереве.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов дерева.</typeparam>
		/// <param name="treeNode">Начальный узел дерева, в котором производится подсчёт.</param>
		/// <returns>Количество узлов в дереве.</returns>
		public static int GetCount<T> (this AvlBinarySearchHashTreeNode<T> treeNode)
		{
			return GetCountInternal (treeNode, 0);
		}

		/// <summary>
		/// Проверяет наличие в дереве узла с указанным значением.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов дерева.</typeparam>
		/// <param name="treeNode">Начальный узел дерева, в котором производится поиск.</param>
		/// <param name="value">Значение для проверки наличия в дереве.</param>
		/// <param name="comparer">Реализация IEqualityComparer&lt;T&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <returns>
		/// True если узел с указанным значением содержится в дереве,
		/// либо False если нет.
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
				var intermediateNode = treeNode as AvlBinarySearchHashTreeNode<T>.IntermediateNode;
				if (intermediateNode == null)
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
		/// Создаёт новое дерево из указанного путём добавлением узла с указанным значением.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов дерева.</typeparam>
		/// <param name="treeNode">Начальный узел дерева, в которое производится добавление.</param>
		/// <param name="value">Значение для добавления в дерево.</param>
		/// <param name="comparer">Реализация IEqualityComparer&lt;T&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <returns>
		/// Начальный узел дерева, полученного из указанного добавлением узла с указанным значением.
		/// Может совпадать с начальным значением если узел с указанным значением уже был в нём и оно не требовало балансировки.
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
		/// Создаёт новое дерево из указанного путём удалением узла с указанным значением.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов дерева.</typeparam>
		/// <param name="treeNode">Начальный узел дерева, из которого производится удаление.</param>
		/// <param name="value">Значение для удаления из дерева.</param>
		/// <param name="comparer">Реализация IEqualityComparer&lt;T&gt;, которую следует использовать при сравнении значений узлов.</param>
		/// <returns>
		/// Начальный узел дерева, полученного из указанного путём удалением узла с указанным значением.
		/// Может совпадать с начальным значением если узла с указанным значением в нём не было и оно не требовало балансировки.
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
		/// Получает перечислитель элементов словаря.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов дерева.</typeparam>
		/// <param name="treeNode">Начальный узел дерева, в котором производится перечисление.</param>
		/// <returns>Перечислитель значений узлов дерева.</returns>
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
			var intermediateNode = treeNode as AvlBinarySearchHashTreeNode<T>.IntermediateNode;
			if (intermediateNode == null)
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

				var node = treeNode as AvlBinarySearchHashTreeNode<T>.IntermediateNode;
				if (node == null)
				{
					return accumulator + 1;
				}

				accumulator = GetCountInternal (node.RightSubtree, accumulator + 1);
				treeNode = node.LeftSubtree;
			}
		}

		internal sealed class BinarySearchHashTreeEnumerator<T> : IEnumerator<T>
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
			/// Получает текущий элемент перечислителя.
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
			/// Перемещает перечислитель к следующему элементу строки.
			/// </summary>
			/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
			/// false, если перечислитель достиг конца.</returns>
			public bool MoveNext ()
			{
				if (!_started)
				{
					_started = true;
					_nodesToExplore = Flatten (new SingleLinkedListNode<AvlBinarySearchHashTreeNode<T>> (_startingNode, null));
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
						var intermediateTreeNode = treeNode as AvlBinarySearchHashTreeNode<T>.IntermediateNode;
						if (intermediateTreeNode == null)
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
