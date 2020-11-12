using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Распределяющая в строку/столбец виртуализующая панель, показывающая добавленный элемент.
	/// </summary>
	/// <remarks>
	/// Можно использовать как панель в списочных элементах чтобы отображать последнюю запись.
	/// </remarks>
	public sealed class ViewAddedItemVirtualizingStackPanel : VirtualizingStackPanel
	{
		/// <summary>
		/// <summary>Инициализирует новый экземпляр класса ViewAddedItemVirtualizingStackPanel.</summary>
		/// </summary>
		public ViewAddedItemVirtualizingStackPanel ()
		{
		}

		/// <summary>Вызывается при изменении коллекции Items, связанной с элементом ItemsControl для данного объекта.</summary>
		/// <param name="sender">Объект, который вызвал событие.</param>
		/// <param name="args">Данные события ItemsChanged.</param>
		protected override void OnItemsChanged (object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged (sender, args);
			if (args?.Action == NotifyCollectionChangedAction.Add)
			{
				var index = this.ItemContainerGenerator.IndexFromGeneratorPosition (args.Position);
				if (index >= 0)
				{
					BringIndexIntoView (index);
				}
			}
		}
	}
}
