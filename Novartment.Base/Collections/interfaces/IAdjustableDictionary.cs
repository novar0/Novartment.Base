using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// A collection of key/value pairs.
	/// </summary>
	/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
	public interface IAdjustableDictionary<TKey, TValue> :
		IReadOnlyDictionary<TKey, TValue>,
		IAdjustableCollection<KeyValuePair<TKey, TValue>>
	{
		/// <summary>
		/// Gets or sets the element that has the specified key in the dictionary.
		/// </summary>
		/// <param name="key">The key to locate.</param>
		new TValue this[TKey key] { get; set; }

		/// <summary>
		/// Removes the value with the specified key from the dictionary.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>True if the element is successfully found and removed; otherwise, False.
		/// This method returns false if key is not found in the dictionary.</returns>
		bool Remove (TKey key);
	}
}
