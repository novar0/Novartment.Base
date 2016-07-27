using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	public static partial class StreamExtensions
	{
		internal class _StreamBinaryDestination :
			IBinaryDestination
		{
			private readonly Stream _stream;

			internal Stream BaseStream => _stream;

			internal _StreamBinaryDestination (Stream writableStream)
			{
				_stream = writableStream;
			}

			public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				return _stream.WriteAsync (buffer, offset, count, cancellationToken);
			}

			public void SetComplete ()
			{
				_stream.Dispose ();
			}
		}
	}
}
