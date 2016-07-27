using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Collections
{
	// Библиотечный ISet не подходит для использования потому, что является избыточным.
	// Все его методы (кроме Add):
	// 1. Не могут быть реализованы внутри класса заметно более эффективно чем снаружи в методах расширения.
	// 2. Не пригодны для конкурентного доступа.

	/// <summary>
	/// Конечное множество уникальных элементов c перечислением, очисткой,
	/// проверкой наличия, добавлением и удалением элементов.
	/// </summary>
	/// <typeparam name="T">Тип элементов множества.</typeparam>
	/// <remarks>
	/// Характерные представители - System.Collections.Generic.SortedSet и System.Collections.Generic.HashSet.
	/// </remarks>
	[SuppressMessage ("Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public interface IAdjustableFiniteSet<T> :
		IReadOnlyFiniteSet<T>,
		IAdjustableCollection<T>
	{
		/// <summary>
		/// Удаляет из множества указанный элемент.
		/// </summary>
		/// <param name="item">Элемент для удаления из множества.</param>
		void Remove (T item);
	}
}
