using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Test
{
	public class BinaryDestinationMock :
		IBinaryDestination
	{
		private readonly byte[] _buffer;
		private bool _completed = false;
		private int _count = 0;

		public BinaryDestinationMock (int maxSize)
		{
			_buffer = new byte[maxSize];
		}

		public ReadOnlySpan<byte> Buffer => _buffer;

		public int Count => _count;

		public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (_completed)
			{
				throw new InvalidOperationException ("Can not write to completed destination.");
			}

			buffer.CopyTo (_buffer.AsMemory (_count, _buffer.Length - _count));
			_count += buffer.Length;
			return default;
		}

		public void SetComplete ()
		{
			_completed = true;
		}
	}
}
