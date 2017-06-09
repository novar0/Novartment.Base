using System.Threading;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Test
{
	public class EvaluatorPartitionedBufferedSourceBaseTests
	{
		// считает содержимым одной части байты >=100, при этом идущие следом байты <100 являются разделителем частей
		public class OneHundredEvaluatorBufferedSource : EvaluatorPartitionedBufferedSourceBase
		{
			private readonly IBufferedSource _source;
			private int _epilogueSize = -1;

			public OneHundredEvaluatorBufferedSource (IBufferedSource source)
				: base (source)
			{
				_source = source;
			}

			protected override bool IsEndOfPartFound => (_epilogueSize >= 0);

			protected override int PartEpilogueSize => _epilogueSize;

			protected override int ValidatePartData (int validatedPartLength)
			{
				_epilogueSize = -1;
				var startOffset = _source.Offset + validatedPartLength;
				while (startOffset < (_source.Offset + _source.Count))
				{
					if (_source.Buffer[startOffset] < 100)
					{
						var epilogueSize = 1;
						while ((startOffset + epilogueSize) < (_source.Offset + _source.Count))
						{
							if (_source.Buffer[startOffset + epilogueSize] >= 100)
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

		[Fact, Trait ("Category", "BufferedSource")]
		public void ReadAndSkipPart ()
		{
			long skipBeforeLimitingSize = 0x408c5f081052008cL;

			long firstPartPos = 0x408c5f08105200c0L;
			long secondPartPos = 0x408c5f08105200ccL;
			long thirdPartPos = 0x408c5f0810520100L;
			int srcBufSize = 32768;

			// части в середине источника
			var subSrc = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			subSrc.TryFastSkipAsync (skipBeforeLimitingSize, CancellationToken.None).Wait ();
			var src = new OneHundredEvaluatorBufferedSource (subSrc);
			src.FillBufferAsync (CancellationToken.None).Wait ();
			Assert.True (src.TrySkipPartAsync (CancellationToken.None).Result);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (firstPartPos), src.Buffer[src.Offset]);
			Assert.Equal (FillFunction (firstPartPos + 1), src.Buffer[src.Offset + 1]);
			Assert.Equal (FillFunction (firstPartPos + 2), src.Buffer[src.Offset + 2]);
			Assert.True (src.TrySkipPartAsync (CancellationToken.None).Result);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (secondPartPos), src.Buffer[src.Offset]);
			Assert.Equal (FillFunction (secondPartPos + 1), src.Buffer[src.Offset + 1]);
			Assert.Equal (FillFunction (secondPartPos + 2), src.Buffer[src.Offset + 2]);
			Assert.True (src.TrySkipPartAsync (CancellationToken.None).Result);
			src.EnsureBufferAsync (3, CancellationToken.None).Wait ();
			Assert.Equal (FillFunction (thirdPartPos), src.Buffer[src.Offset]);
			Assert.Equal (FillFunction (thirdPartPos + 1), src.Buffer[src.Offset + 1]);
			Assert.Equal (FillFunction (thirdPartPos + 2), src.Buffer[src.Offset + 2]);

			// части в конце источника
			long size = 0x4000000000000000L;
			subSrc = new BigBufferedSourceMock (size, srcBufSize, FillFunction);
			subSrc.TryFastSkipAsync (0x3fffffffffffffc0L, CancellationToken.None).Wait (); // отступаем так чтобы осталось две части с хвостиком
			src = new OneHundredEvaluatorBufferedSource (subSrc);
			Assert.True (src.TrySkipPartAsync (CancellationToken.None).Result);
			Assert.True (src.TrySkipPartAsync (CancellationToken.None).Result);
			Assert.False (src.TrySkipPartAsync (CancellationToken.None).Result);
		}
		private static byte FillFunction (long position)
		{
			return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
