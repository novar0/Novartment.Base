using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using static System.Linq.Enumerable;

namespace Novartment.Base
{
	/// <summary>
	/// Фабрика по созданию объектов-сравнителей.
	/// </summary>
	public static class ComparerFactory
	{
		/// <summary>
		/// Создаёт сравнитель на основе указанной функции получения ключа объекта.
		/// Полученные ключи объектов будут использованы при сравнении этих объектов.
		/// </summary>
		/// <typeparam name="TItem">Тип объектов которые будут сравниваться.</typeparam>
		/// <typeparam name="TKey">Тип ключа, по которому будут сравниваться объекты.</typeparam>
		/// <param name="propertySelector">Функция получения ключа объекта.</param>
		/// <param name="descendingOrder">Направление сравнения.</param>
		/// <returns>Объект реализующий IComparer&lt;TItem&gt; и ISortDirection
		/// на основе указанной функции получения ключа объекта,
		/// который можно использовать для их сравнения.</returns>
		public static IComparer<TItem> CreateFromPropertySelector<TItem, TKey> (
			Func<TItem, TKey> propertySelector,
			bool descendingOrder = false)
		{
			if (propertySelector == null)
			{
				throw new ArgumentNullException (nameof (propertySelector));
			}

			Contract.EndContractBlock ();

			var sorter = new InternalSorter<TItem, TKey> (propertySelector);
			((ISortDirectionVariable)sorter).DescendingOrder = descendingOrder;
			return sorter;
		}

		/// <summary>
		/// Создаёт сравнитель на основе указанного имени свойства и направления сортировки.
		/// </summary>
		/// <typeparam name="TItem">Тип объектов которые будут сравниваться.</typeparam>
		/// <param name="propertyName">Имя свойства объекта, по которому будут сравниваться объекты.</param>
		/// <param name="descendingOrder">Направление сравнения.</param>
		/// <returns>Объект реализующий IComparer&lt;TItem&gt; и ISortDirection
		/// на основе указанного имени свойства и направления сортировки,
		/// который можно использовать для их сравнения.</returns>
		public static IComparer<TItem> CreateFromPropertyName<TItem> (string propertyName, bool descendingOrder = false)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException (nameof (propertyName));
			}

			Contract.EndContractBlock ();

			var itemType = typeof (TItem);
			var sortingPropertyInfo = itemType.GetRuntimeProperty (propertyName);
			if (sortingPropertyInfo == null)
			{
				throw new InvalidOperationException ("Specified property not found.");
			}

			var sorterType = typeof (InternalSorter<,>).MakeGenericType (itemType, sortingPropertyInfo.PropertyType);
			var sorterConstructor = sorterType.GetConstructors (BindingFlags.NonPublic | BindingFlags.Instance)
				.Single (item =>
				{
					var pars = item.GetParameters ();
					return (pars.Length == 1) && (pars[0].ParameterType == typeof (PropertyInfo));
				});
			var sorter = (IComparer<TItem>)sorterConstructor.Invoke (new object[] { sortingPropertyInfo });
			((ISortDirectionVariable)sorter).DescendingOrder = descendingOrder;
			return sorter;
		}

		internal sealed class InternalSorter<TItem, TKey> :
			IComparer,
			IComparer<TItem>,
			ISortDirectionVariable
		{
			private readonly IComparer<TKey> _comparer;
			private readonly Func<TItem, TKey> _keySelector;
			private readonly PropertyInfo _propertyInfo;
			private bool _descendingOrder = false;

			internal InternalSorter (Func<TItem, TKey> keySelector)
			{
				_propertyInfo = null;
				_keySelector = keySelector;
				_comparer = Comparer<TKey>.Default;
			}

			internal InternalSorter (PropertyInfo propertyInfo)
			{
				_propertyInfo = propertyInfo;
				_keySelector = GetPropertyByPropertyInfo;
				_comparer = Comparer<TKey>.Default;
			}

			public bool DescendingOrder
			{
				get => _descendingOrder;
				set { _descendingOrder = value; }
			}

			int IComparer.Compare (object item1, object item2)
			{
				return Compare ((TItem)item1, (TItem)item2);
			}

			public int Compare (TItem item1, TItem item2)
			{
				int result;
				var item1EqualsItem2 = ReferenceEquals (item1, item2);
				if (item1EqualsItem2)
				{
					result = 0;
				}
				else
				{
					if (item1 == null)
					{
						result = -1;
					}
					else
					{
						result = (item2 == null) ?
							1 :
							_comparer.Compare (_keySelector.Invoke (item1), _keySelector.Invoke (item2));
					}
				}

				return _descendingOrder ? -result : result;
			}

			private TKey GetPropertyByPropertyInfo (TItem item)
			{
				return (TKey)_propertyInfo.GetValue (item, null);
			}
		}
	}
}
