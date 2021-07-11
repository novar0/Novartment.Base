using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Коллекция из UIElement, поддерживающая уведомления об изменениях.
	/// </summary>
	public class ObservableUIElementCollection : UIElementCollection,
		INotifyCollectionChanged
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса ObservableUIElementCollection с указанными визуальным и логическим родительскими элементами.
		/// </summary>
		/// <param name="visualParent">The System.Windows.UIElement parent of the collection.</param>
		/// <param name="logicalParent">The logical parent of the elements in the collection.</param>
		public ObservableUIElementCollection (UIElement visualParent, FrameworkElement logicalParent)
			: base (visualParent, logicalParent)
		{
		}

		/// <summary>Происходит, когда запись добавляется в словарь.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Добавляет указанный элемент к коллекции.</summary>
		/// <param name="element">Добавляемый объект.</param>
		/// <returns>Позиция индекса добавленного элемента.</returns>
		public override int Add (UIElement element)
		{
			var index = base.Add (element);
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, element, index));
			return index;
		}

		/// <summary>Удаляет указанные элементы из коллекции.</summary>
		/// <param name="element">Элемент, который нужно удалить из коллекции.</param>
		public override void Remove (UIElement element)
		{
			var index = IndexOf (element);
			base.Remove (element);
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, element, index));
		}

		/// <summary>Удаляет элемент по указанному индексу.</summary>
		/// <param name="index">Индекс элемента, который требуется удалить.</param>
		public override void RemoveAt (int index)
		{
			var element = this[index];
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, element, index));
		}

		/// <summary>Удаляет диапазон элементов из коллекции.</summary>
		/// <param name="index">Позиция индекса элемента, где начинается удаление.</param>
		/// <param name="count">Число удаляемых элементов.</param>
		public override void RemoveRange (int index, int count)
		{
			var elements = new ArrayList<UIElement> (count);
			for (var i = index; i < count; i++)
			{
				elements.Add (this[i]);
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, elements));
		}

		/// <summary>Удаляет все элементы из коллекции.</summary>
		public override void Clear ()
		{
			base.Clear ();
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Создаёт событие CollectionChanged с указанными аргументами.
		/// </summary>
		/// <param name="e">Аргументы события.</param>
		protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			this.CollectionChanged?.Invoke (this, e);
		}
	}
}
