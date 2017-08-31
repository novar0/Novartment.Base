using System;
using Novartment.Base.Collections;
using Xunit;
using static System.Linq.Enumerable;

namespace Novartment.Base.Test
{
	public class CollectionExtensionsTests
	{
		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void Array_LoopedArraySegmentClear ()
		{
			var data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			CollectionExtensions.LoopedArraySegmentClear (data, 3, 6, 0, 6);
			Assert.Equal<int> (new int[] { 8, 3, 2, 0, 0, 0, 0, 0, 0, 7 }, data);

			data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			CollectionExtensions.LoopedArraySegmentClear (data, 8, 9, 0, 9);
			Assert.Equal<int> (new int[] { 0, 0, 0, 0, 0, 0, 0, 4, 0, 0 }, data);

			data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			CollectionExtensions.LoopedArraySegmentClear (data, 1, 0, 0, 0);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, data);

			data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			CollectionExtensions.LoopedArraySegmentClear (data, 8, 9, 4, 1);
			Assert.Equal<int> (new int[] { 8, 3, 0, 10, -11, 11, 4, 4, 20, 7 }, data);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void Array_LoopedArraySegmentCopy ()
		{
			var data = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			CollectionExtensions.LoopedArraySegmentCopy (data, 3, 6, 0, 1, 4);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, 10, -11, 11, 4, 20, 7 }, data);

			CollectionExtensions.LoopedArraySegmentCopy (data, 0, 10, 0, 0, 10);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, 10, -11, 11, 4, 20, 7 }, data);

			CollectionExtensions.LoopedArraySegmentCopy (data, 9, 10, 9, 1, 1);
			Assert.Equal<int> (new int[] { 20, 3, 2, 10, 10, -11, 11, 4, 20, 7 }, data);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void AppendableCollection_AddRange ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			var list1 = new AdjustableList_<int> (t1);
			CollectionExtensions.AddRange (list1, Repeat (973, 3));
			Assert.Equal (13, list1.Count);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7, 973, 973, 973 }, list1);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void AppendableCollection_InsertRange ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };
			var list1 = new AdjustableList_<int> (t1);
			CollectionExtensions.InsertRange (list1, 3, Repeat (973, 3));
			Assert.Equal (13, list1.Count);
			Assert.Equal<int> (new int[] { 8, 3, 2, 973, 973, 973, 10, -11, 11, 4, 4, 20, 7 }, list1);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void Enumerable_Split ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			// любой элемент является разделителем. результат должен быть пустой
			var t2 = CollectionExtensions.Split (t1, item => true).ToArray ();
			Assert.Empty (t2);

			// ни один элемент не является разделителем. результат должен повторять исходник
			t2 = CollectionExtensions.Split (t1, item => false).ToArray ();
			Assert.Single (t2);
			Assert.Equal<int> (t1, t2[0]);

			// разделитель - нечётные числа
			t2 = CollectionExtensions.Split (t1, item => (item & 1) != 0).ToArray ();
			Assert.Equal (3, t2.Length);
			Assert.Equal<int> (new int[] { 8 }, t2[0]);
			Assert.Equal<int> (new int[] { 2, 10 }, t2[1]);
			Assert.Equal<int> (new int[] { 4, 4, 20 }, t2[2]);

			// разделитель - чётные числа
			t2 = CollectionExtensions.Split (t1, item => (item & 1) == 0).ToArray ();
			Assert.Equal (3, t2.Length);
			Assert.Equal<int> (new int[] { 3 }, t2[0]);
			Assert.Equal<int> (new int[] { -11, 11 }, t2[1]);
			Assert.Equal<int> (new int[] { 7 }, t2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void AdjustableList_RemoveWhere ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			var c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveWhere (c1, item => item % 2 == 0);
			Assert.Equal<int> (new int[] { 3, -11, 11, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveWhere (c1, item => item > 0);
			Assert.Equal<int> (new int[] { -11 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveWhere (c1, item => true);
			Assert.Equal<int> (Array.Empty<int> (), c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveWhere (c1, item => false);
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void AdjustableList_RemoveItems ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			var c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveItems (c1, Array.Empty<int> ().ToSet ());
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveItems (c1, new int[] { 44, 88 }.ToSet ());
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveItems (c1, new int[] { 4, 88, 8 }.ToSet ());
			Assert.Equal<int> (new int[] { 3, 2, 10, -11, 11, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveItems (c1, new int[] { -11, 11, 4, 20, 7, 8, 3, 2, 10 }.ToSet ());
			Assert.Equal<int> (Array.Empty<int> (), c1);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void AdjustableList_RemoveAtMany ()
		{
			var t1 = new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 };

			var c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, Array.Empty<int> ());
			Assert.Equal<int> (new int[] { 8, 3, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 1 });
			Assert.Equal<int> (new int[] { 8, 2, 10, -11, 11, 4, 4, 20, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 1, 8 });
			Assert.Equal<int> (new int[] { 8, 2, 10, -11, 11, 4, 4, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 0, 9 });
			Assert.Equal<int> (new int[] { 3, 2, 10, -11, 11, 4, 4, 20 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 0, 1, 5, 7, 8 });
			Assert.Equal<int> (new int[] { 2, 10, -11, 4, 7 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 0, 3, 5, 7, 9 });
			Assert.Equal<int> (new int[] { 3, 2, -11, 4, 20 }, c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			Assert.Equal<int> (Array.Empty<int> (), c1);

			c1 = new AdjustableList_<int> (t1);
			CollectionExtensions.RemoveAtMany (c1, new int[] { 0, 1, 2, 3, 5, 6, 7, 8, 9 });
			Assert.Equal<int> (new int[] { -11 }, c1);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void Enumerable_WhereNotNull ()
		{
			var list1 = Repeat<string> (null, 2)
				.Concat (Repeat<string> ("asfc", 1))
				.Concat (Repeat<string> (null, 3))
				.Concat (Repeat<string> ("123", 2));
			var list2 = CollectionExtensions.WhereNotNull (list1).ToArray ();
			Assert.Equal (3, list2.Length);
			Assert.Equal ("asfc", list2[0]);
			Assert.Equal ("123", list2[1]);
			Assert.Equal ("123", list2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void Enumerable_TryGetCount ()
		{
			var list1 = Repeat<string> (null, 2)
				.Concat (Repeat<string> ("asfc", 1))
				.Concat (Repeat<string> (null, 3))
				.Concat (Repeat<string> ("123", 2));
			var list2 = list1.ToArray ();
			Assert.False (CollectionExtensions.TryGetCount (list1, out int cnt));
			Assert.True (CollectionExtensions.TryGetCount (list2, out cnt));
			Assert.Equal (8, cnt);
		}

		[Fact]
		[Trait ("Category", "Collections.CollectionExtensions")]
		public void Enumerable_DuplicateToArray ()
		{
			var list1 = Repeat<int> (0, 2)
				.Concat (Repeat<int> (89, 1))
				.Concat (Repeat<int> (333, 3))
				.Concat (Repeat<int> (-1, 2));
			var list2 = list1.ToArray ();

			var res1 = CollectionExtensions.DuplicateToArray (list1);
			Assert.Equal (8, res1.Length);
			Assert.Equal (0, res1[0]);
			Assert.Equal (0, res1[1]);
			Assert.Equal (89, res1[2]);
			Assert.Equal (333, res1[3]);
			Assert.Equal (333, res1[4]);
			Assert.Equal (333, res1[5]);
			Assert.Equal (-1, res1[6]);
			Assert.Equal (-1, res1[7]);

			var res2 = CollectionExtensions.DuplicateToArray (list2);
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
