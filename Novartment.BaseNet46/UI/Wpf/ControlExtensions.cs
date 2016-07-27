using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using static System.Linq.Enumerable;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для System.Windows.Controls.Primitives.Selector.
	/// </summary>
	public static class SelectorExtensions
	{
		#region method TrySetModeAlwaysSelected

		/// <summary>
		/// По возможности устанавливает для указанного селектора режим
		/// "всегда что нибудь выбрано".
		/// </summary>
		/// <param name="selector">Элемент управления типа селектор, для которого будет установлен режим.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Base type has no meaning here.")]
		public static void TrySetModeAlwaysSelected (this Selector selector)
		{
			if (selector == null)
			{
				throw new ArgumentNullException (nameof (selector));
			}
			Contract.EndContractBlock ();

			var view = selector.ItemsSource as ICollectionView ?? CollectionViewSource.GetDefaultView (selector.ItemsSource);
			if (view != null)
			{
				SafeMethods.TryAddCollectionChangedHandler (view, MoveCurrentToFirstIfNoSelection);
			}
		}

		#endregion

		#region method TryResetModeAlwaysSelected

		/// <summary>
		/// По возможности снимает для указанного селектора режим
		/// "всегда что нибудь выбрано".
		/// </summary>
		/// <param name="selector">Элемент управления типа селектор, для которого будет снят режим.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "Base type has no meaning here.")]
		public static void TryResetModeAlwaysSelected (this Selector selector)
		{
			if (selector == null)
			{
				throw new ArgumentNullException (nameof (selector));
			}
			Contract.EndContractBlock ();

			var view = selector.ItemsSource as ICollectionView ?? CollectionViewSource.GetDefaultView (selector.ItemsSource);
			if (view != null)
			{
				SafeMethods.TryRemoveCollectionChangedHandler (view, MoveCurrentToFirstIfNoSelection);
			}
		}

		#endregion

		#region method RemoveSelectedItem

		/// <summary>
		/// Удаляет выбранные элементы из элемента управления типа Selector.
		/// </summary>
		/// <param name="selector">Элемент управления типа Selector, из которого надо удалить выбранные элементы.</param>
		public static void RemoveSelectedItem (this Selector selector)
		{
			if (selector == null)
			{
				throw new ArgumentNullException (nameof (selector));
			}
			Contract.EndContractBlock ();

			var selectedItem = selector.SelectedItem;
			if ((selector.ItemsSource != null) && (selectedItem != null))
			{
				selector.RemoveAndMoveViewSelectionToNext (selectedItem);
			}
		}

		private static void MoveCurrentToFirstIfNoSelection (object sender, NotifyCollectionChangedEventArgs e)
		{
			var collectionView = sender as ICollectionView;
			if (collectionView == null)
			{
				return;
			}
			if ((collectionView.IsCurrentBeforeFirst || collectionView.IsCurrentAfterLast) && !collectionView.IsEmpty)
			{
				collectionView.MoveCurrentToFirst ();
			}
		}

		#endregion
	}

	/// <summary>
	/// Методы расширения для System.Windows.Controls.ItemsControl.
	/// </summary>
	public static class ItemsControlExtensions
	{
		#region method GetItemsControlItemType

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
			var colView = src as ICollectionView;
			if (colView != null)
			{
				src = colView.SourceCollection;
			}
			if (src == null)
			{
				throw new InvalidOperationException ("Required property control.ItemsSource not specified.");
			}
			var interfaces = src.GetType ().GetTypeInfo ().ImplementedInterfaces;
			var enumerable = interfaces.FirstOrDefault (item => item.IsConstructedGenericType && (item.GetGenericTypeDefinition () == typeof (IEnumerable<>)));
			if (enumerable == null)
			{
				throw new InvalidOperationException ("Control.ItemsSource does not implements IEnumerable<T>.");
			}
			var typeOfElement = enumerable.GenericTypeArguments[0];
			return typeOfElement;
		}

		#endregion

		#region method AddItemToItemsControl

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

			var list = src as IList;
			if (list == null)
			{
				throw new InvalidOperationException ("control.ItemsSource does not implements IList.");
			}
			list.Add (item);
		}

		#endregion

		#region method RemoveAndMoveViewSelectionToNext

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
			var list = collection as IList;
			if (list == null)
			{
				if (view == null)
				{
					throw new InvalidOperationException ("Control.ItemsSource does not implements either ICollectionView nor IList.");
				}
				list = view.SourceCollection as IList;
				if (list == null)
				{
					throw new InvalidOperationException ("(control.ItemsSource as ICollectionView).SourceCollection does not implements IList.");
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

			var col = view?.SourceCollection as ICollection;
			if (col == null)
			{
				list.Remove (item);
			}
			else
			{
				lock (col.SyncRoot)
				{
					list.Remove (item);
				}
			}

			// TODO дальше надо пробовать удалять через интерфейс ICollection<TItem>
		}

		#endregion
	}

	/// <summary>
	/// Методы расширения для System.Windows.Controls.ItemsControl.
	/// </summary>
	public static class ContentControlExtensions
	{
		#region method RemoveFromParentsItems

		/// <summary>
		/// Удаляет указанный ContentControl из родительского контейнера типа ItemsControl.
		/// </summary>
		/// <param name="control">Элемент типа ContentControl, который надо удалть из родительского контейнера.</param>
		public static void RemoveFromParentsItems (this ContentControl control)
		{
			if (control == null)
			{
				throw new ArgumentNullException (nameof (control));
			}
			Contract.EndContractBlock ();

			var itemsPresenter = control.FindVisualAncestor<ItemsPresenter> ();
			if (itemsPresenter == null)
			{
				throw new InvalidOperationException ("Specified control does not have visual ancestor of type ItemsPresenter.");
			}
			var itemsControl = itemsPresenter.TemplatedParent as ItemsControl;
			if (itemsControl == null)
			{
				throw new InvalidOperationException ("Control's ItemsPresenter is not ItemsControl.");
			}
			itemsControl.RemoveAndMoveViewSelectionToNext (control.Content);
		}

		#endregion
	}
}
