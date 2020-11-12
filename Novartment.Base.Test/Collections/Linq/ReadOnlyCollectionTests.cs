using System;
using System.Collections.Generic;
using System.Globalization;
using Novartment.Base.Collections.Linq;
using Xunit;
using Enumerable = System.Linq.Enumerable;

namespace Novartment.Base.Test
{
	public sealed class ReadOnlyCollectionTests
	{
		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Any_ ()
		{
			Assert.False (ReadOnlyCollection.Any (Array.Empty<int> ()));
			Assert.True (ReadOnlyCollection.Any (new int[] { 3 }));
			Assert.True (ReadOnlyCollection.Any (new string[] { "one", "two", "three" }));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Count_ ()
		{
			Assert.Equal (0, ReadOnlyCollection.Count (Array.Empty<int> ()));
			Assert.Equal (1, ReadOnlyCollection.Count (new int[] { 3 }));
			Assert.Equal (3, ReadOnlyCollection.Count (new string[] { "one", "two", "three" }));
			Assert.Equal (0L, ReadOnlyCollection.LongCount (Array.Empty<int> ()));
			Assert.Equal (1L, ReadOnlyCollection.LongCount (new int[] { 3 }));
			Assert.Equal (3L, ReadOnlyCollection.LongCount (new string[] { "one", "two", "three" }));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void DefaultIfEmpty_ ()
		{
			var collection = ToArray (ReadOnlyCollection.DefaultIfEmpty (Array.Empty<int> (), 999));
			Assert.Equal (1, collection.Count);
			Assert.Equal (999, collection[0]);
			collection = ToArray (ReadOnlyCollection.DefaultIfEmpty (new int[] { 9, 3, 1 }, 999));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			var collection2 = ToArray (ReadOnlyCollection.DefaultIfEmpty (Array.Empty<string> (), "999"));
			Assert.Equal (1, collection2.Count);
			Assert.Equal ("999", collection2[0]);
			collection2 = ToArray (ReadOnlyCollection.DefaultIfEmpty (new string[] { "three", "two", "one" }, "999"));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three", collection2[0]);
			Assert.Equal ("two", collection2[1]);
			Assert.Equal ("one", collection2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Skip_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Skip (Array.Empty<int> (), 3));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Skip (new int[] { 9, 3, 1 }, 0));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			collection = ToArray (ReadOnlyCollection.Skip (new int[] { 9, 3, 1 }, 2));
			Assert.Equal (1, collection.Count);
			Assert.Equal (1, collection[0]);

			var collection2 = ToArray (ReadOnlyCollection.Skip (Array.Empty<string> (), 3));
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

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Take_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Take (Array.Empty<int> (), 3));
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

			var collection2 = ToArray (ReadOnlyCollection.Take (Array.Empty<string> (), 3));
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

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Select_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Select (Array.Empty<int> (), item => item * 2));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Select (new int[] { 9, 3, 1 }, item => item + 3));
			Assert.Equal (3, collection.Count);
			Assert.Equal (12, collection[0]);
			Assert.Equal (6, collection[1]);
			Assert.Equal (4, collection[2]);

