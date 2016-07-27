using Novartment.Base.Collections.Immutable;
using Xunit;

namespace Novartment.Base.Test
{
	public class LoopedArraySegmentTests
	{
		[Fact, Trait ("Category", "Collections.LoopedArraySegment")]
		public void Misc ()
		{
			var template = new int[] { -408, -407, -406, -405, -404, -20, 99, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 112 };
			var copy = new int[template.Length];
			template.CopyTo (copy, 0);

			var seg = new LoopedArraySegment<int> (copy);
			for (int i = 0; i < template.Length; i++)
			{
				Assert.Equal (template[i], seg[i]);
			}
			Assert.Equal<int> (template, seg);

			seg = new LoopedArraySegment<int> (copy, 16, 3);
			Assert.Equal (template[16], seg[0]);
			Assert.Equal (template[17], seg[1]);
			Assert.Equal (template[0], seg[2]);
			Assert.Equal<int> (new int[] { template[16], template[17], template[0] }, seg);
		}
	}
}
