using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Односвязный список (семантика стэка).
	/// </summary>
	public static class SingleLinkedList
	{
		/// <summary>
		/// Получает количество узлов в указнном списке.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов списка.</typeparam>
		/// <param name="node">Начальный узел списка, с которого начинается подсчёт.</param>
		/// <returns>Количество узлов в указанном списке.</returns>
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
		/// Создаёт новый список из указанного с добавлением в начало узла с указанным значением.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов списка.</typeparam>
		/// <param name="node">Начальный узел списка.</param>
		/// <param name="value">Значение для добавления в список.</param>
		/// <returns>Новый список, в котором в начало добавлен узел с указанным значением.</returns>
		public static SingleLinkedListNode<T> AddItem<T> (this SingleLinkedListNode<T> node, T value)
		{
			return new SingleLinkedListNode<T> (value, node);
		}

		/// <summary>
		/// Создаёт новый список путём удаления указанного начального узла.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов списка.</typeparam>
		/// <param name="node">Начальный удаляемый узел списка.</param>
		/// <returns>Новый список, в котором удалён указанный узел.</returns>
		public static SingleLinkedListNode<T> RemoveItem<T> (this SingleLinkedListNode<T> node)
		{
			return node?.Next;
		}

		/// <summary>
		/// Получает перечислитель значений узлов списка.
		/// </summary>
		/// <typeparam name="T">Тип значений узлов списка.</typeparam>
		/// <param name="node">Начальный узел списка для перечисления.</param>
		/// <returns>Перечислитель значений узлов списка.</returns>
		public static IEnumerator<T> GetEnumerator<T> (this SingleLinkedListNode<T> node)
		{
			return new _SingleLinkedListEnumerator<T> (node);
		}

		#region class _SingleLinkedListEnumerator<T>

		internal sealed class _SingleLinkedListEnumerator<T> :
			IEnumerator<T>
		{
			private readonly SingleLinkedListNode<T> _startingNode;
			private SingleLinkedListNode<T> _currentNode;
			private bool _started;

			internal _SingleLinkedListEnumerator (SingleLinkedListNode<T> node)
			{
				_startingNode = node;
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
					if (_currentNode == null)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
					}
					return _currentNode.Value;
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
				return (_currentNode != null);
			}

			/// <summary>
			/// Возвращает перечислитель в исходное положение.
			/// </summary>
			public void Reset ()
			{
				_currentNode = null;
				_started = false;
			}

			/// <summary>
			/// Ничего не делает.
			/// </summary>
			public void Dispose () { }
		}

		#endregion
	}
}
