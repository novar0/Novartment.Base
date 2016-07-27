using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Подробности аудио-формата потока AVI-файла.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Avi",
		Justification = "'AVI' represents standard term."),
	DebuggerDisplay ("{DebuggerDisplay,nq}"),
	CLSCompliant (false)]
	public class AviStreamInfoAudioFormat
	{
		/// <summary>Gets waveform-audio format type.</summary>
		public UInt16 FormatTag { get; }

		/// <summary>Gets number of channels in the waveform-audio data.</summary>
		public UInt16 Channels { get; }

		/// <summary>Gets sample rate, in samples per second (hertz).</summary>
		public UInt32 SamplesPerSec { get; }

		/// <summary>Gets average data-transfer rate, in bytes per second.</summary>
		public UInt32 AverageBytesPerSecond { get; }

		/// <summary>Gets block alignment, in bytes.</summary>
		public UInt16 BlockAlign { get; }

		/// <summary>Gets bits per sample.</summary>
		public UInt16 BitsPerSample { get; }

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
			UInt16 formatTag,
			UInt16 channels,
			UInt32 samplesPerSec,
			UInt32 averageBytesPerSecond,
			UInt16 blockAlign,
			UInt16 bitsPerSample)
		{
			this.FormatTag = formatTag;
			this.Channels = channels;
			this.SamplesPerSec = samplesPerSec;
			this.AverageBytesPerSecond = averageBytesPerSecond;
			this.BlockAlign = blockAlign;
			this.BitsPerSample = bitsPerSample;
		}

		/// <summary>
		/// Считывает подробности аудио-формата потока AVI-файла. из указанного буфера.
		/// </summary>
		/// <param name="source">Буфер данных содержащий подробности аудио-формата потока AVI-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Подробности аудио-формата потока AVI-файла, считанные из указанного буфера.</returns>
		public static Task<AviStreamInfoAudioFormat> ParseAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			if (source.Buffer.Length < 18)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}
			Contract.EndContractBlock ();

			Task task;
			try
			{
				task = source.EnsureBufferAsync (18, cancellationToken);
			}
			catch (NotEnoughDataException exception)
			{
				throw new FormatException (
					"Insuficient size of RIFF-chunk 'strf' for stream of type 'auds'. Expected minimum 18 bytes.",
					exception);
			}
			return ParseAsyncFinalizer (task, source);
		}

		private static async Task<AviStreamInfoAudioFormat> ParseAsyncFinalizer (Task task, IBufferedSource source)
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
			var formatTag = BitConverter.ToUInt16 (source.Buffer, source.Offset);
			var channels = BitConverter.ToUInt16 (source.Buffer, source.Offset + 2);
			var samplesPerSec = BitConverter.ToUInt32 (source.Buffer, source.Offset + 4);
			var averageBytesPerSecond = BitConverter.ToUInt32 (source.Buffer, source.Offset + 8);
			var blockAlign = BitConverter.ToUInt16 (source.Buffer, source.Offset + 12);
			var bitsPerSample = BitConverter.ToUInt16 (source.Buffer, source.Offset + 14);

			return new AviStreamInfoAudioFormat (
				formatTag,
				channels,
				samplesPerSec,
				averageBytesPerSecond,
				blockAlign,
				bitsPerSample);
		}

		[DebuggerBrowsable (DebuggerBrowsableState.Never),
		SuppressMessage ("Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay
		{
			get
			{
				return FormattableString.Invariant ($"FormatTag = {this.FormatTag}, Channels = {this.Channels}, SamplesPerSec = {this.SamplesPerSec}");
			}
		}
	}
}
