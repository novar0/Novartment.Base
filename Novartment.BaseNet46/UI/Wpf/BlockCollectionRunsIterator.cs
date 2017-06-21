using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Documents;
using Novartment.Base.Collections;
using static System.Linq.Enumerable;

namespace Novartment.Base.UI.Wpf
{
	internal class BlockCollectionRunsIterator : IEnumerable<Run>
	{
		private readonly BlockCollection _collection;

		internal BlockCollectionRunsIterator (BlockCollection collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			Contract.EndContractBlock ();
			_collection = collection;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerator<Run> GetEnumerator ()
		{
			var stack = new ArrayList<BlockCollection>
			{
				_collection,
			};

			while (stack.TryTakeLast (out BlockCollection collection))
			{
				foreach (var block in collection)
				{
					if (block is List list)
					{
						stack.AddRange (list.ListItems.Select (listItem => listItem.Blocks));
					}

					if (block is Section sect)
					{
						stack.Add (sect.Blocks);
					}

					if (block is Table table)
					{
						var blocks = table.RowGroups.SelectMany (rowGroup => rowGroup.Rows).SelectMany (row => row.Cells).Select (cell => cell.Blocks);
						foreach (var cellBlock in blocks)
						{
							stack.Add (cellBlock);
						}
					}

					if (block is Paragraph par)
					{
						foreach (var run in new InlineCollectionRunsIterator (par.Inlines))
						{
							yield return run;
						}
					}
				}
			}
		}
	}
}
