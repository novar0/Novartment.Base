using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class BufferedSourceExtensionsTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void IsEmpty ()
		{
			var src = new BigBufferedSourceMock (0, 1, FillFunction);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.True (BufferedSourceExtensions.IsEmpty (src));

			src = new BigBufferedSourceMock (1, 1, FillFunction);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.False (BufferedSourceExtensions.IsEmpty (src));
			src.SkipBuffer (1);
			Assert.True (BufferedSourceExtensions.IsEmpty (src));

			src = new BigBufferedSourceMock (long.MaxValue, 32768, FillFunction);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.False (BufferedSourceExtensions.IsEmpty (src));
			src.TryFastSkipAsync (long.MaxValue - 1, CancellationToken.None).Wait ();
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.False (BufferedSourceExtensions.IsEmpty (src));
			src.SkipBuffer (1);
			Assert.True (BufferedSourceExtensions.IsEmpty (src));
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void IndexOf ()
		{
			int bufSize = 120;
			int skipSize = 91;
			var src = new BigBufferedSourceMock (long.MaxValue, bufSize, FillFunction);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.Equal (0, BufferedSourceExtensions.IndexOfAsync (src, FillFunction(0), CancellationToken.None).Result);
			Assert.Equal (1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (1), CancellationToken.None).Result);
			Assert.Equal (bufSize - 1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize - 1), CancellationToken.None).Result);
			Assert.Equal (-1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize), CancellationToken.None).Result);
			Assert.Equal (-1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize + skipSize - 1), CancellationToken.None).Result);
			Assert.Equal (-1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize + skipSize), CancellationToken.None).Result);
			src.SkipBuffer (skipSize);
			Assert.Equal (-1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (0), CancellationToken.None).Result);
			Assert.Equal (-1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (1), CancellationToken.None).Result);
			Assert.Equal (bufSize - 1 - skipSize, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize - 1), CancellationToken.None).Result);
			Assert.Equal (bufSize - skipSize, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize), CancellationToken.None).Result);
			Assert.Equal (bufSize - 1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize + skipSize - 1), CancellationToken.None).Result);
			Assert.Equal (-1, BufferedSourceExtensions.IndexOfAsync (src, FillFunction (bufSize + skipSize), CancellationToken.None).Result);
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Read ()
		{
			int srcBufSize = 32768;
			int testSampleSize = 68;
			int readBufSize = 1000;
			int readBufOffset = 512;
			var src = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			var skip = srcBufSize - testSampleSize;
			src.SkipBuffer (skip);
			var buf = new byte[readBufSize];
			Assert.Equal (testSampleSize, BufferedSourceExtensions.ReadAsync (src, buf, readBufOffset, testSampleSize, CancellationToken.None).Result);
			for (int i = 0; i < testSampleSize; i++)
			{
				Assert.Equal (FillFunction ((long)(skip + i)), buf[readBufOffset + i]);
			}

			src.TryFastSkipAsync (long.MaxValue - (long)srcBufSize - 3, CancellationToken.None).Wait ();
			Assert.Equal (3, BufferedSourceExtensions.ReadAsync (src, buf, 0, buf.Length, CancellationToken.None).Result);
		}

		[Theory]
		[Trait ("Category", "BufferedSource")]
		[InlineData (0, 16)] // пустой источник
		[InlineData (1163, 387)] // чтение больше буфера
		[InlineData (1163, 10467)] // чтение меньше буфера
		public void ReadAllBytes (int testSampleSize, int srcBufSize)
		{
			long skipSize = long.MaxValue - (long)testSampleSize;

			var src = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			src.TryFastSkipAsync (skipSize, CancellationToken.None).Wait ();
			var result = BufferedSourceExtensions.ReadAllBytesAsync (src, CancellationToken.None).Result;
			Assert.Equal (testSampleSize, result.Length);
			for (int i = 0; i < testSampleSize; i++)
			{
				Assert.Equal (FillFunction ((long)(skipSize + i)), result[i]);
			}
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void WriteTo ()
		{
			int testSampleSize = 1163;
			long skipSize = long.MaxValue - (long)testSampleSize;
			int srcBufSize = testSampleSize / 3;
			var src = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			src.TryFastSkipAsync (skipSize, CancellationToken.None).Wait ();
			var dst = new BinaryDestinationMock (8192);
			Assert.Equal (testSampleSize, BufferedSourceExtensions.WriteToAsync (src, dst, CancellationToken.None).Result);

			for (int i = 0; i < testSampleSize; i++)
			{
				Assert.Equal (FillFunction ((long)(skipSize + i)), dst.Buffer[i]);
			}
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
