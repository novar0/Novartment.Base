using System;
using System.Diagnostics;
using Novartment.Base.Collections;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class ArrayListTests
	{

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void Construction ()
		{
			// по умолчанию
			var list = new ArrayList<int> ();
			Assert.Equal (0, list.Count);

			// с указанием вместимости
			list = new ArrayList<int> (3);
			Assert.Equal (3, list.Array.Length);
			Assert.Equal (0, list.Count);

			// прямое использование массива
			var t1 = new int[] { 100, -200, 0, 1 };
			list = new ArrayList<int> (t1, 1, 2);
			Assert.Equal (t1, list.Array);
			Assert.Equal (2, list.Count);
			Assert.Equal (t1[1], list[0]);
			Assert.Equal (t1[2], list[1]);

			// проверяем что изменение исходных данных влияет и на копию и наоборот
			t1[2] = 222;
			Assert.Equal (t1[2], list[1]);
			list[0] = 333;
			Assert.Equal (t1[1], list[0]);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void ChangeCapacity ()
		{
			var list = new ArrayList<int> (6);
			Assert.Equal (6, list.Array.Length);
			Assert.Equal (0, list.Count);
			list.Add (1);
			list.TrimExcess ();
			Assert.Single (list.Array);
			list.EnsureCapacity (50);
			Assert.Equal (50, list.Array.Length);
			list.Clear ();
			Assert.Equal (50, list.Array.Length);
			list.TrimExcess ();
			Assert.Empty (list.Array);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void Slice ()
		{
			var list1 = new ArrayList<int> (new int[] { 5, 1, 3, -1 });
			var list2 = list1.Slice (0, 0);
			Assert.Empty (list2);
			list2 = list1.Slice (4, 0);
			Assert.Empty (list2);
			list2 = list1.Slice (0, 4);
			Assert.Equal (list1, list2);
			list2 = list1.Slice (2, 2);
			Assert.Equal (new int[] { 3, -1 }, list2);

			list1 = new ArrayList<int> (new int[] { 5, 1, 3, -1 }, 2, 3);
			list2 = list1.Slice (0, 3);
			Assert.Equal (list1, list2);
			list2 = list1.Slice (1, 2);
			Assert.Equal (new int[] { -1, 5 }, list2);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void AddTake ()
		{
			var t1 = new int[] { 1, 2, 2, 3 };
			var c1 = new ArrayList<int> (t1);
			var c2 = new ArrayList<int>
			{
				// Add
				4,
				1,
				2,
				2,
				-5,
			};
			Assert.Equal (5, c2.Count);
			Assert.Equal (-5, c2[4]);

			// set this[]
			c2[4] = 3;
			Assert.Equal (3, c2[4]);
			Assert.NotEqual<int> (c1, c2);

			// TryTakeFirst
			Assert.True (c2.TryTakeFirst (out int v1));
			Assert.Equal (4, c2.Count);
			Assert.Equal (4, v1);
			Assert.Equal<int> (c1, c2);
			int[] t2 = new int[c2.Count];
			c2.CopyTo (t2, 0);
			Assert.Equal<int> (t1, t2);

			// TryTakeLast
			Assert.True (c2.TryTakeLast (out v1));
			Assert.Equal (3, c2.Count);
			Assert.Equal (3, v1);

			// Add
			c2.Add (-5);
			Assert.Equal (4, c2.Count);
			Assert.Equal (1, c2[0]);
			Assert.Equal (2, c2[1]);
			Assert.Equal (2, c2[2]);
			Assert.Equal (-5, c2[3]);

			// Clear
			c2.Clear ();
			Assert.Equal (0, c2.Count);

			// TryPeekLast
			Assert.False (c2.TryPeekLast (out v1));
			Assert.Equal (0, v1);

			// TryTakeFirst
			Assert.False (c2.TryTakeFirst (out v1));
			Assert.Equal (0, v1);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void InsertRemove ()
		{
			// создаём такое состояние массива чтобы он переходил через край
			var c2 = new ArrayList<int> ();

			// Insert
			c2.Insert (0, -1);
			var c1 = new ArrayList<int> (new int[] { -1 });
			Assert.Equal<int> (c1, c2);
			c2.Insert (0, -2);
			c1 = new ArrayList<int> (new int[] { -2, -1 });
			Assert.Equal<int> (c1, c2);
			c2.Insert (1, -3);
			c1 = new ArrayList<int> (new int[] { -2, -3, -1 });
			Assert.Equal<int> (c1, c2);
			c2.Insert (3, -4);
			c1 = new ArrayList<int> (new int[] { -2, -3, -1, -4 });
			Assert.Equal<int> (c1, c2);

			// RemoveAt
			c2.RemoveAt (0);
			c1 = new ArrayList<int> (new int[] { -3, -1, -4 });
			Assert.Equal<int> (c1, c2);
			c2.RemoveAt (1);
			c1 = new ArrayList<int> (new int[] { -3, -4 });
			Assert.Equal<int> (c1, c2);
			c2.RemoveAt (1);
			c1 = new ArrayList<int> (new int[] { -3 });
			Assert.Equal<int> (c1, c2);

			// Insert
			c2.Insert (1, -5);
			c1 = new ArrayList<int> (new int[] { -3, -5 });
			Assert.Equal<int> (c1, c2);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void InsertRemoveRange ()
		{
			var c2 = new ArrayList<int> (new int[] { 8, 9, 1, 2 });
			Assert.Equal<int> (new int[] { 8, 9, 1, 2 }, c2);

			// InsertRange
			c2.InsertRange (0, 3);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2 }, c2);
			c2.InsertRange (7, 1);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2, 0 }, c2);
			c2.InsertRange (3, 12);
			Assert.Equal<int> (new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 9, 1, 2, 0 }, c2);

			// RemoveRange
			c2.RemoveRange (3, 12);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2, 0 }, c2);
			c2.RemoveRange (7, 1);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2 }, c2);
			c2.RemoveRange (0, 7);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void Defagment ()
		{
			// пустой
			var c2 = new ArrayList<int> (4)
			{
				-1,
				-2,
				-3,
			};
			c2.RemoveAt (0);
			c2.RemoveAt (0);
			c2.RemoveAt (0);
			Assert.Equal (4, c2.Array.Length);
			Assert.Equal (0, c2.Array[0]);
			Assert.Equal (0, c2.Array[1]);
			Assert.Equal (0, c2.Array[2]);
			Assert.Equal (0, c2.Array[3]);
			c2.Defragment ();
			Assert.Equal (0, c2.Offset);
			Assert.Equal (4, c2.Array.Length);
			Assert.Equal (0, c2.Array[0]);
			Assert.Equal (0, c2.Array[1]);
			Assert.Equal (0, c2.Array[2]);
			Assert.Equal (0, c2.Array[3]);

			// нет дырок, нет зацикливания
			c2 = new ArrayList<int> (4)
			{
				-1,
				-2,
				-3,
				-4,
			};
			Assert.Equal (4, c2.Array.Length);
			Assert.Equal (-1, c2.Array[0]);
			Assert.Equal (-2, c2.Array[1]);
			Assert.Equal (-3, c2.Array[2]);
			Assert.Equal (-4, c2.Array[3]);
			c2.Defragment ();
			Assert.Equal (0, c2.Offset);
			Assert.Equal (-1, c2.Array[0]);
			Assert.Equal (-2, c2.Array[1]);
			Assert.Equal (-3, c2.Array[2]);
			Assert.Equal (-4, c2.Array[3]);

			// дырка в начале
			c2 = new ArrayList<int> (4)
			{
				-1,
				-2,
				-3,
				-4,
			};
			c2.RemoveAt (0);
			Assert.Equal (4, c2.Array.Length);
			Assert.Equal (0, c2.Array[0]);
			Assert.Equal (-2, c2.Array[1]);
			Assert.Equal (-3, c2.Array[2]);
			Assert.Equal (-4, c2.Array[3]);
			c2.Defragment ();
			Assert.Equal (0, c2.Offset);
			Assert.Equal (-2, c2.Array[0]);
			Assert.Equal (-3, c2.Array[1]);
			Assert.Equal (-4, c2.Array[2]);

			// зацикливание
			c2 = new ArrayList<int> (4);
			c2.Insert (0, -1);
			c2.Insert (0, -2);
			c2.Insert (1, -3);
			c2.Insert (3, -4);
			Assert.Equal (4, c2.Array.Length);
			Assert.Equal (-1, c2.Array[0]);
			Assert.Equal (-4, c2.Array[1]);
			Assert.Equal (-2, c2.Array[2]);
			Assert.Equal (-3, c2.Array[3]);
			c2.Defragment ();
			Assert.Equal (0, c2.Offset);
			Assert.Equal (4, c2.Array.Length);
			Assert.Equal (-2, c2.Array[0]);
			Assert.Equal (-3, c2.Array[1]);
			Assert.Equal (-1, c2.Array[2]);
			Assert.Equal (-4, c2.Array[3]);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void SortDefault ()
		{
			var template = new int[] { 23456, 10, 100, 1, 0, -10, -200, };
			var list = new ArrayList<int> (template, 4, 5);
			Assert.Equal (5, list.Count);

			Assert.Equal (0, list[0]);
			Assert.Equal (-10, list[1]);
			Assert.Equal (-200, list[2]);
			Assert.Equal (23456, list[3]);
			Assert.Equal (10, list[4]);

			// перед сортировкой выгоднее пододвигать часть из начала массива
			list.Sort (null);
			Assert.Equal (-200, list[0]);
			Assert.Equal (-10, list[1]);
			Assert.Equal (0, list[2]);
			Assert.Equal (10, list[3]);
			Assert.Equal (23456, list[4]);

			template = new int[] { 23456, 10, 100, 1, 0, -10, -200, };
			list = new ArrayList<int> (template, 5, 5);
			Assert.Equal (5, list.Count);

			Assert.Equal (-10, list[0]);
			Assert.Equal (-200, list[1]);
			Assert.Equal (23456, list[2]);
			Assert.Equal (10, list[3]);
			Assert.Equal (100, list[4]);

			// перед сортировкой выгоднее пододвигать часть из конца массива
			list.Sort (null);

			Assert.Equal (-200, list[0]);
			Assert.Equal (-10, list[1]);
			Assert.Equal (10, list[2]);
			Assert.Equal (100, list[3]);
			Assert.Equal (23456, list[4]);
		}

		[Fact]
		[Trait ("Category", "Collections.ArrayList")]
		public void SortCustom ()
		{
			var template = new Mock4[]
			{
				new Mock4 { Number = 1, Prop1 = "aab", Prop2 = 91 },
				new Mock4 { Number = 2, Prop1 = "ZZZ", Prop2 = 110 },
				new Mock4 { Number = 3, Prop1 = "aaa", Prop2 = 444 },
				new Mock4 { Number = 4, Prop1 = "шшш", Prop2 = 273 },
			};
			var copy = new Mock4[template.Length];
			Array.Copy (template, copy, template.Length);
			var list = new ArrayList<Mock4> (copy);

			list.Sort (ComparerFactory.CreateFromPropertySelector<Mock4, string> (item => item.Prop1));
			Assert.Equal (template[2], list[0]);
			Assert.Equal (template[0], list[1]);
			Assert.Equal (template[1], list[2]);
			Assert.Equal (template[3], list[3]);

			list.Sort (ComparerFactory.CreateFromPropertySelector<Mock4, int> (item => item.Prop2, true));
			Assert.Equal (template[2], list[0]);
			Assert.Equal (template[3], list[1]);
			Assert.Equal (template[1], list[2]);
			Assert.Equal (template[0], list[3]);
		}

		[DebuggerDisplay ("#{Number} Prop1 = {Prop1} Prop2 = {Prop2}")]
		internal sealed class Mock4
		{
			public int Number { get; set; }

			public string Prop1 { get; set; }

			public int Prop2 { get; set; }
		}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.

	}
}
