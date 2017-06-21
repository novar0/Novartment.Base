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

		public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var str = Encoding.UTF8.GetString (buffer, offset, count);
			_queue.Enqueue (str);
			return Task.CompletedTask;
		}

		public void SetComplete ()
		{
		}
	}
}
