using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Множество уникальных значений на основе двоичного дерева поиска, построенного на хэш-функциях элементов.
	/// </summary>
	/// <typeparam name="T">Тип элементов множества.</typeparam>
	/// <remarks>
	/// Для построения дерева используются не значения элементов, а их хэш-код.
	/// Если для элементов существует компаратор на больше/меньше,
	/// то AvlTreeSet при той же функциональности будет работать быстрее.
	/// В дереве могут содержаться дупликаты (хэшей, но не значений),
	/// так как хэш-функция может порождать совпадающие хэши для разных значений.
	/// </remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class AvlHashTreeSet<T> :
		IAdjustableFiniteSet<T>
	{
		private readonly IEqualityComparer<T> _comparer;
		private AvlBinarySearchHashTreeNode<T> _startNode = null; // null является корректным значением, означает пустое дерево
		private int _count = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса AvlHashTreeSet,
		/// использующий указанный компаратор значений множества.
		/// </summary>
		/// <param name="comparer">
		/// Компаратор значений множества,
		/// или null чтобы использовать компаратор по умолчанию.
		/// </param>
		public AvlHashTreeSet (IEqualityComparer<T> comparer = null)
		{
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса AvlHashTreeSet на основе указанного двоичного дерева поиска,
		/// использующий указанный компаратор значений множества.
		/// </summary>
		/// <param name="startNode">Начальный узел двоичного дерева поиска, которое станет содержимым множества.</param>
		/// <param name="comparer">
		/// Компаратор значений множества,
		/// или null чтобы использовать компаратор по умолчанию.
		/// </param>
		public AvlHashTreeSet (AvlBinarySearchHashTreeNode<T> startNode, IEqualityComparer<T> comparer = null)
		{
			_startNode = startNode;
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		/// <summary>
		/// Получает компаратор, используемый при сравнении значений множества,
		/// </summary>
		public IEqualityComparer<T> Comparer => _comparer;

		/// <summary>
		/// Получает количество элементов в множестве.
		/// </summary>
		/// <remarks>Для проверки на пустое множество используйте свойство IsEmpty.</remarks>
		public int Count => _count;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => (_startNode != null) ?
			$"<{typeof(T).Name}> Count={_count} StartNode={_startNode.Value}" :
			$"<{typeof(T).Name}> empty";

		/// <summary>
		/// Проверяет наличие в множестве элемента с указанным значением.
		/// </summary>
		/// <param name="item">Значение элемента для проверки наличия в множестве.</param>
		/// <returns>
		/// True если элемент с указанным значением содержится в множестве,
		/// либо False если нет.
		/// </returns>
		public bool Contains (T item)
		{
			return _startNode.ContainsItem (item, _comparer);
		}

		/// <summary>
		/// Добавляет указанный элемент в множество.
		/// </summary>
		/// <param name="item">Элемент для добавления в множество.</param>
		/// <returns>True если элемент добавлен в множество, False если в множестве уже был элемент с таким значением.</returns>
		public bool Add (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.AddItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (!existsBefore)
			{
				_count++;
			}

			return !existsBefore;
		}

		/// <summary>
		/// Добавляет указанный элемент в множество.
		/// </summary>
		/// <param name="item">Элемент для добавления в множество.</param>
		void IAdjustableCollection<T>.Add (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.AddItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (!existsBefore)
			{
				_count++;
			}
		}

		/// <summary>Очищает множество.</summary>
		public void Clear ()
		{
			_startNode = null;
			_count = 0;
		}

		/// <summary>
		/// Удаляет из множества элемент с указанным значением.
		/// </summary>
		/// <param name="item">Значение элемента для удаления из множества.</param>
		/// <returns>True если элемент удалён из множества, False если в множестве не было элемента с таким значением.</returns>
		public bool Remove (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.RemoveItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (existsBefore)
			{
				_count--;
			}

			return existsBefore;
		}

		/// <summary>
		/// Удаляет из множества элемент с указанным значением.
		/// </summary>
		/// <param name="item">Значение элемента для удаления из множества.</param>
		void IAdjustableFiniteSet<T>.Remove (T item)
		{
			bool existsBefore = false;
			_startNode = AvlBinarySearchHashTree.RemoveItemInternal (_startNode, item, _comparer, ref existsBefore);
			if (existsBefore)
			{
				_count--;
			}
		}

		/// <summary>
		/// Получает перечислитель элементов множества.
		/// </summary>
		/// <returns>Перечислитель элементов множества.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}

		/// <summary>
		/// Получает перечислитель элементов множества.
		/// </summary>
		/// <returns>Перечислитель элементов множества.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}
	}
}
