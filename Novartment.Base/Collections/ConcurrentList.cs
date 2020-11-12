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
	/// A dynamic list for concurrent access
	/// backed by a looped array with a moving head and tail.
	/// Supports vector, stack and queue semantics.
	/// Concurrent access requires external synchronization.
	/// </summary>
	/// <typeparam name="T">
	/// The type of the elements.
	/// Must support atomic assignment, that is, be a reference type
	/// or value type no larger than the size of a pointer on the executing platform.
	/// </typeparam>
	/// <remarks>
	/// Optimized for getting/changing elements in an arbitrary position and lengthening/shortening on the head or tail side.
	/// No synchronization is required for concurrent access.
	/// Does not implement ICollection and IList interfaces due to incompatibility of their contracts with concurrent access.
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
		SpinWait spinWait = default;
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
		/// Initializes a new instance of the ConcurrentList class that is empty.
		/// </summary>
		public ConcurrentList ()
		{
			var isAtomicallyAssignable = ReflectionService.IsAtomicallyAssignable (typeof (T));
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_state = new LoopedArraySegment<T> (Array.Empty<T> ());
		}

		/// <summary>
		/// Initializes a new instance of the ConcurrentList class which is backed by a specified array.
		/// </summary>
		/// <param name="array">The array to be used as backing store directly, without copying.</param>
		public ConcurrentList (T[] array)
			: this (new LoopedArraySegment<T> (array))
		{
		}

		/// <summary>
		/// Initializes a new instance of the ConcurrentList class which is backed by a specified looped array segment.
		/// </summary>
		/// <param name="arraySegment">The looped array segment to be used as backing store directly, without copying.</param>
		public ConcurrentList (LoopedArraySegment<T> arraySegment)
		{
			if (arraySegment == null)
			{
				throw new ArgumentNullException (nameof (arraySegment));
			}

			Contract.EndContractBlock ();

			var isAtomicallyAssignable = ReflectionService.IsAtomicallyAssignable (typeof (T));
			if (!isAtomicallyAssignable)
			{
				throw new InvalidOperationException ("Invalid type of elements. Type must support atomic assignment, that is, to be reference type of privitive type of size not greater than size of platform-specific pointer.");
			}

			_state = arraySegment;
		}

		/// <summary>Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Gets the number of elements contained in the list.</summary>
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
		/// Gets or sets the element at the specified index in list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get.</param>
		public T this[int index]
		{
			get
			{
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
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

		/// <summary>Removes all items from the list.</summary>
		public void Clear ()
		{
			var state1 = _state;
			_state = new LoopedArraySegment<T> (Array.Empty<T> ());
			GenericCollectionExtensions.LoopedArraySegmentClear (
				state1.Array,
				state1.Offset,
				state1.Count,
				0,
				state1.Count);
			this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Adds an element to the list.
		/// </summary>
		/// <param name="item">The element to add to the list</param>
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
		/// Tries to get the first item in a list.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a list, if list was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the list was not empty; otherwise, False.</returns>
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
		/// Tries to get and remove the first item in a list.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a list, if list was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the list was not empty; otherwise, False.</returns>
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
		/// Tries to get the last item in a list.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a list, if list was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the list was not empty; otherwise, False.</returns>
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
		/// Tries to get and remove the last item in a list.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last item in a list, if list was not empty;
		/// otherwise, the default value for the type of the item parameter.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the list was not empty; otherwise, False.</returns>
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
		/// Inserts an element into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The element to insert. The value can be null for reference types.</param>
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
						GenericCollectionExtensions.LoopedArraySegmentCopy (
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
						GenericCollectionExtensions.LoopedArraySegmentCopy (
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
		/// Inserts a specified number of the elements into the list at the specified index.
		/// The inserted elements will have a default value.
		/// </summary>
		/// <param name="index"> The zero-based index at which the new elements should be inserted.</param>
		/// <param name="count">The number of elements to insert.</param>
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
					GenericCollectionExtensions.LoopedArraySegmentCopy (
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
					GenericCollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						index,
						index + count,
						size - index);
				}

				// очищаем место под новые элементы
				GenericCollectionExtensions.LoopedArraySegmentClear (
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
		/// Removes the element at the specified index of the list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
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
						GenericCollectionExtensions.LoopedArraySegmentCopy (
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
						GenericCollectionExtensions.LoopedArraySegmentCopy (
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
		/// Removes a range of elements from the list.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove.</param>
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
					GenericCollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						0,
						count,
						size1);
					GenericCollectionExtensions.LoopedArraySegmentClear (
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
					GenericCollectionExtensions.LoopedArraySegmentCopy (
						newState.Array,
						newState.Offset,
						newState.Count,
						index + count,
						index,
						size2);
					GenericCollectionExtensions.LoopedArraySegmentClear (
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
		/// Reserves space for the specified total number of elements.
		/// </summary>
		/// <param name="min">
		/// Minimum required capacity including items already in the list.
		/// </param>
		/// <remarks>
		/// Corresponds to setting Capacity property of classes System.Collections.ArrayList and System.Collections.Generic.List&lt;T&gt;.
		/// </remarks>
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
		/// Eliminates the list of reserved items.
		/// </summary>
		/// <remarks>
		/// Corresponds to TrimExcess() method of classes in the  System.Collections.Generic namespace:
		/// List&lt;T&gt;, Stack&lt;T&gt;, Queue&lt;T&gt;, HashSet&lt;T&gt;.
		/// </remarks>
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
		/// Returns an enumerator for the list.
		/// </summary>
		/// <returns>An enumerator for the list.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return _state.GetEnumerator ();
		}

		/// <summary>
		/// Returns an enumerator for the list.
		/// </summary>
		/// <returns>An enumerator for the list.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Copies the list to a one-dimensional array,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
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
		/// Returns a hash code for this list.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>The hash code for this list.</returns>
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
		/// Determines whether an object is structurally equal to this list.
		/// </summary>
		/// <param name="other">The object to compare with this list.</param>
		/// <param name="comparer">An object that determines whether element of this list and other are equal.</param>
		/// <returns>True if the two lists are equal; otherwise, False.</returns>
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

			if (other is not ConcurrentList<T> list)
			{
				return false;
			}

			var snapshot1 = _state;
			var snapshot2 = list._state;
			return ReferenceEquals (snapshot1, snapshot2) ||
				((IStructuralEquatable)snapshot1).Equals (snapshot2, comparer);
		}

		/// <summary>
		/// Returns the result of executing the specified function, passing it the list and the specified state object.
		/// The function will be run repeatedly if the list has been changed concurrently during its operation.
		/// </summary>
		/// <typeparam name="TInput">The type of the state parameter that will be passed to the function.</typeparam>
		/// <typeparam name="TOutput">The type of the return value of the function.</typeparam>
		/// <param name="accessMethod">The function to access the list. The list will be passed as the first parameter, and the state parameter as the second.</param>
		/// <param name="state">The state object that will be passed to the function as the second parameter.</param>
		/// <returns>The value returned by the function.</returns>
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

		internal sealed class DebugView
		{
			private readonly ConcurrentList<T> _list;

			public DebugView (ConcurrentList<T> list)
			{
				_list = list;
			}

			[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
			public T[] Items => GenericCollectionExtensions.DuplicateToArray (_list._state);
		}
	}
}
