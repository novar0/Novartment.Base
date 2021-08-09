using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SystemArray = System.Array;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// A segment of the array that loops through the upper boundary.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	/// <remarks>
	/// Semantically equivalent to System.ArraySegment&lt;&gt;, but is a reference type to guarantee atomicity of assignments.
	/// </remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public sealed class LoopedArraySegment<T> :
		IEquatable<LoopedArraySegment<T>>,
		IReadOnlyList<T>,
		IArrayDuplicableCollection<T>,
		IStructuralEquatable
	{
		[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
		private readonly T[] _items;
		private readonly int _capacity;
		private readonly int _offset;
		private readonly int _count;

		/// <summary>
		/// Initializes a new instance of the LoopedArraySegment class that uses the specified array.
		/// </summary>
		/// <param name="array">The array to be used by the segment.</param>
		public LoopedArraySegment (T[] array)
			: this (array, 0, ValidatedArrayLength (array))
		{
		}

		/// <summary>
		/// Initializes a new instance of the LoopedArraySegment class that uses the specified array segment.
		/// </summary>
		/// <param name="array">The array to be used by the segment.</param>
		/// <param name="offset">
		/// The starting position in the source array.
		/// Loops over the edge of the array, meaning offset + count can be larger than the size of the array.
		/// </param>
		/// <param name="count">The number of elements in the source array.</param>
		public LoopedArraySegment (T[] array, int offset, int count)
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

			_items = array;
			_capacity = array.Length;
			_offset = (count > 0) ? offset : 0;
			_count = count;
		}

		/// <summary>
		/// Gets the original array of the segment.
		/// </summary>
		public T[] Array => _items;

		/// <summary>
		/// Gets the start position of the segment in the source array.
		/// </summary>
		public int Offset => _offset;

		/// <summary>
		/// Gets the number of elements in the array segment.
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
						$"{_offset}" :
						$"{_offset}...{(_offset + _count - 1) % _items.Length}";
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
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				index += _offset;
				if (index >= _capacity)
				{
					index -= _capacity;
				}

				return _items[index];
			}

			set
			{
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				index += _offset;
				if (index >= _capacity)
				{
					index -= _capacity;
				}

				_items[index] = value;
			}
		}

		/// <summary>
		/// Forms a slice out of this segment, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
		/// </summary>
		/// <param name="start">The index at which to begin this slice.</param>
		/// <param name="length">The length of the slice.</param>
		/// <returns>
		/// A new LoopedArraySegment that is based by the same array as this segment.
		/// </returns>
		public LoopedArraySegment<T> Slice (int start, int length)
		{
			start += _offset;
			if (start >= _items.Length)
			{
				start -= _items.Length;
			}

			return new LoopedArraySegment<T> (_items, start, length);
		}

		/// <summary>
		/// Returns a value that indicates whether two LoopedArraySegment objects are equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two LoopedArraySegment objects are equal; otherwise, False.</returns>
		public static bool operator == (LoopedArraySegment<T> first, LoopedArraySegment<T> second)
		{
			return first is null ?
				second is null :
				first.Equals (second);
		}

		/// <summary>
		/// Returns a value that indicates whether two LoopedArraySegment objects are not equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two LoopedArraySegment objects are not equal; otherwise, False.</returns>
		public static bool operator != (LoopedArraySegment<T> first, LoopedArraySegment<T> second)
		{
			return !(first is null ?
				second is null :
				first.Equals (second));
		}

		/// <summary>
		/// Copies the elements of the segment to a one-dimensional array,
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

			if (_count > 0)
			{
				var tail = _offset + _count;
				if (tail >= _capacity)
				{
					tail -= _capacity;
				}

				if (_offset < tail)
				{
					SystemArray.Copy (_items, _offset, array, arrayIndex, _count);
				}
				else
				{
					SystemArray.Copy (_items, _offset, array, arrayIndex, _capacity - _offset);
					SystemArray.Copy (_items, 0, array, arrayIndex + _capacity - _offset, tail);
				}
			}
		}

		/// <summary>
		/// Returns an enumerator for the segment.
		/// </summary>
		/// <returns>An enumerator for the segment.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Returns an enumerator for the segment.
		/// </summary>
		/// <returns>An enumerator for the segment.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return new ArraySegmentLoopedEnumerator (this);
		}

		/// <summary>
		/// Returns a hash code for this segment.
		/// </summary>
		/// <returns>The hash code for this segment.</returns>
		public override int GetHashCode ()
		{
			return (_items.GetHashCode () ^ _offset) ^ _count;
		}

		/// <summary>
		/// Determines whether an object is equal to this segment.
		/// </summary>
		/// <param name="obj">The object to compare with this segment.</param>
		/// <returns>True if the two segments are equal; otherwise, False.</returns>
		public override bool Equals (object obj)
		{
			var asSameClass = obj as LoopedArraySegment<T>;
			return (asSameClass != null) && Equals (asSameClass);
		}

		/// <summary>
		/// Determines whether an segment is equal to this segment.
		/// </summary>
		/// <param name="other">The segment to compare with this segment.</param>
		/// <returns>True if the two segments are equal; otherwise, False.</returns>
		public bool Equals (LoopedArraySegment<T> other)
		{
			return ReferenceEquals (this, other) ||
				(!(other is null) &&
				(other._items == _items) &&
				(other._offset == _offset) &&
				(other._count == _count));
		}

		/// <summary>
		/// Returns a structural hash code for this segment.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>The structural hash code for this segment.</returns>
		int IStructuralEquatable.GetHashCode (IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			int hash = 0;

			// макс. 8 элементов с конца для вычисления хэша всей коллекции
			var start = (_count >= 8) ? (_count - 8) : 0;
			for (var i = start; i < _count; i++)
			{
				hash = ((hash << 5) + hash) ^ comparer.GetHashCode (this[i]);
			}

			return hash;
		}

		/// <summary>
		/// Determines whether an object is structurally equal to this segment.
		/// </summary>
		/// <param name="other">The object to compare with this segment.</param>
		/// <param name="comparer">An object that determines whether element of this list and other are equal.</param>
		/// <returns>True if the two segments are equal; otherwise, False.</returns>
		bool IStructuralEquatable.Equals (object other, IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			if (other == null)
			{
				return false;
			}

			var isOtherEqualsThis = ReferenceEquals (this, other);
			if (!isOtherEqualsThis)
			{
				var array = other as LoopedArraySegment<T>;
				if (array == null)
				{
					return false;
				}

				if (array._count != _count)
				{
					return false;
				}

				for (var i = 0; i < _count; i++)
				{
					object x = this[i];
					object y = array[i];
					var isXequalsY = comparer.Equals (x, y);
					if (!isXequalsY)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static int ValidatedArrayLength (T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}

			return array.Length;
		}

		private struct ArraySegmentLoopedEnumerator :
			IEnumerator<T>,
			IDisposable,
			IEnumerator
		{
			private readonly LoopedArraySegment<T> _data;
			private int _index;
			private T _currentElement;

			internal ArraySegmentLoopedEnumerator (LoopedArraySegment<T> data)
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
			/// Advances the enumerator to the next element of the segment.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element;
			/// false if the enumerator has passed the end of the segment.</returns>
			public bool MoveNext ()
			{
				if (_index == -2)
				{
					return false;
				}

				_index++;
				if (_index == _data._count)
				{
					_index = -2;
					_currentElement = default;
					return false;
				}

				_currentElement = _data[_index];
				return true;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the segment.
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
