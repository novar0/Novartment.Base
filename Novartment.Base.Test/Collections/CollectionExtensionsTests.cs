using System;
using System.Collections.Generic;
using Novartment.Base.Collections;
using Xunit;
using static System.Linq.Enumerable;

namespace Novartment.Base.Test
{
	public class GenericCollectionExtensionsTests
	{
		internal class TestSet<T> : HashSet<T>, IReadOnlyFiniteSet<T>
		{
			internal TestSet ()
				: base ()
			{
			}

			internal TestSet (IEnumerable<T> collection)
				: base (collection)
			{
			}
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void Array_LoopedArraySegmentClear ()
		{
			var data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			GenericCollectionExtensions.LoopedArraySegmentClear (data, 3, 6, 0, 6);
			Assert.Equal<int> (new int[] { 8, 3, 2, 0, 0, 0, 0, 0, 0, 7 }, data);

			data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			GenericCollectionExtensions.LoopedArraySegmentClear (data, 8, 9, 0, 9);
			Assert.Equal<int> (new int[] { 0, 0, 0, 0, 0, 0, 0, 4, 0, 0 }, data);

			data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			GenericCollectionExtensions.LoopedArraySegmentClear (data, 1, 0, 0, 0);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, data);

			data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			GenericCollectionExtensions.LoopedArraySegmentClear (data, 8, 9, 4, 1);
			Assert.Equal<int> (new int[] { 8, 3, 0, 10, -11, 11, 4, 4, 20, 7 }, data);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void Array_LoopedArraySegmentCopy ()
		{
			var data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			GenericCollectionExtensions.LoopedArraySegmentCopy (data, 3, 6, 0, 1, 4);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, 10, -11, 11, 4, 20, 7 }, data);

			GenericCollectionExtensions.LoopedArraySegmentCopy (data, 0, 10, 0, 0, 10);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, 10, -11, 11, 4, 20, 7 }, data);

			GenericCollectionExtensions.LoopedArraySegmentCopy (data, 9, 10, 9, 1, 1);
			Assert.Equal<int> (new int[] { 20, 3, 2, 10, 10, -11, 11, 4, 20, 7 }, data);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void AppendableCollection_AddRange ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			var list1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.AddRange (list1, Repeat (973, 3));
			Assert.Equal (13, list1.Count);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7, 973, 973, 973 }, list1);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void AppendableCollection_InsertRange ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			var list1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.InsertRange (list1, 3, Repeat (973, 3));
			Assert.Equal (13, list1.Count);
			Assert.Equal<int> (new int[] { 8, 3, 2, 973, 973, 973, 10, -11, 11, 4, 4, 20, 7 }, list1);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void AdjustableList_RemoveItems ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			var c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveItems (c1, new TestSet<int> ());
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveItems (c1, new TestSet<int> (new int[] { 44, 88 }));
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveItems (c1, new TestSet<int> (new int[] { 4, 88, 8 }));
			Assert.Equal<int> (new int[] { 3, 2, 10, -11, 11, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveItems (c1, new TestSet<int> (new int[] { -11, 11, 4, 20, 7, 8, 3, 2, 10 }));
			Assert.Equal<int> (Array.Empty<int> (), c1);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void AdjustableList_RemoveAtMany ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			var c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, Array.Empty<int> ());
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 1 });
			Assert.Equal<int> (new int[] { 8, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 1, 8 });
			Assert.Equal<int> (new int[] { 8, 2, 10, -11, 11, 4, 4, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 0, 9 });
			Assert.Equal<int> (new int[] { 3, 2, 10, -11, 11, 4, 4, 20 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 0, 1, 5, 7, 8 });
			Assert.Equal<int> (new int[] { 2, 10, -11, 4, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 0, 3, 5, 7, 9 });
			Assert.Equal<int> (new int[] { 3, 2, -11, 4, 20 }, c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			Assert.Equal<int> (Array.Empty<int> (), c1);

			c1 = new AdjustableList_<int> (t1);
			GenericCollectionExtensions.RemoveAtMany (c1, new int[] { 0, 1, 2, 3, 5, 6, 7, 8, 9 });
			Assert.Equal<int> (new int[] { -11 }, c1);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void Enumerable_TryGetCount ()
		{
			var list1 = Repeat<string> (null, 2)
				.Concat (Repeat<string> ("asfc", 1))
				.Concat (Repeat<string> (null, 3))
				.Concat (Repeat<string> ("123", 2));
			var list2 = list1.ToArray ();
			Assert.False (GenericCollectionExtensions.TryGetCount (list1, out _));
			Assert.True (GenericCollectionExtensions.TryGetCount (list2, out int cnt));
			Assert.Equal (8, cnt);
		}

		[Fact]
		[Trait ("Category", "Collections.GenericCollectionExtensions")]
		public void Enumerable_DuplicateToArray ()
		{
			var list1 = Repeat<int> (0, 2)
				.Concat (Repeat<int> (89, 1))
				.Concat (Repeat<int> (333, 3))
				.Concat (Repeat<int> (-1, 2));
			var list2 = list1.ToArray ();

			var res1 = GenericCollectionExtensions.DuplicateToArray (list1);
			Assert.Equal (8, res1.Length);
			Assert.Equal (0, res1[0]);
			Assert.Equal (0, res1[1]);
			Assert.Equal (89, res1[2]);
			Assert.Equal (333, res1[3]);
			Assert.Equal (333, res1[4]);
			Assert.Equal (333, res1[5]);
			Assert.Equal (-1, res1[6]);
			Assert.Equal (-1, res1[7]);

			var res2 = GenericCollectionExtensions.DuplicateToArray (list2);
			Assert.Equal (8, res2.Length);
			Assert.Equal (0, res2[0]);
			Assert.Equal (0, res2[1]);
			Assert.Equal (89, res2[2]);
			Assert.Equal (333, res2[3]);
			Assert.Equal (333, res2[4]);
			Assert.Equal (333, res2[5]);
			Assert.Equal (-1, res2[6]);
			Assert.Equal (-1, res2[7]);
		}

		// Тривиальная обёртка чтобы реализовать поддержку IAdjustableList
		internal class AdjustableList_<T> : System.Collections.Generic.List<T>,
			IAdjustableList<T>,
			IArrayDuplicableCollection<T>
		{
			public AdjustableList_ ()
				: base ()
			{
			}

			public AdjustableList_ (System.Collections.Generic.IEnumerable<T> collection)
				: base (collection)
			{
			}

			public bool IsEmpty => throw new NotImplementedException ();

			public void InsertRange (int index, int count) => throw new NotImplementedException ();

			public bool TryPeekFirst (out T item) => throw new NotImplementedException ();

			public bool TryTakeFirst (out T item) => throw new NotImplementedException ();

			public bool TryPeekLast (out T item) => throw new NotImplementedException ();

			public bool TryTakeLast (out T item) => throw new NotImplementedException ();
		}
	}
}