			var collection2 = ToArray (ReadOnlyCollection.Select (Array.Empty<string> (), item => item + "_"));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Select (new string[] { "three", "two", "one" }, item => item + "_"));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three_", collection2[0]);
			Assert.Equal ("two_", collection2[1]);
			Assert.Equal ("one_", collection2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void SelectIdx ()
		{
			var collection = ToArray (ReadOnlyCollection.Select (Array.Empty<int> (), (item, idx) => item * idx));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Select (new int[] { 9, 3, 1 }, (item, idx) => item * idx));
			Assert.Equal (3, collection.Count);
			Assert.Equal (0, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (2, collection[2]);

			var collection2 = ToArray (ReadOnlyCollection.Select (Array.Empty<string> (), (item, idx) => item + idx.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection2.Count);
			collection2 = ToArray (ReadOnlyCollection.Select (new string[] { "three", "two", "one" }, (item, idx) => item + idx.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (3, collection2.Count);
			Assert.Equal ("three0", collection2[0]);
			Assert.Equal ("two1", collection2[1]);
			Assert.Equal ("one2", collection2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Reverse_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Reverse (Array.Empty<int> ()));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Reverse (new int[] { 3 }));
			Assert.Equal (1, collection.Count);
			Assert.Equal (3, collection[0]);
			collection = ToArray (ReadOnlyCollection.Reverse (new int[] { 9, 3, 1 }));
			Assert.Equal (3, collection.Count);
			Assert.Equal (1, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (9, collection[2]);

			var collection2 = ToArray (ReadOnlyCollection.Reverse (Array.Empty<string> ()));
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

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Ordering ()
		{
			var collection = ToArray (ReadOnlyCollection.OrderBy (Array.Empty<int> (), item => item * 3));
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

			collection = ToArray (ReadOnlyCollection.OrderByDescending (Array.Empty<int> (), item => item * 3));
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
			var col = new string[] { "333", "222", "111", "11", "1111", null, "aaa", "a", string.Empty };
			var collection2 = ToArray (ReadOnlyCollection.ThenBy (ReadOnlyCollection.OrderBy (col, item => item, sizeComparer), item => item, null));
			Assert.Equal (9, collection2.Count);
			Assert.Null (collection2[0]);
			Assert.Equal (string.Empty, collection2[1]);
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
			Assert.Equal (string.Empty, collection2[7]);
			Assert.Null (collection2[8]);

			collection2 = ToArray (ReadOnlyCollection.ThenByDescending (ReadOnlyCollection.OrderBy (col, item => item, sizeComparer), item => item, null));
			Assert.Equal (9, collection2.Count);
			Assert.Null (collection2[0]);
			Assert.Equal (string.Empty, collection2[1]);
			Assert.Equal ("a", collection2[2]);
			Assert.Equal ("11", collection2[3]);
			Assert.Equal ("aaa", collection2[4]);
			Assert.Equal ("333", collection2[5]);
			Assert.Equal ("222", collection2[6]);
			Assert.Equal ("111", collection2[7]);
			Assert.Equal ("1111", collection2[8]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Concat_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Concat (Array.Empty<int> (), Array.Empty<int> ()));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Concat (Array.Empty<int> (), new int[] { 9, 3, 1 }));
			Assert.Equal (3, collection.Count);
			Assert.Equal (9, collection[0]);
			Assert.Equal (3, collection[1]);
			Assert.Equal (1, collection[2]);
			collection = ToArray (ReadOnlyCollection.Concat (new int[] { 9, 3, 1 }, Array.Empty<int> ()));
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

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Zip_ ()
		{
			var collection = ToArray (ReadOnlyCollection.Zip (Array.Empty<int> (), Array.Empty<decimal> (), (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Zip (new int[] { 9, 3, 1 }, Array.Empty<decimal> (), (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Zip (Array.Empty<int> (), new decimal[] { 555.1m, 10m, 2.2m }, (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (0, collection.Count);
			collection = ToArray (ReadOnlyCollection.Zip (new int[] { 9, 3, 1, -2 }, new decimal[] { 555.1m, 10m, 2.2m }, (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture)));
			Assert.Equal (3, collection.Count);
			Assert.Equal ("9555.1", collection[0]);
			Assert.Equal ("310", collection[1]);
			Assert.Equal ("12.2", collection[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void ToArray_ ()
		{
			var collection0 = ReadOnlyCollection.ToArray (Array.Empty<DateTime> ());
			Assert.Empty (collection0);
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

		private static IReadOnlyList<T> ToArray<T> (IEnumerable<T> enumerable)
		{
			return Enumerable.ToArray (enumerable);
		}

		private sealed class StringLenComparer :
			IComparer<string>
		{
			public int Compare (string x, string y)
			{
				if (x == null)
				{
					return (y == null) ? 0 : -1;
				}

				if (y == null)
				{
					return 1;
				}

				return x.Length.CompareTo (y.Length);
			}
		}
	}
}
