using System;
using System.IO;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class StreamBufferedSourceTests
	{
		[Theory]
		[Trait ("Category", "BufferedSource")]
		[InlineData (1)]
		[InlineData (2)]
		[InlineData (3)]
		[InlineData (65536)]
		public static void RequestSkipEmptySource (int bufSize)
		{
			var data = Array.Empty<byte> ();
			var strm = new MemoryStream (data);
			var src = strm.AsBufferedSource (new byte[bufSize]);
			var vTask = src.FillBufferAsync (default);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (0, src.Count);
			Assert.Equal (0, src.TryFastSkipAsync (1, default).Result);
			vTask = src.FillBufferAsync (default);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);
		}

		[Theory]
		[Trait ("Category", "BufferedSource")]
		[InlineData (1)]
		[InlineData (2)]
		[InlineData (3)]
		[InlineData (65536)]
		public void RequestSkipOneByteSource (int bufSize)
		{
			byte nnn = 123;
			var data = new byte[] { nnn };
			var strm = new MemoryStream (data);

			var src = strm.AsBufferedSource (new byte[bufSize]);
			var vTask = src.EnsureBufferAsync (1, default);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (nnn, src.BufferMemory.Span[src.Offset]);
			src.SkipBuffer (1);
			vTask = src.FillBufferAsync (default);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);

			strm.Seek (0, SeekOrigin.Begin);
			src = strm.AsBufferedSource (new byte[bufSize]);
			vTask = src.FillBufferAsync (default);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (nnn, src.BufferMemory.Span[src.Offset]);
			Assert.Equal (1, src.TryFastSkipAsync (bufSize, default).Result);
			vTask = src.FillBufferAsync (default);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);
		}

		// проверяем 2 блока по 3 байта перед каждым делаем пропуск
		[Theory]
		[Trait ("Category", "BufferedSource")]
		[InlineData (6, true, 3, 0, 0, 0, 0, 1000)] // нулевые пропуски, то есть проверяются все байты
		[InlineData (6, false, 3, 0, 0, 0, 0, 1000)]
		[InlineData (65536, true, 10000, 1, 3, 54, 20000, 65536)] // буфер меньше источника
		[InlineData (/*dataSize*/65536, /*canSeek*/false, /*bufSize*/10000, 1, 3, 54, 20000, 65536)]
		[InlineData (10000, true, 65536, 10, 1000, 1, 2000, 10000)] // буфер больше источника
		[InlineData (10000, false, 65536, 10, 1000, 1, 2000, 10000)]
		[InlineData (0x1000000000000, true, 10000, 1, 3, 0x20000000000, 65536, 0x4000000000000000)] // большой источник
		[InlineData (long.MaxValue, true, 10000, 1, 3, 0x2000000000000, 65536, long.MaxValue)]
		public void RequestSkipTest3SkipTest3Skip (
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
			var vTask = src.EnsureBufferAsync (3, default);
			if (!vTask.IsCompletedSuccessfully)
			{
				vTask.AsTask ().GetAwaiter ().GetResult ();
			}
			Assert.Equal (FillFunction(0), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction(1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction(2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (skipOverall1, src.TryFastSkipAsync (skipOverall1, default).Result);
			vTask = src.EnsureBufferAsync (skipBuffer1, default);
			if (!vTask.IsCompletedSuccessfully)
			{
				vTask.AsTask ().GetAwaiter ().GetResult ();
			}
			src.SkipBuffer (skipBuffer1);
			vTask = src.EnsureBufferAsync (3, default);
			if (!vTask.IsCompletedSuccessfully)
			{
				vTask.AsTask ().GetAwaiter ().GetResult ();
			}
			Assert.Equal (FillFunction(skipOverall1 + skipBuffer1), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction(skipOverall1 + skipBuffer1 + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction(skipOverall1 + skipBuffer1 + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (skipOverall2, src.TryFastSkipAsync (skipOverall2, default).Result);
			vTask = src.EnsureBufferAsync (skipBuffer2, default);
			if (!vTask.IsCompletedSuccessfully)
			{
				vTask.AsTask ().GetAwaiter ().GetResult ();
			}
			src.SkipBuffer (skipBuffer2);
			vTask = src.EnsureBufferAsync (3, default);
			if (!vTask.IsCompletedSuccessfully)
			{
				vTask.AsTask ().GetAwaiter ().GetResult ();
			}
			Assert.Equal (FillFunction(skipOverall1 + skipOverall2 + skipBuffer1 + skipBuffer2), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction(skipOverall1 + skipOverall2 + skipBuffer1 + skipBuffer2 + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction(skipOverall1 + skipOverall2 + skipBuffer1 + skipBuffer2 + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.Equal (dataSize - skipOverall1 - skipOverall2 - skipBuffer1 - skipBuffer2, src.TryFastSkipAsync (skipOverEnd, default).Result);
			Assert.True (src.IsExhausted);
			Assert.Equal (0, src.Count);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
