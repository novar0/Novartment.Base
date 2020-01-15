using System;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Создатель сортированного индекса для коллекций на основе сравнения ключей элементов, выбранных указанной функцией.
	/// </summary>
	/// <typeparam name="TElement">The type of the elements.</typeparam>
	/// <typeparam name="TKey">The type of the sorting key.</typeparam>
	internal class CollectionSorter<TElement, TKey> : CollectionSorter<TElement>
	{
		private readonly Func<TElement, TKey> _keySelector;
		private readonly IComparer<TKey> _comparer;
		private readonly bool _reverseOrder;
		private readonly CollectionSorter<TElement> _childSorter;
		private readonly TKey[] _keys;

		internal CollectionSorter (
			IReadOnlyCollection<TElement> items,
			Func<TElement, TKey> keySelector,
			IComparer<TKey> comparer,
			bool reverseOrder,
			CollectionSorter<TElement> childSorter)
			: base (items)
		{
			_keySelector = keySelector;
			_comparer = comparer;
			_reverseOrder = reverseOrder;
			_keys = new TKey[this.Items.Count];
			_childSorter = childSorter;
		}

		public override int Compare (int index1, int index2)
		{
			var num = _comparer.Compare (_keys[index1], _keys[index2]);
			return (num == 0) ?
				(_childSorter == null) ? (index1 - index2) : _childSorter.Compare (index1, index2) :
				_reverseOrder ? -num : num;
		}

		internal override void Initialize ()
		{
			int index = 0;
			foreach (var item in this.Items)
			{
				_keys[index++] = _keySelector.Invoke (item);
			}

			_childSorter?.Initialize ();
		}
	}
}
