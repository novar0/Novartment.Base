using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using SystemArray = System.Array;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// A dynamic list backed by a looped array with a moving head and tail.
	/// Supports vector, stack and queue semantics.
	/// Concurrent access requires external synchronization.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>
	/// Designed to replace List&lt;&gt;, Stack&lt;&gt; and Queue&lt;&gt; library classes
	/// in order to minimize overhead for internal limited usage.
	/// Can be created using a pre-allocated array, without copying.
	/// Gives direct access to the underlying array.
	/// Does not check concurrent changes.
	/// Does not implement interfaces with complex contract checks.
	/// Does not produce change notifications.
	/// Does not contain methods with implicit resource costs.
	/// </remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class ArrayList<T> :
		IAdjustableList<T>,
		IReservedCapacityCollection<T>,
		IArrayDuplicableCollection<T>,
		IStructuralEquatable
	{
		[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
		private T[] _items;
		private int _head = 0;
		private int _count = 0;

		/// <summary>
		/// Initializes a new instance of the ArrayList class that is empty.
		/// </summary>
		public ArrayList ()
		{
			_items = new T[4];
		}

		/// <summary>
		/// Initializes a new instance of the ArrayList class which is empty and backed by an array of the specified size.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		public ArrayList (int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (capacity));
			}

			Contract.EndContractBlock ();

			_items = new T[capacity];
		}

		/// <summary>
		/// Initializes a new instance of the ArrayList class which is backed by a specified array.
		/// </summary>
		/// <param name="array">The array to be used as backing store directly, without copying.</param>
		public ArrayList (T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}

			Contract.EndContractBlock ();

			_items = array;
			_head = 0;
			_count = array.Length;
		}

		/// <summary>
		/// Initializes a new instance of the ArrayList class which is backed by a specified array and represents its specified range.
		/// </summary>
		/// <param name="array">
		/// The array to be used as backing store directly, without copying.
		/// Any array elements, besides those limited by the offset, count range, can be cleared during operations that modify the list.
		/// </param>
		/// <param name="offset">
		/// The starting position of the range in the source array.
		/// Loops over the edge of the array, i.e. offset + count may be larger than the size of the array.
		/// </param>
		/// <param name="count">The number of elements in the range of the source array.</param>
		public ArrayList (T[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}

			if ((offset < 0) || (offset > array.Length) || ((offset == array.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || (count > array.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			_items = array;
			_head = (count > 0) ? offset : 0;
			_count = count;
		}

		/// <summary>
		/// Gets the backing array that is used to store elements of this list.
		/// </summary>
		public T[] Array => _items;

		/// <summary>
		/// Gets the position of the beginning of the range in the backing array in which the list items are stored.
		/// </summary>
		public int Offset => _head;

		/// <summary>
		/// Gets the number of list items.
		/// </summary>
		public int Count => _count;

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay
		{
			get
			{
				var info = (_count < 1) ?
					"empty" :
					(_count == 1) ?
						$"{_head}" :
						$"{_head}...{(_head + _count - 1) % _items.Length}";
				return $"<{typeof (T).Name}>[{info}] (capacity={_items.Length})";
			}
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		public T this[int index]
		{
			get
			{
				// Following trick can reduce the range check by one
				if ((uint)index >= (uint)this.Count)
				{
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
					throw new ArgumentOutOfRangeException (nameof (index));
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
				}

				Contract.EndContractBlock ();

				index += _head;
				if (index >= _items.Length)
				{
					index -= _items.Length;
				}

				return _items[index];
			}

			set
			{
				// Following trick can reduce the range check by one
				if ((uint)index >= (uint)this.Count)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				Contract.EndContractBlock ();

				index += _head;
				if (index >= _items.Length)
				{
					index -= _items.Length;
				}

				_items[index] = value;
			}
		}

		/// <summary>
		/// Forms a slice out of this list, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
		/// </summary>
		/// <param name="start">The index at which to begin this slice.</param>
		/// <param name="length">The length of the slice.</param>
		/// <returns>
		/// A new list that consists of <paramref name="length" /> elements from the this list starting at index <paramref name="start" />.
		/// The returned list is backed by the same array as this list.
		/// </returns>
		public ArrayList<T> Slice (int start, int length)
		{
			if ((start < 0) || (start > this.Count) || ((start == this.Count) && (length > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (length));
			}

			if ((length < 0) || ((start + length) > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (length));
			}

			Contract.EndContractBlock ();

			start += _head;
			if (start >= _items.Length)
			{
				start -= _items.Length;
			}

			return new ArrayList<T> (_items, start, length);
		}

		/// <summary>
		/// Adds an element to the list.
		/// </summary>
		/// <param name="item">The element to add to the list</param>
		public void Add (T item)
		{
			EnsureCapacity (_count + 1);
			var index = _head + _count;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			_items[index] = item;

			_count++;
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
			if (_count < 1)
			{
				item = default;
				return false;
			}

			item = _items[_head];
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
			if (_count < 1)
			{
				item = default;
				return false;
			}

			item = _items[_head];
			_items[_head] = default;
			_head++;
			if (_head >= _items.Length)
			{
				_head -= _items.Length;
			}

			_count--;
			if (_count < 1)
			{
				_head = 0;
			}

			return true;
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
			if (_count < 1)
			{
				item = default;
				return false;
			}

			var index = _head + _count - 1;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			item = _items[index];
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
			if (_count < 1)
			{
				item = default;
				return false;
			}

			var index = _head + _count - 1;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			item = _items[index];
			_items[index] = default;
			_count--;
			if (_count < 1)
			{
				_head = 0;
			}

			return true;
		}

		/// <summary>
		/// Inserts an element into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The element to insert. The value can be null for reference types.</param>
		public void Insert (int index, T item)
		{
			if ((index < 0) || (index > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			EnsureCapacity (_count + 1);

			if (index <= (_count / 2))
			{ // место вставки - первая половина, выгоднее отодвигать назад кусок от начала до индекса
				if (_count > 0)
				{
					_head--;
					if (_head < 0)
					{
						_head += _items.Length;
					}
				}

				_count++;
				if (index > 0)
				{
					CopyInternal (1, 0, index);
				}
			}
			else
			{ // место вставки - вторая половина, выгоднее отодвигать вперёд кусок от индекса до конца
				if (index < _count)
				{
					CopyInternal (index, index + 1, _count - index);
				}

				_count++;
			}

			index += _head;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			_items[index] = item;
		}

		/// <summary>
		/// Inserts a specified number of the elements into the list at the specified index.
		/// The inserted elements will have a default value.
		/// </summary>
		/// <param name="index"> The zero-based index at which the new elements should be inserted.</param>
		/// <param name="count">The number of elements to insert.</param>
		public void InsertRange (int index, int count)
		{
			if ((index < 0) || (index > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (count < 1)
			{
				return;
			}

			EnsureCapacity (_count + count);

			if (index == _count)
			{
				// вставка в конец, поэтому сдвигать и очищать нечего
				_count += count;
				return;
			}

			if (index <= (_count / 2))
			{ // место вставки - первая половина, выгоднее отодвигать назад кусок от начала до индекса
				if (_count > 0)
				{
					_head -= count;
					if (_head < 0)
					{
						_head += _items.Length;
					}
				}

				_count += count;
				CopyInternal (count, 0, index);
			}
			else
			{ // место вставки - вторая половина, выгоднее отодвигать вперёд кусок от индекса до конца
				CopyInternal (index, index + count, _count - index);
				_count += count;
			}

			// очищаем место под новые элементы
			ResetItems (index, count);
		}

		/// <summary>
		/// Removes the element at the specified index of the list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		public void RemoveAt (int index)
		{
			if ((index < 0) || (index >= this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			var lastIndex = _count - 1;

			if (index <= (_count / 2))
			{ // место удаления - первая половина, выгоднее отодвигать вперёд кусок от начала до индекса
				if (index > 0)
				{
					CopyInternal (0, 1, index);
				}

				_items[_head] = default;
				_head++;
				if (_head >= _items.Length)
				{
					_head -= _items.Length;
				}
			}
			else
			{ // место удаления - вторая половина, выгоднее отодвигать назад кусок от индекса до конца
				if (index < lastIndex)
				{
					CopyInternal (index + 1, index, _count - index - 1);
				}

				var idx = _head + _count - 1;
				if (idx >= _items.Length)
				{
					idx -= _items.Length;
				}

				_items[idx] = default;
			}

			_count--;
			if (_count < 1)
			{
				_head = 0;
			}
		}

		/// <summary>
		/// Removes a range of elements from the list.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove.</param>
		public void RemoveRange (int index, int count)
		{
			if ((index < 0) || (index > this.Count) || ((index == this.Count) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (_count == 0)
			{
				return;
			}

			if (_count == 1)
			{
				RemoveAt (index);
			}
			else
			{
				var size1 = index;
				var size2 = _count - index - count;

				// копия удаляемых элементов для последующего уведомления
				if (size1 <= size2)
				{ // выгоднее отодвигать вперёд кусок от начала до индекса
					CopyInternal (0, count, size1);
					ResetItems (0, count);
					_head += count;
					if (_head >= _items.Length)
					{
						_head -= _items.Length;
					}
				}
				else
				{ // выгоднее отодвигать назад кусок от индекса до конца
					CopyInternal (index + count, index, size2);
					ResetItems (index + size2, count);
				}

				_count = size1 + size2;
				if (_count < 1)
				{
					_head = 0;
				}
			}
		}

		/// <summary>Очищает список.</summary>
		public void Clear ()
		{
			ResetItems ();
			_head = 0;
			_count = 0;
		}

		/// <summary>Removes all items from the list.</summary>
		public void ResetItems ()
		{
			if (_count > 0)
			{
				var tail = _head + _count;
				if (tail >= _items.Length)
				{
					tail -= _items.Length;
				}

				if (_head < tail)
				{
					SystemArray.Clear (_items, _head, _count);
				}
				else
				{
					SystemArray.Clear (_items, _head, _items.Length - _head);
					SystemArray.Clear (_items, 0, tail);
				}
			}
		}

		/// <summary>
		/// Resets a range of elements from the list to default values.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to reset.</param>
		/// <param name="count">The number of elements to reset.</param>
		public void ResetItems (int index, int count)
		{
			if ((index < 0) || (index > this.Count) || ((index == this.Count) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			index += _head;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			var tail = _head + _count;
			if (tail > _items.Length)
			{
				tail -= _items.Length;
			}

			var firstPieceSize = _items.Length - index;
			if ((tail > index) || (count <= firstPieceSize))
			{
				// ...head**index*********tail...  или  ***index*********tail...head***
				SystemArray.Clear (_items, index, count);
			}
			else
			{
				// ***tail...head*********index***
				SystemArray.Clear (_items, index, _items.Length - index);
				SystemArray.Clear (_items, 0, count - (_items.Length - index));
			}
		}

		/// <summary>
		/// Copies the list to a one-dimensional array,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <remarks>Corresponds to the System.Collections.ICollection.CopyTo() and System.Array.CopyTo().</remarks>
		public void CopyTo (T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}

			if (array.Length < this.Count)
			{
				throw new ArgumentOutOfRangeException (nameof (array));
			}

			if ((arrayIndex < 0) || ((array.Length - arrayIndex) < this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (arrayIndex));
			}

			Contract.EndContractBlock ();

			if (_count > 0)
			{
				var tail = _head + _count;
				if (tail > _items.Length)
				{
					tail -= _items.Length;
				}

				if (_head < tail)
				{
					SystemArray.Copy (_items, _head, array, arrayIndex, _count);
				}
				else
				{
					SystemArray.Copy (_items, _head, array, arrayIndex, _items.Length - _head);
					SystemArray.Copy (_items, 0, array, arrayIndex + _items.Length - _head, tail);
				}
			}
		}

		/// <summary>
		/// Positions the elements of the backing array so that they go in a solid piece from the starting position.
		/// </summary>
		public void Defragment ()
		{
			if ((_count > 0) && (_head > 0))
			{
				var tail = _head + _count;
				if (tail > _items.Length)
				{
					tail -= _items.Length;
				}

				if (_head < tail)
				{
					SystemArray.Copy (_items, _head, _items, 0, _count);
				}
				else
				{
					var newItems = new T[_count];
					SystemArray.Copy (_items, _head, newItems, 0, _items.Length - _head);
					SystemArray.Copy (_items, 0, newItems, _items.Length - _head, tail);
					_items = newItems;
				}
			}

			_head = 0;
		}

		/// <summary>
		/// Sorts the elements in the entire list using thespecified comparer.
		/// </summary>
		/// <param name="comparer">
		/// The comparator to be used to compare items.
		/// Specify null-reference to use default comparer for T type.
		/// </param>
		/// <remarks>
		/// If the array is looped over the edge, then a smaller part will be copied before sorting.
		/// </remarks>
		public void Sort (IComparer<T> comparer = null)
		{
			if (_count < 1)
			{
				return;
			}

			// проверяем наличие зацикливания через край массива: ***[size1]***tail...head***[size2]***
			var sizeTail = (_head + _count) - _items.Length;
			if (sizeTail > 0)
			{
				// Если массив зациклен через край, то в нём будет два разделённых сегмента, а для сортировки нужен один непрерывный.
				// Пододвигаем один сегмент к другому.
				// Порядок элементов нарушится, но это не важно, потому что дальше они будут отсортированы.
				var sizeHead = _count - sizeTail;
				if (sizeTail >= sizeHead)
				{ // выгоднее отодвигать назад кусок от головы до конца массива
					SystemArray.Copy (_items, _head, _items, sizeTail, sizeHead);
					SystemArray.Clear (_items, _count, _items.Length - _count);
					_head = 0;
				}
				else
				{ // выгоднее отодвигать вперёд кусок от начала массива до хвоста
					_head -= sizeTail;
					SystemArray.Copy (_items, 0, _items, _head, sizeTail);
					SystemArray.Clear (_items, 0, _head);
				}
			}

			SystemArray.Sort (_items, _head, _count, comparer);
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
		/// Returns an enumerator for the list.
		/// </summary>
		/// <returns>An enumerator for the list.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return new SimpleListEnumerator (this);
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
			if (min < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (min));
			}

			Contract.EndContractBlock ();

			if (_items.Length < min)
			{
				var newCapacity = (_items.Length < (int.MaxValue / 2)) ?
					_items.Length * 2 : // увеличиваем минимум на 100%
					int.MaxValue;
				if (newCapacity < (_items.Length + 4))
				{
					newCapacity = _items.Length + 4; // увеличиваем минимум на 4
				}

				if (newCapacity < min)
				{
					newCapacity = min;
				}

				var newItems = new T[newCapacity];
				CopyTo (newItems, 0);
				_items = newItems;
				_head = 0;
			}
		}

		/// <summary>
		/// Eliminates the list of reserved items.
		/// </summary>
		public void TrimExcess ()
		{
			// обрезаем только если наполняемость менее 90%
			var num = (int)(_items.Length * 0.9);
			if (_count < num)
			{
				var newItems = new T[_count];
				CopyTo (newItems, 0);
				_items = newItems;
				_head = 0;
			}
		}

		/// <summary>
		/// Returns a structural hash code for this list.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>The structural hash code for this list.</returns>
		int IStructuralEquatable.GetHashCode (IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			int hash = 0;

			// макс. 8 элементов с конца для вычисления хэша всего списка
			var start = (_count >= 8) ? (_count - 8) : 0;
			for (var i = start; i < _count; i++)
			{
				hash = ((hash << 5) + hash) ^ comparer.GetHashCode (this[i]);
			}

			return hash;
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
			if (!isOtherEqualsThis)
			{
				if (!(other is ArrayList<T> list))
				{
					return false;
				}

				if (list._count != _count)
				{
					return false;
				}

				for (var i = 0; i < _count; i++)
				{
					object x = this[i];
					object y = list[i];
					var isXequalsY = comparer.Equals (x, y);
					if (!isXequalsY)
					{
						return false;
					}
				}
			}

			return true;
		}

		// Копирует без проверок диапазон элементов из одной позиции внутреннего массива в другую.
		private void CopyInternal (int sourceIndex, int destinationIndex, int length)
		{
			var srcIndex = sourceIndex + _head;
			if (srcIndex >= _items.Length)
			{
				srcIndex -= _items.Length;
			}

			var dstIndex = destinationIndex + _head;
			if (dstIndex >= _items.Length)
			{
				dstIndex -= _items.Length;
			}

			var tail = _head + _count;
			if (tail >= _items.Length)
			{
				tail -= _items.Length;
			}

			var srcFirstPieceSize = _items.Length - srcIndex;
			var dstFirstPieceSize = _items.Length - dstIndex;

			var isSrcСontiguous = (tail > srcIndex) || (length <= srcFirstPieceSize);
			var isDstСontiguous = (tail > dstIndex) || (length <= dstFirstPieceSize);

			if (isSrcСontiguous)
			{
				if (isDstСontiguous)
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					SystemArray.Copy (_items, srcIndex, _items, dstIndex, length);
				}
				else
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ***tail...head*********dstIndex***
					SystemArray.Copy (_items, srcIndex, _items, dstIndex, dstFirstPieceSize);
					SystemArray.Copy (_items, srcIndex + dstFirstPieceSize, _items, 0, length - dstFirstPieceSize);
				}
			}
			else
			{
				if (isDstСontiguous)
				{
					// ***tail...head*********srcIndex***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					SystemArray.Copy (_items, srcIndex, _items, dstIndex, srcFirstPieceSize);
					SystemArray.Copy (_items, 0, _items, dstIndex + srcFirstPieceSize, length - srcFirstPieceSize);
				}
				else
				{
					if (srcFirstPieceSize > dstFirstPieceSize)
					{
						// ***tail...head*******srcIndex*****
						// ***tail...head*********dstIndex***
						SystemArray.Copy (_items, srcIndex, _items, dstIndex, dstFirstPieceSize);
						SystemArray.Copy (_items, srcIndex + dstFirstPieceSize, _items, 0, srcFirstPieceSize - dstFirstPieceSize);
						SystemArray.Copy (_items, 0, _items, srcFirstPieceSize - dstFirstPieceSize, length - srcFirstPieceSize);
					}
					else
					{
						// ***tail...head*********srcIndex***
						// ***tail...head*******dstIndex*****
						SystemArray.Copy (_items, srcIndex, _items, dstIndex, srcFirstPieceSize);
						SystemArray.Copy (_items, 0, _items, dstIndex + srcFirstPieceSize, dstFirstPieceSize - srcFirstPieceSize);
						SystemArray.Copy (_items, dstFirstPieceSize - srcFirstPieceSize, _items, 0, length - dstFirstPieceSize);
					}
				}
			}
		}

		private struct SimpleListEnumerator :
			IEnumerator<T>,
			IDisposable,
			IEnumerator
		{
			private readonly ArrayList<T> _data;
			private int _index;
			private T _currentElement;

			internal SimpleListEnumerator (ArrayList<T> data)
			{
				_data = data;
				_index = -1;
				_currentElement = default;
			}

			/// <summary>
			/// Gets the element in the list at the current position of the enumerator.
			/// </summary>
			public T Current
			{
				get
				{
					if (_index == -1)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
					}

					if (_index == -2)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
					}

					return _currentElement;
				}
			}

			object IEnumerator.Current => this.Current;

			/// <summary>
			/// Advances the enumerator to the next element of the list.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element;
			/// false if the enumerator has passed the end of the list.</returns>
			public bool MoveNext ()
			{
				if (_index == -2)
				{
					return false;
				}

				_index++;
				if (_index >= _data._count)
				{
					_index = -2;
					_currentElement = default;
					return false;
				}

				var index = _index + _data._head;
				if (index >= _data._items.Length)
				{
					index -= _data._items.Length;
				}

				_currentElement = _data._items[index];
				return true;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the list.
			/// </summary>
			public void Reset ()
			{
				_index = -1;
				_currentElement = default;
			}

			/// <summary>
			/// Performs resources releasing.
			/// </summary>
			public void Dispose ()
			{
				_index = -2;
				_currentElement = default;
			}
		}
	}
}
