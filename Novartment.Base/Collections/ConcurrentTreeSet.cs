using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Reflection;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Отсортированное множество уникальных значений для конкурентного доступа
	/// на основе двоичного дерева поиска.
	/// </summary>
	/// <typeparam name="T">
	/// Тип элементов множества. Должен поддерживать атомарное присвоение,
	/// то есть быть ссылочным или примитивным типом размером не более чем размер указателя на исполняемой платформе.
	/// </typeparam>
	/// <remarks>
	/// Для корректной работы требует надёжного компаратора значений на больше/меньше,
	/// если компаратора нет - используйте ConcurrentHashTreeSet.
	/// Не реализует интерфейс ICollection ввиду несовместимости его контракта с конкурентным доступом.
	/// Конкурентный доступ осуществляется без блокировок.
	/// </remarks>
	public class ConcurrentTreeSet<T> :
		IAdjustableFiniteSet<T>
	{
		/*
		для обеспечения конкурентного доступа, всё состояние хранится в одном поле _startNode, а любое его изменение выглядит так:
		SpinWait spinWait = default;
		while (true)
		{
		  var state1 = _startNode;
		  var newState = SomeOperation (state1);
		  var state2 = Interlocked.CompareExchange (ref _startNode, newState, state1);
		  if (state1 == state2)
		  {
		    return;
		  }
		  spinWait.SpinOnce ();
		}
		*/

		private readonly IComparer<T> _comparer;
		private AvlBinarySearchTreeNode<T> _startNode; // null является корректным значением, означает пустое дерево

		/// <summary>
		/// Инициализирует новый экземпляр класса ConcurrentTreeSet,
		/// использующий указанный компаратор значений множества.
		/// </summary>
		/// <param name="comparer">
		/// Компаратор значений множества,
		/// или null чтобы использовать компаратор по умолчанию.
		/// </param>
		public ConcurrentTreeSet (IComparer<T> comparer = null)
		{
			var isAtomicallyAssignable = typeof (T).IsAtomicallyAssignable ();
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_comparer = comparer ?? Comparer<T>.Default;
		}

		/// <summary>
		/// Получает компаратор, используемый при сравнении значений множества.
		/// </summary>
		public IComparer<T> Comparer => _comparer;

		/// <summary>
		/// Получает признак того, что множество пустое (не содержит элементов).
		/// </summary>
		public bool IsEmpty => _startNode == null;

		/// <summary>
		/// Получает количество элементов в множестве.
		/// </summary>
		/// <remarks>Для проверки на пустое множество используйте свойство IsEmpty.</remarks>
		public int Count => _startNode.GetCount ();

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
		public void Add (T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var oldState = _startNode;
				var newState = oldState.AddItem (item, _comparer);

				// заменяем состояние если оно не изменилось с момента вызова
				var testState = Interlocked.CompareExchange (ref _startNode, newState, oldState);
				if (testState == oldState)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>Очищает множество.</summary>
		public void Clear ()
		{
			_startNode = null;
		}

		/// <summary>
		/// Удаляет из множества элемент с указанным значением.
		/// </summary>
		/// <param name="item">Значение элемента для удаления из множества.</param>
		public void Remove (T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var oldState = _startNode;
				var newState = oldState.RemoveItem (item, _comparer);

				// заменяем состояние если оно не изменилось с момента вызова
				var testState = Interlocked.CompareExchange (ref _startNode, newState, oldState);
				if (testState == oldState)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Получает перечислитель элементов множества.
		/// </summary>
		/// <returns>Перечислитель элементов множества.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}

		/// <summary>
		/// Получает перечислитель элементов множества.
		/// </summary>
		/// <returns>Перечислитель элементов множества.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return _startNode.GetEnumerator ();
		}
	}
}
