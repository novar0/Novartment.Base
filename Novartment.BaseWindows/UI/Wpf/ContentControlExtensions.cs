using System;
using System.Diagnostics.Contracts;
using System.Windows.Controls;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для System.Windows.Controls.ItemsControl.
	/// </summary>
	public static class ContentControlExtensions
	{
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

			if (itemsPresenter.TemplatedParent is not ItemsControl itemsControl)
			{
				throw new InvalidOperationException("Control's ItemsPresenter is not ItemsControl.");
			}

			itemsControl.RemoveAndMoveViewSelectionToNext (control.Content);
		}
	}
}
