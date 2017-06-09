using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для System.Windows.Controls.Primitives.Selector.
	/// </summary>
	public static class SelectorExtensions
	{
		/// <summary>
		/// По возможности устанавливает для указанного селектора режим
		/// "всегда что нибудь выбрано".
		/// </summary>
		/// <param name="selector">Элемент управления типа селектор, для которого будет установлен режим.</param>
		[SuppressMessage (
			"Microsoft.Design",
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

		/// <summary>
		/// По возможности снимает для указанного селектора режим
		/// "всегда что нибудь выбрано".
		/// </summary>
		/// <param name="selector">Элемент управления типа селектор, для которого будет снят режим.</param>
		[SuppressMessage (
			"Microsoft.Design",
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
	}
}
