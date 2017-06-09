using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция, поддерживающая добавление элементов, перечисление элементов в порядке добавления
	/// и получение/изъятие последнего из добавленных элементов (семантика стэка).
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	/// <remarks>Характерный представитель - System.Collections.Generic.Stack.</remarks>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Lifo",
		Justification = "'LIFO' represents standard term.")]
	public interface ILifoCollection<T> :
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Пытается получить последний элемент коллекции.
		/// </summary>
		/// <param name="item">Значение последнего элемента если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент успешно получен, False если нет.</returns>
		bool TryPeekLast (out T item);

		/// <summary>
		/// Пытается изъять последний элемент коллекции.
		/// </summary>
		/// <param name="item">Значение последнего элемента если он успешно изъят,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент успешно изъят, False если нет.</returns>
		bool TryTakeLast (out T item);
	}
}
