namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция, поддерживающая добавление элементов, перечисление элементов в порядке добавления
	/// и получение/изъятие первого из добавленных элементов (семантика очереди).
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	/// <remarks>Характерный представитель - System.Collections.Generic.Queue.</remarks>
	public interface IFifoCollection<T> :
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Пытается получить первый элемент коллекции.
		/// </summary>
		/// <param name="item">Значение первого элемента если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент успешно получен, False если нет.</returns>
		bool TryPeekFirst (out T item);

		/// <summary>
		/// Пытается изъять первый элемент коллекции.
		/// </summary>
		/// <param name="item">Значение первого элемента если он успешно изъят,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент успешно изъят, False если нет.</returns>
		bool TryTakeFirst (out T item);
	}
}
