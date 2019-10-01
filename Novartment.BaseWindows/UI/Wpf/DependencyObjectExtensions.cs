using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для DependencyObject.
	/// </summary>
	public static class DependencyObjectExtensions
	{
		/// <summary>
		/// В визуальном дереве родительских элементов указанного, ищет ближайший элемент указанного типа.
		/// </summary>
		/// <typeparam name="TItem">Тип искомого элемента, производный от DependencyObject.</typeparam>
		/// <param name="child">Элемент, среди родительских элементов которого производить поиск.</param>
		/// <returns>Найденный элемент либо null если ничего не найдено.</returns>
		public static TItem FindVisualAncestor<TItem> (this DependencyObject child)
			where TItem : DependencyObject
		{
			if (child == null)
			{
				throw new ArgumentNullException (nameof (child));
			}

			Contract.EndContractBlock ();

			if (child is TItem candidate)
			{
				return candidate;
			}

			var parent = VisualTreeHelper.GetParent (child);
			return (parent != null) ? FindVisualAncestor<TItem> (parent) : null;
		}

		/// <summary>
		/// В логическом дереве дочерних элементов указанного, ищет ближайший элемент указанного типа,
		/// удовлетворяющий указанному условию проверки.
		/// </summary>
		/// <typeparam name="TItem">Тип искомого элемента, производный от DependencyObject.</typeparam>
		/// <param name="parent">Элемент, среди дочерних элементов которого производить поиск.</param>
		/// <param name="predicate">Уловие проверки, возвращающее true для подходящих элементов.</param>
		/// <returns>Найденный элемент либо null если ничего не найдено.</returns>
		public static TItem FindLogicalChild<TItem> (this DependencyObject parent, Func<TItem, bool> predicate)
			where TItem : DependencyObject
		{
			if (parent == null)
			{
				throw new ArgumentNullException (nameof (parent));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException (nameof (predicate));
			}

			Contract.EndContractBlock ();

			var candidate = parent as TItem;
			var isPredicatePassed = (candidate != null) && predicate.Invoke (candidate);
			if (isPredicatePassed)
			{
				return candidate;
			}

			foreach (var obj in LogicalTreeHelper.GetChildren (parent))
			{
				if (obj is DependencyObject child)
				{
					var result = FindLogicalChild (child, predicate);
					if (result != null)
					{
						return result;
					}
				}
			}

			return null;
		}
	}
}
