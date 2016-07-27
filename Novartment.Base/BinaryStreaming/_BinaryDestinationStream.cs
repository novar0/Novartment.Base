using System;
using System.IO;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	public static partial class StreamExtensions
	{
		private class _BinaryDestinationStream : Stream
		{
			private readonly IBinaryDestination _destination;

			internal _BinaryDestinationStream (IBinaryDestination destination)
			{
				_destination = destination;
			}

			internal IBinaryDestination BaseBinaryDestination => _destination;

			public override bool CanRead => false;

			public override bool CanWrite => true;

			public override bool CanSeek => false;

			public override long Length { get { throw new NotSupportedException (); } }

			public override long Position { get { throw new NotSupportedException (); } set { throw new NotSupportedException (); } }

			public override long Seek (long offset, SeekOrigin origin) { throw new NotSupportedException (); }

			public override void SetLength (long value) { throw new NotSupportedException (); }

			public override void Flush () { }

			public override int Read (byte[] buffer, int offset, int count) { throw new NotSupportedException (); }

			public override int ReadByte () { throw new NotSupportedException (); }

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

				_destination.WriteAsync (buffer, offset, count, CancellationToken.None).Wait ();
			}

			public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
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

				return _destination.WriteAsync (buffer, offset, count, CancellationToken.None);
			}

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
