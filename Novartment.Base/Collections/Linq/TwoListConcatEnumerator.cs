using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// An iterator over two lists one after the other.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements.</typeparam>
	internal sealed class TwoListConcatEnumerator<TSource> :
		IEnumerator<TSource>
	{
		private readonly IReadOnlyList<TSource> _first;
		private readonly IReadOnlyList<TSource> _second;
		private TSource _current;
		private int _index;

		internal TwoListConcatEnumerator (IReadOnlyList<TSource> first, IReadOnlyList<TSource> second)
		{
			_first = first;
			_second = second;
			_index = -1;
			_current = default;
		}

		/// <summary>
		/// Gets the element in the list at the current position of the enumerator.
		/// </summary>
		public TSource Current
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
			if (_index == (_first.Count + _second.Count))
			{
				_index = -2;
				_current = default;
				return false;
			}

			_current = (_index < _first.Count) ? _first[_index] : _second[_index - _first.Count];
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
