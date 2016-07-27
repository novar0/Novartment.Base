using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция, элементы которой можно перечислять,
	/// получать, устанавливать и удалять по номеру позиции,
	/// а также добавлять новые в конец или указанную позицию.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	/// <remarks>Характерный представитель - System.Collections.Generic.List.</remarks>
	[SuppressMessage ("Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with name.")]
	public interface IAdjustableList<T> :
		IReadOnlyList<T>,
		IAdjustableCollection<T>,
		IFifoCollection<T>,
		ILifoCollection<T>
	{
		/// <summary>
		/// Получает или устанавливает элемент коллекции по номеру позиции.
		/// </summary>
		/// <param name="index">Номер позиции нужного элемента.</param>
		new T this[int index] { get; set; }

		/// <summary>
		/// Вставляет указанный элемент в коллекцию в указанную позицию.
		/// </summary>
		/// <param name="index">Позиция в коллекции, куда будет вставлен элемент.</param>
		/// <param name="item">Элемент для вставки.</param>
		void Insert (int index, T item);

		/// <summary>
		/// Вставляет пустой диапазон элементов в коллекцию в указанную позицию.
		/// </summary>
		/// <param name="index">Позиция в коллекции, куда будут вставлены элементы.</param>
		/// <param name="count">Количество вставляемых элементов.</param>
		void InsertRange (int index, int count);

		/// <summary>
		/// Удаляет элемент из коллекции в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в коллекции.</param>
		void RemoveAt (int index);

		/// <summary>
		/// Удаляет указанное число элементов из коллекции начиная с указанной позиции.
		/// </summary>
		/// <param name="index">Начальная позиция элементов для удаления.</param>
		/// <param name="count">Количество удаляемых элементов.</param>
		void RemoveRange (int index, int count);
	}
}
