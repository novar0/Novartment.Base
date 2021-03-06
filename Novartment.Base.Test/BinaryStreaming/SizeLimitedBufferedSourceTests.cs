using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class SizeLimitedBufferedSourceTests
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
			subSrc.SkipWihoutBufferingAsync (skipBeforeLimitingSize).AsTask ().Wait ();

			var src = new SizeLimitedBufferedSource (subSrc, limitingSize);
			var vTask = src.EnsureAvailableAsync (skipBufferSize + 3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (skipBeforeLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			src.Skip (skipBufferSize);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (skipInsideLimitingSize, src.SkipWihoutBufferingAsync (skipInsideLimitingSize).AsTask ().Result);
			vTask = src.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipInsideLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipInsideLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipInsideLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (limitingSize - skipBufferSize - skipInsideLimitingSize, src.SkipWihoutBufferingAsync (long.MaxValue).AsTask ().Result);

			// ограничение меньше буфера
			srcBufSize = 32767;
			limitingSize = 1293L;
			subSrc = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			subSrc.SkipWihoutBufferingAsync (skipBeforeLimitingSize).AsTask ().Wait ();

			src = new SizeLimitedBufferedSource (subSrc, limitingSize);
			vTask = src.EnsureAvailableAsync (skipBufferSize + 3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (skipBeforeLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			src.Skip (skipBufferSize);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (skipBufferSize, src.SkipWihoutBufferingAsync (skipBufferSize).AsTask ().Result);
			vTask = src.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (limitingSize - skipBufferSize - skipBufferSize, src.SkipWihoutBufferingAsync (long.MaxValue).AsTask ().Result);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
