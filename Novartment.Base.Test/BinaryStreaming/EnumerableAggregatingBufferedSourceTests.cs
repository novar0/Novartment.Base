using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class EnumerableAggregatingBufferedSourceTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void RequestSkip ()
		{
			int srcBufSize = 20;
			var src1 = new BigBufferedSourceMock (3L, srcBufSize, FillFunction);
			var src2 = new BigBufferedSourceMock ((long)int.MaxValue + 56L, srcBufSize, FillFunction);
			src2.TryFastSkipAsync (54);
			var src3 = new BigBufferedSourceMock (24L, srcBufSize, FillFunction);
			src3.TryFastSkipAsync (20);
			var sources = new IBufferedSource[]
			{
				src1,
				src2,
				src3,
			};

			var buf = new byte[7];
			var src = new EnumerableAggregatingBufferedSource (buf, sources);
			Assert.False (src.IsExhausted);
			Assert.False (sources[0].IsEmpty ());
			Assert.False (sources[1].IsEmpty ());
			Assert.False (sources[2].IsEmpty ());

			var vTask = src.EnsureBufferAsync (6);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.False (src.IsExhausted);
			Assert.True (sources[0].IsEmpty ());
			Assert.False (sources[1].IsEmpty ());
			Assert.False (sources[2].IsEmpty ());
			Assert.Equal (FillFunction (0), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (FillFunction (54), src.BufferMemory.Span[src.Offset + 3]);
			Assert.Equal (FillFunction (55), src.BufferMemory.Span[src.Offset + 4]);
			Assert.Equal (FillFunction (56), src.BufferMemory.Span[src.Offset + 5]);
			src.SkipBuffer (6);

			Assert.Equal ((long)int.MaxValue, src.TryFastSkipAsync ((long)int.MaxValue).Result);
			Assert.False (src.IsExhausted);
			Assert.True (sources[0].IsEmpty ());
			Assert.True (sources[1].IsEmpty ());
			Assert.False (sources[2].IsEmpty ());

			vTask = src.EnsureBufferAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (sources[0].IsEmpty ());
			Assert.True (sources[1].IsEmpty ());
			Assert.True (sources[2].IsEmpty ());
			Assert.Equal (FillFunction (21), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (22), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (23), src.BufferMemory.Span[src.Offset + 2]);
			src.SkipBuffer (3);
			vTask = src.FillBufferAsync ();
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (src.IsExhausted);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
