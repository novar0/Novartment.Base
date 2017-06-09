using System;
using System.Collections.Generic;
using System.Linq;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Представляет отсортированную коллекцию.
	/// </summary>
	/// <typeparam name="TElement">Тип элементов коллекции.</typeparam>
	public interface IOrderedReadOnlyCollection<TElement> :
		IOrderedEnumerable<TElement>,
		IReadOnlyCollection<TElement>
	{
		/// <summary>
		/// Выполняет дополнительное упорядочение элементов объекта IOrderedReadOnlyCollection&lt;TElement&gt; по ключу.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа, созданного функцией keySelector.</typeparam>
		/// <param name="keySelector">Функция, используемая для извлечения ключа для каждого элемента.</param>
		/// <param name="comparer">Компаратор, используемый для сравнения ключей при формировании возвращаемой коллекции.</param>
		/// <param name="descending">True, если элементы требуется сортировать в порядке убывания; False, чтобы сортировать элементы в порядке возрастания.</param>
		/// <returns>Коллекция, элементы которой отсортированы по ключу.</returns>
		IOrderedReadOnlyCollection<TElement> CreateOrderedReadOnlyCollection<TKey> (Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
	}
}
