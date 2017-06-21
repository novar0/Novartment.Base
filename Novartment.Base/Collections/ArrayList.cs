using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using SystemArray = System.Array;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Динамический список на основе зацикленного массива с передвижными головой и хвостом.
	/// Поддерживает семантику стэка и очереди.
	/// Для конкурентного доступа требуется внешняя синхронизация.
	/// </summary>
	/// <typeparam name="T">Тип элементов списка.</typeparam>
	/// <remarks>
	/// Предназначен для замены List&lt;&gt;, Stack&lt;&gt; и Queue&lt;&gt; с целью минимизации накладных расходов
	/// при внутреннем ограниченном использовании.
	/// Может быть создан используя указанный массив, без копирования.
	/// Даёт прямой доступ к внутреннему массиву.
	/// Не проверяет конкурентные изменения.
	/// Не реализует интерфейсы со сложными проверками контрактов.
	/// Не производит уведомлений об изменениях.
	/// Не содержит методов с неявным потреблением ресурсов.
	/// </remarks>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class ArrayList<T> :
		IAdjustableList<T>,
		IReservedCapacityCollection<T>,
		IArrayDuplicableCollection<T>,
		IStructuralEquatable
	{
		[DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
		private T[] _items;
		private int _head = 0;
		private int _count = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса ArrayList.
		/// </summary>
		public ArrayList ()
		{
			_items = new T[4];
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ArrayList с указанием начальной вместимости.
		/// </summary>
		/// <param name="capacity">Начальная вместимость списка.</param>
		public ArrayList (int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (capacity));
			}

			Contract.EndContractBlock ();

			_items = new T[capacity];
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ArrayList использующий указанный массив.
		/// </summary>
		/// <param name="array">Массив, который будет использован напрямую, без копирования.</param>
		public ArrayList (T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}

			Contract.EndContractBlock ();

			_items = array;
			_head = 0;
			_count = array.Length;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ArrayList использующий предоставленный диапазон массива.
		/// </summary>
		/// <param name="array">
		/// Массив, который будет использован напрямую, без копирования.
		/// Все элементы массива кроме ограниченных диапазоном offset, count
		/// могут быть очищены при операциях, изменяющих список.
		/// </param>
		/// <param name="offset">
		/// Начальная позиция диапазона в исходном массиве.
		/// Зацикливается через край массива, то есть offset + count может быть больше чем размер массива.
		/// </param>
		/// <param name="count">Количество элементов в диапазоне исходного массива.</param>
		public ArrayList (T[] array, int offset, int count)
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
			_head = (count > 0) ? offset : 0;
			_count = count;
		}

		/// <summary>
		/// Получает внутренний массив, в котором хранятся элементы списка.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Performance",
			"CA1819:PropertiesShouldNotReturnArrays",
			Justification = "This is clearly a property and write access to array is intended.")]
		public T[] Array => _items;

		/// <summary>
		/// Получает позицию начала диапазона во внутреннем массиве, в котором хранятся элементы списка.
		/// </summary>
		public int Offset => _head;

		/// <summary>
		/// Получает количество элементов списка.
		/// </summary>
		public int Count => _count;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[SuppressMessage(
			"Microsoft.Globalization",
			"CA1305:SpecifyIFormatProvider",
			MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		[SuppressMessage(
			"Microsoft.Globalization",
			"CA1305:SpecifyIFormatProvider",
			MessageId = "System.String.Format(System.String,System.Object,System.Object)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		[SuppressMessage(
			"Microsoft.Globalization",
			"CA1305:SpecifyIFormatProvider",
			MessageId = "System.String.Format(System.String,System.Object)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		private string DebuggerDisplay
		{
			get
			{
				var info = (_count < 1) ?
					"empty" :
					(_count == 1) ?
						$"{_head}" :
						$"{_head}...{(_head + _count - 1) % _items.Length}";
				return $"<{typeof(T).Name}>[{info}] (capacity={_items.Length})";
			}
		}

		/// <summary>
		/// Получает или устанавливает элемент списка в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в списке.</param>
		public T this[int index]
		{
			get
			{
				if ((index < 0) || (index >= this.Count))
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				Contract.EndContractBlock();

				index += _head;
				if (index >= _items.Length)
				{
					index -= _items.Length;
				}

				return _items[index];
			}

			set
			{
				if ((index < 0) || (index >= this.Count))
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				Contract.EndContractBlock();

				index += _head;
				if (index >= _items.Length)
				{
					index -= _items.Length;
				}

				_items[index] = value;
			}
		}

		/// <summary>
		/// Добавляет элемент в список.
		/// </summary>
		/// <param name="item">Элемент для добавления в список.</param>
		public void Add (T item)
		{
			EnsureCapacity (_count + 1);
			var index = _head + _count;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			_items[index] = item;

			_count++;
		}

		/// <summary>
		/// Пытается получить первый элемент списка.
		/// </summary>
		/// <param name="item">Значение первого элемента списка, если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент списка успешно получен, false если нет.</returns>
		public bool TryPeekFirst (out T item)
		{
			if (_count < 1)
			{
				item = default (T);
				return false;
			}

			item = _items[_head];
			return true;
		}

		/// <summary>
		/// Пытается изъять первый элемент списка.
		/// </summary>
		/// <param name="item">Значение первого элемента списка если он успешно изъят,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент списка успешно изъят, false если нет.</returns>
		public bool TryTakeFirst (out T item)
		{
			if (_count < 1)
			{
				item = default (T);
				return false;
			}

			item = _items[_head];
			_items[_head] = default (T);
			_head++;
			if (_head >= _items.Length)
			{
				_head -= _items.Length;
			}

			_count--;
			if (_count < 1)
			{
				_head = 0;
			}

			return true;
		}

		/// <summary>
		/// Пытается получить последний элемент списка.
		/// </summary>
		/// <param name="item">Значение последнего элемента списка если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент списка успешно получен, false если нет.</returns>
		public bool TryPeekLast (out T item)
		{
			if (_count < 1)
			{
				item = default (T);
				return false;
			}

			var index = _head + _count - 1;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			item = _items[index];
			return true;
		}

		/// <summary>
		/// Пытается изъять последний элемент списка.
		/// </summary>
		/// <param name="item">Значение последнего элемента списка если он успешно изъят,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент списка успешно изъят, false если нет.</returns>
		public bool TryTakeLast (out T item)
		{
			if (_count < 1)
			{
				item = default (T);
				return false;
			}

			var index = _head + _count - 1;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			item = _items[index];
			_items[index] = default (T);
			_count--;
			if (_count < 1)
			{
				_head = 0;
			}

			return true;
		}

		/// <summary>
		/// Вставляет указанный элемент в список в указанную позицию, отодвигая последующие элементы.
		/// </summary>
		/// <param name="index">Позиция в списке.</param>
		/// <param name="item">Элемент для вставки в указанную позицию.</param>
		public void Insert (int index, T item)
		{
			if ((index < 0) || (index > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			EnsureCapacity (_count + 1);

			if (index <= (_count / 2))
			{ // место вставки - первая половина, выгоднее отодвигать назад кусок от начала до индекса
				if (_count > 0)
				{
					_head--;
					if (_head < 0)
					{
						_head += _items.Length;
					}
				}

				_count++;
				if (index > 0)
				{
					CopyInternal (1, 0, index);
				}
			}
			else
			{ // место вставки - вторая половина, выгоднее отодвигать вперёд кусок от индекса до конца
				if (index < _count)
				{
					CopyInternal (index, index + 1, _count - index);
				}

				_count++;
			}

			index += _head;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			_items[index] = item;
		}

		/// <summary>
		/// Вставляет пустой диапазон элементов в список в указанную позицию.
		/// Вставленные элементы будут иметь значение по-умолчанию.
		/// </summary>
		/// <param name="index">Позиция в коллекции, куда будут вставлены элементы.</param>
		/// <param name="count">Количество вставляемых элементов.</param>
		public void InsertRange (int index, int count)
		{
			if ((index < 0) || (index > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (count < 1)
			{
				return;
			}

			EnsureCapacity (_count + count);

			if (index == _count)
			{
				// вставка в конец, поэтому сдвигать и очищать нечего
				_count += count;
				return;
			}

			if (index <= (_count / 2))
			{ // место вставки - первая половина, выгоднее отодвигать назад кусок от начала до индекса
				if (_count > 0)
				{
					_head -= count;
					if (_head < 0)
					{
						_head += _items.Length;
					}
				}

				_count += count;
				CopyInternal (count, 0, index);
			}
			else
			{ // место вставки - вторая половина, выгоднее отодвигать вперёд кусок от индекса до конца
				CopyInternal (index, index + count, _count - index);
				_count += count;
			}

			// очищаем место под новые элементы
			ResetItems (index, count);
		}

		/// <summary>
		/// Удаляет элемент из списка в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в списке.</param>
		public void RemoveAt (int index)
		{
			if ((index < 0) || (index >= this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			var lastIndex = _count - 1;

			if (index <= (_count / 2))
			{ // место удаления - первая половина, выгоднее отодвигать вперёд кусок от начала до индекса
				if (index > 0)
				{
					CopyInternal (0, 1, index);
				}

				_items[_head] = default (T);
				_head++;
				if (_head >= _items.Length)
				{
					_head -= _items.Length;
				}
			}
			else
			{ // место удаления - вторая половина, выгоднее отодвигать назад кусок от индекса до конца
				if (index < lastIndex)
				{
					CopyInternal (index + 1, index, _count - index - 1);
				}

				var idx = _head + _count - 1;
				if (idx >= _items.Length)
				{
					idx -= _items.Length;
				}

				_items[idx] = default (T);
			}

			_count--;
			if (_count < 1)
			{
				_head = 0;
			}
		}

		/// <summary>
		/// Удаляет указанное число элементов из коллекции начиная с указанной позиции.
		/// </summary>
		/// <param name="index">Начальная позиция элементов для удаления.</param>
		/// <param name="count">Количество удаляемых элементов.</param>
		public void RemoveRange (int index, int count)
		{
			if ((index < 0) || (index > this.Count) || ((index == this.Count) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			if (_count == 0)
			{
				return;
			}

			if (_count == 1)
			{
				RemoveAt (index);
			}
			else
			{
				var size1 = index;
				var size2 = _count - index - count;

				// копия удаляемых элементов для последующего уведомления
				if (size1 <= size2)
				{ // выгоднее отодвигать вперёд кусок от начала до индекса
					CopyInternal (0, count, size1);
					ResetItems (0, count);
					_head += count;
					if (_head >= _items.Length)
					{
						_head -= _items.Length;
					}
				}
				else
				{ // выгоднее отодвигать назад кусок от индекса до конца
					CopyInternal (index + count, index, size2);
					ResetItems (index + size2, count);
				}

				_count = size1 + size2;
				if (_count < 1)
				{
					_head = 0;
				}
			}
		}

		/// <summary>Очищает список.</summary>
		public void Clear ()
		{
			ResetItems ();
			_head = 0;
			_count = 0;
		}

		/// <summary>
		/// Сбрасывает все элементы списка в значение по умолчанию.
		/// </summary>
		public void ResetItems ()
		{
			if (_count > 0)
			{
				var tail = _head + _count;
				if (tail >= _items.Length)
				{
					tail -= _items.Length;
				}

				if (_head < tail)
				{
					SystemArray.Clear (_items, _head, _count);
				}
				else
				{
					SystemArray.Clear (_items, _head, _items.Length - _head);
					SystemArray.Clear (_items, 0, tail);
				}
			}
		}

		/// <summary>
		/// Сбрасывает указанное число элементов списка в значение по умолчанию.
		/// </summary>
		/// <param name="index">Начальная позиция элементов для очистки.</param>
		/// <param name="count">Количество очищаемых элементов.</param>
		public void ResetItems (int index, int count)
		{
			if ((index < 0) || (index > this.Count) || ((index == this.Count) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			index += _head;
			if (index >= _items.Length)
			{
				index -= _items.Length;
			}

			var tail = _head + _count;
			if (tail > _items.Length)
			{
				tail -= _items.Length;
			}

			var firstPieceSize = _items.Length - index;
			if ((tail > index) || (count <= firstPieceSize))
			{
				// ...head**index*********tail...  или  ***index*********tail...head***
				SystemArray.Clear (_items, index, count);
			}
			else
			{
				// ***tail...head*********index***
				SystemArray.Clear (_items, index, _items.Length - index);
				SystemArray.Clear (_items, 0, count - (_items.Length - index));
			}
		}

		/// <summary>
		/// Копирует элементы списка в указанный массив,
		/// начиная с указанного индекса конечного массива.
		/// </summary>
		/// <param name="array">Массив, в который копируются элементы списка.</param>
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
				var tail = _head + _count;
				if (tail > _items.Length)
				{
					tail -= _items.Length;
				}

				if (_head < tail)
				{
					SystemArray.Copy (_items, _head, array, arrayIndex, _count);
				}
				else
				{
					SystemArray.Copy (_items, _head, array, arrayIndex, _items.Length - _head);
					SystemArray.Copy (_items, 0, array, arrayIndex + _items.Length - _head, tail);
				}
			}
		}

		/// <summary>
		/// Располагает элементы внутреннего массива так, чтобы они шли сплошным куском с начальной позиции.
		/// После возврата будет Offset=0.
		/// </summary>
		public void Defragment ()
		{
			if ((_count > 0) && (_head > 0))
			{
				var tail = _head + _count;
				if (tail > _items.Length)
				{
					tail -= _items.Length;
				}

				if (_head < tail)
				{
					SystemArray.Copy (_items, _head, _items, 0, _count);
				}
				else
				{
					var newItems = new T[_count];
					SystemArray.Copy (_items, _head, newItems, 0, _items.Length - _head);
					SystemArray.Copy (_items, 0, newItems, _items.Length - _head, tail);
					_items = newItems;
				}
			}

			_head = 0;
		}

		/// <summary>
		/// Сортирует элементы списка с использованием указанного компаратора.
		/// </summary>
		/// <param name="comparer">
		/// Компаратор, реализующий интерефейс IComparer&lt;T&gt;, который будет использоваться для сравнения элементов,
		/// или null чтобы использовать компаратор по-умолчанию для типа T.
		/// </param>
		/// <remarks>
		/// Если массив зациклен через край, то перед сортировкой будет произведено копирование меньшей части.
		/// </remarks>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public void Sort (IComparer<T> comparer = null)
		{
			if (_count < 1)
			{
				return;
			}

			// проверяем наличие зацикливания через край массива: ***[size1]***tail...head***[size2]***
			var sizeTail = (_head + _count) - _items.Length;
			if (sizeTail > 0)
			{
				// Если массив зациклен через край, то в нём будет два разделённых сегмента, а для сортировки нужен один непрерывный.
				// Пододвигаем один сегмент к другому.
				// Порядок элементов нарушится, но это не важно, потому что дальше они будут отсортированы.
				var sizeHead = _count - sizeTail;
				if (sizeTail >= sizeHead)
				{ // выгоднее отодвигать назад кусок от головы до конца массива
					SystemArray.Copy (_items, _head, _items, sizeTail, sizeHead);
					SystemArray.Clear (_items, _count, _items.Length - _count);
					_head = 0;
				}
				else
				{ // выгоднее отодвигать вперёд кусок от начала массива до хвоста
					_head -= sizeTail;
					SystemArray.Copy (_items, 0, _items, _head, sizeTail);
					SystemArray.Clear (_items, 0, _head);
				}
			}

			SystemArray.Sort (_items, _head, _count, comparer);
		}

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		public IEnumerator<T> GetEnumerator ()
		{
			return new SimpleListEnumerator (this);
		}

		/// <summary>
		/// Резервирует в массиве, в котором хранятся элементы списка, указанное количество элементов.
		/// Позволяет избежать копирований массива при добавлении элементов в список.
		/// </summary>
		/// <param name="min">Минимальная необходимая вместимость включая уже находящиеся в коллекции элементы.</param>
		public void EnsureCapacity (int min)
		{
			if (min < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (min));
			}

			Contract.EndContractBlock ();

			if (_items.Length < min)
			{
				var newCapacity = (_items.Length < (int.MaxValue / 2)) ?
					_items.Length * 2 : // увеличиваем минимум на 100%
					int.MaxValue;
				if (newCapacity < (_items.Length + 4))
				{
					newCapacity = _items.Length + 4; // увеличиваем минимум на 4
				}

				if (newCapacity < min)
				{
					newCapacity = min;
				}

				var newItems = new T[newCapacity];
				CopyTo (newItems, 0);
				_items = newItems;
				_head = 0;
			}
		}

		/// <summary>
		/// Избавляет массив, в котором хранятся элементы списка, от зарезервированных элементов.
		/// Позволяет освободить память, занятую зарезервированными элементами.
		/// </summary>
		public void TrimExcess ()
		{
			// обрезаем только если наполняемость менее 90%
			var num = (int)(_items.Length * 0.9);
			if (_count < num)
			{
				var newItems = new T[_count];
				CopyTo (newItems, 0);
				_items = newItems;
				_head = 0;
			}
		}

		/// <summary>
		/// Получает структурный хэш-код списка.
		/// </summary>
		/// <param name="comparer">Объект, вычисляющий хэш-код элементов списка.</param>
		/// <returns>Структурный хэш-код списка.</returns>
		int IStructuralEquatable.GetHashCode (IEqualityComparer comparer)
		{
			if (comparer == null)
			{
				throw new ArgumentNullException (nameof (comparer));
			}

			Contract.EndContractBlock ();

			int hash = 0;

			// макс. 8 элементов с конца для вычисления хэша всего списка
			var start = (_count >= 8) ? (_count - 8) : 0;
			for (var i = start; i < _count; i++)
			{
				hash = ((hash << 5) + hash) ^ comparer.GetHashCode (this[i]);
			}

			return hash;
		}

		/// <summary>
		/// Определяет, равны ли структурно два экземпляра списка.
		/// </summary>
		/// <param name="other">Список, который требуется сравнить с текущим списком.</param>
		/// <param name="comparer">Объект, определяющий, равенство элементов списка.</param>
		/// <returns>true, если указанный список структурно равен текущему списку; в противном случае — false.</returns>
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
				var list = other as ArrayList<T>;
				if (list == null)
				{
					return false;
				}

				if (list._count != _count)
				{
					return false;
				}

				for (var i = 0; i < _count; i++)
				{
					object x = this[i];
					object y = list[i];
					var isXequalsY = comparer.Equals (x, y);
					if (!isXequalsY)
					{
						return false;
					}
				}
			}

			return true;
		}

		// Копирует без проверок диапазон элементов из одной позиции внутреннего массива в другую.
		private void CopyInternal (int sourceIndex, int destinationIndex, int length)
		{
			var srcIndex = sourceIndex + _head;
			if (srcIndex >= _items.Length)
			{
				srcIndex -= _items.Length;
			}

			var dstIndex = destinationIndex + _head;
			if (dstIndex >= _items.Length)
			{
				dstIndex -= _items.Length;
			}

			var tail = _head + _count;
			if (tail >= _items.Length)
			{
				tail -= _items.Length;
			}

			var srcFirstPieceSize = _items.Length - srcIndex;
			var dstFirstPieceSize = _items.Length - dstIndex;

			var isSrcСontiguous = (tail > srcIndex) || (length <= srcFirstPieceSize);
			var isDstСontiguous = (tail > dstIndex) || (length <= dstFirstPieceSize);

			if (isSrcСontiguous)
			{
				if (isDstСontiguous)
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					SystemArray.Copy (_items, srcIndex, _items, dstIndex, length);
				}
				else
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ***tail...head*********dstIndex***
					SystemArray.Copy (_items, srcIndex, _items, dstIndex, dstFirstPieceSize);
					SystemArray.Copy (_items, srcIndex + dstFirstPieceSize, _items, 0, length - dstFirstPieceSize);
				}
			}
			else
			{
				if (isDstСontiguous)
				{
					// ***tail...head*********srcIndex***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					SystemArray.Copy (_items, srcIndex, _items, dstIndex, srcFirstPieceSize);
					SystemArray.Copy (_items, 0, _items, dstIndex + srcFirstPieceSize, length - srcFirstPieceSize);
				}
				else
				{
					if (srcFirstPieceSize > dstFirstPieceSize)
					{
						// ***tail...head*******srcIndex*****
						// ***tail...head*********dstIndex***
						SystemArray.Copy (_items, srcIndex, _items, dstIndex, dstFirstPieceSize);
						SystemArray.Copy (_items, srcIndex + dstFirstPieceSize, _items, 0, srcFirstPieceSize - dstFirstPieceSize);
						SystemArray.Copy (_items, 0, _items, srcFirstPieceSize - dstFirstPieceSize, length - srcFirstPieceSize);
					}
					else
					{
						// ***tail...head*********srcIndex***
						// ***tail...head*******dstIndex*****
						SystemArray.Copy (_items, srcIndex, _items, dstIndex, srcFirstPieceSize);
						SystemArray.Copy (_items, 0, _items, dstIndex + srcFirstPieceSize, dstFirstPieceSize - srcFirstPieceSize);
						SystemArray.Copy (_items, dstFirstPieceSize - srcFirstPieceSize, _items, 0, length - dstFirstPieceSize);
					}
				}
			}
		}

		internal struct SimpleListEnumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private readonly ArrayList<T> _data;
			private int _index;
			private T _currentElement;

			internal SimpleListEnumerator (ArrayList<T> data)
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
						throw new InvalidOperationException("Can not get current element of enumeration because it not started.");
					}

					if (_index == -2)
					{
						throw new InvalidOperationException("Can not get current element of enumeration because it already ended.");
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
				if (_index >= _data._count)
				{
					_index = -2;
					_currentElement = default (T);
					return false;
				}

				var index = _index + _data._head;
				if (index >= _data._items.Length)
				{
					index -= _data._items.Length;
				}

				_currentElement = _data._items[index];
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
