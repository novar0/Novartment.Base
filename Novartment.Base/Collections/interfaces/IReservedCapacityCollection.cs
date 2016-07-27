
namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция, поддерживающая перечисление и добавление элементов,
	/// а также управление резервированием места под них.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	public interface IReservedCapacityCollection<T> :
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Резервирует место под указанное общее количество элементов.
		/// </summary>
		/// <param name="min">Минимальная необходимая вместимость включая уже находящиеся в коллекции элементы.</param>
		/// <remarks>
		/// Соответствует установке свойства Capacity у классов System.Collections.ArrayList и System.Collections.Generic.List&lt;T&gt;.
		/// </remarks>
		void EnsureCapacity (int min);

		/// <summary>
		/// Избавляет коллекцию от зарезервированных элементов.
		/// </summary>
		/// <remarks>
		/// Соответствует методу TrimExcess() классов в пространстве System.Collections.Generic:
		/// List&lt;T&gt;, Stack&lt;T&gt;, Queue&lt;T&gt;, HashSet&lt;T&gt;.
		/// </remarks>
		void TrimExcess ();
	}
}
