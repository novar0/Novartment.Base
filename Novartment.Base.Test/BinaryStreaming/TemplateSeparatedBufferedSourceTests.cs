﻿using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class TemplateSeparatedBufferedSourceTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void RequestSkipPart ()
		{
			byte templPos = 162;
			var separator = new byte[]
			{
				FillFunction (templPos),
				FillFunction (templPos + 1),
				FillFunction (templPos + 2),
				FillFunction (templPos + 3),
				FillFunction (templPos + 4),
			};
			long skipBeforeLimitingSize = 0xfffffffd;
			long secondPartPos = (skipBeforeLimitingSize | 0xffL) + 1L + (long)templPos + separator.Length;
			int skipBufferSize = 93;
			int srcBufSize = 32768;

			// части в середине источника
			var subSrc = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			subSrc.TryFastSkipAsync (skipBeforeLimitingSize);
			var src = new TemplateSeparatedBufferedSource (subSrc, separator, false);
			var vTask = src.EnsureBufferAsync (skipBufferSize + 3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (skipBeforeLimitingSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			src.SkipBuffer (skipBufferSize);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (skipBeforeLimitingSize + skipBufferSize + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.True (src.TrySkipPartAsync ().Result);
			vTask = src.EnsureBufferAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (secondPartPos), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (secondPartPos + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (secondPartPos + 2), src.BufferMemory.Span[src.Offset + 2]);
			SkipToEnd (src, 256 - separator.Length);

			// части в конце источника
			long size = 4611686018427387904L;
			subSrc = new BigBufferedSourceMock (size, srcBufSize, FillFunction);
			subSrc.TryFastSkipAsync (size - 256 - 256 - 20); // отступаем так чтобы осталось две части с хвостиком
			src = new TemplateSeparatedBufferedSource (subSrc, separator, false);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.False (src.TrySkipPartAsync ().Result);

			// разделитель в конце источника
			separator = new byte[]
			{
				FillFunction (253),
				FillFunction (254),
				FillFunction (255),
			};
			subSrc = new BigBufferedSourceMock (768, srcBufSize, FillFunction);
			src = new TemplateSeparatedBufferedSource (subSrc, separator, false);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.False (src.TrySkipPartAsync ().Result);
		}

		private static void SkipToEnd (IBufferedSource source, int size)
		{
			long skipped = 0L;
			while (!source.IsExhausted)
			{
				var available = source.Count;
				source.SkipBuffer (available);
				skipped += (long)available;

				var vTask = source.FillBufferAsync ();
				Assert.True (vTask.IsCompletedSuccessfully);
			}

			var remainder = source.Count;
			source.SkipBuffer (remainder);
			skipped += (long)remainder;

			Assert.Equal (size, skipped);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
