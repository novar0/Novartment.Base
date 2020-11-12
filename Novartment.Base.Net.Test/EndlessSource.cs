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

		public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken) => new ValueTask (Task.Delay (100, cancellationToken));

		public ValueTask LoadAsync (CancellationToken cancellationToken) => default;

		public void Skip (int size)
		{
		}
	}
}
