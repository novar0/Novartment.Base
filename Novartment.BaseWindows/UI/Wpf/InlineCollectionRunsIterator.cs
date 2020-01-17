﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Documents;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	internal class InlineCollectionRunsIterator :
		IEnumerable<Run>
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
			var stack = new ArrayList<InlineCollection>
			{
				_collection,
			};
			while (stack.TryTakeLast (out InlineCollection collection))
			{
				foreach (var item in collection)
				{
					if (item is Span span)
					{
						stack.Add (span.Inlines);
					}

					if (item is Run run)
					{
						yield return run;
					}
				}
			}
		}
	}
}
