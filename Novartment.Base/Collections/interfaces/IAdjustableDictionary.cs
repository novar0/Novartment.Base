using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Коллекция пар ключ/значение (словарь).
	/// </summary>
	/// <typeparam name="TKey">Тип ключей в словаре.</typeparam>
	/// <typeparam name="TValue">Тип значений в словаре.</typeparam>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
		Justification = "'Dictionary' suffix intended because of base type is IReadOnlyDictionary.")]
	public interface IAdjustableDictionary<TKey, TValue> :
		IReadOnlyDictionary<TKey, TValue>,
		IAdjustableCollection<KeyValuePair<TKey, TValue>>
	{
		/// <summary>
		/// Получает или устанавливает элемент с указанным ключом.
		/// </summary>
		/// <param name="key">Ключ элемента.</param>
		new TValue this[TKey key] { get; set; }

		/// <summary>
		/// Удаляет элемент с указанным ключом.
		/// </summary>
		/// <param name="key">Ключ элемента, который требуется удалить.</param>
		/// <returns>True, если элемент успешно удалён, в противном случае — False.</returns>
		bool Remove (TKey key);
	}
}
