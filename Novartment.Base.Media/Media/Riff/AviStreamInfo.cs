using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Параметры потока AVI-файла.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Avi",
		Justification = "'AVI' represents standard term."),
	DebuggerDisplay ("{DebuggerDisplay,nq}"),
	CLSCompliant (false)]
	public class AviStreamInfo
	{
		/// <summary>Gets FOURCC that specifies the type of the data contained in the stream.
		/// Possible values are 'auds', 'mids', 'txts' and 'vids'.</summary>
		public string Kind { get; }

		/// <summary>Gets FOURCC that identifies a specific data handler.</summary>
		public string Handler { get; }

		/// <summary>Gets options for the data stream:
		/// Disabled=1 (Indicates this stream should not be enabled by default),
		/// VideoPaletteChanges=0x10000 (Indicates this video stream contains palette changes).</summary>
		public UInt32 Options { get; }

		/// <summary>Gets priority of a stream.</summary>
		public UInt16 Priority { get; }

		/// <summary>Gets language tag.</summary>
		public UInt16 Language { get; }

		/// <summary>Gets scale, used with rate to specify the time scale that this stream will use.</summary>
		public UInt32 Scale { get; }

		/// <summary>Gets rate, used with scale to specify the time scale that this stream will use.</summary>
		public UInt32 Rate { get; }

		/// <summary>Gets starting time for this stream.</summary>
		public UInt32 Start { get; }

		/// <summary>Gets length of this stream.</summary>
		public UInt32 Length { get; }

		/// <summary>Gets size of a single sample of data.</summary>
		public UInt32 SampleSize { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public UInt16 Left { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public UInt16 Top { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public UInt16 Right { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public UInt16 Bottom { get; }

		/// <summary>Gets подробности видео-формата потока AVI-файла.</summary>
		public AviStreamInfoVideoFormat VideoFormat { get; }

		/// <summary>Gets подробности аудио-формата потока AVI-файла.</summary>
		public AviStreamInfoAudioFormat AudioFormat { get; }

		/// <summary>
		/// Инициализирует новый экземпляр класса AviStreamInfo на основе указанных данных.
		/// </summary>
		/// <param name="type">FOURCC that specifies the type of the data contained in the stream. Possible values are 'auds', 'mids', 'txts' and 'vids'.</param>
		/// <param name="handler">FOURCC that identifies a specific data handler.</param>
		/// <param name="options">options for the data stream.</param>
		/// <param name="priority">priority of a stream.</param>
		/// <param name="language">language tag.</param>
		/// <param name="scale">scale, used with rate to specify the time scale that this stream will use.</param>
		/// <param name="rate">rate, used with scale to specify the time scale that this stream will use.</param>
		/// <param name="start">starting time for this stream.</param>
		/// <param name="length">length of this stream.</param>
		/// <param name="sampleSize">size of a single sample of data.</param>
		/// <param name="left">Destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="top">Destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="right">Destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="bottom">Destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="videoInfo">Подробности видео-формата потока AVI-файла.</param>
		/// <param name="audioInfo">Подробности аудио-формата потока AVI-файла.</param>
		public AviStreamInfo (
			string type,
			string handler,
			UInt32 options,
			UInt16 priority,
			UInt16 language,
			UInt32 scale,
			UInt32 rate,
			UInt32 start,
			UInt32 length,
			UInt32 sampleSize,
			UInt16 left,
			UInt16 top,
			UInt16 right,
			UInt16 bottom,
			AviStreamInfoVideoFormat videoInfo,
			AviStreamInfoAudioFormat audioInfo)
		{
			this.Kind = type;
			this.Handler = handler;
			this.Options = options;
			this.Priority = priority;
			this.Language = language;
			this.Scale = scale;
			this.Rate = rate;
			this.Start = start;
			this.Length = length;
			this.SampleSize = sampleSize;
			this.Left = left;
			this.Top = top;
			this.Right = right;
			this.Bottom = bottom;
			this.VideoFormat = videoInfo;
			this.AudioFormat = audioInfo;
		}

		/// <summary>
		/// Считывает параметры потока AVI-файла из указанноq коллеции RIFF-порций.
		/// </summary>
		/// <param name="chunkListReader">Коллеции RIFF-порций, содержащая параметры потока AVI-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Параметры потока AVI-файла.</returns>
		public static Task<AviStreamInfo> ParseAsync (RiffChunkListReader chunkListReader, CancellationToken cancellationToken)
		{
			if (chunkListReader == null)
			{
				throw new ArgumentNullException (nameof (chunkListReader));
			}
			Contract.EndContractBlock ();

			return ParseAsyncStateMachine (chunkListReader, cancellationToken);
		}

		private static async Task<AviStreamInfo> ParseAsyncStateMachine (
			RiffChunkListReader chunkListReader,
			CancellationToken cancellationToken)
		{
			string type = null;
			string handler = null;
			UInt32 options = 0;
			UInt16 priority = 0;
			UInt16 language = 0;
			UInt32 scale = 0;
			UInt32 rate = 0;
			UInt32 start = 0;
			UInt32 length = 0;
			UInt32 sampleSize = 0;
			UInt16 left = 0;
			UInt16 top = 0;
			UInt16 right = 0;
			UInt16 bottom = 0;
			AviStreamInfoVideoFormat videoInfo = null;
			AviStreamInfoAudioFormat audioInfo = null;
			while (await chunkListReader.MoveNextAsync (cancellationToken).ConfigureAwait (false))
			{
				var chunk = chunkListReader.Current;
				if (chunk.Id == "strf")
				{
					if (type == "vids")
					{
						videoInfo = await AviStreamInfoVideoFormat.ParseAsync (chunk.Source, cancellationToken).ConfigureAwait (false);
					}
					else
					{
						if (type == "auds")
						{
							audioInfo = await AviStreamInfoAudioFormat.ParseAsync (chunk.Source, cancellationToken).ConfigureAwait (false);
						}
					}
				}
				else
				{
					if (chunk.Id == "strh")
					{
						/*
							FOURCC	fccType;
							FOURCC	fccHandler;
							DWORD	dwFlags;
							WORD	wPriority;
							WORD	wLanguage;
							DWORD	dwInitialFrames;
							DWORD	dwScale;
							DWORD	dwRate;
							DWORD	dwStart;
							DWORD	dwLength;
							DWORD	dwSuggestedBufferSize;
							DWORD	dwQuality;
							DWORD	dwSampleSize;
							short int left;
							short int top;
							short int right;
							short int bottom;
						 */
						await chunk.Source.EnsureBufferAsync (56, cancellationToken).ConfigureAwait (false); // "Insuficient size of RIFF-chunk 'strh'. Expected minimum 56 bytes.");
						type = AsciiCharSet.GetString (chunk.Source.Buffer, chunk.Source.Offset, 4);
						var handlerNumber = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 4);
						handler = (handlerNumber == 0) ? null : (handlerNumber >= 0x20202020) ?
								AsciiCharSet.GetString (chunk.Source.Buffer, chunk.Source.Offset + 4, 4) :
								handlerNumber.ToString (CultureInfo.InvariantCulture);
						options = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 8);
						priority = BitConverter.ToUInt16 (chunk.Source.Buffer, chunk.Source.Offset + 12);
						language = BitConverter.ToUInt16 (chunk.Source.Buffer, chunk.Source.Offset + 14);
						scale = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 20);
						rate = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 24);
						start = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 28);
						length = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 32);
						sampleSize = BitConverter.ToUInt32 (chunk.Source.Buffer, chunk.Source.Offset + 44);
						left = BitConverter.ToUInt16 (chunk.Source.Buffer, chunk.Source.Offset + 48);
						top = BitConverter.ToUInt16 (chunk.Source.Buffer, chunk.Source.Offset + 50);
						right = BitConverter.ToUInt16 (chunk.Source.Buffer, chunk.Source.Offset + 52);
						bottom = BitConverter.ToUInt16 (chunk.Source.Buffer, chunk.Source.Offset + 54);
					}
				}
			}

			return new AviStreamInfo (
				type,
				handler,
				options,
				priority,
				language,
				scale,
				rate,
				start,
				length,
				sampleSize,
				left,
				top,
				right,
				bottom,
				videoInfo,
				audioInfo);
		}

		[DebuggerBrowsable (DebuggerBrowsableState.Never),
		SuppressMessage ("Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay
		{
			get
			{
				return FormattableString.Invariant ($"Type = {this.Kind}, Handler = {this.Handler}, Length = {this.Length}");
			}
		}
	}
}
