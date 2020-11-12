using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class EnumerableAggregatingBufferedSourceTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void RequestSkip ()
		{
			int srcBufSize = 20;
			var src1 = new BigBufferedSourceMock (3L, srcBufSize, FillFunction);
			var src2 = new BigBufferedSourceMock ((long)int.MaxValue + 56L, srcBufSize, FillFunction);
			src2.SkipWihoutBufferingAsync (54).AsTask ().Wait ();
			var src3 = new BigBufferedSourceMock (24L, srcBufSize, FillFunction);
			src3.SkipWihoutBufferingAsync (20).AsTask ().Wait ();
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

			var vTask = src.EnsureAvailableAsync (6);
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
			src.Skip (6);

			Assert.Equal ((long)int.MaxValue, src.SkipWihoutBufferingAsync ((long)int.MaxValue).AsTask ().Result);
			Assert.False (src.IsExhausted);
			Assert.True (sources[0].IsEmpty ());
			Assert.True (sources[1].IsEmpty ());
			Assert.False (sources[2].IsEmpty ());

			vTask = src.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (sources[0].IsEmpty ());
			Assert.True (sources[1].IsEmpty ());
			Assert.True (sources[2].IsEmpty ());
			Assert.Equal (FillFunction (21), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (22), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (23), src.BufferMemory.Span[src.Offset + 2]);
			src.Skip (3);
			vTask = src.LoadAsync ();
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (src.IsExhausted);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
