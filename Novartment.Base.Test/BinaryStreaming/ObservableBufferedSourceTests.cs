using System;
using System.Collections.Generic;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class ObservableBufferedSourceTests
	{
		internal class ProgressHistory :
			IProgress<long>
		{
			public List<long> History { get; } = new List<long> ();

			public int CompletedCount { get; private set; } = 0;

			public void Report (long value) => this.History.Add (value);

			public void OnCompleted () => CompletedCount++;
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Empty ()
		{
			var subSrc = new BigBufferedSourceMock (10, 10, pos => (byte)(0xAA ^ (pos & 0xFF)));
			subSrc.FillBufferAsync ();
			subSrc.SkipBuffer (10);
			var monitor = new ProgressHistory ();
			var src = new ObservableBufferedSource (subSrc, monitor, monitor.OnCompleted);

			Assert.Empty (monitor.History);
			Assert.Equal (1, monitor.CompletedCount);

			src.TryFastSkipAsync (10);
			Assert.Empty (monitor.History);
			Assert.Equal (1, monitor.CompletedCount);
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Small ()
		{
			var subSrc = new BigBufferedSourceMock (10, 1000, pos => (byte)(0xAA ^ (pos & 0xFF)));
			var monitor = new ProgressHistory ();
			var src = new ObservableBufferedSource (subSrc, monitor, monitor.OnCompleted);

			src.FillBufferAsync ();
			Assert.Empty (monitor.History);
			Assert.Equal (0, monitor.CompletedCount);

			src.SkipBuffer (2);
			Assert.Equal (new long[] { 2 }, monitor.History);
			Assert.Equal (0, monitor.CompletedCount);

			src.SkipBuffer (8);
			Assert.Equal (new long[] { 2, 8 }, monitor.History);
			Assert.Equal (1, monitor.CompletedCount);

			src.TryFastSkipAsync (int.MaxValue);
			Assert.Equal (new long[] { 2, 8 }, monitor.History);
			Assert.Equal (1, monitor.CompletedCount);

			src.TryFastSkipAsync (1);
			Assert.Equal (new long[] { 2, 8 }, monitor.History);
			Assert.Equal (1, monitor.CompletedCount);
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Large ()
		{
			var subSrc = new BigBufferedSourceMock (100, 10, pos => (byte)(0xAA ^ (pos & 0xFF)));
			var monitor = new ProgressHistory ();
			var src = new ObservableBufferedSource (subSrc, monitor, monitor.OnCompleted);

			src.FillBufferAsync ();
			Assert.Empty (monitor.History);
			Assert.Equal (0, monitor.CompletedCount);

			src.SkipBuffer (2);
			Assert.Equal (new long[] { 2 }, monitor.History);
			Assert.Equal (0, monitor.CompletedCount);

			src.TryFastSkipAsync (int.MaxValue);
			Assert.Equal (new long[] { 2, 98 }, monitor.History);
			Assert.Equal (1, monitor.CompletedCount);

			src.TryFastSkipAsync (1);
			Assert.Equal (new long[] { 2, 98 }, monitor.History);
			Assert.Equal (1, monitor.CompletedCount);
		}
	}
}
