using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция, поддерживающее перечисление и копирование всех элементов в массив.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	public interface IArrayDuplicableCollection<T> : IReadOnlyCollection<T>
	{
		/// <summary>
		/// Копирует элементы коллекции в указанный массив,
		/// начиная с указанной позиции конечного массива.
		/// </summary>
		/// <param name="array">Массив, в который копируются элементы коллекции.</param>
		/// <param name="arrayIndex">Отсчитываемая от нуля позиция в массиве array, указывающий начало копирования.</param>
		/// <remarks>Соответствует System.Collections.ICollection.CopyTo() и System.Array.CopyTo().</remarks>
		void CopyTo (T[] array, int arrayIndex);
	}
}
