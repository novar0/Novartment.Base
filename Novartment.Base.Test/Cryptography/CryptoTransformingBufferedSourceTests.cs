using System;
using System.Security.Cryptography;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class CryptoTransformingBufferedSourceTests
	{
		[Theory]
		[Trait ("Category", "BufferedSourceCrypto")]
		[InlineData (5142, 3191, 0, 2334806618776893638L, 8261, true, 11634, 1205L, 1594)]
		[InlineData (5142, 3191, 5, 2334806618776893638L, 8261, true, 11634, 1205L, 1594)]
		[InlineData (5142, 3191, 0, 2334806618776893638L, 8261, false, 11634, 1205L, 1594)]
		[InlineData (5142, 3191, 5, 2334806618776893638L, 8261, false, 11634, 1205L, 1594)]
		[InlineData (121, 3191, 0, 1728006203025082346L, 17580, true, 4003, 12050L, 1)]
		[InlineData (121, 3191, 1, 1728006203025082346L, 17580, true, 4003, 12050L, 1)]
		[InlineData (121, 3191, 0, 1728006203025082346L, 17580, false, 4003, 12050L, 1)]
		[InlineData (121, 3191, 1, 1728006203025082346L, 17580, false, 4003, 12050L, 1)]
		[InlineData (3191, 3191, 0, 3168L, 8261, true, 11634, 1205L, 1594)]
		[InlineData (3191, 3191, 5, 3168L, 8261, true, 11634, 1205L, 1594)]
		[InlineData (3191, 3191, 0, 3168L, 8261, false, 11634, 1205L, 1594)]
		[InlineData (3191, 3191, 5, 3168L, 8261, false, 11634, 1205L, 1594)]
		[InlineData (3191, 3191, 0, 757372522887491490L, 17580, true, 4003, 12050L, 1)]
		[InlineData (3191, 3191, 1, 757372522887491490L, 17580, true, 4003, 12050L, 1)]
		[InlineData (3191, 3191, 0, 757372522887491490L, 17580, false, 4003, 12050L, 1)]
		[InlineData (3191, 3191, 1, 757372522887491490L, 17580, false, 4003, 12050L, 1)]
		[InlineData (1, 3191, 0, int.MaxValue, 6, true, 11634, 1205L, 1594)]
		[InlineData (1, 3191, 5, int.MaxValue, 6, true, 11634, 1205L, 1594)]
		[InlineData (1, 3191, 0, int.MaxValue, 6, false, 11634, 1205L, 1594)]
		[InlineData (1, 3191, 5, int.MaxValue, 6, false, 11634, 1205L, 1594)]
		[InlineData (1, 3191, 0, 151852718393714646L, 256, true, 4003, 12050L, 1)]
		[InlineData (1, 3191, 1, 151852718393714646L, 256, true, 4003, 12050L, 1)]
		[InlineData (1, 3191, 0, 151852718393714646L, 256, false, 4003, 12050L, 1)]
		[InlineData (1, 3191, 1, 151852718393714646L, 256, false, 4003, 12050L, 1)]
		[InlineData (191, 1, 0, 2101L, 8261, true, 6, 5L, 0)]
		[InlineData (191, 1, 5, 2101L, 8261, true, 6, 5L, 0)]
		[InlineData (191, 1, 0, 2101L, 8261, false, 6, 5L, 0)]
		[InlineData (191, 1, 5, 2101L, 8261, false, 6, 5L, 0)]
		[InlineData (191, 1, 0, 1925178341944135524L, 17580, true, 256, 12050L, 1)]
		[InlineData (191, 1, 1, 1925178341944135524L, 17580, true, 256, 12050L, 1)]
		[InlineData (191, 1, 0, 1925178341944135524L, 17580, false, 256, 12050L, 1)]
		[InlineData (191, 1, 1, 1925178341944135524L, 17580, false, 256, 12050L, 1)]
		[InlineData (1, 1, 0, 11L, 8261, true, 6, 5L, 0)]
		[InlineData (1, 1, 5, 11L, 8261, true, 6, 5L, 0)]
		[InlineData (1, 1, 0, 11L, 8261, false, 6, 5L, 0)]
		[InlineData (1, 1, 5, 11L, 8261, false, 6, 5L, 0)]
		[InlineData (1, 1, 0, 1925178341944135524L, 17580, true, 256, 12050L, 1)]
		[InlineData (1, 1, 1, 1925178341944135524L, 17580, true, 256, 12050L, 1)]
		[InlineData (1, 1, 0, 1925178341944135524L, 17580, false, 256, 12050L, 1)]
		[InlineData (1, 1, 1, 1925178341944135524L, 17580, false, 256, 12050L, 1)]
		public void CryptoTransformingBufferedSource_SkipTransformChunk (
			int inBlockSize,
			int outBlockSize,
			int inputCacheBlocks,
			long dataSize,
			int bufSize,
			bool canTransformMultipleBlocks,
			int transformBufferSize,
			long totalSkip,
			int bufferSkip)
		{
			var src = new BigBufferedSourceMock (dataSize, bufSize, FillFunction);
			var mock = new CryptoTransformMock (inBlockSize, outBlockSize, inputCacheBlocks, canTransformMultipleBlocks);
			var transform = new CryptoTransformingBufferedSource (src, mock, new byte[transformBufferSize]);

			Skip (transform, totalSkip);
			var vTask = transform.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal ((byte)~FillFunction (MapTransformIndexBackToOriginal (totalSkip, mock)), transform.BufferMemory.Span[transform.Offset]);
			Assert.Equal ((byte)~FillFunction (MapTransformIndexBackToOriginal (totalSkip + 1, mock)), transform.BufferMemory.Span[transform.Offset + 1]);
			Assert.Equal ((byte)~FillFunction (MapTransformIndexBackToOriginal (totalSkip + 2, mock)), transform.BufferMemory.Span[transform.Offset + 2]);
			vTask = transform.EnsureAvailableAsync (bufferSkip);
			Assert.True (vTask.IsCompletedSuccessfully);
			transform.Skip (bufferSkip);
			vTask = transform.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal ((byte)~FillFunction (MapTransformIndexBackToOriginal (totalSkip + bufferSkip, mock)), transform.BufferMemory.Span[transform.Offset]);
			Assert.Equal ((byte)~FillFunction (MapTransformIndexBackToOriginal (totalSkip + bufferSkip + 1, mock)), transform.BufferMemory.Span[transform.Offset + 1]);
			Assert.Equal ((byte)~FillFunction (MapTransformIndexBackToOriginal (totalSkip + bufferSkip + 2, mock)), transform.BufferMemory.Span[transform.Offset + 2]);
		}

		[Theory]
		[Trait ("Category", "BufferedSourceCrypto")]
		[InlineData (7283, 2911, 0, 1474, 11824, true, 8007)]
		[InlineData (7283, 2911, 2, 1474, 11824, true, 8007)]
		[InlineData (7283, 2911, 0, 1474, 11824, false, 8007)]
		[InlineData (7283, 2911, 2, 1474, 11824, false, 8007)]
		[InlineData (3106, 9476, 0, 7199, 10486, true, 19051)]
		[InlineData (3106, 9476, 3, 7199, 10486, true, 19051)]
		[InlineData (3106, 9476, 0, 7199, 10486, false, 19051)]
		[InlineData (3106, 9476, 3, 7199, 10486, false, 19051)]
		[InlineData (2911, 2911, 0, 1474, 11824, true, 8007)]
		[InlineData (2911, 2911, 2, 1474, 11824, true, 8007)]
		[InlineData (2911, 2911, 0, 1474, 11824, false, 8007)]
		[InlineData (2911, 2911, 2, 1474, 11824, false, 8007)]
		[InlineData (9476, 9476, 0, 7199, 10486, true, 19051)]
		[InlineData (9476, 9476, 3, 7199, 10486, true, 19051)]
		[InlineData (9476, 9476, 0, 7199, 10486, false, 19051)]
		[InlineData (9476, 9476, 3, 7199, 10486, false, 19051)]
		[InlineData (1, 211, 0, 144, 3, true, 8007)]
		[InlineData (1, 211, 2, 144, 3, true, 8007)]
		[InlineData (1, 211, 0, 144, 3, false, 8007)]
		[InlineData (1, 211, 2, 144, 3, false, 8007)]
		[InlineData (1, 76, 0, 71, 127, true, 19051)]
		[InlineData (1, 76, 3, 71, 127, true, 19051)]
		[InlineData (1, 76, 0, 71, 127, false, 19051)]
		[InlineData (1, 76, 3, 71, 127, false, 19051)]
		[InlineData (211, 1, 0, 144, 11824, true, 3)]
		[InlineData (211, 1, 2, 144, 11824, true, 3)]
		[InlineData (211, 1, 0, 144, 11824, false, 3)]
		[InlineData (211, 1, 2, 144, 11824, false, 3)]
		[InlineData (76, 1, 0, 71, 10486, true, 127)]
		[InlineData (76, 1, 3, 71, 10486, true, 127)]
		[InlineData (76, 1, 0, 71, 10486, false, 127)]
		[InlineData (76, 1, 3, 71, 10486, false, 127)]
		[InlineData (1, 1, 0, 144, 11824, true, 3)]
		[InlineData (1, 1, 2, 144, 11824, true, 3)]
		[InlineData (1, 1, 0, 144, 11824, false, 3)]
		[InlineData (1, 1, 2, 144, 11824, false, 3)]
		[InlineData (1, 1, 0, 71, 10486, true, 127)]
		[InlineData (1, 1, 3, 71, 10486, true, 127)]
		[InlineData (1, 1, 0, 71, 10486, false, 127)]
		[InlineData (1, 1, 3, 71, 10486, false, 127)]
		public void CryptoTransformingBufferedSource_TransformAll (
			int inBlockSize,
			int outBlockSize,
			int inputCacheBlocks,
			int dataSize,
			int bufSize,
			bool canTransformMultipleBlocks,
			int transformBufferSize)
		{
			var src = new BigBufferedSourceMock (dataSize, bufSize, FillFunction);
			var mock = new CryptoTransformMock (inBlockSize, outBlockSize, inputCacheBlocks, canTransformMultipleBlocks);
			var transform = new CryptoTransformingBufferedSource (src, mock, new byte[transformBufferSize]);

			var fullBlocks = dataSize / inBlockSize;
			var inReminder = dataSize - (fullBlocks * inBlockSize);
			var outReminder = Math.Min (inReminder, outBlockSize);
			var resultSize = (fullBlocks * outBlockSize) + outReminder;

			// считываем весь результат преобразования
			int len = 0;
			var result = new byte[resultSize];
			while (true)
			{
				var vTask = transform.LoadAsync ();
				Assert.True (vTask.IsCompletedSuccessfully);
				if (transform.Count <= 0)
				{
					break;
				}

				transform.BufferMemory.Span.Slice (transform.Offset, transform.Count).CopyTo (result.AsSpan (len));
				len += transform.Count;
				transform.Skip (transform.Count);
			}

			Assert.True (transform.IsExhausted);
			Assert.Equal (resultSize, len);

			// проверяем что результат преобразования совпадает с тестовым массивом
			for (var i = 0; i < fullBlocks; i++)
			{
				for (int j = 0; j < outBlockSize; j++)
				{
					var inIndex = (i * inBlockSize) + (j % inBlockSize);
					var outIndex = (i * outBlockSize) + j;
					Assert.Equal (FillFunction (inIndex), (byte)~result[outIndex]);
				}
			}

			for (int j = 0; j < outReminder; j++)
			{
				var inIndex = (fullBlocks * inBlockSize) + (j % inBlockSize);
				var outIndex = (fullBlocks * outBlockSize) + j;
				Assert.Equal (FillFunction (inIndex), (byte)~result[outIndex]);
			}
		}

		private static long MapTransformIndexBackToOriginal (long resultIndex, ISpanCryptoTransform transform)
		{
			var blockNumber = resultIndex / (long)transform.OutputBlockSize;
			var blockOffset = (resultIndex % (long)transform.OutputBlockSize) % (long)transform.InputBlockSize;
			return (blockNumber * transform.InputBlockSize) + blockOffset;
		}

		private static void Skip (IBufferedSource source, long size)
		{
			var available = source.Count;
			if (size <= (long)available)
			{
				// достаточно доступных данных буфера
				source.Skip ((int)size);
				return;
			}

			while (!source.IsExhausted && (size > (long)source.Count))
			{
				available = source.Count;
				source.Skip (available);
				size -= (long)available;

				var vTask = source.LoadAsync ();
				Assert.True (vTask.IsCompletedSuccessfully);
			}

			Assert.InRange (size, 0, (long)source.Count);
			var reminder = Math.Min (size, (long)source.Count);
			source.Skip ((int)reminder);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
