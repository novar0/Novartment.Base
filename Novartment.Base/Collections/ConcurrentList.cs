using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Reflection;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Динамический список для конкурентного доступа с уведомлением об изменениях
	/// на основе зацикленного массива с передвижными головой и хвостом.
	/// Поддерживает семантику стэка и очереди.
	/// </summary>
	/// <typeparam name="T">Тип элементов списка. Должен поддерживать атомарное присвоение,
	/// то есть быть ссылочным или примитивным типом размером не более чем размер указателя на исполняемой платформе.</typeparam>
	/// <remarks>
	/// Оптимизирован для получения/изменения элементов в произвольной позиции
	/// и удлинения/укорочения со стороны головы или хвоста.
	/// Для конкурентного доступа синхронизация не требуется.
	/// Не реализует интерфейсы ICollection и IList ввиду несовместимости их контрактов с конкурентным доступом.
	/// </remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[DebuggerTypeProxy (typeof (ConcurrentList<>.DebugView))]
	public class ConcurrentList<T> :
		IAdjustableList<T>,
		IReservedCapacityCollection<T>,
		IArrayDuplicableCollection<T>,
		IStructuralEquatable,
		INotifyCollectionChanged
	{
		/*
		для обеспечения конкурентного доступа, всё состояние хранится в одном поле _state, а любое его изменение выглядит так:
		var spinWait = default;
		while (true)
		{
		  var state1 = _state;
		  var newState = SomeOperation (state1);
		  var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
		  if (state1 == state2)
		  {
		    return;
		  }
		  spinWait.SpinOnce ();
		}
		*/

		[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
		private LoopedArraySegment<T> _state;

		/// <summary>
		/// Инициализирует новый экземпляр класса ConcurrentList.
		/// </summary>
		public ConcurrentList ()
		{
			var isAtomicallyAssignable = typeof (T).IsAtomicallyAssignable ();
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_state = new LoopedArraySegment<T> (Array.Empty<T> ());
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ConcurrentList на основе указанной коллекции.
		/// </summary>
		/// <param name="array">Массив, данные которого будут начальным содержимым создаваемого списка.
		/// Массив будет использован напрямую, без копирования.</param>
		public ConcurrentList (T[] array)
			: this (new LoopedArraySegment<T> (array))
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ConcurrentList на основе указанного сегмента массива.
		/// </summary>
		/// <param name="arraySegment">Сегмент массива, данные которого будут начальным содержимым создаваемого списка.
		/// Сегмент массива будет использован напрямую, без копирования.</param>
		public ConcurrentList (LoopedArraySegment<T> arraySegment)
		{
			if (arraySegment == null)
			{
				throw new ArgumentNullException (nameof (arraySegment));
			}

			Contract.EndContractBlock ();

			var isAtomicallyAssignable = typeof (T).IsAtomicallyAssignable ();
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_state = arraySegment;
		}

		/// <summary>Происходит, когда список изменяется.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Получает количество элементов в списке.
		/// </summary>
		public int Count => _state.Count;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay
		{
			get
			{
				var info = (_state.Count < 1) ?
					"empty" :
					(_state.Count == 1) ?
						$"{_state.Offset}" :
						$"{_state.Offset}...{(_state.Offset + _state.Count - 1) % _state.Array.Length}";
				return $"<{typeof(T).Name}>[{info}] (capacity={_state.Array.Length})";
			}
		}

		/// <summary>
		/// Получает или устанавливает элемент списка в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в списке.</param>
		public T this[int index]
		{
			get
			{
				if (index < 0)
				{
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
					throw new ArgumentOutOfRangeException (nameof (index));
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
				}

				Contract.EndContractBlock ();

				return _state[index];
			}

			set
			{
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				Contract.EndContractBlock ();

				var handler = this.CollectionChanged;
				if (handler == null)
				{
					_state[index] = value;
				}
				else
				{
					var snapshot = _state;
					var oldValue = snapshot[index];
					snapshot[index] = value;
					handler.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, value, oldValue, index));
				}
			}
		}

		/// <summary>Очищает список.</summary>
		public void Clear ()
		{
			var state1 = _state;
			_state = new LoopedArraySegment<T> (Array.Empty<T> ());
			CollectionExtensions.LoopedArraySegmentClear (
				state1.Array,
				state1.Offset,
				state1.Count,
				0,
				state1.Count);
			this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Добавляет элемент в список.
		/// </summary>
		/// <param name="item">Элемент для добавления в список.</param>
		public void Add (T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var cnt = state1.Count;
				var newState = EnsureCapacity (state1, cnt + 1);
				newState[cnt] = item;
				newState = new LoopedArraySegment<T> (newState.Array, newState.Offset, cnt + 1);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, item, cnt));
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Пытается получить первый элемент списка.
		/// </summary>
		/// <param name="item">Значение первого элемента списка, если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент списка успешно получен, false если нет.</returns>
		public bool TryPeekFirst (out T item)
		{
			var snapshot = _state;
			if (snapshot.Count < 1)
			{
				item = default;
				return false;
			}

			item = snapshot[0];
			return true;
		}

		/// <summary>
		/// Пытается изъять первый элемент списка.
		/// </summary>
		/// <param name="item">Значение первого элемента списка если он успешно изъят,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент списка успешно изъят, false если нет.</returns>
		public bool TryTakeFirst (out T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				if (state1.Count < 1)
				{
					item = default;
					return false;
				}

				item = state1[0];
				state1[0] = default;
				var newOffset = state1.Offset + 1;
				if (newOffset >= state1.Array.Length)
				{
					newOffset -= state1.Array.Length;
				}

				var newState = new LoopedArraySegment<T> (state1.Array, newOffset, state1.Count - 1);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, item, 0));
					return true;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Пытается получить последний элемент списка.
		/// </summary>
		/// <param name="item">Значение последнего элемента списка если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент списка успешно получен, false если нет.</returns>
		public bool TryPeekLast (out T item)
		{
			var snapshot = _state;
			var lastIndex = snapshot.Count - 1;
			if (lastIndex < 0)
			{
				item = default;
				return false;
			}

			item = snapshot[lastIndex];
			return true;
		}

		/// <summary>
		/// Пытается изъять последний элемент списка.
		/// </summary>
		/// <param name="item">Значение последнего элемента списка если он успешно изъят,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент списка успешно изъят, false если нет.</returns>
		public bool TryTakeLast (out T item)
		{
			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var lastIndex = state1.Count - 1;
				if (lastIndex < 0)
				{
					item = default;
					return false;
				}

				item = state1[lastIndex];
				state1[lastIndex] = default;
				var newState = new LoopedArraySegment<T> (state1.Array, state1.Offset, lastIndex);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, item, lastIndex));
					return true;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Вставляет указанный элемент в список в указанную позицию, отодвигая последующие элементы.
		/// </summary>
		/// <param name="index">Позиция в списке.</param>
		/// <param name="item">Элемент для вставки в указанную позицию.</param>
		public void Insert (int index, T item)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var newState = state1;
				var count = newState.Count;
				if (count >= newState.Array.Length)
				{
					newState = EnsureCapacity (newState, count + 1);
				}

				if (index > count)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				if (index <= (count / 2))
				{ // место вставки - первая половина, выгоднее отодвигать назад кусок от начала до индекса
					var newOffset = newState.Offset;
					if (count > 0)
					{
						newOffset--;
						if (newOffset < 0)
						{
							newOffset += newState.Array.Length;
						}
					}

					newState = new LoopedArraySegment<T> (newState.Array, newOffset, count + 1);
					if (index > 0)
					{
						CollectionExtensions.LoopedArraySegmentCopy (
							newState.Array,
							newState.Offset,
							newState.Count,
							1,
							0,
							index);
					}
				}
				else
				{ // место вставки - вторая половина, выгоднее отодвигать вперёд кусок от индекса до конца
					newState = new LoopedArraySegment<T> (newState.Array, newState.Offset, count + 1);
					if (index < count)
					{
						CollectionExtensions.LoopedArraySegmentCopy (
							newState.Array,
							newState.Offset,
							newState.Count,
							index,
							index + 1,
							count - index);
					}
				}

				newState[index] = item;

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, item, index));
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Вставляет пустой диапазон элементов в список в указанную позицию.
		/// Вставленные элементы будут иметь значение по-умолчанию.
		/// </summary>
		/// <param name="index">Позиция в коллекции, куда будут вставлены элементы.</param>
		/// <param name="count">Количество вставляемых элементов.</param>
		public void InsertRange (int index, int count)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (count == 0)
			{
				return;
			}

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var size = state1.Count;
				if (index > size)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				var newState = EnsureCapacity (state1, size + count);

				if (index <= (size / 2))
				{ // место вставки - первая половина, выгоднее отодвигать назад кусок от начала до индекса
					var newOffset = newState.Offset;
					if (size > 0)
					{
						newOffset -= count;
						if (newOffset < 0)
						{
							newOffset += newState.Array.Length;
						}
					}

					newState = new LoopedArraySegment<T> (newState.Array, newOffset, size + count);
					CollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						count,
						0,
						index);
				}
				else
				{ // место вставки - вторая половина, выгоднее отодвигать вперёд кусок от индекса до конца
					newState = new LoopedArraySegment<T> (newState.Array, newState.Offset, size + count);
					CollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						index,
						index + count,
						size - index);
				}

				// очищаем место под новые элементы
				CollectionExtensions.LoopedArraySegmentClear (
					newState.Array,
					newState.Offset,
					newState.Count,
					index,
					count);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, new T[count], index));
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Удаляет элемент из списка в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в списке.</param>
		public void RemoveAt (int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var newState = state1;
				var count = newState.Count;
				var lastIndex = count - 1;

				if (index > lastIndex)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				var deletedItem = newState[index];
				if (index <= (count / 2))
				{ // место удаления - первая половина, выгоднее отодвигать вперёд кусок от начала до индекса
					if (index > 0)
					{
						CollectionExtensions.LoopedArraySegmentCopy (
							newState.Array,
							newState.Offset,
							newState.Count,
							0,
							1,
							index);
					}

					newState[0] = default;
					var newOffset = newState.Offset + 1;
					if (newOffset >= newState.Array.Length)
					{
						newOffset -= newState.Array.Length;
					}

					newState = new LoopedArraySegment<T> (newState.Array, newOffset, count - 1);
				}
				else
				{ // место удаления - вторая половина, выгоднее отодвигать назад кусок от индекса до конца
					if (index < lastIndex)
					{
						CollectionExtensions.LoopedArraySegmentCopy (
							newState.Array,
							newState.Offset,
							newState.Count,
							index + 1,
							index,
							count - index - 1);
					}

					_state[lastIndex] = default;
					newState = new LoopedArraySegment<T> (newState.Array, newState.Offset, count - 1);
				}

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, deletedItem, index));
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Удаляет указанное число элементов из коллекции начиная с указанной позиции.
		/// </summary>
		/// <param name="index">Начальная позиция элементов для удаления.</param>
		/// <param name="count">Количество удаляемых элементов.</param>
		public void RemoveRange (int index, int count)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (count == 0)
			{
				return;
			}

			if (count == 1)
			{
				RemoveAt (index);
				return;
			}

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var newState = state1;
				var size1 = index;
				var size2 = newState.Count - index - count;

				// копия удаляемых элементов для последующего уведомления
				T[] notifyList = null;
				var handler = this.CollectionChanged;
				if (handler != null)
				{
					var offset = index + state1.Offset;
					if (offset >= state1.Array.Length)
					{
						offset -= state1.Array.Length;
					}

					notifyList = new T[count];
					new LoopedArraySegment<T> (state1.Array, offset, count).CopyTo (notifyList, 0);
				}

				if (size1 <= size2)
				{ // выгоднее отодвигать вперёд кусок от начала до индекса
					CollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						0,
						count,
						size1);
					CollectionExtensions.LoopedArraySegmentClear (
						newState.Array,
						newState.Offset,
						newState.Count,
						0,
						count);
					var newOffset = newState.Offset + count;
					if (newOffset >= newState.Array.Length)
					{
						newOffset -= newState.Array.Length;
					}

					newState = new LoopedArraySegment<T> (newState.Array, newOffset, size1 + size2);
				}
				else
				{ // выгоднее отодвигать назад кусок от индекса до конца
					CollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						index + count,
						index,
						size2);
					CollectionExtensions.LoopedArraySegmentClear (
						newState.Array,
						newState.Offset,
						newState.Count,
						index + size2,
						count);
					newState = new LoopedArraySegment<T> (newState.Array, newState.Offset, size1 + size2);
				}

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					handler?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, notifyList, index));
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Резервирует в массиве, в котором хранятся элементы списка, указанное количество элементов.
		/// Позволяет избежать копирований массива при добавлении элементов в список.
		/// </summary>
		/// <param name="min">Минимальная необходимая вместимость включая уже находящиеся в коллекции элементы.</param>
		public void EnsureCapacity (int min)
		{
			if (min < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (min));
			}

			Contract.EndContractBlock ();

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var newState = EnsureCapacity (state1, min);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Избавляет массив, в котором хранятся элементы списка, от зарезервированных элементов.
		/// Позволяет освободить память, занятую зарезервированными элементами.
		/// </summary>
		public void TrimExcess ()
		{
			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var num = (int)(state1.Array.Length * 0.9);
				if (state1.Count >= num)
				{
					return;
				}

				var newItems = new T[state1.Count];
				state1.CopyTo (newItems, 0);
				var newState = new LoopedArraySegment<T> (newItems);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _state, newState, state1);
				if (state1 == state2)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return _state.GetEnumerator ();
		}

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Копирует элементы списка в указанный массив,
		/// начиная с указанного индекса конечного массива.
		/// </summary>
		/// <param name="array">Массив, в который копируются элементы списка.</param>
		/// <param name="arrayIndex">Отсчитываемый от нуля индекс в массиве array, указывающий начало копирования.</param>
		public void CopyTo (T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}

			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (arrayIndex));
			}

			Contract.EndContractBlock ();

			_state.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Returns a hash code for the current instance.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>The hash code for the current instance.</returns>
		int IStructuralEquatable.GetHashCode (IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			return ((IStructuralEquatable)_state).GetHashCode (comparer);
		}

		/// <summary>
		/// Determines whether an object is structurally equal to the current instance.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">An object that determines whether the current instance and other are equal.</param>
		/// <returns>true if the two objects are equal; otherwise, false.</returns>
		bool IStructuralEquatable.Equals (object other, IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			if (other == null)
			{
				return false;
			}

			var isOtherEqualsThis = ReferenceEquals (this, other);
			if (isOtherEqualsThis)
			{
				return true;
			}

			if (!(other is ConcurrentList<T> list))
			{
				return false;
			}

			var snapshot1 = _state;
			var snapshot2 = list._state;
			return ReferenceEquals (snapshot1, snapshot2) ||
				((IStructuralEquatable)snapshot1).Equals (snapshot2, comparer);
		}

		/// <summary>
		/// Возвращает результат выполнения указанного делегата-функции, передавая ему список и указанный объект-параметр.
		/// Делегат-функциия будет запускаться повторно если во время его работы список был конкуррентно изменён.
		/// </summary>
		/// <typeparam name="TInput">Тип объекта-параметра, который будет передан в делегат-функцию.</typeparam>
		/// <typeparam name="TOutput">Тип возвращаемого значения делегата-функции.</typeparam>
		/// <param name="accessMethod">Делегат-функция.</param>
		/// <param name="state">Объект-параметр, который будет передан в делегат-функцию.</param>
		/// <returns>Значение, которое вернула делегат-функция.</returns>
		public TOutput AccessCollectionRetryIfConcurrentlyChanged<TInput, TOutput> (Func<ConcurrentList<T>, TInput, TOutput> accessMethod, TInput state)
		{
			if (accessMethod == null)
			{
				throw new ArgumentNullException (nameof (accessMethod));
			}

			Contract.EndContractBlock ();

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _state;
				var result = accessMethod.Invoke (this, state);
				if (state1 == _state)
				{
					return result;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		private static LoopedArraySegment<T> EnsureCapacity (LoopedArraySegment<T> arraySegment, int min)
		{
			var oldCapacity = arraySegment.Array.Length;
			if (oldCapacity < min)
			{
				var newCapacity = (int)((oldCapacity * 200L) / 100L);
				if (newCapacity < (oldCapacity + 4))
				{
					newCapacity = oldCapacity + 4;
				}

				if (newCapacity < min)
				{
					newCapacity = min;
				}

				var newItems = new T[newCapacity];
				arraySegment.CopyTo (newItems, 0);
				arraySegment = new LoopedArraySegment<T> (newItems, 0, arraySegment.Count);
			}

			return arraySegment;
		}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
		internal sealed class DebugView
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
		{
			private readonly ConcurrentList<T> _list;

			public DebugView (ConcurrentList<T> list)
			{
				_list = list;
			}

			[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
			public T[] Items => CollectionExtensions.DuplicateToArray (_list._state);
		}
	}
}
