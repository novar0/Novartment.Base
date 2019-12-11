using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal class StringWritingStream : IBinaryDestination
	{
		private readonly Queue<string> _queue = new Queue<string> ();

		internal Queue<string> Queue => _queue;

		public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var str = Encoding.UTF8.GetString (buffer.Span);
			_queue.Enqueue (str);
			return default;
		}

		public void SetComplete ()
		{
		}
	}
}
