using Enumerable = System.Linq.Enumerable;
using System.Collections.Generic;
using Xunit;

namespace Novartment.Base.Collections.Linq.Test
{
	public class ReadOnlyFiniteSetTests
	{
		private static IReadOnlyList<T> ToArray<T> (IEnumerable<T> enumerable)
		{
			return Enumerable.ToArray (enumerable);
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Empty ()
		{
			var set = ReadOnlyFiniteSet.Empty<int> ();
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (0));
			Assert.False (set.Contains (1));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Range ()
		{
			var set = ReadOnlyFiniteSet.Range (0, 0);
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (0));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.Range (0, 1);
			Assert.Equal (1, set.Count);
			Assert.True (set.Contains (0));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.Range (-2, 5);
			Assert.Equal (5, set.Count);
			var list = ToArray (set);
			Assert.Equal (-2, list[0]);
			Assert.Equal (-1, list[1]);
			Assert.Equal (0, list[2]);
			Assert.Equal (1, list[3]);
			Assert.Equal (2, list[4]);
			Assert.True (set.Contains (-2));
			Assert.True (set.Contains (-1));
			Assert.True (set.Contains (0));
			Assert.True (set.Contains (1));
			Assert.True (set.Contains (2));
			Assert.False (set.Contains (-3));
			Assert.False (set.Contains (3));
		}

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
		[Fact, Trait ("Category", "Collections.Linq")]
		public void DefaultIfEmpty ()
		{
			var set = ReadOnlyFiniteSet.DefaultIfEmpty (new TestSet<int> (), 999);
			Assert.Equal (1, set.Count);
			Assert.True (set.Contains (999));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.DefaultIfEmpty (new TestSet<int> () { 9, 3, 1 }, 999);
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (2));
			var set2 = ReadOnlyFiniteSet.DefaultIfEmpty (new TestSet<string> (), "999");
			Assert.Equal (1, set2.Count);
			Assert.True (set2.Contains ("999"));
			Assert.False (set2.Contains (""));
			Assert.False (set2.Contains (null));
			set2 = ReadOnlyFiniteSet.DefaultIfEmpty (new TestSet<string> () { "three", "two", "one" }, "999");
			Assert.Equal (3, set2.Count);
			Assert.True (set2.Contains ("three"));
			Assert.True (set2.Contains ("two"));
			Assert.True (set2.Contains ("one"));
			Assert.False (set2.Contains (""));
			Assert.False (set2.Contains (null));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Contains ()
		{
			var set = new TestSet<int> ();
			Assert.False (ReadOnlyFiniteSet.Contains (set, 0));
			Assert.False (ReadOnlyFiniteSet.Contains (set, -1));
			set = new TestSet<int> () { 9, 3, 1 };
			Assert.True (ReadOnlyFiniteSet.Contains (set, 3));
			Assert.True (ReadOnlyFiniteSet.Contains (set, 1));
			Assert.True (ReadOnlyFiniteSet.Contains (set, 9));
			Assert.False (ReadOnlyFiniteSet.Contains (set, 0));
			Assert.False (ReadOnlyFiniteSet.Contains (set, -1));
			var set2 = new TestSet<string> () { "three", "two", "one" };
			Assert.True (ReadOnlyFiniteSet.Contains (set2, "three"));
			Assert.True (ReadOnlyFiniteSet.Contains (set2, "two"));
			Assert.True (ReadOnlyFiniteSet.Contains (set2, "one"));
			Assert.False (ReadOnlyFiniteSet.Contains (set2, ""));
			Assert.False (ReadOnlyFiniteSet.Contains (set2, null));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Reverse ()
		{
			var set = ReadOnlyFiniteSet.Reverse (new TestSet<int> ());
			Assert.Equal (0, set.Count);
			set = ReadOnlyFiniteSet.Reverse (new TestSet<int> () { 3 });
			Assert.Equal (1, set.Count);
			Assert.True (set.Contains (3));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.Reverse (new TestSet<int> () { 9, 3, 1 });
			var list = ToArray (set);
			Assert.Equal (3, list.Count);
			Assert.Equal (1, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (9, list[2]);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));

			var set2 = ReadOnlyFiniteSet.Reverse (new TestSet<string> ());
			Assert.Equal (0, set2.Count);
			set2 = ReadOnlyFiniteSet.Reverse (new TestSet<string> () { "two" });
			Assert.Equal (1, set2.Count);
			Assert.True (set2.Contains ("two"));
			Assert.False (set2.Contains (""));
			Assert.False (set2.Contains (null));
			set2 = ReadOnlyFiniteSet.Reverse (new TestSet<string> () { "three", "two", "one" });
			var list2 = ToArray (set2);
			Assert.Equal (3, list2.Count);
			Assert.Equal ("one", list2[0]);
			Assert.Equal ("two", list2[1]);
			Assert.Equal ("three", list2[2]);
			Assert.True (set2.Contains ("three"));
			Assert.True (set2.Contains ("two"));
			Assert.True (set2.Contains ("one"));
			Assert.False (set2.Contains (""));
			Assert.False (set2.Contains (null));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Distinct ()
		{
			var set = ReadOnlyFiniteSet.Distinct (new TestSet<int> ());
			Assert.Equal (0, set.Count);
			set = ReadOnlyFiniteSet.Distinct (new TestSet<int> () { 3 });
			Assert.Equal (1, set.Count);
			Assert.True (set.Contains (3));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.Distinct (new TestSet<int> () { 9, 3, 1 });
			var list = ToArray (set);
			Assert.Equal (3, list.Count);
			Assert.Equal (9, list[0]);
			Assert.Equal (3, list[1]);
			Assert.Equal (1, list[2]);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));

