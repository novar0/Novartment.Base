using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Перечислитель диапазона элементов списка.
	/// </summary>
	/// <typeparam name="TSource">Тип элементов списка.</typeparam>
	internal class ListRangeEnumerator<TSource> : IEnumerator<TSource>
	{
		private readonly IReadOnlyList<TSource> _source;
		private readonly int _offset;
		private readonly int _count;
		private TSource _current;
		private int _index;

		internal ListRangeEnumerator (IReadOnlyList<TSource> source, int offset, int count)
		{
			_source = source;
			_offset = offset;
			_count = count;
			_index = -1;
			_current = default (TSource);
		}

		/// <summary>
		/// Получает текущий элемент перечислителя.
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
			if (_index == _count)
			{
				_index = -2;
				_current = default (TSource);
				return false;
			}

			_current = _source[_offset + _index];
			return true;
		}

		/// <summary>
		/// Возвращает перечислитель в исходное положение.
		/// </summary>
		public void Reset ()
		{
			_index = -1;
			_current = default (TSource);
		}

#pragma warning disable CA1063 // Implement IDisposable Correctly
		/// <summary>
		/// Освобождает занятые объектом ресурсы.
		/// </summary>
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			_index = -2;
			_current = default (TSource);
		}
	}
}
