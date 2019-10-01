using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Windows.Controls;
using static System.Linq.Enumerable;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для System.Windows.Controls.ItemsControl.
	/// </summary>
	public static class ItemsControlExtensions
	{
		/// <summary>
		/// Получает тип элементов коллекции-источника указанного элемента управления типа ItemsControl.
		/// </summary>
		/// <param name="control">Элемент управления типа ItemsControl, для которого необходимо получить тип элементов.</param>
		/// <returns>тип элементов коллекции-источника указанного элемента управления типа ItemsControl.</returns>
		public static Type GetItemsControlItemType (this ItemsControl control)
		{
			if (control == null)
			{
				throw new ArgumentNullException (nameof (control));
			}

			Contract.EndContractBlock ();

			var src = control.ItemsSource;
			if (src is ICollectionView colView)
			{
				src = colView.SourceCollection;
			}

			if (src == null)
			{
				throw new InvalidOperationException ("Required property control.ItemsSource not specified.");
			}

			var interfaces = src.GetType ().GetInterfaces ();
			var enumerable = interfaces.FirstOrDefault (item => item.IsConstructedGenericType && (item.GetGenericTypeDefinition () == typeof (IEnumerable<>)));
			if (enumerable == null)
			{
				throw new InvalidOperationException ("Control.ItemsSource does not implements IEnumerable<T>.");
			}

			var typeOfElement = enumerable.GenericTypeArguments[0];
			return typeOfElement;
		}

		/// <summary>
		/// Добавляет указанный элемент в коллекцию-источник указанного элемента управления типа ItemsControl.
		/// </summary>
		/// <param name="control">Элемент управления типа ItemsControl, в который будет добавлен указанный элемент.</param>
		/// <param name="item">Элемент для добавления.</param>
		public static void AddItemToItemsControl (this ItemsControl control, object item)
		{
			if (control == null)
			{
				throw new ArgumentNullException (nameof (control));
			}

			Contract.EndContractBlock ();

			var src = control.ItemsSource;
			var colView = src as ICollectionView;
			if (colView != null)
			{
				src = colView.SourceCollection;
			}

			if (colView == null)
			{
				throw new InvalidOperationException ("Required property control.ItemsSource not specified.");
			}

			if (!(src is IList list))
			{
				throw new InvalidOperationException("control.ItemsSource does not implements IList.");
			}

			list.Add (item);
		}

		/// <summary>
		/// Удаляет указанный элемент из указанного элемента управления типа ItemsControl
		/// и в случае необходимости делает выбранным другой.
		/// </summary>
		/// <param name="control">Элемент управления типа ItemsControl, из которого надо удалить указанный элемент.</param>
		/// <param name="item">Элемент, который надо удалить из коллекции.</param>
		public static void RemoveAndMoveViewSelectionToNext (this ItemsControl control, object item)
		{
			if (control == null)
			{
				throw new ArgumentNullException (nameof (control));
			}

			Contract.EndContractBlock ();

			var collection = control.ItemsSource;
			var view = collection as ICollectionView;
			if (!(collection is IList list))
			{
				if (view == null)
				{
					throw new InvalidOperationException("Control.ItemsSource does not implements either ICollectionView nor IList.");
				}

				list = view.SourceCollection as IList;
				if (list == null)
				{
					throw new InvalidOperationException("(control.ItemsSource as ICollectionView).SourceCollection does not implements IList.");
				}
			}

			// если удаляемый элемент является выбранным, то надо сначала выбрать другой
			var isViewCurrentItemEqualsItem = (view != null) && view.CurrentItem.Equals (item);
			if (isViewCurrentItemEqualsItem)
			{
				if (view.CurrentPosition == (list.Count - 1))
				{
					view.MoveCurrentToPrevious ();
				}
				else
				{
					view.MoveCurrentToNext ();
				}
			}

			if (!(view?.SourceCollection is ICollection col))
			{
				list.Remove(item);
			}
			else
			{
				lock (col.SyncRoot)
				{
					list.Remove(item);
				}
			}

			// TODO дальше надо пробовать удалять через интерфейс ICollection<TItem>
		}
	}
}
