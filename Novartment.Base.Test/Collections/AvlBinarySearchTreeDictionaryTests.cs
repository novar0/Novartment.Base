using System;
using System.Collections.Generic;
using Novartment.Base.Collections.Immutable;
using Xunit;

namespace Novartment.Base.Test
{
	public class AvlBinarySearchTreeDictionaryTests
	{
		[Fact]
		[Trait ("Category", "Collections.AvlBinarySearchTreeDictionary")]
		public void OperationsOnValueKey ()
		{
			AvlBinarySearchTreeDictionaryNode<int, string> map = null;
			IComparer<int> comparer = Comparer<int>.Default;

			var enumerator = map.GetEnumerator ();
			Assert.NotNull (enumerator);
			Assert.False (enumerator.MoveNext ());
			enumerator.Dispose ();

			Assert.Equal (0, map.GetCount ());
			Assert.False (map.ContainsKey (10, comparer));
			Assert.False (map.TryGetItem (0, comparer, out _));

			map = map.RemoveKey (0, comparer);
			Assert.Equal (0, map.GetCount ());

			map = map.SetValue (10, "10", comparer);
			Assert.Equal (1, map.GetCount ());
			Assert.False (map.ContainsKey (11, comparer));
			Assert.True (map.ContainsKey (10, comparer));
			Assert.False (map.TryGetItem (11, comparer, out _));
			Assert.True (map.TryGetItem (10, comparer, out string value));
			Assert.Equal ("10", value);

			map = map.SetValue (-20, string.Empty, comparer);
			Assert.Equal (2, map.GetCount ());

			map = map.SetValue (-20, "-20", comparer);
			Assert.Equal (2, map.GetCount ());

			Assert.False (map.ContainsKey (11, comparer));
			Assert.False (map.TryGetItem (11, comparer, out _));
			Assert.True (map.ContainsKey (-20, comparer));
			Assert.True (map.TryGetItem (-20, comparer, out value));
			Assert.Equal ("-20", value);
			Assert.True (map.ContainsKey (10, comparer));
			Assert.True (map.TryGetItem (10, comparer, out value));
			Assert.Equal ("10", value);

			map = map.RemoveKey (1111, comparer);
			Assert.Equal (2, map.GetCount ());

			map = map.RemoveKey (10, comparer);
			Assert.Equal (1, map.GetCount ());
			Assert.False (map.ContainsKey (10, comparer));
			Assert.False (map.TryGetItem (10, comparer, out _));
			Assert.True (map.ContainsKey (-20, comparer));
			Assert.True (map.TryGetItem (-20, comparer, out value));
			Assert.Equal ("-20", value);

			map = map.SetValue (105, "105", comparer);
			Assert.Equal (2, map.GetCount ());
			map = map.SetValue (101, "101", comparer);
			Assert.Equal (3, map.GetCount ());
			map = map.SetValue (110, "110", comparer);
			Assert.Equal (4, map.GetCount ());

			// где то тут должен случиться перекос вправо внутреннего двоичного дерева
			map = map.SetValue (102, "102", comparer);
			Assert.Equal (5, map.GetCount ());
			map = map.SetValue (103, "103", comparer);
			map = map.SetValue (106, "106", comparer);
			map = map.SetValue (107, "107", comparer);
			map = map.SetValue (108, "108", comparer);
			map = map.SetValue (104, "104", comparer);
			map = map.SetValue (112, "112", comparer);
			map = map.SetValue (109, "109", comparer);
			map = map.SetValue (99, "99", comparer);
			Assert.Equal (13, map.GetCount ());

			// где то тут должен случиться перекос влево внутреннего двоичного дерева
			map = map.SetValue (-408, "-408", comparer);
			map = map.SetValue (-407, "-407", comparer);
			map = map.SetValue (-406, "-406", comparer);
			map = map.SetValue (-405, "-405", comparer);
			map = map.SetValue (-404, "-404", comparer);
			Assert.Equal (18, map.GetCount ());
			var templateKeys = new int[] { -408, -407, -406, -405, -404, -20, 99, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 112 };
			var templateValues = new string[] { "-408", "-407", "-406", "-405", "-404", "-20", "99", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "112" };
			var keys = new int[18];
			var values = new string[18];
			enumerator = map.GetEnumerator ();
			Assert.NotNull (enumerator);
			int idx = 0;
			while (enumerator.MoveNext ())
			{
				Assert.NotNull (enumerator.Current);
				keys[idx] = enumerator.Current.Key;
				values[idx] = enumerator.Current.Value;
				idx++;
			}

			enumerator.Dispose ();
			Assert.Equal (18, idx);
			Assert.Equal<int> (templateKeys, keys);
			Assert.Equal<string> (templateValues, values);
		}

