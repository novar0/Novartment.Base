using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция, поддерживающая перечисление, очистку и добавление элементов.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	public interface IAdjustableCollection<T> :
		IReadOnlyCollection<T>
	{
		/// <summary>
		/// Добавляет элемент в коллекцию.
		/// </summary>
		/// <param name="item">Элемент для добавления в коллекцию.</param>
		void Add (T item);

		/// <summary>Очищает коллекцию.</summary>
		void Clear ();
	}
}
