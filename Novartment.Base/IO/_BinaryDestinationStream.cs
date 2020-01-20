using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	public static partial class BinaryStreamingStreamExtensions
	{
		/// <content>
		/// Класс-обёртка BinaryDestinationStream для представления IBinaryDestination в виде Stream.
		/// </content>
		private class BinaryDestinationStream : Stream
		{
			private readonly IBinaryDestination _destination;

			internal BinaryDestinationStream (IBinaryDestination destination)
			{
				_destination = destination;
			}

			public override bool CanRead => false;

			public override bool CanWrite => true;

			public override bool CanSeek => false;

			public override long Length => throw new NotSupportedException ();

			public override long Position
			{
				get => throw new NotSupportedException ();
				set => throw new NotSupportedException ();
			}

			internal IBinaryDestination BaseBinaryDestination => _destination;

			public override long Seek (long offset, SeekOrigin origin) => throw new NotSupportedException ();

			public override void SetLength (long value) => throw new NotSupportedException ();

			public override void Flush ()
			{
			}

			public override int Read (byte[] buffer, int offset, int count) => throw new NotSupportedException ();

			public override int ReadByte () => throw new NotSupportedException ();

			public override void Write (byte[] buffer, int offset, int count)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException (nameof (buffer));
				}

				if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
				{
					throw new ArgumentOutOfRangeException (nameof (offset));
				}

				if ((count < 0) || ((offset + count) > buffer.Length))
				{
					throw new ArgumentOutOfRangeException (nameof (count));
				}

				Contract.EndContractBlock ();

				var vTask = _destination.WriteAsync (buffer.AsMemory (offset, count), default);
				if (vTask.IsCompletedSuccessfully)
				{
					return;
				}
				vTask.AsTask ().GetAwaiter ().GetResult ();
			}

			public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException (nameof (buffer));
				}

				if ((offset < 0) || (offset > buffer.Length) || ((offset == buffer.Length) && (count > 0)))
				{
					throw new ArgumentOutOfRangeException (nameof (offset));
				}

				if ((count < 0) || ((offset + count) > buffer.Length))
				{
					throw new ArgumentOutOfRangeException (nameof (count));
				}

				Contract.EndContractBlock ();

				return _destination.WriteAsync (buffer.AsMemory (offset, count), cancellationToken).AsTask ();
			}

#if !NETSTANDARD2_0
			public override ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
			{
				return _destination.WriteAsync (buffer, cancellationToken);
			}
#endif

			protected override void Dispose (bool disposing)
			{
				base.Dispose (disposing);
				if (disposing)
				{
					_destination.SetComplete ();
				}
			}
		}
	}
}
