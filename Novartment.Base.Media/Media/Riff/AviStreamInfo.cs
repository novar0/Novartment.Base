using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Text;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Параметры потока AVI-файла.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Avi",
		Justification = "'AVI' represents standard term.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public class AviStreamInfo
	{
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
		/// <param name="left">Left of destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="top">Top of destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="right">Right of destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="bottom">Bottom of destination rectangle for a text or video stream within the movie rectangle.</param>
		/// <param name="videoInfo">Подробности видео-формата потока AVI-файла.</param>
		/// <param name="audioInfo">Подробности аудио-формата потока AVI-файла.</param>
		public AviStreamInfo (
			string type,
			string handler,
			uint options,
			ushort priority,
			ushort language,
			uint scale,
			uint rate,
			uint start,
			uint length,
			uint sampleSize,
			ushort left,
			ushort top,
			ushort right,
			ushort bottom,
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

		/// <summary>Gets FOURCC that specifies the type of the data contained in the stream.
		/// Possible values are 'auds', 'mids', 'txts' and 'vids'.</summary>
		public string Kind { get; }

		/// <summary>Gets FOURCC that identifies a specific data handler.</summary>
		public string Handler { get; }

		/// <summary>Gets options for the data stream:
		/// Disabled=1 (Indicates this stream should not be enabled by default),
		/// VideoPaletteChanges=0x10000 (Indicates this video stream contains palette changes).</summary>
		public uint Options { get; }

		/// <summary>Gets priority of a stream.</summary>
		public ushort Priority { get; }

		/// <summary>Gets language tag.</summary>
		public ushort Language { get; }

		/// <summary>Gets scale, used with rate to specify the time scale that this stream will use.</summary>
		public uint Scale { get; }

		/// <summary>Gets rate, used with scale to specify the time scale that this stream will use.</summary>
		public uint Rate { get; }

		/// <summary>Gets starting time for this stream.</summary>
		public uint Start { get; }

		/// <summary>Gets length of this stream.</summary>
		public uint Length { get; }

		/// <summary>Gets size of a single sample of data.</summary>
		public uint SampleSize { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public ushort Left { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public ushort Top { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public ushort Right { get; }

		/// <summary>Gets destination rectangle for a text or video stream within the movie rectangle.</summary>
		public ushort Bottom { get; }

		/// <summary>Gets подробности видео-формата потока AVI-файла.</summary>
		public AviStreamInfoVideoFormat VideoFormat { get; }

		/// <summary>Gets подробности аудио-формата потока AVI-файла.</summary>
		public AviStreamInfoAudioFormat AudioFormat { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay => FormattableString.Invariant ($"Type = {this.Kind}, Handler = {this.Handler}, Length = {this.Length}");

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

			return ParseAsyncStateMachine ();

			async Task<AviStreamInfo> ParseAsyncStateMachine ()
			{
				string type = null;
				string handler = null;
				uint options = 0;
				ushort priority = 0;
				ushort language = 0;
				uint scale = 0;
				uint rate = 0;
				uint start = 0;
				uint length = 0;
				uint sampleSize = 0;
				ushort left = 0;
				ushort top = 0;
				ushort right = 0;
				ushort bottom = 0;
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
		}
	}
}
