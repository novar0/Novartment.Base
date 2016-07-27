using System.Collections;
using System.Collections.Generic;
using Novartment.Base.Collections.Immutable;
using Xunit;

namespace Novartment.Base.Test
{
	public class ReadOnlyArrayTests
	{
		[Fact, Trait ("Category", "Collections.ReadOnlyArray")]
		public void Misc ()
		{
			var t1 = new int[] { 100, -200, 0, 1 };
			var t2 = new int[] { 100, -200, 0, 1 }; // same as t1

			// свойства
			var list = new ReadOnlyArray<int> (new int[0]);
			Assert.Equal (0, list.Count);
			list = new ReadOnlyArray<int> (t1, 2);
			Assert.Equal (2, list.Count);
			Assert.Equal (t1[0], list[0]);
			Assert.Equal (t1[1], list[1]);

			// перечислитель
			var en = list.GetEnumerator ();
			Assert.NotNull (en);
			Assert.True (en.MoveNext ());
			Assert.Equal (t1[0], en.Current);
			Assert.True (en.MoveNext ());
			Assert.Equal (t1[1], en.Current);
			Assert.False (en.MoveNext ());
			en.Dispose ();

			// копирование
			var copy = new int[] { 1, 2, 3, 4 };
			list.CopyTo (copy, 1);
			Assert.Equal (1, copy[0]);
			Assert.Equal (t1[0], copy[1]);
			Assert.Equal (t1[1], copy[2]);
			Assert.Equal (4, copy[3]);

			// компаратор
			list = new ReadOnlyArray<int> (t1);
			var list2 = new ReadOnlyArray<int> (t1);
			var list3 = new ReadOnlyArray<int> (t2);
			Assert.True (list.Equals (list2));
			Assert.False (list.Equals (list3));

			// структурный компаратор
			Assert.True (((IStructuralEquatable)list).Equals (list2, EqualityComparer<int>.Default));
			Assert.True (((IStructuralEquatable)list).Equals (list3, EqualityComparer<int>.Default));
			t2[2] = 123;
			Assert.False (((IStructuralEquatable)list).Equals (list3, EqualityComparer<int>.Default));
		}
	}
}