		[Fact]
		[Trait ("Category", "Collections.AvlBinarySearchTreeDictionary")]
		public void OperationsOnReferenceKey ()
		{
			AvlBinarySearchTreeDictionaryNode<string, int> map = null;
			IComparer<string> comparer = StringComparer.Ordinal;

			var enumerator = map.GetEnumerator ();
			Assert.NotNull (enumerator);
			Assert.False (enumerator.MoveNext ());
			enumerator.Dispose ();

			Assert.Equal (0, map.GetCount ());
			Assert.False (map.ContainsKey ("10", comparer));
			Assert.False (map.TryGetItem ("0", comparer, out _));

			map = map.RemoveKey ("0", comparer);
			Assert.Equal (0, map.GetCount ());

			map = map.SetValue ("10", 10, comparer);
			Assert.Equal (1, map.GetCount ());
			Assert.False (map.ContainsKey ("11", comparer));
			Assert.True (map.ContainsKey ("10", comparer));
			Assert.False (map.TryGetItem ("11", comparer, out _));
			Assert.True (map.TryGetItem ("10", comparer, out int value));
			Assert.Equal (10, value);

			map = map.SetValue ("-20", -999, comparer);
			Assert.Equal (2, map.GetCount ());

			map = map.SetValue ("-20", -20, comparer);
			Assert.Equal (2, map.GetCount ());

			Assert.False (map.ContainsKey ("11", comparer));
			Assert.False (map.TryGetItem ("11", comparer, out _));
			Assert.True (map.ContainsKey ("-20", comparer));
			Assert.True (map.TryGetItem ("-20", comparer, out value));
			Assert.Equal (-20, value);
			Assert.True (map.ContainsKey ("10", comparer));
			Assert.True (map.TryGetItem ("10", comparer, out value));
			Assert.Equal (10, value);

			map = map.RemoveKey ("1111", comparer);
			Assert.Equal (2, map.GetCount ());

			map = map.RemoveKey ("10", comparer);
			Assert.Equal (1, map.GetCount ());
			Assert.False (map.ContainsKey ("10", comparer));
			Assert.False (map.TryGetItem ("10", comparer, out _));
			Assert.True (map.ContainsKey ("-20", comparer));
			Assert.True (map.TryGetItem ("-20", comparer, out value));
			Assert.Equal (-20, value);

			map = map.SetValue ("105", 105, comparer);
			Assert.Equal (2, map.GetCount ());
			map = map.SetValue ("101", 101, comparer);
			Assert.Equal (3, map.GetCount ());
			map = map.SetValue ("110", 110, comparer);
			Assert.Equal (4, map.GetCount ());

			// где то тут должен случиться перекос вправо внутреннего двоичного дерева
			map = map.SetValue ("102", 102, comparer);
			Assert.Equal (5, map.GetCount ());
			map = map.SetValue ("103", 103, comparer);
			map = map.SetValue ("106", 106, comparer);
			map = map.SetValue ("107", 107, comparer);
			map = map.SetValue ("108", 108, comparer);
			map = map.SetValue ("104", 104, comparer);
			map = map.SetValue ("112", 112, comparer);
			map = map.SetValue ("109", 109, comparer);
			map = map.SetValue ("99", 99, comparer);
			Assert.Equal (13, map.GetCount ());

			// где то тут должен случиться перекос влево внутреннего двоичного дерева
			map = map.SetValue ("-408", -408, comparer);
			map = map.SetValue ("-407", -407, comparer);
			map = map.SetValue ("-406", -406, comparer);
			map = map.SetValue ("-405", -405, comparer);
			map = map.SetValue ("-404", -404, comparer);
			Assert.Equal (18, map.GetCount ());
			var templateKeys = new string[] { "-20", "-404", "-405", "-406", "-407", "-408", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "112", "99" };
			var templateValues = new int[] { -20, -404, -405, -406, -407, -408, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 112, 99 };
			var keys = new string[18];
			var values = new int[18];
			enumerator = map.GetEnumerator ();
			Assert.NotNull (enumerator);
			int idx = 0;
			while (enumerator.MoveNext ())
			{
				Assert.NotNull (enumerator.Current);
				keys[idx] = enumerator.Current.Key;
				values[idx] = enumerator.Current.Value;
				idx++;
			}

			enumerator.Dispose ();
			Assert.Equal (18, idx);
			Assert.Equal<string> (templateKeys, keys);
			Assert.Equal<int> (templateValues, values);
		}

		[Fact]
		[Trait ("Category", "Collections.AvlBinarySearchTreeDictionary")]
		public void UsingDifferentComparers ()
		{
			AvlBinarySearchTreeDictionaryNode<string, int> map1 = null;
			AvlBinarySearchTreeDictionaryNode<string, int> map2 = null;
			IComparer<string> comparer1 = StringComparer.Ordinal;
			IComparer<string> comparer2 = StringComparer.OrdinalIgnoreCase;

			map1 = map1.SetValue ("aaa", 1, comparer1);
			map2 = map2.SetValue ("aaa", 1, comparer2);

			map1 = map1.SetValue ("BBB", 2, comparer1);
			map2 = map2.SetValue ("BBB", 2, comparer2);

			map1 = map1.SetValue ("CCC", 3, comparer1);
			map2 = map2.SetValue ("CCC", 3, comparer2);

			map1 = map1.SetValue ("ddd", 4, comparer1);
			map2 = map2.SetValue ("ddd", 4, comparer2);

			Assert.True (map1.ContainsKey ("aaa", comparer1));
			Assert.True (map2.ContainsKey ("aaa", comparer2));

			Assert.False (map1.ContainsKey ("AAA", comparer1));
			Assert.True (map2.ContainsKey ("AAA", comparer2));

			Assert.False (map1.ContainsKey ("bbb", comparer1));
			Assert.True (map2.ContainsKey ("bbb", comparer2));

			Assert.True (map1.ContainsKey ("BBB", comparer1));
			Assert.True (map2.ContainsKey ("BBB", comparer2));

			Assert.False (map1.ContainsKey ("ccc", comparer1));
			Assert.True (map2.ContainsKey ("ccc", comparer2));

			Assert.True (map1.ContainsKey ("CCC", comparer1));
			Assert.True (map2.ContainsKey ("CCC", comparer2));

			Assert.True (map1.ContainsKey ("ddd", comparer1));
			Assert.True (map2.ContainsKey ("ddd", comparer2));

			Assert.False (map1.ContainsKey ("DDD", comparer1));
			Assert.True (map2.ContainsKey ("DDD", comparer2));
		}
	}
}