			var set2 = ReadOnlyFiniteSet.Distinct (new TestSet<string> ());
			Assert.Equal (0, set2.Count);
			set2 = ReadOnlyFiniteSet.Distinct (new TestSet<string> () { "two" });
			Assert.Equal (1, set2.Count);
			Assert.True (set2.Contains ("two"));
			Assert.False (set2.Contains (""));
			Assert.False (set2.Contains (null));
			set2 = ReadOnlyFiniteSet.Distinct (new TestSet<string> () { "three", "two", "one" });
			var list2 = ToArray (set2);
			Assert.Equal (3, list2.Count);
			Assert.Equal ("three", list2[0]);
			Assert.Equal ("two", list2[1]);
			Assert.Equal ("one", list2[2]);
			Assert.True (set2.Contains ("three"));
			Assert.True (set2.Contains ("two"));
			Assert.True (set2.Contains ("one"));
			Assert.False (set2.Contains (""));
			Assert.False (set2.Contains (null));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Except ()
		{
			var set = ReadOnlyFiniteSet.Except (new TestSet<int> (), new TestSet<int> () { 9, 3, 1 });
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (1));
			Assert.False (set.Contains (-1));
			set = ReadOnlyFiniteSet.Except (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> ());
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
			set = ReadOnlyFiniteSet.Except (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 8, 3, 0 });
			Assert.Equal (2, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (3));
			Assert.False (set.Contains (0));
			set = ReadOnlyFiniteSet.Except (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 1, 9, 3 });
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (1));
			Assert.False (set.Contains (3));
			Assert.False (set.Contains (9));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void SymmetricExcept ()
		{
			var set = ReadOnlyFiniteSet.SymmetricExcept (new TestSet<int> (), new TestSet<int> () { 9, 3, 1 });
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
			set = ReadOnlyFiniteSet.SymmetricExcept (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> ());
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
			set = ReadOnlyFiniteSet.SymmetricExcept (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 8, 3, 0 });
			Assert.Equal (4, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (1));
			Assert.True (set.Contains (8));
			Assert.True (set.Contains (0));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (3));
			set = ReadOnlyFiniteSet.SymmetricExcept (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 1, 9, 3 });
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (1));
			Assert.False (set.Contains (3));
			Assert.False (set.Contains (9));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Intersect ()
		{
			var set = ReadOnlyFiniteSet.Intersect (new TestSet<int> (), new TestSet<int> () { 9, 3, 1 });
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (9));
			Assert.False (set.Contains (3));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.Intersect (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> ());
			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (9));
			Assert.False (set.Contains (3));
			Assert.False (set.Contains (1));
			set = ReadOnlyFiniteSet.Intersect (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 8, 3, 0 });
			Assert.Equal (1, set.Count);
			Assert.True (set.Contains (3));
			Assert.False (set.Contains (9));
			Assert.False (set.Contains (1));
			Assert.False (set.Contains (8));
			Assert.False (set.Contains (0));
			Assert.False (set.Contains (-1));
			set = ReadOnlyFiniteSet.Intersect (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 1, 9, 3 });
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
		}

		[Fact, Trait ("Category", "Collections.Linq")]
		public void Union ()
		{
			var set = ReadOnlyFiniteSet.Union (new TestSet<int> (), new TestSet<int> () { 9, 3, 1 });
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
			set = ReadOnlyFiniteSet.Union (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> ());
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
			set = ReadOnlyFiniteSet.Union (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 8, 3, 0 });
			Assert.Equal (5, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.True (set.Contains (8));
			Assert.True (set.Contains (0));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (10));
			set = ReadOnlyFiniteSet.Union (new TestSet<int> () { 9, 3, 1 }, new TestSet<int> () { 1, 9, 3 });
			Assert.Equal (3, set.Count);
			Assert.True (set.Contains (9));
			Assert.True (set.Contains (3));
			Assert.True (set.Contains (1));
			Assert.False (set.Contains (-1));
			Assert.False (set.Contains (0));
		}
	}
}
