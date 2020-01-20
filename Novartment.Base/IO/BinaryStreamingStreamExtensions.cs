using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Extension methods for translating the Stream to/from IBufferedSource/IBinaryDestination.
	/// </summary>
	public static partial class BinaryStreamingStreamExtensions
	{
		/// <summary>
		/// Creates a data source represented by a byte buffer, when requesting data from which, data will be read from the specified stream.
		/// </summary>
		/// <param name="readableStream">The stream from which data requested for reading in the data source will be read.</param>
		/// <param name="buffer">A byte buffer that will contain the data read from the stream.</param>
		/// <returns>The data source represented by a byte buffer,
		/// when requesting data from which, data will be read from the specified stream.</returns>
		public static IFastSkipBufferedSource AsBufferedSource (this Stream readableStream, byte[] buffer)
		{
			if (readableStream == null)
			{
				throw new ArgumentNullException (nameof (readableStream));
			}

			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if (!readableStream.CanRead)
			{
				throw new ArgumentOutOfRangeException (nameof (readableStream));
			}

			if (buffer.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			Contract.EndContractBlock ();

			// нельзя использовать (readableStream as _BufferedSourceStream).BaseStream, потому что не будет использован указанный buffer
			return new StreamBufferedSource (readableStream, buffer);
		}

		/// <summary>
		/// Creates a binary data destination, when written to which data will be transferred to the specified stream.
		/// When writing to the destination is complete, the stream will be closed.
		/// </summary>
		/// <param name="writableStream">The stream to which all data written to the binary data destination will be transferred.</param>
		/// <returns>The binary data destination, when written to which data will be transferred to the specified stream.</returns>
		public static IBinaryDestination AsBinaryDestination (this Stream writableStream)
		{
			if (writableStream == null)
			{
				throw new ArgumentNullException (nameof (writableStream));
			}

			if (!writableStream.CanWrite)
			{
				throw new ArgumentOutOfRangeException (nameof (writableStream));
			}

			Contract.EndContractBlock ();

			return (writableStream is BinaryDestinationStream destinaton) ?
				destinaton.BaseBinaryDestination :
				new StreamBinaryDestination (writableStream);
		}

		/// <summary>
		/// Creates a read-only stream that receives data from the specified data source, represented by a byte buffer.
		/// </summary>
		/// <param name="source">The data source represented by the byte buffer from which the stream will receive data.</param>
		/// <returns>The read-only stream that receives data from the specified data source, represented by a byte buffer.</returns>
		public static Stream AsReadOnlyStream (this IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// нельзя использовать (source as _StreamBufferedSource).BaseStream, потому что у потока должен быть только один считыватель
			return new BufferedSourceStream (source);
		}

		/// <summary>
		/// Creates a write-only stream that passes all the data being written to the specified binary data destination.
		/// Closing the stream will mean completion of the write for the binary data destination.
		/// </summary>
		/// <param name="destination">The binary data destination to which all data written to the stream will be transmitted.</param>
		/// <returns>The write-only stream that passes all the data being written to the specified binary data destination.</returns>
		public static Stream AsWriteOnlyStream (this IBinaryDestination destination)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			return (destination is StreamBinaryDestination strm) ?
				strm.BaseStream :
				new BinaryDestinationStream (destination);
		}
	}
}
