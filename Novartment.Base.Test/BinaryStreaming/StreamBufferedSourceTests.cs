using System.Threading;
using System.IO;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Test
{
	public class StreamBufferedSourceTests
	{
		[Theory, Trait ("Category", "BufferedSource"),
		InlineData (1),
		InlineData (2),
		InlineData (3),
		InlineData (65536)]
		public static void RequestSkipEmptySource (int bufSize)
		{
			var data = new byte[0];
			var strm = new MemoryStream (data);
			var src = strm.AsBufferedSource (new byte[bufSize]);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.Equal (0, src.Count);
			Assert.Equal (0, src.TryFastSkipAsync (1, CancellationToken.None).Result);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);
		}

		[Theory, Trait ("Category", "BufferedSource"),
		InlineData (1),
		InlineData (2),
		InlineData (3),
		InlineData (65536)]
		public static void RequestSkipOneByteSource (int bufSize)
		{
			byte nnn = 123;
			var data = new byte[] { nnn };
			var strm = new MemoryStream (data);

			var src = strm.AsBufferedSource (new byte[bufSize]);
			src.EnsureBufferAsync (1, CancellationToken.None).Wait ();
			Assert.Equal (nnn, src.Buffer[src.Offset]);
			src.SkipBuffer (1);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);

			strm.Seek (0, SeekOrigin.Begin);
			src = strm.AsBufferedSource (new byte[bufSize]);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.Equal (nnn, src.Buffer[src.Offset]);
			Assert.Equal (1, src.TryFastSkipAsync (bufSize, CancellationToken.None).Result);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
		// проверяем 2 блока по 3 байта перед каждым делаем пропуск
		[Theory, Trait ("Category", "BufferedSource"),
		// нулевые пропуски, то есть проверяются все байты
		InlineData (6, true, 3, 0, 0, 0, 0, 1000),
		InlineData (6, false, 3, 0, 0, 0, 0, 1000),
		// буфер меньше источника
		InlineData (65536, true, 10000, 1, 3, 54, 20000, 65536),
		InlineData (/*dataSize*/65536, /*canSeek*/false, /*bufSize*/10000, 1, 3, 54, 20000, 65536),
		// буфер больше источника
		InlineData (10000, true, 65536, 10, 1000, 1, 2000, 10000),
		InlineData (10000, false, 65536, 10, 1000, 1, 2000, 10000),
		// большой источник
		InlineData (0x1000000000000, true, 10000, 1, 3, 0x20000000000, 65536, 0x4000000000000000),
		InlineData (long.MaxValue, true, 10000, 1, 3, 0x2000000000000, 65536, long.MaxValue)]
		public static void RequestSkipTest3SkipTest3Skip (
			long dataSize,
			bool canSeek,
			int bufSize,
			int skipBuffer1,
			int skipBuffer2,
			long skipOverall1,
			long skipOverall2,
			long skipOverEnd)
		{
			var strm = new BigStreamMock (dataSize, canSeek, FillFunction);
			var src = strm.AsBufferedSource (new byte[bufSize]);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction(0), src.Buffer[src.Offset]);
			Assert.Equal (FillFunction(1), src.Buffer[src.Offset + 1]);
			Assert.Equal (FillFunction(2), src.Buffer[src.Offset + 2]);
			Assert.Equal (skipOverall1, src.TryFastSkipAsync (skipOverall1, CancellationToken.None).Result);
			src.EnsureBufferAsync (skipBuffer1, CancellationToken.None).Wait ();
			src.SkipBuffer (skipBuffer1);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction(skipOverall1 + skipBuffer1), src.Buffer[src.Offset]);
			Assert.Equal (FillFunction(skipOverall1 + skipBuffer1 + 1), src.Buffer[src.Offset + 1]);
			Assert.Equal (FillFunction(skipOverall1 + skipBuffer1 + 2), src.Buffer[src.Offset + 2]);
			Assert.Equal (skipOverall2, src.TryFastSkipAsync (skipOverall2, CancellationToken.None).Result);
			src.EnsureBufferAsync (skipBuffer2, CancellationToken.None).Wait ();
			src.SkipBuffer (skipBuffer2);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction(skipOverall1 + skipOverall2 + skipBuffer1 + skipBuffer2), src.Buffer[src.Offset]);
			Assert.Equal (FillFunction(skipOverall1 + skipOverall2 + skipBuffer1 + skipBuffer2 + 1), src.Buffer[src.Offset + 1]);
			Assert.Equal (FillFunction(skipOverall1 + skipOverall2 + skipBuffer1 + skipBuffer2 + 2), src.Buffer[src.Offset + 2]);
			Assert.Equal (dataSize - skipOverall1 - skipOverall2 - skipBuffer1 - skipBuffer2, src.TryFastSkipAsync (skipOverEnd, CancellationToken.None).Result);
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);
		}
	}
}
