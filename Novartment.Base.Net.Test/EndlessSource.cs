using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal class EndlessSource :
		IBufferedSource
	{
		public ReadOnlyMemory<byte> BufferMemory => new byte[20];

		public int Count => 20;

		public bool IsExhausted => false;

		public int Offset => 0;

		public ValueTask EnsureBufferAsync (int size, CancellationToken cancellationToken) => new ValueTask (Task.Delay (100));

		public ValueTask FillBufferAsync (CancellationToken cancellationToken) => default;

		public void SkipBuffer (int size)
		{
		}
	}
}
