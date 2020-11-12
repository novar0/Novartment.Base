using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Extension methods for collection interfaces.
	/// </summary>
	public static class GenericCollectionExtensions
	{
		private sealed class AsyncEnumerable<TItem> :
			IAsyncEnumerable<TItem>
		{
			private sealed class AsyncEnumerator :
				IAsyncEnumerator<TItem>
			{
				private readonly IEnumerator<TItem> _source;

				internal AsyncEnumerator (IEnumerator<TItem> source)
				{
					_source = source ?? throw new ArgumentNullException (nameof (source));
				}

				public TItem Current => _source.Current;

				public ValueTask<bool> MoveNextAsync () => new ValueTask<bool> (_source.MoveNext ());

				public ValueTask DisposeAsync ()
				{
					_source.Dispose ();
					return default;
				}
			}

			private readonly IEnumerable<TItem> _source;

			internal AsyncEnumerable (IEnumerable<TItem> source)
			{
				_source = source ?? throw new ArgumentNullException (nameof (source));
			}

			/// <summary>Returns an enumerator that iterates asynchronously through the collection.</summary>
			/// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the asynchronous iteration.</param>
			/// <returns>An enumerator that can be used to iterate asynchronously through the collection.</returns>
			public IAsyncEnumerator<TItem> GetAsyncEnumerator (CancellationToken cancellationToken = default)
			{
				return new AsyncEnumerator (_source.GetEnumerator ());
			}
		}

		/// <summary>
		/// Represents a synchronous enumerator as an asynchronous one.
		/// </summary>
		/// <typeparam name="TItem">The type of the elements.</typeparam>
		/// <param name="source">The synchronous enumerator.</param>
		/// <returns>The asynchronous enumerator.</returns>
		public static IAsyncEnumerable<TItem> AsAsyncEnumerable<TItem> (this IEnumerable<TItem> source)
		{
			return new AsyncEnumerable<TItem> (source);
		}

		/// <summary>
		/// Clears (resets to default value) the range of elements in a looped array segment.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="segmentItems">The array, in which the segment is defined.</param>
		/// <param name="segmentOffset">The starting position of the segment in the array.</param>
		/// <param name="segmentCount">The number of elements in the segment of the array.</param>
		/// <param name="index">The starting position of the range to clear (counting from the beginning of the segment).</param>
		/// <param name="count">The number of items to clean.</param>
		public static void LoopedArraySegmentClear<T> (
			T[] segmentItems,
			int segmentOffset,
			int segmentCount,
			int index,
			int count)
		{
			if (segmentItems == null)
			{
				throw new ArgumentNullException (nameof (segmentItems));
			}

			if ((segmentOffset < 0) || (segmentOffset > segmentItems.Length) || ((segmentOffset == segmentItems.Length) && (segmentCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentOffset));
			}

			if ((segmentCount < 0) || (segmentCount > segmentItems.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentCount));
			}

			if ((index < 0) || (index > segmentCount) || ((index == segmentCount) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > segmentCount))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			index += segmentOffset;
			if (index >= segmentItems.Length)
			{
				index -= segmentItems.Length;
			}

			var tail = segmentOffset + segmentCount;
			if (tail >= segmentItems.Length)
			{
				tail -= segmentItems.Length;
			}

			var firstPieceSize = segmentItems.Length - index;
			if ((tail > index) || (count <= firstPieceSize))
			{
				// ...head**index*********tail...  или  ***index*********tail...head***
				Array.Clear (segmentItems, index, count);
			}
			else
			{
				// ***tail...head*********index***
				Array.Clear (segmentItems, index, segmentItems.Length - index);
				Array.Clear (segmentItems, 0, count - (segmentItems.Length - index));
			}
		}

		/// <summary>
		/// Copies the range of elements of a looped array segment from one position to another.
		/// Copy positions are indicated from the beginning of the segment.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="segmentItems">The array, in which the segment is defined.</param>
		/// <param name="segmentOffset">The starting position of the segment in the array.</param>
		/// <param name="segmentCount">The number of elements in the segment of the array.</param>
		/// <param name="sourceIndex">The starting position of the range to copy (counting from the beginning of the segment).</param>
		/// <param name="destinationIndex">The destination range position in the segment (counting from the beginning of the segment).</param>
		/// <param name="count">The number of items to copy.</param>
		public static void LoopedArraySegmentCopy<T> (
			T[] segmentItems,
			int segmentOffset,
			int segmentCount,
			int sourceIndex,
			int destinationIndex,
			int count)
		{
			if (segmentItems == null)
			{
				throw new ArgumentNullException (nameof (segmentItems));
			}

			if ((segmentOffset < 0) || (segmentOffset > segmentItems.Length) || ((segmentOffset == segmentItems.Length) && (segmentCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentOffset));
			}

			if ((segmentCount < 0) || (segmentCount > segmentItems.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentCount));
			}

			if ((sourceIndex < 0) || (sourceIndex > segmentCount) || ((sourceIndex == segmentCount) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (sourceIndex));
			}

			if ((destinationIndex < 0) || (destinationIndex > segmentCount) || ((destinationIndex == segmentCount) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (destinationIndex));
			}

			if ((count < 0) || ((sourceIndex + count) > segmentCount) || ((destinationIndex + count) > segmentCount))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			var srcIndex = sourceIndex + segmentOffset;
			if (srcIndex >= segmentItems.Length)
			{
				srcIndex -= segmentItems.Length;
			}

			var dstIndex = destinationIndex + segmentOffset;
			if (dstIndex >= segmentItems.Length)
			{
				dstIndex -= segmentItems.Length;
			}

			var tail = segmentOffset + segmentCount;
			if (tail >= segmentItems.Length)
			{
				tail -= segmentItems.Length;
			}

			var srcFirstPieceSize = segmentItems.Length - srcIndex;
			var dstFirstPieceSize = segmentItems.Length - dstIndex;

			var isSrcСontiguous = (tail > srcIndex) || (count <= srcFirstPieceSize);
			var isDstСontiguous = (tail > dstIndex) || (count <= dstFirstPieceSize);

			if (isSrcСontiguous)
			{
				if (isDstСontiguous)
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, count);
				}
				else
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ***tail...head*********dstIndex***
					Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, dstFirstPieceSize);
					Array.Copy (segmentItems, srcIndex + dstFirstPieceSize, segmentItems, 0, count - dstFirstPieceSize);
				}
			}
			else
			{
				if (isDstСontiguous)
				{
					// ***tail...head*********srcIndex***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, srcFirstPieceSize);
					Array.Copy (segmentItems, 0, segmentItems, dstIndex + srcFirstPieceSize, count - srcFirstPieceSize);
				}
				else
				{
					if (srcFirstPieceSize > dstFirstPieceSize)
					{
						// ***tail...head*******srcIndex*****
						// ***tail...head*********dstIndex***
						Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, dstFirstPieceSize);
						Array.Copy (segmentItems, srcIndex + dstFirstPieceSize, segmentItems, 0, srcFirstPieceSize - dstFirstPieceSize);
						Array.Copy (segmentItems, 0, segmentItems, srcFirstPieceSize - dstFirstPieceSize, count - srcFirstPieceSize);
					}
					else
					{
						// ***tail...head*********srcIndex***
						// ***tail...head*******dstIndex*****
						Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, srcFirstPieceSize);
						Array.Copy (segmentItems, 0, segmentItems, dstIndex + srcFirstPieceSize, dstFirstPieceSize - srcFirstPieceSize);
						Array.Copy (segmentItems, dstFirstPieceSize - srcFirstPieceSize, segmentItems, 0, count - dstFirstPieceSize);
					}
				}
			}
		}

		/// <summary>
		/// Adds the elements of one collection to the end of the another collection,
		/// possibly reserving the space for new elements in advance.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="collection">Destination collection in which new elemenets will be added.</param>
		/// <param name="items">Source collection whose elements will be added to the destination.</param>
		public static void AddRange<T> (this IAdjustableCollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			if (items == null)
			{
				throw new ArgumentNullException (nameof (items));
			}

			Contract.EndContractBlock ();

			if (collection is IReservedCapacityCollection<T> reservableCapacityCollection)
			{
				var isCounted = TryGetCount (items, out int count);
				if (isCounted)
				{
					reservableCapacityCollection.EnsureCapacity (collection.Count + count);
				}
			}

			foreach (var item in items)
			{
				collection.Add (item);
			}
		}

		/// <summary>
		/// Inserts the elements of collection to the specified position the specified list,
		/// possibly reserving the space for new elements in advance.
		/// </summary>
		/// <typeparam name="T">Тип элементов списка.</typeparam>
		/// <param name="list">Destination list in which new elemenets will be inserted.</param>
		/// <param name="index">The zero-based index of the destination at which the new elements should be inserted.</param>
		/// <param name="items">Source collection whose elements will be inserted to the destination.</param>
		public static void InsertRange<T> (this IAdjustableList<T> list, int index, IEnumerable<T> items)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (items == null)
			{
				throw new ArgumentNullException (nameof (items));
			}

			if ((index < 0) || (index > list.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			var isCounted = items.TryGetCount (out int count);
			if (isCounted)
			{ // количество вставляемых элементов известно, можно заранее зарезервировать место
				if (count > 0)
				{
					list.InsertRange (index, count);
					foreach (var item in items)
					{
						list[index++] = item;
					}
				}
			}
			else
			{ // если количество вставляемых элементов неизвестно, то никакая оптимизации невозможна
				foreach (var item in items)
				{
					list.Insert (index++, item);
				}
			}
		}

		/// <summary>
		/// Deletes in the specified list all elements contained in the specified set.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="list">A list whose items will be checked and possibly deleted.</param>
		/// <param name="collectionToRemove">Set of items еo remove from the list.</param>
		/// <returns>The number of deleted items.</returns>
		/// <remarks>Creates a temporary list of indexes to delete.</remarks>
		public static int RemoveItems<T> (this IAdjustableList<T> list, IReadOnlyFiniteSet<T> collectionToRemove)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (collectionToRemove == null)
			{
				throw new ArgumentNullException (nameof (collectionToRemove));
			}

			Contract.EndContractBlock ();

			if (list.Count < 1)
			{
				return 0;
			}

			var indexesToRemove = new ArrayList<int> ();
			{
				for (var index = 0; index < list.Count; index++)
				{
					var isRemoveItem = collectionToRemove.Contains (list[index]);
					if (isRemoveItem)
					{
						indexesToRemove.Add (index);
					}
				}
			}

			list.RemoveAtMany (indexesToRemove);
			return indexesToRemove.Count;
		}

		/// <summary>
		/// Deletes items from the list according to the specified sorted list of indexes to delete.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="list">A list whose items will be deleted according to the specified list of indexes.</param>
		/// <param name="indexes">
		/// List of indexes to delete in list.
		/// Must be sorted from smallest to largest.
		/// </param>
		public static void RemoveAtMany<T> (this IAdjustableList<T> list, IReadOnlyList<int> indexes)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (indexes == null)
			{
				throw new ArgumentNullException (nameof (indexes));
			}

			Contract.EndContractBlock ();

			var count = indexes.Count;
			if ((count < 1) || (list.Count < 1))
			{
				return;
			}

			int rangeStart = 0;
			var rangeEnd = indexes[count - 1];
			var lastIndex = rangeEnd + 1;
			int rangeSize;

			// собираем индексы в непрерывные диапазоны чтобы минимизировать операции удаления
			for (var i = count - 1; i >= 0; i--)
			{
				var index = indexes[i];
				if (index > lastIndex)
				{
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Array of indexes to delete is not sorted in position {i}."));
				}

				if (index == (lastIndex - 1))
				{
					rangeStart = index;
				}
				else
				{
					rangeSize = rangeEnd - rangeStart + 1;
					list.RemoveRange (rangeStart, rangeSize);
					rangeStart = index;
					rangeEnd = index;
				}

				lastIndex = index;
			}

			rangeSize = rangeEnd - lastIndex + 1;
			list.RemoveRange (lastIndex, rangeSize);
		}

		/// <summary>
		/// Tries to get the first element of the specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="source">The collection from which the first element will be obtained.</param>
		/// <param name="item">
		/// Will contain the first element of the collection if it was successfully obtained,
		/// or the default value if the collection was empty.
		/// </param>
		/// <returns>True if the first item in the collection is successfully obtained, or False if the collection is empty.</returns>
		public static bool TryGetFirst<T> (this IEnumerable<T> source, out T item)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is IFifoCollection<T> fifoCollection)
			{
				var isPeeked = fifoCollection.TryPeekFirst (out item);
				if (isPeeked)
				{
					return true;
				}
			}
			else
			{
				if (source is IReadOnlyList<T> readOnlyList)
				{
					if (readOnlyList.Count > 0)
					{
						item = readOnlyList[0];
						return true;
					}
				}
				else
				{
					if (source is IList<T> list)
					{
						if (list.Count > 0)
						{
							item = list[0];
							return true;
						}
					}
					else
					{
						using var enumerator = source.GetEnumerator ();
						var isMovedToNext = enumerator.MoveNext ();
						if (isMovedToNext)
						{
							item = enumerator.Current;
							return true;
						}
					}
				}
			}

			item = default;
			return false;
		}

		/// <summary>
		/// Tries to get the last element of the specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="source">The collection from which the last element will be obtained.</param>
		/// <param name="item">
		/// Will contain the last element of the collection if it was successfully obtained,
		/// or the default value if the collection was empty.
		/// </param>
		/// <returns>True if the last item in the collection is successfully obtained, or False if the collection is empty.</returns>
		public static bool TryGetLast<T> (this IEnumerable<T> source, out T item)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is ILifoCollection<T> lifoCollection)
			{
				var isPeeked = lifoCollection.TryPeekLast (out item);
				if (isPeeked)
				{
					return true;
				}
			}
			else
			{
				if (source is IReadOnlyList<T> readOnlyList)
				{
					var count = readOnlyList.Count;
					if (count > 0)
					{
						item = readOnlyList[count - 1];
						return true;
					}
				}
				else
				{
					if (source is IList<T> list)
					{
						var count = list.Count;
						if (count > 0)
						{
							item = list[count - 1];
							return true;
						}
					}
					else
					{
						using var enumerator = source.GetEnumerator ();
						var isMovedToNext = enumerator.MoveNext ();
						if (isMovedToNext)
						{
							do
							{
								item = enumerator.Current;
							}
							while (enumerator.MoveNext ());
							return true;
						}
					}
				}
			}

			item = default;
			return false;
		}

		/// <summary>
		/// Tries to get the number of elements in a collection without iterating through it.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="source">The collection for which you want to get the number of elements.</param>
		/// <param name="count">
		/// Will contain the number of items in the collection if successful, otherwise it will contain zero.
		/// </param>
		/// <returns>True if the number of elements is successfully obtained, otherwise False.</returns>
		public static bool TryGetCount<T> (this IEnumerable<T> source, out int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is IReadOnlyCollection<T> countable1)
			{
				count = countable1.Count;
				return true;
			}

			if (source is ICollection<T> countable2)
			{
				count = countable2.Count;
				return true;
			}

			if (source is ICollection countable3)
			{
				count = countable3.Count;
				return true;
			}

			count = default;
			return false;
		}

		/// <summary>
		/// Creates an array in which all elements of the specified collection are optimally copied.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="source">The collection to be copied to the array.</param>
		/// <returns>A newly allocated array containing a copy of all elements of the specified collection.</returns>
		public static T[] DuplicateToArray<T> (this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			T[] array;
			int length;

			if (source is IArrayDuplicableCollection<T> collection1)
			{
				length = collection1.Count;
				array = new T[length];
				if (length > 0)
				{
					collection1.CopyTo (array, 0);
				}

				return array;
			}

			if (source is ICollection<T> collection2)
			{
				length = collection2.Count;
				array = new T[length];
				if (length > 0)
				{
					collection2.CopyTo (array, 0);
				}

				return array;
			}

			if (source is ICollection collection3)
			{
				length = collection3.Count;
				array = new T[length];
				if (length > 0)
				{
					collection3.CopyTo (array, 0);
				}

				return array;
			}

			if (source is IReadOnlyCollection<T> collection4)
			{
				length = collection4.Count;
				array = new T[length];
				if (length > 0)
				{
					int idx = 0;
					foreach (var item in collection4)
					{
						array[idx++] = item;
					}
				}

				return array;
			}

			length = 0;
			array = new T[TryGetCount (source, out int cnt) ? cnt : 4];
			foreach (var item in source)
			{
				if (array.Length == length)
				{
					var newArray = new T[length * 2];
					Array.Copy (array, newArray, length);
					array = newArray;
				}

				array[length++] = item;
			}

			if (array.Length == length)
			{
				return array;
			}

			if (length == 0)
			{
				return Array.Empty<T> ();
			}

			var resultArray = new T[length];
			Array.Copy (array, resultArray, length);
			return resultArray;
		}

		/// <summary>
		/// Creates a list in which all elements of the specified collection are optimally copied.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="source">The collection to be copied to the list.</param>
		/// <returns>A newly allocated list containing a copy of all elements of the specified collection.</returns>
		public static IAdjustableList<T> DuplicateToList<T> (this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			T[] array;
			int length;

			if (source is IArrayDuplicableCollection<T> collection1)
			{
				length = collection1.Count;
				array = new T[length];
				if (length > 0)
				{
					collection1.CopyTo (array, 0);
				}

				return new ArrayList<T> (array);
			}

			if (source is ICollection<T> collection2)
			{
				length = collection2.Count;
				array = new T[length];
				if (length > 0)
				{
					collection2.CopyTo (array, 0);
				}

				return new ArrayList<T> (array);
			}

			if (source is ICollection collection3)
			{
				length = collection3.Count;
				array = new T[length];
				if (length > 0)
				{
					collection3.CopyTo (array, 0);
				}

				return new ArrayList<T> (array);
			}

			if (source is IReadOnlyCollection<T> collection4)
			{
				length = collection4.Count;
				array = new T[length];
				if (length > 0)
				{
					int idx = 0;
					foreach (var item in collection4)
					{
						array[idx++] = item;
					}
				}

				return new ArrayList<T> (array);
			}

			length = 0;
			array = new T[TryGetCount (source, out int cnt) ? cnt : 4];
			foreach (var item in source)
			{
				if (array.Length == length)
				{
					var newArray = new T[length * 2];
					Array.Copy (array, newArray, length);
					array = newArray;
				}

				array[length++] = item;
			}

			return new ArrayList<T> (array, 0, length);
		}
	}
}
