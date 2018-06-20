using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Суммарная информация об AVI-файле.
	/// </summary>
	[CLSCompliant (false)]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class AviInfo
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса AviInfo на основе указанных данных.
		/// </summary>
		/// <param name="microSecPerFrame">Number of microseconds between frames. This value indicates the overall timing for the file.</param>
		/// <param name="options">
		/// Contains a bitwise combination of zero or more of the following flags:
		/// AVIF_COPYRIGHTED, AVIF_HASINDEX, AVIF_ISINTERLEAVED, AVIF_MUSTUSEINDEX, AVIF_WASCAPTUREFILE.
		/// </param>
		/// <param name="totalFrames">Total number of frames of data in the file.</param>
		/// <param name="width">Width of the AVI file in pixels.</param>
		/// <param name="height">Height of the AVI file in pixels. </param>
		/// <param name="streams">Коллекция потоков файла.</param>
		public AviInfo (
			uint microSecPerFrame,
			uint options,
			uint totalFrames,
			uint width,
			uint height,
			IReadOnlyList<AviStreamInfo> streams)
		{
			this.MicroSecPerFrame = microSecPerFrame;
			this.Options = options;
			this.TotalFrames = totalFrames;
			this.Width = width;
			this.Height = height;
			this.Streams = streams;
		}

		/// <summary>Gets number of microseconds between frames. This value indicates the overall timing for the file.</summary>
		public uint MicroSecPerFrame { get; }

		/// <summary>Gets a bitwise combination of zero or more of the flags:
		/// HasIndex=0x10, MustUseIndex=0x20, IsInterleaved=0x100, TrustCKType=0x800, WasCaptureFile=0x10000, Copyrighted=0x20000.
		/// </summary>
		public uint Options { get; }

		/// <summary>Gets total number of frames of data in the file.</summary>
		public uint TotalFrames { get; }

		/// <summary>Gets width of the AVI file in pixels.</summary>
		public uint Width { get; }

		/// <summary>Gets height of the AVI file in pixels.</summary>
		public uint Height { get; }

		/// <summary>Gets collection of media streams.</summary>
		public IReadOnlyList<AviStreamInfo> Streams { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => FormattableString.Invariant ($"Frames = {this.TotalFrames}, Width = {this.Width}, Height = {this.Height}, Streams = {this.Streams.Count}");

		/// <summary>
		/// Считывает суммарные данные об AVI-файле из указанного буфера.
		/// </summary>
		/// <param name="source">Буфер данных содержащий суммарные данные об AVI-файле.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Суммарные данные об AVI-файле, считанные из указанного буфера.</returns>
		public static Task<AviInfo> ParseAsync (
			IBufferedSource source,
#pragma warning disable CA1801 // Review unused parameters
			CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return ParseAsyncStateMachine ();

			async Task<AviInfo> ParseAsyncStateMachine ()
			{
				var rootChunk = await RiffChunk.ParseAsync (source, cancellationToken).ConfigureAwait (false);
				if (rootChunk.Id != "RIFF")
				{
					throw new FormatException ("Root chunk is not 'RIFF'.");
				}

				if (!rootChunk.IsSubChunkList)
				{
					throw new FormatException ("Root RIFF-chunk does not contain any subchunks.");
				}

				await source.EnsureBufferAsync (4, cancellationToken).ConfigureAwait (false);
				var reader = new RiffChunkListReader (rootChunk.Source);
				if (reader.ListId != "AVI ")
				{
					throw new FormatException ("Specified source is not valid RIFF/AVI.");
				}

				var isMovedToNextChunk = await reader.MoveNextAsync (cancellationToken).ConfigureAwait (false);
				if (!isMovedToNextChunk)
				{
					throw new FormatException ("Specified source is not valid RIFF/AVI.");
				}

				var aviChunk = reader.Current;

				if (!aviChunk.IsSubChunkList)
				{
					throw new FormatException ("Specified source does not contain RIFF-chunk with main header 'LIST/hdrl'.");
				}

				await source.EnsureBufferAsync (4, cancellationToken).ConfigureAwait (false);
				reader = new RiffChunkListReader (aviChunk.Source);
				if ((reader == null) ||
					(reader.ListId != "hdrl"))
				{
					throw new FormatException ("Specified source does not contain RIFF-chunk with main header 'LIST/hdrl'.");
				}

				uint microSecPerFrame = 0;
				uint flags = 0;
				uint totalFrames = 0;
				uint width = 0;
				uint height = 0;
				var streamInfos = new ArrayList<AviStreamInfo> ();
				while (await reader.MoveNextAsync (cancellationToken).ConfigureAwait (false))
				{
					var chunk = reader.Current;
					if (chunk.Id == "avih")
					{
						/*
						DWORD dwMicroSecPerFrame;
						DWORD dwMaxBytesPerSec;
						DWORD dwPaddingGranularity;
						DWORD dwFlags;
						DWORD dwTotalFrames;
						DWORD dwInitialFrames;
						DWORD dwStreams;
						DWORD dwSuggestedBufferSize;
						DWORD dwWidth;
						DWORD dwHeight;
						DWORD dwReserved[4];
						*/
						await chunk.Source.EnsureBufferAsync (56, cancellationToken).ConfigureAwait (false); // "To small size of RIFF-chunk 'avih'. Expected minimum 56 bytes.");

						// TODO: сделать проще, без использования BitConverter
#if NETCOREAPP2_1
						var sourceBuf = chunk.Source.BufferMemory;
						microSecPerFrame = BitConverter.ToUInt32 (sourceBuf.Span.Slice (chunk.Source.Offset));
						flags = BitConverter.ToUInt32 (sourceBuf.Span.Slice (chunk.Source.Offset + 12));
						totalFrames = BitConverter.ToUInt32 (sourceBuf.Span.Slice (chunk.Source.Offset + 16));
						width = BitConverter.ToUInt32 (sourceBuf.Span.Slice (chunk.Source.Offset + 32));
						height = BitConverter.ToUInt32 (sourceBuf.Span.Slice (chunk.Source.Offset + 36));
#else
						var tempBuf = new byte[4];
						chunk.Source.BufferMemory.Slice (chunk.Source.Offset, 4).CopyTo (tempBuf);
						microSecPerFrame = BitConverter.ToUInt32 (tempBuf, 0);
						chunk.Source.BufferMemory.Slice (chunk.Source.Offset + 12, 4).CopyTo (tempBuf);
						flags = BitConverter.ToUInt32 (tempBuf, 0);
						chunk.Source.BufferMemory.Slice (chunk.Source.Offset + 16, 4).CopyTo (tempBuf);
						totalFrames = BitConverter.ToUInt32 (tempBuf, 0);
						chunk.Source.BufferMemory.Slice (chunk.Source.Offset + 32, 4).CopyTo (tempBuf);
						width = BitConverter.ToUInt32 (tempBuf, 0);
						chunk.Source.BufferMemory.Slice (chunk.Source.Offset + 36, 4).CopyTo (tempBuf);
						height = BitConverter.ToUInt32 (tempBuf, 0);
#endif
					}

					if (chunk.IsSubChunkList)
					{
						await chunk.Source.EnsureBufferAsync (4, cancellationToken).ConfigureAwait (false);
						var subReader = new RiffChunkListReader (chunk.Source);
						if (subReader.ListId == "strl")
						{
							var streamInfo = await AviStreamInfo.ParseAsync (subReader, cancellationToken).ConfigureAwait (false);
							streamInfos.Add (streamInfo);
						}
					}
				}

				return new AviInfo (
					microSecPerFrame,
					flags,
					totalFrames,
					width,
					height,
					streamInfos);
			}
		}
	}
}
