using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// Коллекция только для чтения на основе массива.
	/// </summary>
	/// <typeparam name="T">Тип элементов массива.</typeparam>
	/// <remarks>Никакой дополнительной функциональности, только делегирование к указанному массиву.</remarks>
	[SuppressMessage ("Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name."),
	DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class ReadOnlyArray<T> :
		IReadOnlyList<T>,
		IArrayDuplicableCollection<T>,
		IEquatable<ReadOnlyArray<T>>,
		IStructuralEquatable
	{
		private readonly T[] _items;
		private readonly int _count;

		/// <summary>
		/// Инициализирует новый экземпляр класса ReadOnlyArray на основе указанного массива.
		/// </summary>
		/// <param name="items">Одномерный, начинающийся с нуля массив,
		/// на основе которого будет создана коллекция только для чтения.</param>
		public ReadOnlyArray (T[] items)
		{
			if (items == null)
			{
				throw new ArgumentNullException (nameof (items));
			}
			var isNormalArray = (items.Rank == 1) && (items.GetLowerBound (0) == 0);
			if (!isNormalArray)
			{
				throw new ArgumentOutOfRangeException (nameof (items));
			}
			Contract.EndContractBlock ();

			_items = items;
			_count = items.Length;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ReadOnlyArray на основе указанного массива и количества элементов в нём.
		/// </summary>
		/// <param name="items">Одномерный, начинающийся с нуля массив,
		/// на основе которого будет создана коллекция только для чтения.</param>
		/// <param name="count">Количество элементов в массиве.</param>
		public ReadOnlyArray (T[] items, int count)
		{
			if (items == null)
			{
				throw new ArgumentNullException (nameof (items));
			}
			var isNormalArray = (items.Rank == 1) && (items.GetLowerBound (0) == 0);
			if (!isNormalArray)
			{
				throw new ArgumentOutOfRangeException (nameof (items));
			}
			if ((count < 0) || (count > items.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}
			Contract.EndContractBlock ();

			_items = items;
			_count = count;
		}

		/// <summary>
		/// Получает элемент коллекции в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в коллекции.</param>
		public T this[int index]
		{
			get
			{
				if ((index < 0) || (index >= this.Count))
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}
				Contract.EndContractBlock ();

				return _items[index];
			}
		}

		/// <summary>
		/// Получает количество элементов коллекции.
		/// </summary>
		public int Count => _count;

		/// <summary>
		/// Получает перечислитель элементов коллекции.
		/// </summary>
		/// <returns>Перечислитель элементов коллекции.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return new _SimpleArrayEnumerator (this);
		}

		/// <summary>
		/// Получает перечислитель элементов коллекции.
		/// </summary>
		/// <returns>Перечислитель элементов коллекции.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Копирует элементы коллекции в указанный массив,
		/// начиная с указанного индекса конечного массива.
		/// </summary>
		/// <param name="array">Массив, в который копируются элементы списка.</param>
		/// <param name="arrayIndex">Позиция в конечном массиве, указывающая начало копирования.</param>
		public void CopyTo (T[] array, int arrayIndex)
		{
			Array.Copy (_items, 0, array, arrayIndex, _count);
		}

		/// <summary>
		/// Получает хэш-код коллекции.
		/// </summary>
		/// <returns>Хэш-код коллекции.</returns>
		public override int GetHashCode ()
		{
			return _items.GetHashCode () ^ _count.GetHashCode ();
		}

		/// <summary>
		/// Определяет, равны ли два экземпляра коллекции.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущей коллекцией.</param>
		/// <returns>true, если указанный объект равен текущей коллекции; в противном случае — false.</returns>
		public override bool Equals (object obj)
		{
			var asSameClass = obj as ReadOnlyArray<T>;
			return ((asSameClass != null) && Equals (asSameClass));
		}

		/// <summary>
		/// Определяет, равны ли два экземпляра коллекции.
		/// </summary>
		/// <param name="other">Коллекция, которую требуется сравнить с текущей коллекцией.</param>
		/// <returns>true, если указанная коллекция равна текущей коллекции; в противном случае — false.</returns>
		public bool Equals (ReadOnlyArray<T> other)
		{
			return !ReferenceEquals (other, null) &&
			(other._items == _items) &&
			(other._count == _count);
		}

		/// <summary>
		/// Определяет равенство двух коллекций.
		/// </summary>
		/// <param name="first">Коллекция, которая находится слева от оператора равенства.</param>
		/// <param name="second">Коллекция, которая находится справа от оператора равенства.</param>
		/// <returns>true, если значения параметров first и second равны; в противном случае — false.</returns>
		public static bool operator ==(ReadOnlyArray<T> first, ReadOnlyArray<T> second)
		{
			return ReferenceEquals (first, null) ?
				ReferenceEquals (second, null) :
				first.Equals (second);
		}

		/// <summary>
		/// Определяет неравенство двух коллекций.
		/// </summary>
		/// <param name="first">Коллекция, который находится слева от оператора равенства.</param>
		/// <param name="second">Коллекция, который находится справа от оператора равенства.</param>
		/// <returns>true, если значения параметров first и second не равны; в противном случае — false.</returns>
		public static bool operator !=(ReadOnlyArray<T> first, ReadOnlyArray<T> second)
		{
			return !(ReferenceEquals (first, null) ?
				ReferenceEquals (second, null) :
				first.Equals (second));
		}

		/// <summary>
		/// Получает структурный хэш-код коллекции.
		/// </summary>
		/// <param name="comparer">Объект, вычисляющий хэш-код элементов коллекции.</param>
		/// <returns>Структурный хэш-код коллекции.</returns>
		int IStructuralEquatable.GetHashCode (IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}
			Contract.EndContractBlock ();

			int num = 0;
			for (var i = (_count >= 8) ? (_count - 8) : 0; i < _count; i++)
			{
				num = (((num << 5) + num) ^ comparer.GetHashCode (_items[i]));
			}
			return num;
		}

		/// <summary>
		/// Определяет, равны ли структурно два экземпляра коллекции.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущей коллекцией.</param>
		/// <param name="comparer">Объект, определяющий равенство элементов коллекции.</param>
		/// <returns>true, если указанная коллекция структурно равен текущей коллекции; в противном случае — false.</returns>
		bool IStructuralEquatable.Equals (object other, IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}
			Contract.EndContractBlock ();

			var otherArr = other as ReadOnlyArray<T>;
			if ((otherArr == null) || (otherArr._count != _count))
			{
				return false;
			}
			if (otherArr._items != _items)
			{
				for (var i = 0; i < _count; i++)
				{
					var isItemEqualsOtherItem = comparer.Equals (_items[i], otherArr._items[i]);
					if (!isItemEqualsOtherItem)
					{
						return false;
					}
				}
			}
			return true;
		}

		#region struct _SimpleArrayEnumerator

		internal struct _SimpleArrayEnumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private readonly ReadOnlyArray<T> _data;
			private int _index;
			private T _currentElement;

			internal _SimpleArrayEnumerator (ReadOnlyArray<T> data)
			{
				_data = data;
				_index = -1;
				_currentElement = default (T);
			}

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
				if (_index >= _data._count)
				{
					_index = -2;
					_currentElement = default (T);
					return false;
				}
				_currentElement = _data._items[_index];
				return true;
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

		#endregion

		[DebuggerBrowsable (DebuggerBrowsableState.Never),
		SuppressMessage ("Microsoft.Globalization",
			"CA1305:SpecifyIFormatProvider",
			MessageId = "System.String.Format(System.String,System.Object,System.Object)",
			Justification = "String is not exposed to the end user and will not be localized."),
		SuppressMessage ("Microsoft.Globalization",
			"CA1305:SpecifyIFormatProvider",
			MessageId = "System.String.Format(System.String,System.Object)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		private string DebuggerDisplay
		{
			get
			{
				return $"<{typeof (T).Name}> ({_count})";
			}
		}
	}
}
