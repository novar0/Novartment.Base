using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Конечное множество уникальных значений c возможностью перечисления и проверкой принадлежности.
	/// </summary>
	/// <typeparam name="T">Тип элементов множества.</typeparam>
	[SuppressMessage ("Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public interface IReadOnlyFiniteSet<T> :
		IReadOnlyCollection<T>
	{
		/// <summary>
		/// Проверяет принадлежность указанного значения множеству.
		/// </summary>
		/// <param name="item">Значение для проверки принадлежности множеству.</param>
		/// <returns>
		/// True если указанное значение принадлежит множеству, либо False если не принадлежит.
		/// </returns>
		/// <remarks>
		/// Соответствует System.Collections.Generic.ICollection&lt;&gt;.Contains().
		/// </remarks>
		bool Contains (T item);
	}
}
