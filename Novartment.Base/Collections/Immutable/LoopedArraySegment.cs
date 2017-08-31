using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using SystemArray = System.Array;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Сегмент массива, зацикленно переходящий через верхнюю границу массива.
	/// </summary>
	/// <typeparam name="T">Тип элементов сегмента массива.</typeparam>
	/// <remarks>
	/// Семантически эквивалентен System.ArraySegment&lt;&gt;,
	/// но является ссылочным типом чтобы гарантировать атомарность присвоений.
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
		/// Инициализирует новый экземпляр класса LoopedArraySegment использующий указанный массив.
		/// </summary>
		/// <param name="array">Массив, который будет использован сегментом.</param>
		public LoopedArraySegment (T[] array)
			: this (array, 0, ValidatedArrayLength (array))
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса LoopedArraySegment использующий указанный диапазона массива.
		/// </summary>
		/// <param name="array">Массив, который будет использован сегментом.</param>
		/// <param name="offset">
		/// Начальная позиция в исходном массиве.
		/// Зацикливается через край массива, то есть offset + count может быть больше чем размер массива.
		/// </param>
		/// <param name="count">Количество элементов исходного массива.</param>
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

			Contract.EndContractBlock ();

			_items = array;
			_capacity = array.Length;
			_offset = (count > 0) ? offset : 0;
			_count = count;
		}

		/// <summary>
		/// Получает исходный массив сегмента.
		/// </summary>
		public T[] Array => _items;

		/// <summary>
		/// Получает позицию начала сегмента в исходном массиве.
		/// </summary>
		public int Offset => _offset;

		/// <summary>
		/// Получает количество элементов сегмента массива.
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
		/// Получает или устанавливает элемент сегмента массива в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в сегменте массива.</param>
		public T this[int index]
		{
			get
			{
				if (index < 0)
				{
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
					throw new ArgumentOutOfRangeException (nameof (index));
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
				}

				Contract.EndContractBlock ();

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

				Contract.EndContractBlock ();

				index += _offset;
				if (index >= _capacity)
				{
					index -= _capacity;
				}

				_items[index] = value;
			}
		}

		/// <summary>
		/// Определяет равенство двух сегментов массива.
		/// </summary>
		/// <param name="first">Сегмент массива, который находится слева от оператора равенства.</param>
		/// <param name="second">Сегмент массива, который находится справа от оператора равенства.</param>
		/// <returns>true, если значения параметров first и second равны; в противном случае — false.</returns>
		public static bool operator == (LoopedArraySegment<T> first, LoopedArraySegment<T> second)
		{
			return ReferenceEquals (first, null) ?
				ReferenceEquals (second, null) :
				first.Equals (second);
		}

		/// <summary>
		/// Определяет неравенство двух сегментов массива.
		/// </summary>
		/// <param name="first">Сегмент массива, который находится слева от оператора равенства.</param>
		/// <param name="second">Сегмент массива, который находится справа от оператора равенства.</param>
		/// <returns>true, если значения параметров first и second не равны; в противном случае — false.</returns>
		public static bool operator != (LoopedArraySegment<T> first, LoopedArraySegment<T> second)
		{
			return !(ReferenceEquals (first, null) ?
				ReferenceEquals (second, null) :
				first.Equals (second));
		}

		/// <summary>
		/// Копирует элементы сегмента массива в указанный массив,
		/// начиная с указанного индекса конечного массива.
		/// </summary>
		/// <param name="array">Массив, в который копируются элементы сегмента массива.</param>
		/// <param name="arrayIndex">Позиция в конечном массиве, указывающая начало копирования.</param>
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

			Contract.EndContractBlock ();

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
		/// Получает перечислитель элементов сегмента массива.
		/// </summary>
		/// <returns>Перечислитель элементов сегмента массива.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Получает перечислитель элементов сегмента массива.
		/// </summary>
		/// <returns>Перечислитель элементов сегмента массива.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return new ArraySegmentLoopedEnumerator (this);
		}

		/// <summary>
		/// Получает хэш-код сегмента массива.
		/// </summary>
		/// <returns>Хэш-код сегмента массива.</returns>
		public override int GetHashCode ()
		{
			return (_items.GetHashCode () ^ _offset) ^ _count;
		}

		/// <summary>
		/// Определяет, равны ли два экземпляра сегмента массива.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим сегментом массива.</param>
		/// <returns>true, если указанный объект равен текущему сегменту массива; в противном случае — false.</returns>
		public override bool Equals (object obj)
		{
			var asSameClass = obj as LoopedArraySegment<T>;
			return (asSameClass != null) && Equals (asSameClass);
		}

		/// <summary>
		/// Определяет, равны ли два экземпляра сегмента массива.
		/// </summary>
		/// <param name="other">Сегмента массива, который требуется сравнить с текущим сегментом массива.</param>
		/// <returns>true, если указанный сегмент массива равен текущему сегменту массива; в противном случае — false.</returns>
		public bool Equals (LoopedArraySegment<T> other)
		{
			return ReferenceEquals (this, other) ||
				(!ReferenceEquals (other, null) &&
				(other._items == _items) &&
				(other._offset == _offset) &&
				(other._count == _count));
		}

		/// <summary>
		/// Получает структурный хэш-код сегмента массива.
		/// </summary>
		/// <param name="comparer">Объект, вычисляющий хэш-код элементов сегмента массива.</param>
		/// <returns>Структурный хэш-код сегмента массива.</returns>
		int IStructuralEquatable.GetHashCode (IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

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
		/// Определяет, равны ли структурно два экземпляра сегмента массива.
		/// </summary>
		/// <param name="other">Сегмента массива, который требуется сравнить с текущим сегментом массива.</param>
		/// <param name="comparer">Объект, определяющий, равенство элементов сегмента массива.</param>
		/// <returns>true, если указанный сегмент массива структурно равен текущему сегменту массива; в противном случае — false.</returns>
		bool IStructuralEquatable.Equals (object other, IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

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

		internal struct ArraySegmentLoopedEnumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private readonly LoopedArraySegment<T> _data;
			private int _index;
			private T _currentElement;

			internal ArraySegmentLoopedEnumerator (LoopedArraySegment<T> data)
			{
				_data = data;
				_index = -1;
				_currentElement = default (T);
			}

			/// <summary>
			/// Получает текущий элемент перечислителя.
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
				if (_index == _data._count)
				{
					_index = -2;
					_currentElement = default (T);
					return false;
				}

				_currentElement = _data[_index];
				return true;
			}

			/// <summary>
			/// Возвращает перечислитель в исходное положение.
			/// </summary>
			public void Reset ()
			{
				_index = -1;
				_currentElement = default (T);
			}

			/// <summary>
			/// Освобождает занятые объектом ресурсы.
			/// </summary>
			public void Dispose ()
			{
				_index = -2;
				_currentElement = default (T);
			}
		}
	}
}
