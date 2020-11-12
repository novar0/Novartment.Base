using System;
using System.Globalization;
using Novartment.Base.Collections;
using Xunit;
using static System.Linq.Enumerable;

namespace Novartment.Base.Test
{
	public class AvlHashTreeSetTests
	{

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

		[Fact]
		[Trait ("Category", "Collections.Set")]
		public void AddRemoveContains ()
		{
			var set = new AvlHashTreeSet<MockStr2> ();
			var offset = 99;
			var vvv = Range (-offset, offset * 2).Select (n => new MockStr2 (n, 0.3d)).ToArray ();

			Assert.Equal (0, set.Count);

			set.Add (vvv[16 + offset]);
			set.Add (vvv[18 + offset]);
			set.Add (vvv[20 + offset]);
			set.Add (vvv[22 + offset]);
			set.Add (vvv[10 + offset]);
			set.Add (vvv[14 + offset]);
			set.Add (vvv[12 + offset]);

			Assert.Equal (7, set.Count);

			set.Add (vvv[22 + offset]);
			set.Add (vvv[20 + offset]);
			set.Add (vvv[18 + offset]);
			set.Add (vvv[16 + offset]);
			set.Add (vvv[14 + offset]);
			set.Add (vvv[12 + offset]);
			set.Add (vvv[10 + offset]);

			Assert.False (set.Contains (vvv[9 + offset]));
			Assert.False (set.Contains (vvv[11 + offset]));
			Assert.False (set.Contains (vvv[13 + offset]));
			Assert.False (set.Contains (vvv[15 + offset]));
			Assert.False (set.Contains (vvv[17 + offset]));
			Assert.False (set.Contains (vvv[19 + offset]));
			Assert.False (set.Contains (vvv[21 + offset]));
			Assert.False (set.Contains (vvv[23 + offset]));

			Assert.True (set.Contains (vvv[22 + offset]));
			Assert.True (set.Contains (vvv[20 + offset]));
			Assert.True (set.Contains (vvv[18 + offset]));
			Assert.True (set.Contains (vvv[16 + offset]));
			Assert.True (set.Contains (vvv[10 + offset]));
			Assert.True (set.Contains (vvv[14 + offset]));
			Assert.True (set.Contains (vvv[12 + offset]));

			set.Remove (vvv[9 + offset]);
			set.Remove (vvv[11 + offset]);
			set.Remove (vvv[13 + offset]);
			set.Remove (vvv[15 + offset]);
			set.Remove (vvv[17 + offset]);
			set.Remove (vvv[19 + offset]);
			set.Remove (vvv[21 + offset]);
			set.Remove (vvv[23 + offset]);
			set.Remove (vvv[90 + offset]);
			set.Remove (vvv[-90 + offset]);

			Assert.Equal (7, set.Count);

			set.Remove (vvv[22 + offset]);
			set.Remove (vvv[20 + offset]);
			set.Remove (vvv[18 + offset]);
			set.Remove (vvv[16 + offset]);
			set.Remove (vvv[14 + offset]);
			set.Remove (vvv[12 + offset]);
			set.Remove (vvv[10 + offset]);

			Assert.Equal (0, set.Count);

			set.Add (vvv[16 + offset]);
			set.Add (vvv[18 + offset]);
			set.Add (vvv[20 + offset]);
			set.Add (vvv[22 + offset]);
			set.Add (vvv[10 + offset]);
			set.Add (vvv[14 + offset]);
			set.Add (vvv[12 + offset]);

			Assert.Equal (7, set.Count);
		}

		[Fact]
		[Trait ("Category", "Collections.Set")]
		public void BalanceAndOrder ()
		{
			var set = new AvlHashTreeSet<MockStr2> ();
			var offset = 1999;

			// массив элементов возращающих один и тотже хэш (худший случай для хэш-множества)
			var vvv = Range (-offset, offset * 2).Select (n => new MockStr2 (n, 0.0d)).ToArray ();
			set.Add (vvv[-20 + offset]);
			set.Add (vvv[105 + offset]);
			set.Add (vvv[101 + offset]);
			set.Add (vvv[110 + offset]);

			// где то тут должен случиться перекос вправо внутреннего двоичного дерева
			set.Add (vvv[102 + offset]);
			set.Add (vvv[103 + offset]);
			set.Add (vvv[106 + offset]);
			set.Add (vvv[107 + offset]);
			set.Add (vvv[108 + offset]);
			set.Add (vvv[104 + offset]);
			set.Add (vvv[112 + offset]);
			set.Add (vvv[109 + offset]);
			set.Add (vvv[99 + offset]);

			// где то тут должен случиться перекос влево внутреннего двоичного дерева
			set.Add (vvv[-408 + offset]);
			set.Add (vvv[-407 + offset]);
			set.Add (vvv[-406 + offset]);
			set.Add (vvv[-405 + offset]);
			set.Add (vvv[-404 + offset]);
			Assert.Equal (18, set.Count);

			// если хэш всегда один и тотже, то элементы будут в порядке добавления
			var template = new int[] { -20, 105, 101, 110, 102, 103, 106, 107, 108, 104, 112, 109, 99, -408, -407, -406, -405, -404 };
			var value = set.Select (item => item.Value).ToArray ();
			Assert.Equal<int> (template, value);

			// массив элементов возращающих уникальный хэш (идеальный случай для хэш-множества)
			set = new AvlHashTreeSet<MockStr2> ();
			vvv = Range (-offset, offset * 2).Select (n => new MockStr2 (n, 1.0d)).ToArray ();
			set.Add (vvv[-20 + offset]);
			set.Add (vvv[105 + offset]);
			set.Add (vvv[101 + offset]);
			set.Add (vvv[110 + offset]);

			// где то тут должен случиться перекос вправо внутреннего двоичного дерева
			set.Add (vvv[102 + offset]);
			set.Add (vvv[103 + offset]);
			set.Add (vvv[106 + offset]);
			set.Add (vvv[107 + offset]);
			set.Add (vvv[108 + offset]);
			set.Add (vvv[104 + offset]);
			set.Add (vvv[112 + offset]);
			set.Add (vvv[109 + offset]);
			set.Add (vvv[99 + offset]);

			// где то тут должен случиться перекос влево внутреннего двоичного дерева
			set.Add (vvv[-408 + offset]);
			set.Add (vvv[-407 + offset]);
			set.Add (vvv[-406 + offset]);
			set.Add (vvv[-405 + offset]);
			set.Add (vvv[-404 + offset]);
			Assert.Equal (18, set.Count);

			// если хэш соответствует значению, то элементы будут в порядке возрастания значения
			template = new int[] { -408, -407, -406, -405, -404, -20, 99, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 112 };
			value = set.Select (item => item.Value).ToArray ();
			Assert.Equal<int> (template, value);
		}

		// Обёртка для int имеющая высокую вероятность коллизии хэша.
		internal class MockStr2 :
			IEquatable<MockStr2>
		{
			private readonly double _hashMod;

			public MockStr2 (int value, double hashMod)
			{
				this.Value = value;
				_hashMod = hashMod;
			}

			public int Value { get; }

			public override int GetHashCode ()
			{
				// высокая вероятность коллизии
				return (int)((double)this.Value * _hashMod);
			}

			public override string ToString ()
			{
				return string.Format (CultureInfo.InvariantCulture, "{0}, {1}", this.Value, GetHashCode ());
			}

			public bool Equals (MockStr2 other)
			{
				return ReferenceEquals (this, other) ||
					(other is not null && (this.Value == other.Value));
			}

			public override bool Equals (object obj)
			{
				return Equals (obj as MockStr2);
			}
		}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.

	}
}
