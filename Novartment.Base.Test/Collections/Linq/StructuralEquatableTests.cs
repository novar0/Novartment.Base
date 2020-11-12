using System;
using Novartment.Base.Collections.Linq;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class StructuralEquatableTests
	{
		[Fact]
		[Trait ("Category", "Collections.Linq")]
		public void SequenceEqual ()
		{
			var a0 = Array.Empty<byte> ();
			var a1 = Array.Empty<byte> ();
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
