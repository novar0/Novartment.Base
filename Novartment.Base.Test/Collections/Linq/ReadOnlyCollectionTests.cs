using System;
using Enumerable = System.Linq.Enumerable;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Novartment.Base.Collections.Linq.Test
{
	public class ReadOnlyCollectionTests
	{
		private static IReadOnlyList<T> ToArray<T> (IEnumerable<T> enumerable)
		{
			return Enumerable.ToArray (enumerable);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Any_ ()
		{
			Assert.False (ReadOnlyCollection.Any (new int[0]));
			Assert.True (ReadOnlyCollection.Any (new int[] { 3 }));
			Assert.True (ReadOnlyCollection.Any (new string[] { "one", "two", "three" }));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Count_ ()
		{
			Assert.Equal (0, ReadOnlyCollection.Count (new int[0]));
			Assert.Equal (1, ReadOnlyCollection.Count (new int[] { 3 }));
			Assert.Equal (3, ReadOnlyCollection.Count (new string[] { "one", "two", "three" }));
			Assert.Equal (0L, ReadOnlyCollection.LongCount (new int[0]));
			Assert.Equal (1L, ReadOnlyCollection.LongCount (new int[] { 3 }));
			Assert.Equal (3L, ReadOnlyCollection.LongCount (new string[] { "one", "two", "three" }));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void DefaultIfEmpty_ ()
		{
			var collection = ToArray (ReadOnlyCollection.DefaultIfEmpty (new int[0], 999));
			Assert.Equal (1, collection.Count);
			Assert.Equal (999, collection[0]);
			collection = ToArray (ReadOnlyCollection.DefaultIfEmpty (new int[] { 9, 3, 1 }, 999));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			var collection2 = ToArray (ReadOnlyCollection.DefaultIfEmpty (new string[0], "999"));
			Assert.Equal (1, collection2.Count);
			Assert.Equal ("999", collection2[0]);
			collection2 = ToArray (ReadOnlyCollection.DefaultIfEmpty (new string[] { "three", "two", "one" }, "999"));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three", collection2[0]);
			Assert.Equal ("two", collection2[1]);
			Assert.Equal ("one", collection2[2]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Skip_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Skip (new int[0], 3));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Skip (new int[] { 9, 3, 1 }, 0));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			collection = ToArray (ReadOnlyCollection.Skip (new int[] { 9, 3, 1 }, 2));
			Assert.Equal (1, collection.Count);
			Assert.Equal (1, collection[0]);

			var collection2 = ToArray (ReadOnlyCollection.Skip (new string[0], 3));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Skip (new string[] { "three", "two", "one" }, 0));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three", collection2[0]);
			Assert.Equal ("two", collection2[1]);
			Assert.Equal ("one", collection2[2]);
			collection2 = ToArray (ReadOnlyCollection.Skip (new string[] { "three", "two", "one" }, 2));
			Assert.Equal (1, collection2.Count);
			Assert.Equal ("one", collection2[0]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Take_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Take (new int[0], 3));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Take (new int[] { 9, 3, 1 }, 0));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Take (new int[] { 9, 3, 1 }, 5));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			collection = ToArray (ReadOnlyCollection.Take (new int[] { 9, 3, 1 }, 1));
			Assert.Equal (1, collection.Count);
			Assert.Equal (9, collection[0]);

			var collection2 = ToArray (ReadOnlyCollection.Take (new string[0], 3));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Take (new string[] { "three", "two", "one" }, 0));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Take (new string[] { "three", "two", "one" }, 4));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three", collection2[0]);
			Assert.Equal ("two", collection2[1]);
			Assert.Equal ("one", collection2[2]);
			collection2 = ToArray (ReadOnlyCollection.Take (new string[] { "three", "two", "one" }, 1));
			Assert.Equal (1, collection2.Count);
			Assert.Equal ("three", collection2[0]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Select_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Select (new int[0], item => item * 2));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Select (new int[] { 9, 3, 1 }, item => item + 3));
			Assert.Equal (3, collection.Count);
			Assert.Equal (12, collection[0]);
			Assert.Equal (6, collection[1]);
			Assert.Equal (4, collection[2]);

			var collection2 = ToArray (ReadOnlyCollection.Select (new string[0], item => item + "_"));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Select (new string[] { "three", "two", "one" }, item => item + "_"));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three_", collection2[0]);
			Assert.Equal ("two_", collection2[1]);
			Assert.Equal ("one_", collection2[2]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void SelectIdx ()
		{
			var collection = ToArray (ReadOnlyCollection.Select (new int[0], (item, idx) => item * idx));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Select (new int[] { 9, 3, 1 }, (item, idx) => item * idx));
			Assert.Equal (3, collection.Count);
			Assert.Equal (0, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (2, collection[2]);

			var collection2 = ToArray (ReadOnlyCollection.Select (new string[0], (item, idx) => item + idx.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Select (new string[] { "three", "two", "one" }, (item, idx) => item + idx.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three0", collection2[0]);
			Assert.Equal ("two1", collection2[1]);
			Assert.Equal ("one2", collection2[2]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Reverse_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Reverse (new int[0]));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Reverse (new int[] { 3 }));
			Assert.Equal (1, collection.Count);
			Assert.Equal (3, collection[0]);
			collection = ToArray (ReadOnlyCollection.Reverse (new int[] { 9, 3, 1 }));
			Assert.Equal (3, collection.Count);
			Assert.Equal (1, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (9, collection[2]);

			var collection2 = ToArray (ReadOnlyCollection.Reverse (new string[0]));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Reverse (new string[] { "two" }));
			Assert.Equal (1, collection2.Count);
			Assert.Equal ("two", collection2[0]);
			collection2 = ToArray (ReadOnlyCollection.Reverse (new string[] { "three", "two", "one" }));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("one", collection2[0]);
			Assert.Equal ("two", collection2[1]);
			Assert.Equal ("three", collection2[2]);
		}

		private class StringLenComparer : IComparer<string>
		{
			public int Compare (string x, string y)
			{
				if (x == null) return (y == null) ? 0 : -1;
				if (y == null) return 1;
				return x.Length.CompareTo (y.Length);
			}
		}
		[Fact, Trait ("Category", "Collections.Linq")]
		public void Ordering ()
		{
			var collection = ToArray (ReadOnlyCollection.OrderBy (new int[0], item => item * 3));
			Assert.Equal (0, collection.Count);
			var source = new int[] { 9, 1, -3 };
			collection = ToArray (ReadOnlyCollection.OrderBy (source, item => item));
			Assert.Equal (3, collection.Count);
			Assert.Equal (-3, collection[0]);
			Assert.Equal (1, collection[1]);
			Assert.Equal (9, collection[2]);
			collection = ToArray (ReadOnlyCollection.OrderBy (source, item => Math.Abs (item)));
			Assert.Equal (3, collection.Count);
			Assert.Equal (1, collection[0]);
			Assert.Equal (-3, collection[1]);
			Assert.Equal (9, collection[2]);

			collection = ToArray (ReadOnlyCollection.OrderByDescending (new int[0], item => item * 3));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.OrderByDescending (source, item => item));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (1, collection[1]);
			Assert.Equal (-3, collection[2]);
			collection = ToArray (ReadOnlyCollection.OrderByDescending (source, item => Math.Abs (item)));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (-3, collection[1]);
			Assert.Equal (1, collection[2]);

			var sizeComparer = new StringLenComparer ();
			var col = new string[] { "333", "222", "111", "11", "1111", null, "aaa", "a", "" };
			var collection2 = ToArray (ReadOnlyCollection.ThenBy (ReadOnlyCollection.OrderBy (col, item => item, sizeComparer), item => item, null));
			Assert.Equal (9, collection2.Count);
			Assert.Equal (null, collection2[0]);
			Assert.Equal ("", collection2[1]);
			Assert.Equal ("a", collection2[2]);
			Assert.Equal ("11", collection2[3]);
			Assert.Equal ("111", collection2[4]);
			Assert.Equal ("222", collection2[5]);
			Assert.Equal ("333", collection2[6]);
			Assert.Equal ("aaa", collection2[7]);
			Assert.Equal ("1111", collection2[8]);

			collection2 = ToArray (ReadOnlyCollection.ThenBy (ReadOnlyCollection.OrderByDescending (col, item => item, sizeComparer), item => item, null));
			Assert.Equal (9, collection2.Count);
			Assert.Equal ("1111", collection2[0]);
			Assert.Equal ("111", collection2[1]);
			Assert.Equal ("222", collection2[2]);
			Assert.Equal ("333", collection2[3]);
			Assert.Equal ("aaa", collection2[4]);
			Assert.Equal ("11", collection2[5]);
			Assert.Equal ("a", collection2[6]);
			Assert.Equal ("", collection2[7]);
			Assert.Equal (null, collection2[8]);

			collection2 = ToArray (ReadOnlyCollection.ThenByDescending (ReadOnlyCollection.OrderBy (col, item => item, sizeComparer), item => item, null));
			Assert.Equal (9, collection2.Count);
			Assert.Equal (null, collection2[0]);
			Assert.Equal ("", collection2[1]);
			Assert.Equal ("a", collection2[2]);
			Assert.Equal ("11", collection2[3]);
			Assert.Equal ("aaa", collection2[4]);
			Assert.Equal ("333", collection2[5]);
			Assert.Equal ("222", collection2[6]);
			Assert.Equal ("111", collection2[7]);
			Assert.Equal ("1111", collection2[8]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Concat_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Concat (new int[0], new int[0]));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Concat (new int[0], new int[] { 9, 3, 1 }));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			collection = ToArray (ReadOnlyCollection.Concat (new int[] { 9, 3, 1 }, new int[0]));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			collection = ToArray (ReadOnlyCollection.Concat (new int[] { 9, 3, 1 }, new int[] { 2, 4 }));
			Assert.Equal (5, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			Assert.Equal (2, collection[3]);
			Assert.Equal (4, collection[4]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Zip_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Zip (new int[0], new decimal[0], (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Zip (new int[] { 9, 3, 1 }, new decimal[0], (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Zip (new int[0], new decimal[] { 555.1m, 10m, 2.2m }, (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Zip (new int[] { 9, 3, 1, -2 }, new decimal[] { 555.1m, 10m, 2.2m }, (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (3, collection.Count);
			Assert.Equal ("9555.1", collection[0]);
			Assert.Equal ("310", collection[1]);
			Assert.Equal ("12.2", collection[2]);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void ToArray_ ()
		{
			var collection0 = ReadOnlyCollection.ToArray (new DateTime[0]);
			Assert.Equal (0, collection0.Length);
			var collection1 = ReadOnlyCollection.ToArray (new int[] { 9, 3, 1 });
			Assert.Equal (3, collection1.Length);
			Assert.Equal (9, collection1[0]);
			Assert.Equal (3, collection1[1]);
			Assert.Equal (1, collection1[2]);
			var collection2 = ReadOnlyCollection.ToArray (new string[] { "three", "two", "one", "zero" });
			Assert.Equal (4, collection2.Length);
			Assert.Equal ("three", collection2[0]);
			Assert.Equal ("two", collection2[1]);
			Assert.Equal ("one", collection2[2]);
			Assert.Equal ("zero", collection2[3]);
		}
	}
}
