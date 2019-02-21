﻿using System;
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

			public Task WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
			{
#if NETCOREAPP2_2
				return _stream.WriteAsync (buffer, cancellationToken).AsTask ();
#else
				return _stream.WriteAsync (buffer.ToArray (), 0, buffer.Length, cancellationToken);
#endif
			}

			public void SetComplete ()
			{
				_stream.Dispose ();
			}
		}
	}
}