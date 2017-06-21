using Xunit;

namespace Novartment.Base.Collections.Linq.Test
{
	public class StructuralEquatableTests
	{
		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void SequenceEqual ()
		{
			var a0 = new byte[0];
			var a1 = new byte[0];
			var a2 = new byte[] { 1, 2, 3 };
			var a3 = new byte[] { 1, 3, 3 };
			var a4 = new byte[] { 1, 2, 3, 4 };
			Assert.True (StructuralEquatable.SequenceEqual (a0, a1));
			Assert.False (StructuralEquatable.SequenceEqual (a1, a2));
			Assert.False (StructuralEquatable.SequenceEqual (a2, a3));
			a3[1] = 2;
			Assert.True (StructuralEquatable.SequenceEqual (a2, a3));
			Assert.False (StructuralEquatable.SequenceEqual (a2, a4));
		}
	}
}
