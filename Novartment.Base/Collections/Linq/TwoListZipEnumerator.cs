using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Перечислитель элементов, образованных применением функции к элементам сразу двух списков.
	/// </summary>
	/// <typeparam name="TFirst">Тип элементов первого списка.</typeparam>
	/// <typeparam name="TSecond">Тип элементов второго списка.</typeparam>
	/// <typeparam name="TResult">Тип элементов результирующего списка.</typeparam>
	internal class TwoListZipEnumerator<TFirst, TSecond, TResult> : IEnumerator<TResult>
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
			_current = default (TResult);
		}

		/// <summary>
		/// Получает текущий элемент перечислителя.
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

		object IEnumerator.Current => Current;

		/// <summary>
		/// Перемещает перечислитель к следующему элементу строки.
		/// </summary>
		/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
		/// false, если перечислитель достиг конца.</returns>
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
				_current = default (TResult);
				return false;
			}

			_current = _selector.Invoke (_first[_index], _second[_index]);
			return true;
		}

		/// <summary>
		/// Возвращает перечислитель в исходное положение.
		/// </summary>
		public void Reset ()
		{
			_index = -1;
			_current = default (TResult);
		}

#pragma warning disable CA1063 // Implement IDisposable Correctly
		/// <summary>
		/// Освобождает занятые объектом ресурсы.
		/// </summary>
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			_index = -2;
			_current = default (TResult);
		}
	}
}
