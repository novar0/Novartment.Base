using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <content>
	/// Класс-обёртка StreamBinaryDestination для представления Stream в виде IBinaryDestination.
	/// </content>
	public static partial class StreamExtensions
	{
		internal class StreamBinaryDestination :
			IBinaryDestination
		{
			private readonly Stream _stream;

			internal StreamBinaryDestination(Stream writableStream)
			{
				_stream = writableStream;
			}

			internal Stream BaseStream => _stream;

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
