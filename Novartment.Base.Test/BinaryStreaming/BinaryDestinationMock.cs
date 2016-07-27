using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Test
{
	public class BinaryDestinationMock : IBinaryDestination
	{
		private bool _completed = false;
		private int _count = 0;
		private readonly byte[] _buffer;

		public byte[] Buffer => _buffer;

		public int Count => _count;

		public BinaryDestinationMock (int maxSize)
		{
			_buffer = new byte[maxSize];
		}

		public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (_completed) throw new InvalidOperationException ("Can not write to completed destination.");
			if (count > _buffer.Length - _count) throw new InvalidOperationException ("Insufficient buffer size.");
			Array.Copy (buffer, offset, _buffer, _count, count);
			_count += count;
			return Task.CompletedTask;
		}

		public void SetComplete ()
		{
			_completed = true;
		}
	}
}
