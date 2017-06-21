using System.Diagnostics;
using Xunit;

namespace Novartment.Base.Test
{
	public class ComparerFactoryTests
	{
		[Fact]
		[Trait ("Category", "ComparerFactory")]
		public void UsePropertySelector ()
		{
			var item1 = new Mock5 { Number = 1, Prop1 = "aab", Prop2 = -91 };
			var item2 = new Mock5 { Number = 2, Prop1 = null, Prop2 = 110 };
			var item3 = new Mock5 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			var item4 = new Mock5 { Number = 4, Prop1 = null, Prop2 = 273 };

			var comparer = ComparerFactory.CreateFromPropertySelector<Mock5, int> (item => item.Prop2);

			Assert.Equal (0, comparer.Compare (null, null));
			Assert.Equal (1, comparer.Compare (item1, null));
			Assert.Equal (-1, comparer.Compare (null, item1));

			Assert.Equal (0, comparer.Compare (item1, item1));
			Assert.Equal (1, comparer.Compare (item3, item1));
			Assert.Equal (-1, comparer.Compare (item1, item3));

			(comparer as ISortDirectionVariable).DescendingOrder = true;

			Assert.Equal (0, comparer.Compare (null, null));
			Assert.Equal (-1, comparer.Compare (item1, null));
			Assert.Equal (1, comparer.Compare (null, item1));

			Assert.Equal (0, comparer.Compare (item1, item1));
			Assert.Equal (-1, comparer.Compare (item3, item1));
			Assert.Equal (1, comparer.Compare (item1, item3));
		}

		[Fact]
		[Trait ("Category", "ComparerFactory")]
		public void UsePropertyName ()
		{
			var item1 = new Mock5 { Number = 1, Prop1 = "aab", Prop2 = -91 };
			var item2 = new Mock5 { Number = 2, Prop1 = null, Prop2 = 110 };
			var item3 = new Mock5 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			var item4 = new Mock5 { Number = 4, Prop1 = null, Prop2 = 273 };
			var item5 = new Mock5 { Number = 5, Prop1 = "aab", Prop2 = 2222 };

			var comparer = ComparerFactory.CreateFromPropertyName<Mock5> ("Prop1");

			Assert.Equal (0, comparer.Compare (null, null));
			Assert.Equal (1, comparer.Compare (item1, null));
			Assert.Equal (-1, comparer.Compare (null, item1));

			Assert.Equal (0, comparer.Compare (item2, item4));
			Assert.Equal (1, comparer.Compare (item1, item2));
			Assert.Equal (-1, comparer.Compare (item4, item1));

			Assert.Equal (0, comparer.Compare (item1, item5));
			Assert.Equal (1, comparer.Compare (item1, item2));
			Assert.Equal (-1, comparer.Compare (item2, item3));

			(comparer as ISortDirectionVariable).DescendingOrder = true;

			Assert.Equal (0, comparer.Compare (null, null));
			Assert.Equal (-1, comparer.Compare (item1, null));
			Assert.Equal (1, comparer.Compare (null, item1));

			Assert.Equal (0, comparer.Compare (item2, item4));
			Assert.Equal (-1, comparer.Compare (item1, item2));
			Assert.Equal (1, comparer.Compare (item4, item1));

			Assert.Equal (0, comparer.Compare (item1, item5));
			Assert.Equal (-1, comparer.Compare (item1, item2));
			Assert.Equal (1, comparer.Compare (item2, item3));
		}

		[DebuggerDisplay ("#{Number} Prop1 = {Prop1} Prop2 = {Prop2}")]
		internal class Mock5
		{
			public int Number { get; set; }

			public string Prop1 { get; set; }

			public int Prop2 { get; set; }
		}
	}
}
