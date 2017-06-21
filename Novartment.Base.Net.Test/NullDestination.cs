using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal class NullDestination : IBinaryDestination
	{
		public void SetComplete ()
		{
		}

		public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
