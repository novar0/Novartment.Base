using System.Collections.Generic;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Данные о контексте-коллекции, для которого вызвана команда.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	public class ContextCollectionData<T>
	{
		/// <summary>
		/// Получает контекст элемента, для которого вызвана команда.
		/// </summary>
		public object Context { get; }

		/// <summary>
		/// Получает коллекцию, которая является источником списка элементов, частью которого является элемент, для которого вызвана команда.
		/// </summary>
		public IEnumerable<T> ContextCollection { get; }

		/// <summary>
		/// Получает элементы, выбранные в ContextCollection.
		/// </summary>
		public IReadOnlyFiniteSet<T> ContextCollectionSelectedItems { get; }

		/// <summary>
		/// Инициализирует новый экземпляр ContextCollectionData на основе указанных данных.
		/// </summary>
		/// <param name="context">Контекст элемента, для которого вызвана команда.</param>
		/// <param name="contextCollection">Коллекция, которая является источником списка элементов,
		/// частью которого является элемент, для которого вызвана команда.</param>
		/// <param name="contextCollectionSelectedItems">Эементы, выбранные в ContextCollection.</param>
		public ContextCollectionData (object context, IEnumerable<T> contextCollection, IReadOnlyFiniteSet<T> contextCollectionSelectedItems)
		{
			this.Context = context;
			this.ContextCollection = contextCollection;
			this.ContextCollectionSelectedItems = contextCollectionSelectedItems;
		}
	}
}
