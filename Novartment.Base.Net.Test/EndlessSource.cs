using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal class EndlessSource : IBufferedSource
	{
		public byte[] Buffer => new byte[20];

		public int Count => 20;

		public bool IsExhausted => false;

		public int Offset => 0;

		public Task EnsureBufferAsync (int size, CancellationToken cancellationToken) => Task.Delay (100);

		public Task FillBufferAsync (CancellationToken cancellationToken) => Task.CompletedTask;

		public void SkipBuffer (int size)
		{
		}
	}
}
