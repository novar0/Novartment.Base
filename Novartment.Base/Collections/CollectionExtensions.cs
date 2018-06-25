using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections.Immutable;
using static System.Linq.Enumerable;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Методы расширения к интерфейсам коллекций.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Очищает диапазон элементов зацикленного сегмента массива.
		/// </summary>
		/// <typeparam name="T">Тип элементов массива.</typeparam>
		/// <param name="segmentItems">Массив элементов сегмента.</param>
		/// <param name="segmentOffset">Начальная позиция элементов сегмента.</param>
		/// <param name="segmentCount">Количество элементов сегмента.</param>
		/// <param name="index">Начальная позиция диапазона для очистки (отсчёт от начала сегмента).</param>
		/// <param name="count">Количество очищаемых элементов.</param>
		public static void LoopedArraySegmentClear<T> (
			T[] segmentItems,
			int segmentOffset,
			int segmentCount,
			int index,
			int count)
		{
			if (segmentItems == null)
			{
				throw new ArgumentNullException (nameof (segmentItems));
			}

			if ((segmentOffset < 0) || (segmentOffset > segmentItems.Length) || ((segmentOffset == segmentItems.Length) && (segmentCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentOffset));
			}

			if ((segmentCount < 0) || (segmentCount > segmentItems.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentCount));
			}

			if ((index < 0) || (index > segmentCount) || ((index == segmentCount) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > segmentCount))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			index += segmentOffset;
			if (index >= segmentItems.Length)
			{
				index -= segmentItems.Length;
			}

			var tail = segmentOffset + segmentCount;
			if (tail >= segmentItems.Length)
			{
				tail -= segmentItems.Length;
			}

			var firstPieceSize = segmentItems.Length - index;
			if ((tail > index) || (count <= firstPieceSize))
			{
				// ...head**index*********tail...  или  ***index*********tail...head***
				Array.Clear (segmentItems, index, count);
			}
			else
			{
				// ***tail...head*********index***
				Array.Clear (segmentItems, index, segmentItems.Length - index);
				Array.Clear (segmentItems, 0, count - (segmentItems.Length - index));
			}
		}

		/// <summary>
		/// Копирует диапазон элементов зацикленного сегмента массива из одной позиции в другую.
		/// Позиции для копирования указываются от начала сегмента.
		/// </summary>
		/// <typeparam name="T">Тип элементов массива.</typeparam>
		/// <param name="segmentItems">Массив элементов сегмента.</param>
		/// <param name="segmentOffset">Начальная позиция элементов сегмента.</param>
		/// <param name="segmentCount">Количество элементов сегмента.</param>
		/// <param name="sourceIndex">Позиция исходного диапазона в сегменте (отсчёт от начала сегмента).</param>
		/// <param name="destinationIndex">Позиция диапазона назначения в сегменте (отсчёт от начала сегмента).</param>
		/// <param name="count">Число копируемых элементов.</param>
		public static void LoopedArraySegmentCopy<T> (
			T[] segmentItems,
			int segmentOffset,
			int segmentCount,
			int sourceIndex,
			int destinationIndex,
			int count)
		{
			if (segmentItems == null)
			{
				throw new ArgumentNullException (nameof (segmentItems));
			}

			if ((segmentOffset < 0) || (segmentOffset > segmentItems.Length) || ((segmentOffset == segmentItems.Length) && (segmentCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentOffset));
			}

			if ((segmentCount < 0) || (segmentCount > segmentItems.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (segmentCount));
			}

			if ((sourceIndex < 0) || (sourceIndex > segmentCount) || ((sourceIndex == segmentCount) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (sourceIndex));
			}

			if ((destinationIndex < 0) || (destinationIndex > segmentCount) || ((destinationIndex == segmentCount) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (destinationIndex));
			}

			if ((count < 0) || ((sourceIndex + count) > segmentCount) || ((destinationIndex + count) > segmentCount))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			var srcIndex = sourceIndex + segmentOffset;
			if (srcIndex >= segmentItems.Length)
			{
				srcIndex -= segmentItems.Length;
			}

			var dstIndex = destinationIndex + segmentOffset;
			if (dstIndex >= segmentItems.Length)
			{
				dstIndex -= segmentItems.Length;
			}

			var tail = segmentOffset + segmentCount;
			if (tail >= segmentItems.Length)
			{
				tail -= segmentItems.Length;
			}

			var srcFirstPieceSize = segmentItems.Length - srcIndex;
			var dstFirstPieceSize = segmentItems.Length - dstIndex;

			var isSrcСontiguous = (tail > srcIndex) || (count <= srcFirstPieceSize);
			var isDstСontiguous = (tail > dstIndex) || (count <= dstFirstPieceSize);

			if (isSrcСontiguous)
			{
				if (isDstСontiguous)
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, count);
				}
				else
				{
					// ...head***srcIndex*********tail...  или  ***srcIndex*********tail...head***
					// ***tail...head*********dstIndex***
					Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, dstFirstPieceSize);
					Array.Copy (segmentItems, srcIndex + dstFirstPieceSize, segmentItems, 0, count - dstFirstPieceSize);
				}
			}
			else
			{
				if (isDstСontiguous)
				{
					// ***tail...head*********srcIndex***
					// ...head***dstIndex*********tail...  или  ***dstIndex*********tail...head***
					Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, srcFirstPieceSize);
					Array.Copy (segmentItems, 0, segmentItems, dstIndex + srcFirstPieceSize, count - srcFirstPieceSize);
				}
				else
				{
					if (srcFirstPieceSize > dstFirstPieceSize)
					{
						// ***tail...head*******srcIndex*****
						// ***tail...head*********dstIndex***
						Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, dstFirstPieceSize);
						Array.Copy (segmentItems, srcIndex + dstFirstPieceSize, segmentItems, 0, srcFirstPieceSize - dstFirstPieceSize);
						Array.Copy (segmentItems, 0, segmentItems, srcFirstPieceSize - dstFirstPieceSize, count - srcFirstPieceSize);
					}
					else
					{
						// ***tail...head*********srcIndex***
						// ***tail...head*******dstIndex*****
						Array.Copy (segmentItems, srcIndex, segmentItems, dstIndex, srcFirstPieceSize);
						Array.Copy (segmentItems, 0, segmentItems, dstIndex + srcFirstPieceSize, dstFirstPieceSize - srcFirstPieceSize);
						Array.Copy (segmentItems, dstFirstPieceSize - srcFirstPieceSize, segmentItems, 0, count - dstFirstPieceSize);
					}
				}
			}
		}

		/// <summary>
		/// Создаёт массив, в который оптимально доступным образом скопированы все элементы указанного сегмента массива.
		/// </summary>
		/// <typeparam name="T">Тип элементов сегмента массива.</typeparam>
		/// <param name="arraySegment">Сегмент массива, который необходимо скопировать в массив.</param>
		/// <returns>Массив, содержащий копию всех элементов исходного сегмента массива.</returns>
		public static T[] DuplicateToArray<T> (this ArraySegment<T> arraySegment)
		{
			if (arraySegment == null)
			{
				throw new ArgumentNullException (nameof (arraySegment));
			}

			if (arraySegment.Array == null)
			{
				throw new ArgumentOutOfRangeException (nameof (arraySegment));
			}

			if ((arraySegment.Count < 0) || (arraySegment.Offset < 0) || (arraySegment.Offset >= arraySegment.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (arraySegment));
			}

			Contract.EndContractBlock ();

			var arrayCopy = new T[arraySegment.Count];
			Array.Copy (arraySegment.Array, arraySegment.Offset, arrayCopy, 0, arraySegment.Count);
			return arrayCopy;
		}

		/// <summary>
		/// Добавляет к коллекции указанную последовательность по возможности заранее резервируя место под новые элементы.
		/// </summary>
		/// <typeparam name="T">Тип элементов коллекции.</typeparam>
		/// <param name="collection">Коллекция, в которую будут добавлены элементы.</param>
		/// <param name="items">Последовательность элементов, которые будут добавлены в коллекцию.</param>
		public static void AddRange<T> (this IAdjustableCollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			if (items == null)
			{
				throw new ArgumentNullException (nameof (items));
			}

			Contract.EndContractBlock ();

			if (collection is IReservedCapacityCollection<T> reservableCapacityCollection)
			{
				var isCounted = TryGetCount (items, out int count);
				if (isCounted)
				{
					reservableCapacityCollection.EnsureCapacity (collection.Count + count);
				}
			}

			foreach (var item in items)
			{
				collection.Add (item);
			}
		}

		/// <summary>
		/// Вставляет в список в указанную позицию элементы указанной последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элементов списка.</typeparam>
		/// <param name="list">Список, в который будут вставлены элементы.</param>
		/// <param name="index">Позиция в списке, куда будут вставлены элементы.</param>
		/// <param name="items">Последовательность вставляемых элементов.</param>
		public static void InsertRange<T> (this IAdjustableList<T> list, int index, IEnumerable<T> items)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (items == null)
			{
				throw new ArgumentNullException (nameof (items));
			}

			if ((index < 0) || (index > list.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			Contract.EndContractBlock ();

			var isCounted = items.TryGetCount (out int count);
			if (isCounted)
			{ // количество вставляемых элементов известно, можно заранее зарезервировать место
				if (count > 0)
				{
					list.InsertRange (index, count);
					foreach (var item in items)
					{
						list[index++] = item;
					}
				}
			}
			else
			{ // если количество вставляемых элементов неизвестно, то никакая оптимизации невозможна
				foreach (var item in items)
				{
					list.Insert (index++, item);
				}
			}
		}

		/// <summary>
		/// Разделяет последовательность на под-последовательности в местах,
		/// отобранных по результатам применения указанной функции к элементам исходной последовательности.
		/// Отобранные функцией элементы не входят в результирующие под-последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="collection">Исходная последовательность, которая будут разделена на под-последовательности.</param>
		/// <param name="splitHereFunc">Функция, которая будет вызвана для каждого элемента исходной последовательности.
		/// Возвращает True для тех элементов, которые разделяют отдельные под-последовательности.</param>
		/// <returns>Последовательность под-последовательностей.</returns>
		public static IEnumerable<IReadOnlyList<T>> Split<T> (this IEnumerable<T> collection, Func<T, bool> splitHereFunc)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			if (splitHereFunc == null)
			{
				throw new ArgumentNullException (nameof (splitHereFunc));
			}

			Contract.EndContractBlock ();

			var list = new ArrayList<T> ();
			foreach (var item in collection)
			{
				var isSplitItem = splitHereFunc.Invoke (item);
				if (isSplitItem)
				{
					if (list.Count > 0)
					{
						yield return list;
					}

					list = new ArrayList<T> ();
				}
				else
				{
					list.Add (item);
				}
			}

			if (list.Count > 0)
			{
				yield return list;
			}
		}

		/// <summary>Удаляет элементы в списке, прошедшие фильтрацию указанным предикатом.</summary>
		/// <typeparam name="T">Тип элементов списка.</typeparam>
		/// <param name="list">Список, элементы которого будут протестированы и возможно удалены.</param>
		/// <param name="predicate">Функция тестирования элементов списка.
		/// Должна возвращать True для тех элементов, которые подлежат удалению.</param>
		/// <returns>Количество удалённых элементов.</returns>
		/// <remarks>Создаёт временный список позиций для удаления.</remarks>
		public static int RemoveWhere<T> (this IAdjustableList<T> list, Func<T, bool> predicate)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException (nameof (predicate));
			}

			Contract.EndContractBlock ();

			if (list.Count < 1)
			{
				return 0;
			}

			var indexesToRemove = new ArrayList<int> ();
			for (var i = 0; i < list.Count; i++)
			{
				var isRemoveItem = predicate.Invoke (list[i]);
				if (isRemoveItem)
				{
					indexesToRemove.Add (i);
				}
			}

			list.RemoveAtMany (indexesToRemove);
			return indexesToRemove.Count;
		}

		/// <summary>Удаляет в указанном списке все элементы, содержащиеся в указанном множестве.</summary>
		/// <typeparam name="T">Тип элементов списка.</typeparam>
		/// <param name="list">Список, элементы которого будут проверены и возможно удалены.</param>
		/// <param name="collectionToRemove">Множество элементов для удаления из списка.</param>
		/// <returns>Количество удалённых элементов.</returns>
		/// <remarks>Создаёт временный список позиций для удаления.</remarks>
		public static int RemoveItems<T> (this IAdjustableList<T> list, IReadOnlyFiniteSet<T> collectionToRemove)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (collectionToRemove == null)
			{
				throw new ArgumentNullException (nameof (collectionToRemove));
			}

			Contract.EndContractBlock ();

			if (list.Count < 1)
			{
				return 0;
			}

			var indexesToRemove = new ArrayList<int> ();
			{
				for (var index = 0; index < list.Count; index++)
				{
					var isRemoveItem = collectionToRemove.Contains (list[index]);
					if (isRemoveItem)
					{
						indexesToRemove.Add (index);
					}
				}
			}

			list.RemoveAtMany (indexesToRemove);
			return indexesToRemove.Count;
		}

		/// <summary>
		/// Удаляет элементы из списка согласно указанному отсортированному списку позиций для удаления.
		/// </summary>
		/// <typeparam name="T">Тип элементов списка.</typeparam>
		/// <param name="list">Список, элементы которого будут удалены согласно указанному списку позиций.</param>
		/// <param name="indexes">
		/// Список позиций в которых должны быть удалены элементы.
		/// Должен быть отсортирован от меньшего к большему.
		/// </param>
		public static void RemoveAtMany<T> (this IAdjustableList<T> list, IReadOnlyList<int> indexes)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}

			if (indexes == null)
			{
				throw new ArgumentNullException (nameof (indexes));
			}

			Contract.EndContractBlock ();

			var count = indexes.Count;
			if ((count < 1) || (list.Count < 1))
			{
				return;
			}

			int rangeStart = 0;
			var rangeEnd = indexes[count - 1];
			var lastIndex = rangeEnd + 1;
			int rangeSize;

			// собираем индексы в непрерывные диапазоны чтобы минимизировать операции удаления
			for (var i = count - 1; i >= 0; i--)
			{
				var index = indexes[i];
				if (index > lastIndex)
				{
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Array of indexes to delete is not sorted in position {i}."));
				}

				if (index == (lastIndex - 1))
				{
					rangeStart = index;
				}
				else
				{
					rangeSize = rangeEnd - rangeStart + 1;
					list.RemoveRange (rangeStart, rangeSize);
					rangeStart = index;
					rangeEnd = index;
				}

				lastIndex = index;
			}

			rangeSize = rangeEnd - lastIndex + 1;
			list.RemoveRange (lastIndex, rangeSize);
		}

		/// <summary>
		/// Создаёт конечное множество уникальных элементов, содержащихся в указанной последовательности,
		/// используя указанный компаратор.
		/// </summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="source">Исходная последовательность, из элементов которого будет создано множество.</param>
		/// <param name="comparer">Компаратор, используемый для сравнения элементов последовательности.</param>
		/// <returns>
		/// Конечное множество уникальных элементов, содержащихся в указанной последовательности,
		/// выбранных с использованием указанный компаратора.
		/// </returns>
		public static IAdjustableFiniteSet<T> ToSet<T> (this IEnumerable<T> source, IComparer<T> comparer = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var set = new AvlTreeSet<T> (comparer);
			foreach (var item in source)
			{
				set.Add (item);
			}

			return set;
		}

		/// <summary>
		/// Создаёт конечное множество уникальных элементов, содержащихся в указанной последовательности,
		/// созданное на основе хэш-функции указанного компаратора.
		/// </summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="source">Исходная последовательность, из элементов которого будет создано множество.</param>
		/// <param name="comparer">Компаратор, используемый для сравнения элементов последовательности.</param>
		/// <returns>
		/// Конечное множество уникальных элементов, содержащихся в указанной последовательности,
		/// выбранных с использованием указанный компаратора.
		/// </returns>
		public static IAdjustableFiniteSet<T> ToHashSet<T> (this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var set = new AvlHashTreeSet<T> (comparer);
			foreach (var item in source)
			{
				set.Add (item);
			}

			return set;
		}

		/// <summary>Повторяет последовательность, пропуская null-элементы.</summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="source">Последовательность, элементы которой будут возвращены если отличны от null.</param>
		/// <returns>Последовательность в которой пропущены null-элементы.</returns>
		public static IEnumerable<T> WhereNotNull<T> (this IEnumerable<T> source)
			where T : class
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return source.Where (item => !(item is null));
		}

		/// <summary>
		/// Пытается получить первый элемент указанной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов коллекции.</typeparam>
		/// <param name="source">Коллекция, из которой будет получен первый элемент.</param>
		/// <param name="item">Будет содержать первый элемент коллекции если он успешно получен,
		/// либо значение по умолчанию если коллекция была пуста.</param>
		/// <returns>True если первый элемент коллекции успешно получен, либо False если коллекция пустая.</returns>
		public static bool TryGetFirst<T> (this IEnumerable<T> source, out T item)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is IFifoCollection<T> fifoCollection)
			{
				var isPeeked = fifoCollection.TryPeekFirst (out item);
				if (isPeeked)
				{
					return true;
				}
			}
			else
			{
				if (source is IReadOnlyList<T> readOnlyList)
				{
					if (readOnlyList.Count > 0)
					{
						item = readOnlyList[0];
						return true;
					}
				}
				else
				{
					if (source is IList<T> list)
					{
						if (list.Count > 0)
						{
							item = list[0];
							return true;
						}
					}
					else
					{
						using (var enumerator = source.GetEnumerator ())
						{
							var isMovedToNext = enumerator.MoveNext ();
							if (isMovedToNext)
							{
								item = enumerator.Current;
								return true;
							}
						}
					}
				}
			}

			item = default;
			return false;
		}

		/// <summary>
		/// Пытается получить последний элемент указанной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов коллекции.</typeparam>
		/// <param name="source">Коллекция, из которой будет получен последний элемент.</param>
		/// <param name="item">Будет содержать последний элемент коллекции если он успешно получен,
		/// либо значение по умолчанию если коллекция была пуста.</param>
		/// <returns>True если последний элемент коллекции успешно получен, либо False если коллекция пустая.</returns>
		public static bool TryGetLast<T> (this IEnumerable<T> source, out T item)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is ILifoCollection<T> lifoCollection)
			{
				var isPeeked = lifoCollection.TryPeekLast (out item);
				if (isPeeked)
				{
					return true;
				}
			}
			else
			{
				if (source is IReadOnlyList<T> readOnlyList)
				{
					var count = readOnlyList.Count;
					if (count > 0)
					{
						item = readOnlyList[count - 1];
						return true;
					}
				}
				else
				{
					if (source is IList<T> list)
					{
						var count = list.Count;
						if (count > 0)
						{
							item = list[count - 1];
							return true;
						}
					}
					else
					{
						using (var enumerator = source.GetEnumerator ())
						{
							var isMovedToNext = enumerator.MoveNext ();
							if (isMovedToNext)
							{
								do
								{
									item = enumerator.Current;
								}
								while (enumerator.MoveNext ());
								return true;
							}
						}
					}
				}
			}

			item = default;
			return false;
		}

		/// <summary>
		/// Пытается получить количество элементов последовательности без её перебора.
		/// </summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="source">Последовательность, для которой надо получить количество элементов.</param>
		/// <param name="count">
		/// После завершения работы метода будет содержать количество элементов в коллекции в случае успеха,
		/// иначе будет содержать ноль.
		/// </param>
		/// <returns>True если количество элементов успешно получено, иначе False.</returns>
		public static bool TryGetCount<T> (this IEnumerable<T> source, out int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is IReadOnlyCollection<T> countable1)
			{
				count = countable1.Count;
				return true;
			}

			if (source is ICollection<T> countable2)
			{
				count = countable2.Count;
				return true;
			}

			if (source is ICollection countable3)
			{
				count = countable3.Count;
				return true;
			}

			count = 0;
			return false;
		}

		/// <summary>
		/// Создаёт массив, в который оптимально доступным образом скопированы все элементы указанной последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="source">Последовательность, которую необходимо скопировать в массив.</param>
		/// <returns>Массив, содержащий копию всех элементов исходной последовательности.</returns>
		public static T[] DuplicateToArray<T> (this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			T[] array;
			int length;

			if (source is IArrayDuplicableCollection<T> collection1)
			{
				length = collection1.Count;
				array = new T[length];
				if (length > 0)
				{
					collection1.CopyTo (array, 0);
				}

				return array;
			}

			if (source is ICollection<T> collection2)
			{
				length = collection2.Count;
				array = new T[length];
				if (length > 0)
				{
					collection2.CopyTo (array, 0);
				}

				return array;
			}

			if (source is ICollection collection3)
			{
				length = collection3.Count;
				array = new T[length];
				if (length > 0)
				{
					collection3.CopyTo (array, 0);
				}

				return array;
			}

			if (source is IReadOnlyCollection<T> collection4)
			{
				length = collection4.Count;
				array = new T[length];
				if (length > 0)
				{
					int idx = 0;
					foreach (var item in collection4)
					{
						array[idx++] = item;
					}
				}

				return array;
			}

			length = 0;
			array = new T[TryGetCount (source, out int cnt) ? cnt : 4];
			foreach (var item in source)
			{
				if (array.Length == length)
				{
					var newArray = new T[length * 2];
					Array.Copy (array, newArray, length);
					array = newArray;
				}

				array[length++] = item;
			}

			if (array.Length == length)
			{
				return array;
			}

			if (length == 0)
			{
				return Array.Empty<T> ();
			}

			var resultArray = new T[length];
			Array.Copy (array, resultArray, length);
			return resultArray;
		}

		/// <summary>
		/// Создаёт список, в который оптимально доступным образом скопированы все элементы указанной последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элементов списка.</typeparam>
		/// <param name="source">Последовательность, которую необходимо скопировать в список.</param>
		/// <returns>Список, содержащий копию всех элементов исходной последовательности.</returns>
		public static IAdjustableList<T> DuplicateToList<T> (this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			T[] array;
			int length;

			if (source is IArrayDuplicableCollection<T> collection1)
			{
				length = collection1.Count;
				array = new T[length];
				if (length > 0)
				{
					collection1.CopyTo (array, 0);
				}

				return new ArrayList<T> (array);
			}

			if (source is ICollection<T> collection2)
			{
				length = collection2.Count;
				array = new T[length];
				if (length > 0)
				{
					collection2.CopyTo (array, 0);
				}

				return new ArrayList<T> (array);
			}

			if (source is ICollection collection3)
			{
				length = collection3.Count;
				array = new T[length];
				if (length > 0)
				{
					collection3.CopyTo (array, 0);
				}

				return new ArrayList<T> (array);
			}

			if (source is IReadOnlyCollection<T> collection4)
			{
				length = collection4.Count;
				array = new T[length];
				if (length > 0)
				{
					int idx = 0;
					foreach (var item in collection4)
					{
						array[idx++] = item;
					}
				}

				return new ArrayList<T> (array);
			}

			length = 0;
			array = new T[TryGetCount (source, out int cnt) ? cnt : 4];
			foreach (var item in source)
			{
				if (array.Length == length)
				{
					var newArray = new T[length * 2];
					Array.Copy (array, newArray, length);
					array = newArray;
				}

				array[length++] = item;
			}

			return new ArrayList<T> (array, 0, length);
		}
	}
}
