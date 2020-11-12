using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Подробности аудио-формата потока AVI-файла.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public sealed class AviStreamInfoAudioFormat
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса AviStreamInfoAudioFormat на основе указанных данных.
		/// </summary>
		/// <param name="formatTag">Waveform-audio format type.</param>
		/// <param name="channels">Number of channels in the waveform-audio data.</param>
		/// <param name="samplesPerSec">Sample rate, in samples per second (hertz).</param>
		/// <param name="averageBytesPerSecond">Average data-transfer rate, in bytes per second.</param>
		/// <param name="blockAlign">Block alignment, in bytes.</param>
		/// <param name="bitsPerSample">Bits per sample.</param>
		public AviStreamInfoAudioFormat (
			ushort formatTag,
			ushort channels,
			uint samplesPerSec,
			uint averageBytesPerSecond,
			ushort blockAlign,
			ushort bitsPerSample)
		{
			this.FormatTag = formatTag;
			this.Channels = channels;
			this.SamplesPerSec = samplesPerSec;
			this.AverageBytesPerSecond = averageBytesPerSecond;
			this.BlockAlign = blockAlign;
			this.BitsPerSample = bitsPerSample;
		}

		/// <summary>Gets waveform-audio format type.</summary>
		public ushort FormatTag { get; }

		/// <summary>Gets number of channels in the waveform-audio data.</summary>
		public ushort Channels { get; }

		/// <summary>Gets sample rate, in samples per second (hertz).</summary>
		public uint SamplesPerSec { get; }

		/// <summary>Gets average data-transfer rate, in bytes per second.</summary>
		public uint AverageBytesPerSecond { get; }

		/// <summary>Gets block alignment, in bytes.</summary>
		public ushort BlockAlign { get; }

		/// <summary>Gets bits per sample.</summary>
		public ushort BitsPerSample { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => FormattableString.Invariant ($"FormatTag = {this.FormatTag}, Channels = {this.Channels}, SamplesPerSec = {this.SamplesPerSec}");

		/// <summary>
		/// Считывает подробности аудио-формата потока AVI-файла. из указанного буфера.
		/// </summary>
		/// <param name="source">Буфер данных содержащий подробности аудио-формата потока AVI-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Подробности аудио-формата потока AVI-файла, считанные из указанного буфера.</returns>
		public static Task<AviStreamInfoAudioFormat> ParseAsync (IBufferedSource source, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.BufferMemory.Length < 18)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Contract.EndContractBlock ();

			Task task;
			try
			{
				task = source.EnsureAvailableAsync (18, cancellationToken).AsTask ();
			}
			catch (NotEnoughDataException exception)
			{
				throw new FormatException (
					"Insuficient size of RIFF-chunk 'strf' for stream of type 'auds'. Expected minimum 18 bytes.",
					exception);
			}

			return ParseAsyncFinalizer ();

			async Task<AviStreamInfoAudioFormat> ParseAsyncFinalizer ()
			{
				try
				{
					await task.ConfigureAwait (false);
				}
				catch (NotEnoughDataException exception)
				{
					throw new FormatException (
						"Insuficient size of RIFF-chunk 'strf' for stream of type 'auds'. Expected minimum 18 bytes.",
						exception);
				}

				/*  WORD	wFormatTag;
					WORD	nChannels;
					DWORD	nSamplesPerSec;
					DWORD	nAvgBytesPerSec;
					WORD	nBlockAlign;
					WORD	wBitsPerSample;
					WORD	cbSize;
				*/

				var sourceBuf = source.BufferMemory;
				var formatTag = BinaryPrimitives.ReadUInt16LittleEndian (sourceBuf.Span[source.Offset..]);
				var channels = BinaryPrimitives.ReadUInt16LittleEndian (sourceBuf.Span[(source.Offset + 2)..]);
				var samplesPerSec = BinaryPrimitives.ReadUInt32LittleEndian (sourceBuf.Span[(source.Offset + 4)..]);
				var averageBytesPerSecond = BinaryPrimitives.ReadUInt32LittleEndian (sourceBuf.Span[(source.Offset + 8)..]);
				var blockAlign = BinaryPrimitives.ReadUInt16LittleEndian (sourceBuf.Span[(source.Offset + 12)..]);
				var bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian (sourceBuf.Span[(source.Offset + 14)..]);

				return new AviStreamInfoAudioFormat (
					formatTag,
					channels,
					samplesPerSec,
					averageBytesPerSecond,
					blockAlign,
					bitsPerSample);
			}
		}
	}
}
