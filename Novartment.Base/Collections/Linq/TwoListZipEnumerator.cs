using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// An iterator over elements formed by applying the function to elements of two lists at once.
	/// </summary>
	/// <typeparam name="TFirst">The type of the elements of first list.</typeparam>
	/// <typeparam name="TSecond">The type of the elements of second list.</typeparam>
	/// <typeparam name="TResult">The type of the elements of resulting list.</typeparam>
	internal sealed class TwoListZipEnumerator<TFirst, TSecond, TResult> :
		IEnumerator<TResult>
	{
		private readonly IReadOnlyList<TFirst> _first;
		private readonly IReadOnlyList<TSecond> _second;
		private readonly Func<TFirst, TSecond, TResult> _selector;
		private TResult _current;
		private int _index;

		internal TwoListZipEnumerator (IReadOnlyList<TFirst> first, IReadOnlyList<TSecond> second, Func<TFirst, TSecond, TResult> selector)
		{
			_first = first;
			_second = second;
			_selector = selector;
			_index = -1;
			_current = default;
		}

		/// <summary>
		/// Gets the element in the list at the current position of the enumerator.
		/// </summary>
		public TResult Current
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

				return _current;
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
			var firstSecondMin = Math.Min (_first.Count, _second.Count);
			if (_index == firstSecondMin)
			{
				_index = -2;
				_current = default;
				return false;
			}

			_current = _selector.Invoke (_first[_index], _second[_index]);
			return true;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the list.
		/// </summary>
		public void Reset ()
		{
			_index = -1;
			_current = default;
		}

		/// <summary>
		/// Performs resources releasing.
		/// </summary>
		public void Dispose ()
		{
			_index = -2;
			_current = default;
		}
	}
}
