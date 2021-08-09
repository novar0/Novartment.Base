using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Extension methods for read only finite sets.
	/// </summary>
	public static class ReadOnlyFiniteSet
	{
		/// <summary>
		/// Creates an empty set.
		/// </summary>
		/// <typeparam name="TResult">The type of the elements.</typeparam>
		/// <returns>An empty set.</returns>
		public static IReadOnlyFiniteSet<TResult> Empty<TResult> ()
		{
			return ReadOnlyList.EmptyReadOnlyList<TResult>.GetInstance ();
		}

		/// <summary>
		/// Creates a set consisting of integers in the specified range.
		/// </summary>
		/// <param name="start">The value of the first integer in the set.</param>
		/// <param name="count">The number of sequential integers to generate.</param>
		/// <returns>Set consisting of integers in the specified range.</returns>
		public static IReadOnlyFiniteSet<int> Range (int start, int count)
		{
			if ((count < 0) || (((long)start + (long)count - 1L) > (long)int.MaxValue))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			if (count < 1)
			{
				return ReadOnlyList.EmptyReadOnlyList<int>.GetInstance ();
			}

			return new ReadOnlyList.RangeReadOnlyList (start, count);
		}

		/// <summary>
		/// Returns a set consisting of the elements of the specified set or a set consisting of the single specified value if the set is empty.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">The set to return the specified value for if it is empty.</param>
		/// <param name="defaultValue">The value to return if the set is empty.</param>
		/// <returns>An set that contains defaultValue if source is empty; otherwise, source.</returns>
		public static IReadOnlyFiniteSet<TSource> DefaultIfEmpty<TSource> (this IReadOnlyFiniteSet<TSource> source, TSource defaultValue = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			return (source.Count > 0) ? source : new OneItemReadOnlyFiniteSet<TSource> (defaultValue, null);
		}

		/// <summary>
		/// Inverts the order of the elements in a set.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="source">A set of values to reverse.</param>
		/// <returns>A set whose elements correspond to those of the input set in reverse order.</returns>
		public static IReadOnlyFiniteSet<TSource> Reverse<TSource> (this IReadOnlyFiniteSet<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.Count < 2)
			{
				return source;
			}

			return new ReverseReadOnlyFiniteSet<TSource> (source);
		}

		/// <summary>
		/// Produces the set difference of two sets.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="first">An set whose elements that are not also in second will be returned.</param>
		/// <param name="second">An set whose elements that also occur in the first set
		/// will cause those elements to be removed from the returned set.</param>
		/// <returns>A set that contains the set difference of the elements of two sets.</returns>
		public static IReadOnlyFiniteSet<TSource> Except<TSource> (this IReadOnlyFiniteSet<TSource> first, IReadOnlyFiniteSet<TSource> second)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			if ((first.Count < 1) || (second.Count < 1))
			{
				return first;
			}

			if (first == second)
			{
				return ReadOnlyList.EmptyReadOnlyList<TSource>.GetInstance ();
			}

			// заранее считаем количество элементов
			int count = first.Count;
			if (first.Count < second.Count)
			{
				foreach (var item in first)
				{
					var isSecondContainsItem = second.Contains (item);
					if (isSecondContainsItem)
					{
						count--;
					}
				}
			}
			else
			{
				foreach (var item in second)
				{
					var firstContainsItem = first.Contains (item);
					if (firstContainsItem)
					{
						count--;
					}
				}
			}

			return new ExceptReadOnlyFiniteSet<TSource> (first, second, count);
		}

		/// <summary>
		/// Produces the symmetric set difference of two sets.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="first">A first set for creating symmetric difference.</param>
		/// <param name="second">A second set for creating symmetric difference.</param>
		/// <returns>A set that contains the symmetric set difference of the elements of two sets.</returns>
		public static IReadOnlyFiniteSet<TSource> SymmetricExcept<TSource> (this IReadOnlyFiniteSet<TSource> first, IReadOnlyFiniteSet<TSource> second)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			if (first.Count < 1)
			{
				return second;
			}

			if (second.Count < 1)
			{
				return first;
			}

			if (first == second)
			{
				return ReadOnlyList.EmptyReadOnlyList<TSource>.GetInstance ();
			}

			// заранее считаем количество элементов
			int count = first.Count + second.Count;
			foreach (var item in first)
			{
				var secondContainsItem = second.Contains (item);
				if (secondContainsItem)
				{
					count--;
				}
			}

			foreach (var item in second)
			{
				var firstContainsItem = first.Contains (item);
				if (firstContainsItem)
				{
					count--;
				}
			}

			return new SymmetricExceptReadOnlyFiniteSet<TSource> (first, second, count);
		}

		/// <summary>
		/// Produces the set intersection of two sets.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="first">A set whose distinct elements that also appear in set will be returned.</param>
		/// <param name="second">A set whose distinct elements that also appear in the first set will be returned.</param>
		/// <returns>A set that contains the elements that form the set intersection of two sets.</returns>
		public static IReadOnlyFiniteSet<TSource> Intersect<TSource> (this IReadOnlyFiniteSet<TSource> first, IReadOnlyFiniteSet<TSource> second)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			if ((first.Count < 1) || (first == second))
			{
				return first;
			}

			if (second.Count < 1)
			{
				return second;
			}

			// делаем первым тот, в котором больше элементов
			if (first.Count < second.Count)
			{
				var ttt = first;
				first = second;
				second = ttt;
			}

			// заранее считаем количество элементов
			int count = 0;
			foreach (var item in second)
			{
				var firstContainsItem = first.Contains (item);
				if (firstContainsItem)
				{
					count++;
				}
			}

			return new IntersectReadOnlyFiniteSet<TSource> (first, second, count);
		}

		/// <summary>
		/// Produces the set union of two sets.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements.</typeparam>
		/// <param name="first">A first set for the union.</param>
		/// <param name="second">A second set for the union.</param>
		/// <returns>An set that contains the elements from both input sets.</returns>
		public static IReadOnlyFiniteSet<TSource> Union<TSource> (this IReadOnlyFiniteSet<TSource> first, IReadOnlyFiniteSet<TSource> second)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			if (first.Count < 1)
			{
				return second;
			}

			if ((second.Count < 1) || (first == second))
			{
				return first;
			}

			// делаем первым тот, в котором больше элементов
			if (first.Count < second.Count)
			{
				var ttt = first;
				first = second;
				second = ttt;
			}

			// заранее считаем количество элементов
			int count = first.Count;
			foreach (var item in second)
			{
				var firstContainsItem = first.Contains (item);
				if (!firstContainsItem)
				{
					count++;
				}
			}

			return new UnionReadOnlyFiniteSet<TSource> (first, second, count);
		}

		private sealed class ReverseReadOnlyFiniteSet<TSource> :
			IReadOnlyFiniteSet<TSource>
		{
			private readonly IReadOnlyFiniteSet<TSource> _source;
			private TSource[] _buffer = null;

			internal ReverseReadOnlyFiniteSet (IReadOnlyFiniteSet<TSource> source)
			{
				_source = source;
			}

			public int Count => _source.Count;

			public bool Contains (TSource item)
			{
				return _source.Contains (item);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				if (_buffer == null)
				{
					var buf = ReadOnlyCollection.ToArray (_source);
					Array.Reverse (buf);
					_buffer = buf;
				}

				return ((IEnumerable<TSource>)_buffer).GetEnumerator ();
			}
		}

		private sealed class OneItemReadOnlyFiniteSet<T> :
			IReadOnlyFiniteSet<T>
		{
			private readonly T[] _item = new T[1];
			private readonly IEqualityComparer<T> _comparer;

			internal OneItemReadOnlyFiniteSet (T item, IEqualityComparer<T> comparer)
			{
				_item[0] = item;
				_comparer = comparer ?? EqualityComparer<T>.Default;
			}

			public int Count => 1;

			public bool Contains (T item)
			{
				return _comparer.Equals (_item[0], item);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<T> GetEnumerator ()
			{
				return ((IEnumerable<T>)_item).GetEnumerator ();
			}
		}

		private sealed class ExceptReadOnlyFiniteSet<TSource> :
			IReadOnlyFiniteSet<TSource>
		{
			private readonly IReadOnlyFiniteSet<TSource> _first;
			private readonly IReadOnlyFiniteSet<TSource> _second;
			private readonly int _count;

			internal ExceptReadOnlyFiniteSet (
				IReadOnlyFiniteSet<TSource> first,
				IReadOnlyFiniteSet<TSource> second,
				int count)
			{
				_first = first;
				_second = second;
				_count = count;
			}

			public int Count => _count;

			public bool Contains (TSource item)
			{
				return _first.Contains (item) && !_second.Contains (item);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				foreach (var item in _first)
				{
					var secondContainsItem = _second.Contains (item);
					if (!secondContainsItem)
					{
						yield return item;
					}
				}
			}
		}

		private sealed class SymmetricExceptReadOnlyFiniteSet<TSource> :
			IReadOnlyFiniteSet<TSource>
		{
			private readonly IReadOnlyFiniteSet<TSource> _first;
			private readonly IReadOnlyFiniteSet<TSource> _second;
			private readonly int _count;

			internal SymmetricExceptReadOnlyFiniteSet (
				IReadOnlyFiniteSet<TSource> first,
				IReadOnlyFiniteSet<TSource> second,
				int count)
			{
				_first = first;
				_second = second;
				_count = count;
			}

			public int Count => _count;

			public bool Contains (TSource item)
			{
				return _first.Contains (item) ^ _second.Contains (item);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				foreach (var item in _first)
				{
					var secondContainsItem = _second.Contains (item);
					if (!secondContainsItem)
					{
						yield return item;
					}
				}

				foreach (var item in _second)
				{
					var firstContainsItem = _first.Contains (item);
					if (!firstContainsItem)
					{
						yield return item;
					}
				}
			}
		}

		private sealed class IntersectReadOnlyFiniteSet<TSource> :
			IReadOnlyFiniteSet<TSource>
		{
			private readonly IReadOnlyFiniteSet<TSource> _first;
			private readonly IReadOnlyFiniteSet<TSource> _second;
			private readonly int _count;

			internal IntersectReadOnlyFiniteSet (
				IReadOnlyFiniteSet<TSource> first,
				IReadOnlyFiniteSet<TSource> second,
				int count)
			{
				_first = first;
				_second = second;
				_count = count;
			}

			public int Count => _count;

			public bool Contains (TSource item)
			{
				return _first.Contains (item) && _second.Contains (item);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<TSource> GetEnumerator ()
			{
				foreach (var item in _second)
				{
					var firstContainsIitem = _first.Contains (item);
					if (firstContainsIitem)
					{
						yield return item;
					}
				}
			}
		}

		private sealed class UnionReadOnlyFiniteSet<TSource> :
			IReadOnlyFiniteSet<TSource>
		{
			private readonly IReadOnlyFiniteSet<TSource> _first;
			private readonly IReadOnlyFiniteSet<TSource> _second;
			private readonly int _count;

			internal UnionReadOnlyFiniteSet (
				IReadOnlyFiniteSet<TSource> first,
				IReadOnlyFiniteSet<TSource> second,
				int count)
			{
				_first = first;
				_second = second;
				_count = count;
			}

			public int Count => _count;

			public bool Contains (TSource item)
			{
				return _first.Contains (item) || _second.Contains (item);
			}

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
					var firstContainsItem = _first.Contains (item);
					if (!firstContainsItem)
					{
						yield return item;
					}
				}
			}
		}
	}
}
