using Xunit;

namespace Novartment.Base.Test
{
	public class ByteArrayExtensionsTests
	{
		[Fact]
		[Trait ("Category", "ByteArrayExtensions")]
		public void IndexOf ()
		{
			Assert.Equal (-1, ByteArrayExtensions.IndexOf (new byte[] { }, new byte[] { 0, 1, 2 }));
			Assert.Equal (-1, ByteArrayExtensions.IndexOf (new byte[] { 0, 1 }, new byte[] { 0, 1, 2 }));
			Assert.Equal (-1, ByteArrayExtensions.IndexOf (new byte[] { 0, 1, 3 }, new byte[] { 0, 1, 2 }));

			Assert.Equal (0, ByteArrayExtensions.IndexOf (new byte[] { 0, 1, 2 }, new byte[] { 0, 1, 2 }));
			Assert.Equal (1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2 }, new byte[] { 0, 1, 2 }));
			Assert.Equal (1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }));

			Assert.Equal (1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 0, 5));
			Assert.Equal (1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 0, 4));
			Assert.Equal (1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 1, 4));
			Assert.Equal (1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 1, 3));

			Assert.Equal (-1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 0, 3));
			Assert.Equal (-1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 1, 2));
			Assert.Equal (-1, ByteArrayExtensions.IndexOf (new byte[] { 0, 0, 1, 2, 3 }, new byte[] { 0, 1, 2 }, 2, 3));
		}
	}
}
