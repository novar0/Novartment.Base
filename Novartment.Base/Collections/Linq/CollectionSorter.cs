using System;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Factory to create a sorted index for collections.
	/// </summary>
	/// <typeparam name="TElement">The type of the elements.</typeparam>
	internal abstract class CollectionSorter<TElement> : IComparer<int>
	{
		private readonly IReadOnlyCollection<TElement> _items;

		protected CollectionSorter (IReadOnlyCollection<TElement> items)
		{
			_items = items;
		}

		protected IReadOnlyCollection<TElement> Items => _items;

		public abstract int Compare (int index1, int index2);

		internal abstract void Initialize ();

		internal int[] CreateIndex ()
		{
			Initialize ();
			var indexMap = new int[_items.Count];
			for (int index = 0; index < _items.Count; index++)
			{
				indexMap[index] = index;
			}

			Array.Sort<int> (indexMap, this);
			return indexMap;
		}
	}
}
