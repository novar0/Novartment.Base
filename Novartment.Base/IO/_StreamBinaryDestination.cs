using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	public static partial class BinaryStreamingStreamExtensions
	{
		/// <content>
		/// Класс-обёртка StreamBinaryDestination для представления Stream в виде IBinaryDestination.
		/// </content>
		internal class StreamBinaryDestination :
			IBinaryDestination
		{
			private readonly Stream _stream;

			internal StreamBinaryDestination(Stream writableStream)
			{
				_stream = writableStream;
			}

			internal Stream BaseStream => _stream;

			public ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
			{
#if NETSTANDARD2_0
				return new ValueTask (_stream.WriteAsync (buffer.ToArray (), 0, buffer.Length, cancellationToken));
#else
				return _stream.WriteAsync (buffer, cancellationToken);
#endif
			}

			public void SetComplete ()
			{
				_stream.Dispose ();
			}
		}
	}
}
