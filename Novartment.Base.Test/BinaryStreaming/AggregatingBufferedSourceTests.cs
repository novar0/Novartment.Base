using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class AggregatingBufferedSourceTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void RequestSkip ()
		{
			int srcBufSize = 20;
			var src1 = new BigBufferedSourceMock (3L, srcBufSize, FillFunction);
			var src2 = new BigBufferedSourceMock ((long)int.MaxValue + 56L, srcBufSize, FillFunction);
			src2.TryFastSkipAsync (54).Wait ();
			var src3 = new BigBufferedSourceMock (24L, srcBufSize, FillFunction);
			src3.TryFastSkipAsync (20).Wait ();
			var sources = new JobCompletionSource<IBufferedSource, int>[]
			{
				new JobCompletionSource<IBufferedSource, int> (src1),
				new JobCompletionSource<IBufferedSource, int> (src2),
				new JobCompletionSource<IBufferedSource, int> (src3),
				JobCompletionSourceMarker.Create<IBufferedSource, int> (),
			};
			var processingSrc = new ProcessingTaskProviderMock (sources);

			var buf = new byte[7];
			var src = new AggregatingBufferedSource (buf, processingSrc);
			Assert.False (src.IsExhausted);
			Assert.False (sources[0].Task.IsCompleted);
			Assert.False (sources[1].Task.IsCompleted);
			Assert.False (sources[2].Task.IsCompleted);

			src.EnsureBufferAsync (6).Wait ();
			Assert.False (src.IsExhausted);
			Assert.Equal (TaskStatus.RanToCompletion, sources[0].Task.Status);
			Assert.False (sources[1].Task.IsCompleted);
			Assert.False (sources[2].Task.IsCompleted);
			Assert.Equal (FillFunction (0), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (FillFunction (54), src.BufferMemory.Span[src.Offset + 3]);
			Assert.Equal (FillFunction (55), src.BufferMemory.Span[src.Offset + 4]);
			Assert.Equal (FillFunction (56), src.BufferMemory.Span[src.Offset + 5]);
			src.SkipBuffer (6);

			Assert.Equal ((long)int.MaxValue, src.TryFastSkipAsync ((long)int.MaxValue).Result);
			Assert.False (src.IsExhausted);
			Assert.Equal (TaskStatus.RanToCompletion, sources[0].Task.Status);
			Assert.Equal (TaskStatus.RanToCompletion, sources[1].Task.Status);
			Assert.False (sources[2].Task.IsCompleted);

			src.EnsureBufferAsync (3).Wait ();
			Assert.Equal (TaskStatus.RanToCompletion, sources[0].Task.Status);
			Assert.Equal (TaskStatus.RanToCompletion, sources[1].Task.Status);
			Assert.Equal (TaskStatus.RanToCompletion, sources[2].Task.Status);
			Assert.Equal (FillFunction (21), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (22), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (23), src.BufferMemory.Span[src.Offset + 2]);
			src.SkipBuffer (3);
			src.FillBufferAsync ().Wait ();
			Assert.True (src.IsExhausted);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}

		internal class ProcessingTaskProviderMock : IJobProvider<IBufferedSource, int>
		{
			private readonly JobCompletionSource<IBufferedSource, int>[] _sources;
			private int index = 0;

			internal ProcessingTaskProviderMock (JobCompletionSource<IBufferedSource, int>[] sources)
			{
				_sources = sources;
			}

			public Task<JobCompletionSource<IBufferedSource, int>> TakeJobAsync (CancellationToken cancellationToken = default)
			{
				return Task.FromResult (_sources[index++]);
			}
		}
	}
}
