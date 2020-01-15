using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Extension methods for read only collections.
	/// </summary>
	public static class ReadOnlyCollection
	{
		/// <summary>
		/// Determines whether a collection contains any elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The collection to check for emptiness.</param>
		/// <returns>True if the source collection contains any elements; otherwise, False.</returns>
		public static bool Any<TSource> (this IReadOnlyCollection<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return source.Count > 0;
		}

		/// <summary>
		/// Returns the number of elements in a collection.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">A collection that contains elements to be counted.</param>
		/// <returns>The number of elements in the input collection.</returns>
		public static int Count<TSource> (this IReadOnlyCollection<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return source.Count;
		}

		/// <summary>
		/// Returns an Int64 that represents the number of elements in a collection.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">A collection that contains the elements to be counted.</param>
		/// <returns>The number of elements in the source collection.</returns>
		public static long LongCount<TSource> (this IReadOnlyCollection<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return source.Count;
		}

		/// <summary>
		/// Returns the elements of the specified collection or
		/// the specified value in a singleton collection if the collection is empty.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The collection to return the specified value for if it is empty.</param>
		/// <param name="defaultValue">The value to return if the collection is empty.</param>
		/// <returns>A collection that contains defaultValue if source is empty; otherwise, source.</returns>
		public static IReadOnlyCollection<TSource> DefaultIfEmpty<TSource> (this IReadOnlyCollection<TSource> source, TSource defaultValue = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return (source.Count > 0) ? source : ReadOnlyList.Repeat<TSource> (defaultValue, 1);
		}

		/// <summary>
		/// Bypasses a specified number of elements in a collection and then returns the remaining elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">A collection to return elements from.</param>
		/// <param name="count">The number of elements to skip before returning the remaining elements.</param>
		/// <returns>A collection that contains the elements that occur after the specified index in the input collection.</returns>
		public static IReadOnlyCollection<TSource> Skip<TSource> (this IReadOnlyCollection<TSource> source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (source.Count < 1)
			{
				return source;
			}

			if (count >= source.Count)
			{
				return ReadOnlyList.Empty<TSource> ();
			}

			return new SkipReadOnlyCollection<TSource> (source, count);
		}

		/// <summary>
		/// Returns a specified number of contiguous elements from the start of a collection.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The collection to return elements from.</param>
		/// <param name="count">The number of elements to return.</param>
		/// <returns>A collection that contains the specified number of elements from the start of the input collection.</returns>
		public static IReadOnlyCollection<TSource> Take<TSource> (this IReadOnlyCollection<TSource> source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if ((source.Count < 1) || (count < 1))
			{
				return ReadOnlyList.Empty<TSource> ();
			}

			if (count >= source.Count)
			{
				return source;
			}

			return new TakeReadOnlyCollection<TSource> (source, count);
		}

		/// <summary>
		/// Projects each element of a collection into a new form.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A collection of values to invoke a transform function on.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>A collection whose elements are the result of invoking the transform function on each element of source.</returns>
		public static IReadOnlyCollection<TResult> Select<TSource, TResult> (this IReadOnlyCollection<TSource> source, Func<TSource, TResult> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (selector == null)
			{
				throw new ArgumentNullException (nameof (selector));
			}

			Contract.EndContractBlock ();

			return (source.Count < 1) ?
				(IReadOnlyCollection<TResult>)ReadOnlyList.Empty<TResult> () :
				new SelectReadOnlyCollection<TSource, TResult> (source, selector);
		}

		/// <summary>
		/// Projects each element of a collection into a new form by incorporating the element's index.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A collection of values to invoke a transform function on.</param>
		/// <param name="selector">
		/// A transform function to apply to each source element;
		/// the second parameter of the function represents the index of the source element.
		/// </param>
		/// <returns>A collection whose elements are the result of invoking the transform function on each element of source.</returns>
		public static IReadOnlyCollection<TResult> Select<TSource, TResult> (this IReadOnlyCollection<TSource> source, Func<TSource, int, TResult> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (selector == null)
			{
				throw new ArgumentNullException (nameof (selector));
			}

			Contract.EndContractBlock ();

			return (source.Count < 1) ?
				(IReadOnlyCollection<TResult>)ReadOnlyList.Empty<TResult> () :
				new SelectIndexReadOnlyCollection<TSource, TResult> (source, selector);
		}

		/// <summary>
		/// Inverts the order of the elements in a collection.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">A collection of values to reverse.</param>
		/// <returns>A collection whose elements correspond to those of the input collection in reverse order.</returns>
		public static IReadOnlyCollection<TSource> Reverse<TSource> (this IReadOnlyCollection<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.Count < 2)
			{
				return source;
			}

			return new ReverseReadOnlyCollection<TSource> (source);
		}

		/// <summary>
		/// Sorts the elements of a collection in ascending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A collection of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <param name="comparer">A comparer to compare keys.</param>
		/// <returns>A collection whose elements are sorted according to a key.</returns>
		public static IOrderedReadOnlyCollection<TSource> OrderBy<TSource, TKey> (
			this IReadOnlyCollection<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (keySelector == null)
			{
				throw new ArgumentNullException (nameof (keySelector));
			}

			Contract.EndContractBlock ();

			return new OrderedReadOnlyCollection<TSource, TKey> (source, keySelector, comparer, false, null);
		}

		/// <summary>
		/// Sorts the elements of a collection in descending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A collection of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <param name="comparer">A comparer to compare keys.</param>
		/// <returns>A collection whose elements are sorted in descending order according to a key.</returns>
		public static IOrderedReadOnlyCollection<TSource> OrderByDescending<TSource, TKey> (
			this IReadOnlyCollection<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (keySelector == null)
			{
				throw new ArgumentNullException (nameof (keySelector));
			}

			Contract.EndContractBlock ();

			return new OrderedReadOnlyCollection<TSource, TKey> (source, keySelector, comparer, true, null);
		}

		/// <summary>
		/// Performs a subsequent ordering of the elements in a collection in ascending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A collection that contains elements to sort.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">A comparer to compare keys.</param>
		/// <returns>A collection whose elements are sorted according to a key.</returns>
		public static IOrderedReadOnlyCollection<TSource> ThenBy<TSource, TKey> (
			this IOrderedReadOnlyCollection<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.Count < 2)
			{
				return source;
			}

			return source.CreateOrderedReadOnlyCollection<TKey> (keySelector, comparer, false);
		}

		/// <summary>
		/// Performs a subsequent ordering of the elements in a collection in descending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A collection that contains elements to sort.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">A comparer to compare keys.</param>
		/// <returns>A collection whose elements are sorted in descending order according to a key</returns>
		public static IOrderedReadOnlyCollection<TSource> ThenByDescending<TSource, TKey> (
			this IOrderedReadOnlyCollection<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.Count < 2)
			{
				return source;
			}

			return source.CreateOrderedReadOnlyCollection<TKey> (keySelector, comparer, true);
		}

		/// <summary>
		/// Concatenates two sequences.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="first">The first collection to concatenate.</param>
		/// <param name="second">The collection to concatenate to the first collection.</param>
		/// <returns>A collection that contains the concatenated elements of the two input collections.</returns>
		public static IReadOnlyCollection<TSource> Concat<TSource> (
			this IReadOnlyCollection<TSource> first,
			IReadOnlyCollection<TSource> second)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			Contract.EndContractBlock ();

			if (first.Count < 1)
			{
				return second;
			}

			if (second.Count < 1)
			{
				return first;
			}

			return new ConcatReadOnlyCollection<TSource> (first, second);
		}

		/// <summary>
		/// Applies a specified function to the corresponding elements of two collections, producing a collection of the results.
		/// </summary>
		/// <typeparam name="TFirst">The type of the elements of the first input collection.</typeparam>
		/// <typeparam name="TSecond">The type of the elements of the second input collection.</typeparam>
		/// <typeparam name="TResult">The type of the elements of the result collection.</typeparam>
		/// <param name="first">The first collection to merge.</param>
		/// <param name="second">The second collection to merge.</param>
		/// <param name="selector">A function that specifies how to merge the elements from the two collections.</param>
		/// <returns>A collection that contains merged elements of two input collections.</returns>
		public static IReadOnlyCollection<TResult> Zip<TFirst, TSecond, TResult> (
			this IReadOnlyCollection<TFirst> first,
			IReadOnlyCollection<TSecond> second,
			Func<TFirst, TSecond, TResult> selector)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			if (selector == null)
			{
				throw new ArgumentNullException (nameof (selector));
			}

			Contract.EndContractBlock ();

			return ((first.Count < 1) || (second.Count < 1)) ?
				(IReadOnlyCollection<TResult>)ReadOnlyList.Empty<TResult> () :
				new ZipReadOnlyCollection<TFirst, TSecond, TResult> (first, second, selector);
		}

		/// <summary>
		/// СCreates an array from a collection.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">A collection to create an array from.</param>
		/// <returns>An array that contains the elements from the input collection.</returns>
		public static TSource[] ToArray<TSource> (this IReadOnlyCollection<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var array = new TSource[source.Count];
			if (source.Count > 0)
			{
				if (source is IArrayDuplicableCollection<TSource> arrayDuplicableCollection)
				{
					arrayDuplicableCollection.CopyTo (array, 0);
				}
				else
				{
					int idx = 0;
					foreach (var item in source)
					{
						array[idx++] = item;
					}
				}
			}

			return array;
		}

		private class SkipReadOnlyCollection<TSource> :
			IReadOnlyCollection<TSource>
		{
			private readonly IReadOnlyCollection<TSource> _source;
			private readonly int _skipCount;

			internal SkipReadOnlyCollection (IReadOnlyCollection<TSource> source, int skipCount)
			{
				_source = source;
				_skipCount = skipCount;
			}

			public int Count => _source.Count - _skipCount;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				using var enumerator = _source.GetEnumerator ();
				var count = _skipCount;
				while ((count > 0) && enumerator.MoveNext ())
				{
					--count;
				}

				if (count <= 0)
				{
					while (enumerator.MoveNext ())
					{
						yield return enumerator.Current;
					}
				}
			}
		}

		private class TakeReadOnlyCollection<TSource> :
			IReadOnlyCollection<TSource>
		{
			private readonly IReadOnlyCollection<TSource> _source;
			private readonly int _count;

			internal TakeReadOnlyCollection (IReadOnlyCollection<TSource> source, int count)
			{
				_source = source;
				_count = count;
			}

			public int Count => _count;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				var count = _count;
				foreach (var item in _source)
				{
					yield return item;
					if (--count == 0)
					{
						break;
					}
				}
			}
		}

		private class SelectReadOnlyCollection<TSource, TResult> :
			IReadOnlyCollection<TResult>
		{
			private readonly IReadOnlyCollection<TSource> _source;
			private readonly Func<TSource, TResult> _selector;

			internal SelectReadOnlyCollection (IReadOnlyCollection<TSource> source, Func<TSource, TResult> selector)
			{
				_source = source;
				_selector = selector;
			}

			public int Count => _source.Count;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TResult> GetEnumerator ()
			{
				foreach (var item in _source)
				{
					yield return _selector.Invoke (item);
				}
			}
		}

		private class SelectIndexReadOnlyCollection<TSource, TResult> :
			IReadOnlyCollection<TResult>
		{
			private readonly IReadOnlyCollection<TSource> _source;
			private readonly Func<TSource, int, TResult> _selector;

			internal SelectIndexReadOnlyCollection (IReadOnlyCollection<TSource> source, Func<TSource, int, TResult> selector)
			{
				_source = source;
				_selector = selector;
			}

			public int Count => _source.Count;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TResult> GetEnumerator ()
			{
				int idx = 0;
				foreach (var item in _source)
				{
					yield return _selector.Invoke (item, idx++);
				}
			}
		}

		private class ReverseReadOnlyCollection<TSource> :
			IReadOnlyCollection<TSource>
		{
			private readonly IReadOnlyCollection<TSource> _source;
			private TSource[] _buffer = null;

			internal ReverseReadOnlyCollection (IReadOnlyCollection<TSource> source)
			{
				_source = source;
			}

			public int Count => _source.Count;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				if (_buffer == null)
				{
					var buf = ToArray (_source);
					Array.Reverse (buf);
					_buffer = buf;
				}

				return ((IEnumerable<TSource>)_buffer).GetEnumerator ();
			}
		}

		private class ConcatReadOnlyCollection<TSource> :
			IReadOnlyCollection<TSource>
		{
			private readonly IReadOnlyCollection<TSource> _first;
			private readonly IReadOnlyCollection<TSource> _second;

			internal ConcatReadOnlyCollection (IReadOnlyCollection<TSource> first, IReadOnlyCollection<TSource> second)
			{
				_first = first;
				_second = second;
			}

			public int Count => _first.Count + _second.Count;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				foreach (var item in _first)
				{
					yield return item;
				}

				foreach (var item in _second)
				{
					yield return item;
				}
			}
		}

		private class ZipReadOnlyCollection<TFirst, TSecond, TResult> :
			IReadOnlyCollection<TResult>
		{
			private readonly IReadOnlyCollection<TFirst> _first;
			private readonly IReadOnlyCollection<TSecond> _second;
			private readonly Func<TFirst, TSecond, TResult> _selector;

			internal ZipReadOnlyCollection (IReadOnlyCollection<TFirst> first, IReadOnlyCollection<TSecond> second, Func<TFirst, TSecond, TResult> selector)
			{
				_first = first;
				_second = second;
				_selector = selector;
			}

			public int Count => Math.Min (_first.Count, _second.Count);

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TResult> GetEnumerator ()
			{
				using var enumerator1 = _first.GetEnumerator ();
				using var enumerator2 = _second.GetEnumerator ();
				while (enumerator1.MoveNext () && enumerator2.MoveNext ())
				{
					yield return _selector.Invoke (enumerator1.Current, enumerator2.Current);
				}
			}
		}

		private abstract class OrderedReadOnlyCollection<TElement> :
			IOrderedReadOnlyCollection<TElement>
		{
			private readonly IReadOnlyCollection<TElement> _source;

			protected OrderedReadOnlyCollection (IReadOnlyCollection<TElement> source)
			{
				_source = source;
			}

			public int Count => _source.Count;

			protected IReadOnlyCollection<TElement> Source => _source;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TElement> GetEnumerator ()
			{
				if (_source.Count > 0)
				{
					var sorter = CreateSorter (null);
					var map = sorter.CreateIndex ();

					if (_source is IReadOnlyList<TElement> list)
					{
						for (int i = 0; i < list.Count; ++i)
						{
							yield return list[map[i]];
						}
					}
					else
					{
						var buffer = ToArray (_source);
						for (int i = 0; i < buffer.Length; ++i)
						{
							yield return buffer[map[i]];
						}
					}
				}
			}

			IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
			{
				return new OrderedReadOnlyCollection<TElement, TKey> (_source, keySelector, comparer, descending, this);
			}

			IOrderedReadOnlyCollection<TElement> IOrderedReadOnlyCollection<TElement>.CreateOrderedReadOnlyCollection<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
			{
				return new OrderedReadOnlyCollection<TElement, TKey> (_source, keySelector, comparer, descending, this);
			}

			internal abstract CollectionSorter<TElement> CreateSorter (CollectionSorter<TElement> next);
		}

		private class OrderedReadOnlyCollection<TElement, TKey> : OrderedReadOnlyCollection<TElement>
		{
			private readonly OrderedReadOnlyCollection<TElement> _parent = null;
			private readonly Func<TElement, TKey> _keySelector;
			private readonly IComparer<TKey> _comparer;
			private readonly bool _descending;

			internal OrderedReadOnlyCollection (IReadOnlyCollection<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, OrderedReadOnlyCollection<TElement> parent)
				: base (source)
			{
				if (source == null)
				{
					throw new ArgumentNullException (nameof (source));
				}

				if (keySelector == null)
				{
					throw new ArgumentNullException (nameof (keySelector));
				}

				_keySelector = keySelector;
				_comparer = comparer ?? Comparer<TKey>.Default;
				_descending = descending;
				_parent = parent;
			}

			internal override CollectionSorter<TElement> CreateSorter (CollectionSorter<TElement> next)
			{
				CollectionSorter<TElement> sorter = new CollectionSorter<TElement, TKey> (this.Source, _keySelector, _comparer, _descending, next);
				if (_parent != null)
				{
					sorter = _parent.CreateSorter (sorter);
				}

				return sorter;
			}
		}
	}
}
