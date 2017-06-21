using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Novartment.Base.Collections.Linq.Test
{
	public class ReadOnlyListTests
	{
		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Empty ()
		{
			var list = ReadOnlyList.Empty<int> ();
			Assert.Equal (0, list.Count);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Range ()
		{
			var list = ReadOnlyList.Range (0, 0);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Range (0, 1);
			Assert.Equal (1, list.Count);
			Assert.Equal (0, list[0]);
			list = ReadOnlyList.Range (-2, 5);
			Assert.Equal (5, list.Count);
			Assert.Equal (-2, list[0]);
			Assert.Equal (-1, list[1]);
			Assert.Equal (0, list[2]);
			Assert.Equal (1, list[3]);
			Assert.Equal (2, list[4]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Repeat ()
		{
			var list = ReadOnlyList.Repeat<int> (0, 0);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Repeat<int> (1, 1);
			Assert.Equal (1, list.Count);
			Assert.Equal (1, list[0]);
			string tmpl = "one";
			var list2 = ReadOnlyList.Repeat<string> (tmpl, 5);
			Assert.Equal (5, list2.Count);
			Assert.Equal (tmpl, list2[0]);
			Assert.Equal (tmpl, list2[1]);
			Assert.Equal (tmpl, list2[2]);
			Assert.Equal (tmpl, list2[3]);
			Assert.Equal (tmpl, list2[4]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void First ()
		{
			var list = new int[] { 3 };
			Assert.Equal (3, ReadOnlyList.First (list));
			list = new int[] { 9, 3, 1 };
			Assert.Equal (9, ReadOnlyList.First (list));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void FirstOrDefault ()
		{
			var list = new int[] { };
			Assert.Equal (0, ReadOnlyList.FirstOrDefault (list));
			list = new int[] { 3 };
			Assert.Equal (3, ReadOnlyList.FirstOrDefault (list));
			list = new int[] { 9, 3, 1 };
			Assert.Equal (9, ReadOnlyList.FirstOrDefault (list));
			var list2 = new string[] { };
			Assert.Equal ((string)null, ReadOnlyList.FirstOrDefault (list2));
			list2 = new string[] { "one" };
			Assert.Equal ("one", ReadOnlyList.FirstOrDefault (list2));
			list2 = new string[] { "three", "two", "one" };
			Assert.Equal ("three", ReadOnlyList.FirstOrDefault (list2));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Last ()
		{
			var list = new int[] { 3 };
			Assert.Equal (3, ReadOnlyList.Last (list));
			list = new int[] { 9, 3, 1 };
			Assert.Equal (1, ReadOnlyList.Last (list));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void LastOrDefault ()
		{
			var list = new int[] { };
			Assert.Equal (0, ReadOnlyList.LastOrDefault (list));
			list = new int[] { 3 };
			Assert.Equal (3, ReadOnlyList.LastOrDefault (list));
			list = new int[] { 9, 3, 1 };
			Assert.Equal (1, ReadOnlyList.LastOrDefault (list));
			var list2 = new string[] { };
			Assert.Equal ((string)null, ReadOnlyList.LastOrDefault (list2));
			list2 = new string[] { "one" };
			Assert.Equal ("one", ReadOnlyList.LastOrDefault (list2));
			list2 = new string[] { "three", "two", "one" };
			Assert.Equal ("one", ReadOnlyList.LastOrDefault (list2));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void ElementAt ()
		{
			var list = new int[] { 3 };
			Assert.Equal (3, ReadOnlyList.ElementAt (list, 0));
			list = new int[] { 9, 3, 1 };
			Assert.Equal (3, ReadOnlyList.ElementAt (list, 1));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void ElementAtOrDefault ()
		{
			var list = new int[0];
			Assert.Equal (0, ReadOnlyList.ElementAtOrDefault (list, 3));
			list = new int[] { 3 };
			Assert.Equal (3, ReadOnlyList.ElementAtOrDefault (list, 0));
			Assert.Equal (0, ReadOnlyList.ElementAtOrDefault (list, 1));
			list = new int[] { 9, 3, 1 };
			Assert.Equal (1, ReadOnlyList.ElementAtOrDefault (list, 2));
			var list2 = new string[] { };
			Assert.Equal ((string)null, ReadOnlyList.ElementAtOrDefault (list2, 0));
			list2 = new string[] { "one" };
			Assert.Equal ("one", ReadOnlyList.ElementAtOrDefault (list2, 0));
			list2 = new string[] { "three", "two", "one" };
			Assert.Equal ("two", ReadOnlyList.ElementAtOrDefault (list2, 1));
			Assert.Equal ((string)null, ReadOnlyList.ElementAtOrDefault (list2, 3));
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void DefaultIfEmpty ()
		{
			var list = ReadOnlyList.DefaultIfEmpty (new int[0], 999);
			Assert.Equal (1, list.Count);
			Assert.Equal (999, list[0]);
			list = ReadOnlyList.DefaultIfEmpty (new int[] { 9, 3, 1 }, 999);
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			var list2 = ReadOnlyList.DefaultIfEmpty (new string[0], "999");
			Assert.Equal (1, list2.Count);
			Assert.Equal ("999", list2[0]);
			list2 = ReadOnlyList.DefaultIfEmpty (new string[] { "three", "two", "one" }, "999");
			Assert.Equal (3, list2.Count);
			Assert.Equal ("three", list2[0]);
			Assert.Equal ("two", list2[1]);
			Assert.Equal ("one", list2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Skip ()
		{
			var list = ReadOnlyList.Skip (new int[0], 3);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Skip (new int[] { 9, 3, 1 }, 0);
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			list = ReadOnlyList.Skip (new int[] { 9, 3, 1 }, 2);
			Assert.Equal (1, list.Count);
			Assert.Equal (1, list[0]);

			var list2 = ReadOnlyList.Skip (new string[0], 3);
			Assert.Equal (0, list2.Count);
			list2 = ReadOnlyList.Skip (new string[] { "three", "two", "one" }, 0);
			Assert.Equal (3, list2.Count);
			Assert.Equal ("three", list2[0]);
			Assert.Equal ("two", list2[1]);
			Assert.Equal ("one", list2[2]);
			list2 = ReadOnlyList.Skip (new string[] { "three", "two", "one" }, 2);
			Assert.Equal (1, list2.Count);
			Assert.Equal ("one", list2[0]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Take ()
		{
			var list = ReadOnlyList.Take (new int[0], 3);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Take (new int[] { 9, 3, 1 }, 0);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Take (new int[] { 9, 3, 1 }, 5);
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			list = ReadOnlyList.Take (new int[] { 9, 3, 1 }, 1);
			Assert.Equal (1, list.Count);
			Assert.Equal (9, list[0]);

			var list2 = ReadOnlyList.Take (new string[0], 3);
			Assert.Equal (0, list2.Count);
			list2 = ReadOnlyList.Take (new string[] { "three", "two", "one" }, 0);
			Assert.Equal (0, list2.Count);
			list2 = ReadOnlyList.Take (new string[] { "three", "two", "one" }, 4);
			Assert.Equal (3, list2.Count);
			Assert.Equal ("three", list2[0]);
			Assert.Equal ("two", list2[1]);
			Assert.Equal ("one", list2[2]);
			list2 = ReadOnlyList.Take (new string[] { "three", "two", "one" }, 1);
			Assert.Equal (1, list2.Count);
			Assert.Equal ("three", list2[0]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Select ()
		{
			var list = ReadOnlyList.Select (new int[0], item => item * 2);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Select (new int[] { 9, 3, 1 }, item => item + 3);
			Assert.Equal (3, list.Count);
			Assert.Equal (12, list[0]);
			Assert.Equal (6, list[1]);
			Assert.Equal (4, list[2]);

			var list2 = ReadOnlyList.Select (new string[0], item => item + "_");
			Assert.Equal (0, list2.Count);
			list2 = ReadOnlyList.Select (new string[] { "three", "two", "one" }, item => item + "_");
			Assert.Equal (3, list2.Count);
			Assert.Equal ("three_", list2[0]);
			Assert.Equal ("two_", list2[1]);
			Assert.Equal ("one_", list2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void SelectIdx ()
		{
			var list = ReadOnlyList.Select (new int[0], (item, idx) => item * idx);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Select (new int[] { 9, 3, 1 }, (item, idx) => item * idx);
			Assert.Equal (3, list.Count);
			Assert.Equal (0, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (2, list[2]);

			var list2 = ReadOnlyList.Select (new string[0], (item, idx) => item + idx.ToString (CultureInfo.InvariantCulture));
			Assert.Equal (0, list2.Count);
			list2 = ReadOnlyList.Select (new string[] { "three", "two", "one" }, (item, idx) => item + idx.ToString (CultureInfo.InvariantCulture));
			Assert.Equal (3, list2.Count);
			Assert.Equal ("three0", list2[0]);
			Assert.Equal ("two1", list2[1]);
			Assert.Equal ("one2", list2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Reverse ()
		{
			var list = ReadOnlyList.Reverse (new int[0]);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Reverse (new int[] { 3 });
			Assert.Equal (1, list.Count);
			Assert.Equal (3, list[0]);
			list = ReadOnlyList.Reverse (new int[] { 9, 3, 1 });
			Assert.Equal (3, list.Count);
			Assert.Equal (1, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (9, list[2]);

			var list2 = ReadOnlyList.Reverse (new string[0]);
			Assert.Equal (0, list2.Count);
			list2 = ReadOnlyList.Reverse (new string[] { "two" });
			Assert.Equal (1, list2.Count);
			Assert.Equal ("two", list2[0]);
			list2 = ReadOnlyList.Reverse (new string[] { "three", "two", "one" });
			Assert.Equal (3, list2.Count);
			Assert.Equal ("one", list2[0]);
			Assert.Equal ("two", list2[1]);
			Assert.Equal ("three", list2[2]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Ordering ()
		{
			var list = ReadOnlyList.OrderBy (new int[0], item => item * 3);
			Assert.Equal (0, list.Count);
			var source = new int[] { 9, 1, -3 };
			list = ReadOnlyList.OrderBy (source, item => item);
			Assert.Equal (3, list.Count);
			Assert.Equal (-3, list[0]);
			Assert.Equal (1, list[1]);
			Assert.Equal (9, list[2]);
			list = ReadOnlyList.OrderBy (source, item => Math.Abs (item));
			Assert.Equal (3, list.Count);
			Assert.Equal (1, list[0]);
			Assert.Equal (-3, list[1]);
			Assert.Equal (9, list[2]);

			list = ReadOnlyList.OrderByDescending (new int[0], item => item * 3);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.OrderByDescending (source, item => item);
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (1, list[1]);
			Assert.Equal (-3, list[2]);
			list = ReadOnlyList.OrderByDescending (source, item => Math.Abs (item));
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (-3, list[1]);
			Assert.Equal (1, list[2]);

			var sizeComparer = new StringLenComparer ();
			var src = new string[] { "333", "222", "111", "11", "1111", null, "aaa", "a", string.Empty };
			var list2 = ReadOnlyList.ThenBy (ReadOnlyList.OrderBy (src, item => item, sizeComparer), item => item, null);
			Assert.Equal (9, list2.Count);
			Assert.Equal (null, list2[0]);
			Assert.Equal (string.Empty, list2[1]);
			Assert.Equal ("a", list2[2]);
			Assert.Equal ("11", list2[3]);
			Assert.Equal ("111", list2[4]);
			Assert.Equal ("222", list2[5]);
			Assert.Equal ("333", list2[6]);
			Assert.Equal ("aaa", list2[7]);
			Assert.Equal ("1111", list2[8]);

			list2 = ReadOnlyList.ThenBy (ReadOnlyList.OrderByDescending (src, item => item, sizeComparer), item => item, null);
			Assert.Equal (9, list2.Count);
			Assert.Equal ("1111", list2[0]);
			Assert.Equal ("111", list2[1]);
			Assert.Equal ("222", list2[2]);
			Assert.Equal ("333", list2[3]);
			Assert.Equal ("aaa", list2[4]);
			Assert.Equal ("11", list2[5]);
			Assert.Equal ("a", list2[6]);
			Assert.Equal (string.Empty, list2[7]);
			Assert.Equal (null, list2[8]);

			list2 = ReadOnlyList.ThenByDescending (ReadOnlyList.OrderBy (src, item => item, sizeComparer), item => item, null);
			Assert.Equal (9, list2.Count);
			Assert.Equal (null, list2[0]);
			Assert.Equal (string.Empty, list2[1]);
			Assert.Equal ("a", list2[2]);
			Assert.Equal ("11", list2[3]);
			Assert.Equal ("aaa", list2[4]);
			Assert.Equal ("333", list2[5]);
			Assert.Equal ("222", list2[6]);
			Assert.Equal ("111", list2[7]);
			Assert.Equal ("1111", list2[8]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Concat ()
		{
			var list = ReadOnlyList.Concat (new int[0], new int[0]);
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Concat (new int[0], new int[] { 9, 3, 1 });
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			list = ReadOnlyList.Concat (new int[] { 9, 3, 1 }, new int[0]);
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			list = ReadOnlyList.Concat (new int[] { 9, 3, 1 }, new int[] { 2, 4 });
			Assert.Equal (5, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			Assert.Equal (2, list[3]);
			Assert.Equal (4, list[4]);
		}

		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void Zip ()
		{
			var list = ReadOnlyList.Zip (new int[0], new decimal[0], (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture));
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Zip (new int[] { 9, 3, 1 }, new decimal[0], (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture));
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Zip (new int[0], new decimal[] { 555.1m, 10m, 2.2m }, (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture));
			Assert.Equal (0, list.Count);
			list = ReadOnlyList.Zip (new int[] { 9, 3, 1, -2 }, new decimal[] { 555.1m, 10m, 2.2m }, (i1, i2) => i1.ToString (CultureInfo.InvariantCulture) + i2.ToString (CultureInfo.InvariantCulture));
			Assert.Equal (3, list.Count);
			Assert.Equal ("9555.1", list[0]);
			Assert.Equal ("310", list[1]);
			Assert.Equal ("12.2", list[2]);
		}

		private class StringLenComparer : IComparer<string>
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
