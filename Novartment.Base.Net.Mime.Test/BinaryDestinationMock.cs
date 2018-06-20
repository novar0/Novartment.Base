using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime.Test
{
	internal class BinaryDestinationMock : IBinaryDestination
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

		public void Write (ReadOnlyMemory<byte> buffer)
		{
			if (_completed)
			{
				throw new InvalidOperationException ("Can not write to completed destination.");
			}

			if (buffer.Length > (_buffer.Length - _count))
			{
				throw new InvalidOperationException ("Insufficient buffer size.");
			}

			buffer.CopyTo (_buffer.AsMemory (_count));
			_count += buffer.Length;
		}

		public Task WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
		{
			Write (buffer);
			return Task.CompletedTask;
		}

		public void SetComplete ()
		{
			_completed = true;
		}
	}
}
