using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Extension methods for read only lists.
	/// </summary>
	public static class ReadOnlyList
	{
		/// <summary>
		/// Returns an empty list.
		/// </summary>
		/// <typeparam name="TResult">The type of the elements.</typeparam>
		/// <returns>Empty list.</returns>
		public static IReadOnlyList<TResult> Empty<TResult> ()
		{
			return EmptyReadOnlyList<TResult>.GetInstance ();
		}

		/// <summary>
		/// Generates a list of integral numbers within a specified range.
		/// </summary>
		/// <param name="start">The value of the first integer in the list.</param>
		/// <param name="count">The number of sequential integers to generate.</param>
		/// <returns>A list that contains a range of sequential integral numbers.</returns>
		public static IReadOnlyList<int> Range (int start, int count)
		{
			if ((count < 0) || (((long)start + (long)count - 1L) > (long)int.MaxValue))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			return (count == 0) ?
				(IReadOnlyList<int>)EmptyReadOnlyList<int>.GetInstance () :
				new RangeReadOnlyList (start, count);
		}

		/// <summary>
		/// Generates a list that contains one repeated value.
		/// </summary>
		/// <typeparam name="TResult">The type of the value to be repeated in the result list.</typeparam>
		/// <param name="element">The value to be repeated.</param>
		/// <param name="count">The number of times to repeat the value in the generated list.</param>
		/// <returns>A list that contains a repeated value.</returns>
		public static IReadOnlyList<TResult> Repeat<TResult> (TResult element, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			return (count == 0) ?
				(IReadOnlyList<TResult>)EmptyReadOnlyList<TResult>.GetInstance () :
				new RepeatReadOnlyList<TResult> (element, count);
		}

		/// <summary>
		/// Returns the first element of a list.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">The list to return the first element of.</param>
		/// <returns>The first element in the specified list.</returns>
		public static TSource First<TSource> (this IReadOnlyList<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.Count < 1)
			{
				throw new InvalidOperationException ("No elements in collection");
			}

			return source[0];
		}

		/// <summary>
		/// Returns the first element of a list, or a default value if the list contains no elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return the first element of.</param>
		/// <returns>default(TSource) if source is empty; otherwise, the first element in source.</returns>
		public static TSource FirstOrDefault<TSource> (this IReadOnlyList<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return (source.Count > 0) ? source[0] : default;
		}

		/// <summary>
		/// Returns the last element of a list.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return the last element of.</param>
		/// <returns>The value at the last position in the source list.</returns>
		public static TSource Last<TSource> (this IReadOnlyList<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.Count < 1)
			{
				throw new InvalidOperationException ("No elements in collection");
			}

			return source[source.Count - 1];
		}

		/// <summary>
		/// Returns the last element of a list, or a default value if the list contains no elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return the last element of.</param>
		/// <returns>default(TSource) if the source list is empty;
		/// otherwise, the last element in the list.</returns>
		public static TSource LastOrDefault<TSource> (this IReadOnlyList<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return (source.Count > 0) ? source[source.Count - 1] : default;
		}

		/// <summary>
		/// Returns the element at a specified index in a list.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return an element from.</param>
		/// <param name="index">The zero-based index of the element to retrieve.</param>
		/// <returns>The element at the specified position in the source list.</returns>
		public static TSource ElementAt<TSource> (this IReadOnlyList<TSource> source, int index)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if ((index < 0) || (index >= source.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			return source[index];
		}

		/// <summary>
		/// Returns the element at a specified index in a list or a default value if the index is out of range.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return an element from.</param>
		/// <param name="index">The zero-based index of the element to retrieve.</param>
		/// <returns>default(TSource) if the index is outside the bounds of the source list;
		/// otherwise, the element at the specified position in the source list.</returns>
		public static TSource ElementAtOrDefault<TSource> (this IReadOnlyList<TSource> source, int index)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if ((index < 0) || (index >= source.Count))
			{
				return default;
			}

			return source[index];
		}

		/// <summary>
		/// Returns the elements of a list, or a default valued singleton list if the list is empty.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return the specified value for if it is empty.</param>
		/// <param name="defaultValue">The value to return if the list is empty.</param>
		/// <returns>A list that contains defaultValue if source is empty; otherwise, source.</returns>
		public static IReadOnlyList<TSource> DefaultIfEmpty<TSource> (this IReadOnlyList<TSource> source, TSource defaultValue = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return (source.Count > 0) ? source : ReadOnlyList.Repeat (defaultValue, 1);
		}

		/// <summary>
		/// Bypasses a specified number of elements in a list and then returns the remaining elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return elements from.</param>
		/// <param name="count">The number of elements to skip before returning the remaining elements.</param>
		/// <returns>The list that contains the elements that occur after the specified index in the input list.</returns>
		public static IReadOnlyList<TSource> Skip<TSource> (this IReadOnlyList<TSource> source, int count)
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
				return EmptyReadOnlyList<TSource>.GetInstance ();
			}

			return new SkipReadOnlyList<TSource> (source, count);
		}

		/// <summary>
		/// Returns a specified number of contiguous elements from the start of a list.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The list to return elements from.</param>
		/// <param name="count">The number of elements to return.</param>
		/// <returns>A list that contains the specified number of elements from the start of the input list.</returns>
		public static IReadOnlyList<TSource> Take<TSource> (this IReadOnlyList<TSource> source, int count)
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
				return EmptyReadOnlyList<TSource>.GetInstance ();
			}

			if (count >= source.Count)
			{
				return source;
			}

			return new TakeReadOnlyList<TSource> (source, count);
		}

		/// <summary>
		/// Projects each element of a list into a new form.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A list of values to invoke a transform function on.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>A list whose elements are the result of invoking the transform function on each element of source.</returns>
		public static IReadOnlyList<TResult> Select<TSource, TResult> (this IReadOnlyList<TSource> source, Func<TSource, TResult> selector)
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
				(IReadOnlyList<TResult>)EmptyReadOnlyList<TResult>.GetInstance () :
				new SelectReadOnlyList<TSource, TResult> (source, selector);
		}

		/// <summary>
		/// Projects each element of a list into a new form by incorporating the element's index.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A list of values to invoke a transform function on.</param>
		/// <param name="selector">A transform function to apply to each source element;
		/// the second parameter of the function represents the index of the source element.</param>
		/// <returns>A list whose elements are the result of invoking the transform function on each element of source.</returns>
		public static IReadOnlyList<TResult> Select<TSource, TResult> (this IReadOnlyList<TSource> source, Func<TSource, int, TResult> selector)
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
				(IReadOnlyList<TResult>)EmptyReadOnlyList<TResult>.GetInstance () :
				new SelectIndexReadOnlyList<TSource, TResult> (source, selector);
		}

		/// <summary>
		/// Inverts the order of the elements in a list.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">A list of values to reverse.</param>
		/// <returns>A list whose elements correspond to those of the input list in reverse order.</returns>
		public static IReadOnlyList<TSource> Reverse<TSource> (this IReadOnlyList<TSource> source)
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

			return new ReverseReadOnlyList<TSource> (source);
		}

		/// <summary>
		/// Sorts the elements of a list in ascending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A list of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <param name="comparer">An comparer to compare keys.</param>
		/// <returns>A list whose elements are sorted according to a key.</returns>
		public static IOrderedReadOnlyList<TSource> OrderBy<TSource, TKey> (
			this IReadOnlyList<TSource> source,
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

			return new OrderedReadOnlyList<TSource, TKey> (source, keySelector, comparer, false, null);
		}

		/// <summary>
		/// Sorts the elements of a list in descending order.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A list of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <param name="comparer">An comparer to compare keys.</param>
		/// <returns>A list whose elements are sorted in descending order according to a key.</returns>
		public static IOrderedReadOnlyList<TSource> OrderByDescending<TSource, TKey> (
			this IReadOnlyList<TSource> source,
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

			return new OrderedReadOnlyList<TSource, TKey> (source, keySelector, comparer, true, null);
		}

		/// <summary>
		/// Performs a subsequent ordering of the elements in a list in ascending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">The list that contains elements to sort.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">An comparer to compare keys.</param>
		/// <returns>A list whose elements are sorted according to a key.</returns>
		public static IOrderedReadOnlyList<TSource> ThenBy<TSource, TKey> (
			this IOrderedReadOnlyList<TSource> source,
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

			return source.CreateOrderedReadOnlyList<TKey> (keySelector, comparer, false);
		}

		/// <summary>
		/// Performs a subsequent ordering of the elements in a list in descending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">The list that contains elements to sort.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">An comparer to compare keys.</param>
		/// <returns>A list whose elements are sorted in descending order according to a key.</returns>
		public static IOrderedReadOnlyList<TSource> ThenByDescending<TSource, TKey> (
			this IOrderedReadOnlyList<TSource> source,
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

			return source.CreateOrderedReadOnlyList<TKey> (keySelector, comparer, true);
		}

		/// <summary>
		/// Concatenates two lists.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="first">The first list to concatenate.</param>
		/// <param name="second">The list to concatenate to the first list.</param>
		/// <returns>A list that contains the concatenated elements of the two input lists.</returns>
		public static IReadOnlyList<TSource> Concat<TSource> (this IReadOnlyList<TSource> first, IReadOnlyList<TSource> second)
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

			return new ConcatReadOnlyList<TSource> (first, second);
		}

		/// <summary>
		/// Applies a specified function to the corresponding elements of two lists, producing a list of the results.
		/// </summary>
		/// <typeparam name="TFirst">The type of the elements of the first input list.</typeparam>
		/// <typeparam name="TSecond">The type of the elements of the second input list.</typeparam>
		/// <typeparam name="TResult">The type of the elements of the result list.</typeparam>
		/// <param name="first">The first list to merge.</param>
		/// <param name="second">The second list to merge.</param>
		/// <param name="selector">A function that specifies how to merge the elements from the two lists.</param>
		/// <returns>A list that contains merged elements of two input lists.</returns>
		public static IReadOnlyList<TResult> Zip<TFirst, TSecond, TResult> (this IReadOnlyList<TFirst> first, IReadOnlyList<TSecond> second, Func<TFirst, TSecond, TResult> selector)
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
				(IReadOnlyList<TResult>)EmptyReadOnlyList<TResult>.GetInstance () :
				new ZipReadOnlyList<TFirst, TSecond, TResult> (first, second, selector);
		}

		internal static class EmptyReadOnlyList<T>
		{
			private static volatile EmptySetImplementation _instance;

			internal static EmptySetImplementation GetInstance ()
			{
				if (_instance == null)
				{
					_instance = new EmptySetImplementation ();
				}

				return _instance;
			}

			internal sealed class EmptySetImplementation :
				IReadOnlyFiniteSet<T>,
				IReadOnlyList<T>
			{
				private readonly IEnumerable<T> _items = Array.Empty<T> ();

				public EmptySetImplementation()
				{
				}

				public int Count => 0;

				public T this[int index] => throw new ArgumentOutOfRangeException(nameof(index));

				public bool Contains (T item) => false;

				IEnumerator IEnumerable.GetEnumerator () => _items.GetEnumerator ();

				public IEnumerator<T> GetEnumerator () => _items.GetEnumerator ();
			}
		}

		internal sealed class RangeReadOnlyList :
			IReadOnlyList<int>,
			IReadOnlyFiniteSet<int>
		{
			private readonly int _start;
			private readonly int _count;

			internal RangeReadOnlyList (int start, int count)
			{
				_start = start;
				_count = count;
			}

			public int Count => _count;

			public int this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _start + index;
				}
			}

			public bool Contains (int item)
			{
				return (item >= _start) && (item < (_start + _count));
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<int> GetEnumerator ()
			{
				for (int i = 0; i < _count; ++i)
				{
					yield return _start + i;
				}
			}
		}

		private sealed class RepeatReadOnlyList<TResult> :
			IReadOnlyList<TResult>
		{
			private readonly TResult _element;
			private readonly int _count;

			internal RepeatReadOnlyList (TResult element, int count)
			{
				_element = element;
				_count = count;
			}

			public int Count => _count;

			public TResult this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _element;
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TResult> GetEnumerator ()
			{
				for (int i = 0; i < _count; ++i)
				{
					yield return _element;
				}
			}
		}

		private sealed class SkipReadOnlyList<TSource> :
			IReadOnlyList<TSource>
		{
			private readonly IReadOnlyList<TSource> _source;
			private readonly int _skipCount;

			internal SkipReadOnlyList (IReadOnlyList<TSource> source, int skipCount)
			{
				_source = source;
				_skipCount = skipCount;
			}

			public int Count => _source.Count - _skipCount;

			public TSource this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _source[_skipCount + index];
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				return new ListRangeEnumerator<TSource> (_source, _skipCount, _source.Count - _skipCount);
			}
		}

		private sealed class TakeReadOnlyList<TSource> :
			IReadOnlyList<TSource>
		{
			private readonly IReadOnlyList<TSource> _source;
			private readonly int _count;

			internal TakeReadOnlyList (IReadOnlyList<TSource> source, int count)
			{
				_source = source;
				_count = count;
			}

			public int Count => _count;

			public TSource this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _source[index];
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				return new ListRangeEnumerator<TSource> (_source, 0, _count);
			}
		}

		private sealed class SelectReadOnlyList<TSource, TResult> :
			IReadOnlyList<TResult>
		{
			private readonly IReadOnlyList<TSource> _source;
			private readonly Func<TSource, TResult> _selector;

			internal SelectReadOnlyList (IReadOnlyList<TSource> source, Func<TSource, TResult> selector)
			{
				_source = source;
				_selector = selector;
			}

			public int Count => _source.Count;

			public TResult this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _selector.Invoke(_source[index]);
				}
			}

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

		private sealed class SelectIndexReadOnlyList<TSource, TResult> :
			IReadOnlyList<TResult>
		{
			private readonly IReadOnlyList<TSource> _source;
			private readonly Func<TSource, int, TResult> _selector;

			internal SelectIndexReadOnlyList (IReadOnlyList<TSource> source, Func<TSource, int, TResult> selector)
			{
				_source = source;
				_selector = selector;
			}

			public int Count => _source.Count;

			public TResult this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _selector.Invoke (_source[index], index);
				}
			}

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

		private sealed class ReverseReadOnlyList<TSource> :
			IReadOnlyList<TSource>
		{
			private readonly IReadOnlyList<TSource> _source;

			internal ReverseReadOnlyList (IReadOnlyList<TSource> source)
			{
				_source = source;
			}

			public int Count => _source.Count;

			public TSource this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _source[_source.Count - index - 1];
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				return new ReverseListEnumerator<TSource> (_source);
			}
		}

		private sealed class ConcatReadOnlyList<TSource> :
			IReadOnlyList<TSource>
		{
			private readonly IReadOnlyList<TSource> _first;
			private readonly IReadOnlyList<TSource> _second;

			internal ConcatReadOnlyList (IReadOnlyList<TSource> first, IReadOnlyList<TSource> second)
			{
				_first = first;
				_second = second;
			}

			public int Count => _first.Count + _second.Count;

			public TSource this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return (index < _first.Count) ? _first[index] : _second[index - _first.Count];
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				return new TwoListConcatEnumerator<TSource> (_first, _second);
			}
		}

		private sealed class ZipReadOnlyList<TFirst, TSecond, TResult> :
			IReadOnlyList<TResult>
		{
			private readonly IReadOnlyList<TFirst> _first;
			private readonly IReadOnlyList<TSecond> _second;
			private readonly Func<TFirst, TSecond, TResult> _selector;

			internal ZipReadOnlyList (IReadOnlyList<TFirst> first, IReadOnlyList<TSecond> second, Func<TFirst, TSecond, TResult> selector)
			{
				_first = first;
				_second = second;
				_selector = selector;
			}

			public int Count => Math.Min (_first.Count, _second.Count);

			public TResult this[int index]
			{
				get
				{
					// Following trick can reduce the range check by one
					if ((uint)index >= (uint)this.Count)
					{
						throw new ArgumentOutOfRangeException (nameof (index));
					}

					return _selector.Invoke (_first[index], _second[index]);
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TResult> GetEnumerator ()
			{
				return new TwoListZipEnumerator<TFirst, TSecond, TResult> (_first, _second, _selector);
			}
		}

		private abstract class OrderedReadOnlyList<TElement> :
			IOrderedReadOnlyList<TElement>
		{
			private readonly IReadOnlyList<TElement> _source;

			private int[] _indexMap = null;

			protected OrderedReadOnlyList (IReadOnlyList<TElement> source)
			{
				_source = source;
			}

			public int Count => _source.Count;

			protected IReadOnlyCollection<TElement> Source => _source;

			public TElement this[int index]
			{
				get
				{
					if (_indexMap == null)
					{
						var sorter = CreateSorter (null);
						_indexMap = sorter.CreateIndex ();
					}

					return _source[_indexMap[index]];
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TElement> GetEnumerator ()
			{
				if (_source.Count > 0)
				{
					if (_indexMap == null)
					{
						var sorter = CreateSorter (null);
						_indexMap = sorter.CreateIndex ();
					}

					for (int i = 0; i < _source.Count; ++i)
					{
						yield return _source[_indexMap[i]];
					}
				}
			}

			IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
			{
				return new OrderedReadOnlyList<TElement, TKey> (_source, keySelector, comparer, descending, this);
			}

			IOrderedReadOnlyCollection<TElement> IOrderedReadOnlyCollection<TElement>.CreateOrderedReadOnlyCollection<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
			{
				return new OrderedReadOnlyList<TElement, TKey> (_source, keySelector, comparer, descending, this);
			}

			IOrderedReadOnlyList<TElement> IOrderedReadOnlyList<TElement>.CreateOrderedReadOnlyList<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
			{
				return new OrderedReadOnlyList<TElement, TKey> (_source, keySelector, comparer, descending, this);
			}

			internal abstract CollectionSorter<TElement> CreateSorter (CollectionSorter<TElement> next);
		}

		private sealed class OrderedReadOnlyList<TElement, TKey> : OrderedReadOnlyList<TElement>
		{
			private readonly OrderedReadOnlyList<TElement> _parent = null;
			private readonly Func<TElement, TKey> _keySelector;
			private readonly IComparer<TKey> _comparer;
			private readonly bool _descending;

			internal OrderedReadOnlyList (IReadOnlyList<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, OrderedReadOnlyList<TElement> parent)
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
