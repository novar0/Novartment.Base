using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Методы расширения к конечным множествам.
	/// </summary>
	public static class ReadOnlyFiniteSet
	{
		/// <summary>
		/// Создаёт пустое множество.
		/// </summary>
		/// <typeparam name="TResult">Тип элементов множества.</typeparam>
		/// <returns>Пустое множество.</returns>
		public static IReadOnlyFiniteSet<TResult> Empty<TResult> ()
		{
			return ReadOnlyList.EmptyReadOnlyList<TResult>.GetInstance ();
		}

		/// <summary>
		/// Создаёт множество целых чисел в заданном диапазоне.
		/// </summary>
		/// <param name="start">Значение первого целого числа множества.</param>
		/// <param name="count">Количество генерируемых последовательных целых чисел.</param>
		/// <returns>Множество, содержащее диапазон последовательных целых чисел.</returns>
		public static IReadOnlyFiniteSet<int> Range (int start, int count)
		{
			if ((count < 0) || (((long)start + (long)count - 1L) > (long)int.MaxValue))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (count < 1)
			{
				return ReadOnlyList.EmptyReadOnlyList<int>.GetInstance ();
			}

			return new ReadOnlyList.RangeReadOnlyList (start, count);
		}

		/// <summary>
		/// Возвращает указанное конечное множество или одноэлементное множество,
		/// содержащее указанное значение, если указанное множество пустое.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множества.</typeparam>
		/// <param name="source">Множество, для которого возвращается указанное значение, если оно пустое.</param>
		/// <param name="defaultValue">Значение, возвращаемое в случае пустого множества.</param>
		/// <returns>Множество, содержащее значение defaultValue, если множество source пустое;
		/// в противном случае возвращается source.</returns>
		public static IReadOnlyFiniteSet<TSource> DefaultIfEmpty<TSource> (this IReadOnlyFiniteSet<TSource> source, TSource defaultValue = default (TSource))
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return (source.Count > 0) ? source : new OneItemReadOnlyFiniteSet<TSource> (defaultValue, null);
		}

		/// <summary>
		/// Определяет, содержится ли указанный элемент в указанном конечном множестве.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множества.</typeparam>
		/// <param name="source">Множество, в котором требуется найти данное значение.</param>
		/// <param name="value">Значение, которое требуется найти в множестве.</param>
		/// <param name="notUsed">Не используется.</param>
		/// <returns>True, если множество содержит элемент с указанным значением, в противном случае — False.</returns>
		public static bool Contains<TSource> (
			this IReadOnlyFiniteSet<TSource> source,
			TSource value,
#pragma warning disable CA1801 // Review unused parameters
			IEqualityComparer<TSource> notUsed = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return source.Contains (value);
		}

		/// <summary>
		/// Изменяет порядок элементов указанного конечного множества на противоположный.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множества.</typeparam>
		/// <param name="source">Множество, элементы которого следует расставить в обратном порядке.</param>
		/// <returns>Множество, элементы которого содержат те же элементы, но следуют в противоположном порядке.</returns>
		public static IReadOnlyFiniteSet<TSource> Reverse<TSource> (this IReadOnlyFiniteSet<TSource> source)
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

			return new ReverseReadOnlyFiniteSet<TSource> (source);
		}

		/// <summary>
		/// Возвращает указанное множество.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множества.</typeparam>
		/// <param name="source">Конечное множество.</param>
		/// <param name="notUsed">Не используется.</param>
		/// <returns>Указанное множество.</returns>
		public static IReadOnlyFiniteSet<TSource> Distinct<TSource> (
			this IReadOnlyFiniteSet<TSource> source,
			IEqualityComparer<TSource> notUsed = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return source;
		}

		/// <summary>
		/// Получает разность (дополнение) указанных конечных множеств.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множеств.</typeparam>
		/// <param name="first">Множество, из которого будут получена разность с множеством second.</param>
		/// <param name="second">Множество, которое будет использовано для получения разности с множеством first.</param>
		/// <param name="notUsed">Не используется.</param>
		/// <returns>Множество, представляющее собой разность двух множеств.</returns>
		public static IReadOnlyFiniteSet<TSource> Except<TSource> (
			this IReadOnlyFiniteSet<TSource> first,
			IReadOnlyFiniteSet<TSource> second,
			IEqualityComparer<TSource> notUsed = null)
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
		/// Получает симметрическую разность указанных конечных множеств.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множеств.</typeparam>
		/// <param name="first">Первое множество для вычисления симметрической разности.</param>
		/// <param name="second">Второе множество для вычисления симметрической разности.</param>
		/// <param name="notUsed">Не используется.</param>
		/// <returns>Множество, представляющее собой симметрическую разность двух множеств.</returns>
		public static IReadOnlyFiniteSet<TSource> SymmetricExcept<TSource> (
			this IReadOnlyFiniteSet<TSource> first,
			IReadOnlyFiniteSet<TSource> second,
			IEqualityComparer<TSource> notUsed = null)
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
		/// Получает пересечение указанных конечных множеств.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множеств.</typeparam>
		/// <param name="first">Первое множество для вычисления пересечения.</param>
		/// <param name="second">Второе множество для вычисления пересечения.</param>
		/// <param name="notUsed">Не используется.</param>
		/// <returns>Множество, представляющее собой пересечение двух множеств.</returns>
		public static IReadOnlyFiniteSet<TSource> Intersect<TSource> (
			this IReadOnlyFiniteSet<TSource> first,
			IReadOnlyFiniteSet<TSource> second,
			IEqualityComparer<TSource> notUsed = null)
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
		/// Получает объединение указанных конечных множеств.
		/// </summary>
		/// <typeparam name="TSource">Тип элементов множеств.</typeparam>
		/// <param name="first">Первое множество для вычисления объединения.</param>
		/// <param name="second">Второе множество для вычисления объединения.</param>
		/// <param name="notUsed">Не используется.</param>
		/// <returns>Множество, представляющее собой объединение двух множеств.</returns>
		public static IReadOnlyFiniteSet<TSource> Union<TSource> (
			this IReadOnlyFiniteSet<TSource> first,
			IReadOnlyFiniteSet<TSource> second,
			IEqualityComparer<TSource> notUsed = null)
#pragma warning restore CA1801 // Review unused parameters
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

		private class ReverseReadOnlyFiniteSet<TSource> : IReadOnlyFiniteSet<TSource>
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

		private class OneItemReadOnlyFiniteSet<T> : IReadOnlyFiniteSet<T>
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

		private class ExceptReadOnlyFiniteSet<TSource> : IReadOnlyFiniteSet<TSource>
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

		private class SymmetricExceptReadOnlyFiniteSet<TSource> : IReadOnlyFiniteSet<TSource>
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

		private class IntersectReadOnlyFiniteSet<TSource> : IReadOnlyFiniteSet<TSource>
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

		private class UnionReadOnlyFiniteSet<TSource> : IReadOnlyFiniteSet<TSource>
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
