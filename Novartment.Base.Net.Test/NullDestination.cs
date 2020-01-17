using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal class NullDestination :
		IBinaryDestination
	{
		public void SetComplete ()
		{
		}

		public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken) => default;
	}
}
