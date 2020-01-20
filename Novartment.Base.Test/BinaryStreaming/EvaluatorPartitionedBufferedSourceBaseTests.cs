using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class EvaluatorPartitionedBufferedSourceBaseTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void ReadAndSkipPart ()
		{
			long skipBeforeLimitingSize = 0x408c5f081052008cL;

			long firstPartPos = 0x408c5f08105200c0L;
			long secondPartPos = 0x408c5f08105200ccL;
			long thirdPartPos = 0x408c5f0810520100L;
			int srcBufSize = 32768;

			// части в середине источника
			var subSrc = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			subSrc.SkipWihoutBufferingAsync (skipBeforeLimitingSize);
			var src = new OneHundredEvaluatorBufferedSource (subSrc);
			var vTask = src.LoadAsync ();
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.True (src.TrySkipPartAsync ().Result);
			vTask = src.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (firstPartPos), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (firstPartPos + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (firstPartPos + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.True (src.TrySkipPartAsync ().Result);
			vTask = src.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (secondPartPos), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (secondPartPos + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (secondPartPos + 2), src.BufferMemory.Span[src.Offset + 2]);
			Assert.True (src.TrySkipPartAsync ().Result);
			vTask = src.EnsureAvailableAsync (3);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (FillFunction (thirdPartPos), src.BufferMemory.Span[src.Offset]);
			Assert.Equal (FillFunction (thirdPartPos + 1), src.BufferMemory.Span[src.Offset + 1]);
			Assert.Equal (FillFunction (thirdPartPos + 2), src.BufferMemory.Span[src.Offset + 2]);

			// части в конце источника
			long size = 0x4000000000000000L;
			subSrc = new BigBufferedSourceMock (size, srcBufSize, FillFunction);
			subSrc.SkipWihoutBufferingAsync (0x3fffffffffffffc0L); // отступаем так чтобы осталось две части с хвостиком
			src = new OneHundredEvaluatorBufferedSource (subSrc);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.True (src.TrySkipPartAsync ().Result);
			Assert.False (src.TrySkipPartAsync ().Result);
		}

		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}

		// считает содержимым одной части байты >=100, при этом идущие следом байты <100 являются разделителем частей
		internal class OneHundredEvaluatorBufferedSource : EvaluatorPartitionedBufferedSourceBase
		{
			private readonly IBufferedSource _source;
			private int _epilogueSize = -1;

			public OneHundredEvaluatorBufferedSource (IBufferedSource source)
				: base (source)
			{
				_source = source;
			}

			protected override bool IsEndOfPartFound => _epilogueSize >= 0;

			protected override int PartEpilogueSize => _epilogueSize;

			protected override int ValidatePartData (int validatedPartLength)
			{
				_epilogueSize = -1;
				var startOffset = _source.Offset + validatedPartLength;
				while (startOffset < (_source.Offset + _source.Count))
				{
					if (_source.BufferMemory.Span[startOffset] < 100)
					{
						var epilogueSize = 1;
						while ((startOffset + epilogueSize) < (_source.Offset + _source.Count))
						{
							if (_source.BufferMemory.Span[startOffset + epilogueSize] >= 100)
							{
								_epilogueSize = epilogueSize;
								break;
							}

							epilogueSize++;
						}

						if (_source.IsExhausted)
						{
							_epilogueSize = epilogueSize;
						}

						return startOffset - _source.Offset;
					}

					startOffset++;
				}

				return _source.Count;
			}
		}
	}
}
