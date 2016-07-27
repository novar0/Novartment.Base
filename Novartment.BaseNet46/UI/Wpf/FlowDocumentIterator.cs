using System;
using static System.Linq.Enumerable;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

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
			var stack = new ArrayList<BlockCollection> ();
			stack.Add (_collection);

			BlockCollection collection;
			while (stack.TryTakeLast (out collection))
			{
				foreach (var block in collection)
				{
					var list = block as List;
					if (list != null)
					{
						stack.AddRange (list.ListItems.Select (listItem => listItem.Blocks));
					}

					var sect = block as Section;
					if (sect != null)
					{
						stack.Add (sect.Blocks);
					}

					var table = block as Table;
					if (table != null)
					{
						var blocks = table.RowGroups.SelectMany (rowGroup => rowGroup.Rows).SelectMany (row => row.Cells).Select (cell => cell.Blocks);
						foreach (var cellBlock in blocks)
						{
							stack.Add (cellBlock);
						}
					}

					var par = block as Paragraph;
					if (par != null)
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

	internal class InlineCollectionRunsIterator : IEnumerable<Run>
	{
		private readonly InlineCollection _collection;

		internal InlineCollectionRunsIterator (InlineCollection collection)
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
			var stack = new ArrayList<InlineCollection> ();
			stack.Add (_collection);

			InlineCollection collection;
			while (stack.TryTakeLast (out collection))
			{
				foreach (var item in collection)
				{
					var span = item as Span;
					if (span != null)
					{
						stack.Add (span.Inlines);
					}

					var run = item as Run;
					if (run != null)
					{
						yield return run;
					}
				}
			}
		}
	}
}
