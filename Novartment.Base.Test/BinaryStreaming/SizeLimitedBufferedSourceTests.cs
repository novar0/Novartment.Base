using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class SizeLimitedBufferedSourceTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void RequestSkip ()
		{
			long skipBeforeLimitingSize = int.MaxValue;
			int skipBufferSize = 123;
			long skipInsideLimitingSize = 562945658454016;

			// ограничение больше буфера
			int srcBufSize = 32768;
			long limitingSize = srcBufSize + 4611686018427387904L;
			var subSrc = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			subSrc.TryFastSkipAsync (skipBeforeLimitingSize, CancellationToken.None).Wait ();

			var src = new SizeLimitedBufferedSource (subSrc, limitingSize);
			src.EnsureBufferAsync (skipBufferSize + 3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (skipBeforeLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			src.SkipBuffer (skipBufferSize);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (skipInsideLimitingSize, src.TryFastSkipAsync (skipInsideLimitingSize, CancellationToken.None).Result);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipInsideLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipInsideLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipInsideLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (limitingSize - skipBufferSize - skipInsideLimitingSize, src.TryFastSkipAsync (long.MaxValue, CancellationToken.None).Result);

			// ограничение меньше буфера
			srcBufSize = 32767;
			limitingSize = 1293L;
			subSrc = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			subSrc.TryFastSkipAsync (skipBeforeLimitingSize, CancellationToken.None).Wait ();

			src = new SizeLimitedBufferedSource (subSrc, limitingSize);
			src.EnsureBufferAsync (skipBufferSize + 3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (skipBeforeLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			src.SkipBuffer (skipBufferSize);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (skipBufferSize, src.TryFastSkipAsync (skipBufferSize, CancellationToken.None).Result);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (limitingSize - skipBufferSize - skipBufferSize, src.TryFastSkipAsync (long.MaxValue, CancellationToken.None).Result);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
