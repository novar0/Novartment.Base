using Novartment.Base.Collections;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class AvlTreeSetTests
	{

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

		[Fact]
		[Trait ("Category", "Collections.Set")]
		public void Misc ()
		{
			var set = new AvlTreeSet<int> ();

			Assert.Equal (0, set.Count);
			Assert.False (set.Contains (10));
			set.Remove (1111);

			set.Add (10);
			Assert.Equal (1, set.Count);

			Assert.False (set.Contains (11));
			Assert.True (set.Contains (10));

			set.Add (-20);
			Assert.Equal (2, set.Count);

			set.Add (10);
			Assert.Equal (2, set.Count);

			Assert.False (set.Contains (11));
			Assert.True (set.Contains (-20));
			Assert.True (set.Contains (10));

			set.Remove (1111);
			Assert.Equal (2, set.Count);

			set.Remove (10);
			Assert.Equal (1, set.Count);

			Assert.False (set.Contains (10));
			Assert.True (set.Contains (-20));

			set.Add (105);
			Assert.Equal (2, set.Count);
			set.Add (101);
			Assert.Equal (3, set.Count);
			set.Add (110);
			Assert.Equal (4, set.Count);

			// где то тут должен случиться перекос вправо внутреннего двоичного дерева
			set.Add (102);
			Assert.Equal (5, set.Count);
			set.Add (103);
			set.Add (106);
			set.Add (107);
			set.Add (108);
			set.Add (104);
			set.Add (112);
			set.Add (109);
			set.Add (99);
			Assert.Equal (13, set.Count);

			// где то тут должен случиться перекос влево внутреннего двоичного дерева
			set.Add (-408);
			set.Add (-407);
			set.Add (-406);
			set.Add (-405);
			set.Add (-404);
			Assert.Equal<int> (new int[] { -408, -407, -406, -405, -404, -20, 99, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 112 }, set);
		}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.

	}
}
