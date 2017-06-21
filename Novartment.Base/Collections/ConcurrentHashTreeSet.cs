using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Reflection;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Множество уникальных значений для конкурентного доступа
	/// на основе двоичного дерева поиска, построенного на хэш-функциях элементов.
	/// </summary>
	/// <typeparam name="T">
	/// Тип элементов множества. Должен поддерживать атомарное присвоение,
	/// то есть быть ссылочным или примитивным типом размером не более чем размер указателя на исполняемой платформе.
	/// </typeparam>
	/// <remarks>
	/// Для построения дерева используются не значения элементов, а их хэш-код.
	/// Если для элементов существует компаратор на больше/меньше,
	/// то ConcurrentTreeSet при той же функциональности будет работать быстрее.
	/// В дереве могут содержаться дупликаты (хэшей, но не значений),
	/// так как хэш-функция может порождать совпадающие хэши для разных значений.
	/// Не реализует интерфейс ICollection ввиду несовместимости его контракта с конкурентным доступом.
	/// </remarks>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public class ConcurrentHashTreeSet<T> :
		IAdjustableFiniteSet<T>
	{
		private readonly IEqualityComparer<T> _comparer;
		private AvlBinarySearchHashTreeNode<T> _startNode; // null является корректным значением, означает пустое дерево

		/// <summary>
		/// Инициализирует новый экземпляр класса ConcurrentHashTreeSet,
		/// использующий указанный компаратор значений множества.
		/// </summary>
		/// <param name="comparer">
		/// Компаратор значений множества,
		/// или null чтобы использовать компаратор по умолчанию.
		/// </param>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public ConcurrentHashTreeSet (IEqualityComparer<T> comparer = null)
		{
			var isAtomicallyAssignable = typeof (T).IsAtomicallyAssignable ();
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		/// <summary>
		/// Получает компаратор, используемый при сравнении значений множества,
		/// </summary>
		public IEqualityComparer<T> Comparer => _comparer;

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
			var spinWait = default (SpinWait);
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
			var spinWait = default (SpinWait);
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
