using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

namespace Novartment.Base.Media
{
/*
типичная структура mkv-сегмента

[11][4D][9B][74]	SeekHead
[EC]				Void
[15][49][A9][66]	Info
[16][54][AE][6B]	Tracks
[EC]				Void
[10][43][A7][70]	Chapters
[EC]				Void
[1F][43][B6][75]	Cluster
...
[1F][43][B6][75]	Cluster
[1C][53][BB][6B]	Cues
[11][4D][9B][74]	SeekHead
[EC]				Void
[15][49][A9][66]	Info
[16][54][AE][6B]	Tracks
[EC]				Void
[1F][43][B6][75]	Cluster
...
[1F][43][B6][75]	Cluster-
[1C][53][BB][6B]	Cues
 */
	/// <summary>
	/// Суммарная информация о сегменте matroska-файла.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Matroska",
		Justification = "'Matroska' represents standard term."),
	DebuggerDisplay ("{DebuggerDisplay,nq}"),
	CLSCompliant (false)]
	public class MatroskaSegmentInfo
	{
		/// <summary>
		/// Получает название сегмента.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Получает дату/время сегмента.
		/// </summary>
		public DateTime? Date { get; }

		/// <summary>
		/// Получает продолжительность сегмента.
		/// </summary>
		public double? Duration { get; }

		/// <summary>
		/// Получает масштаб времени сегмента.
		/// </summary>
		public ulong? TimeCodeScale { get; }

		/// <summary>
		/// Получает список трэков сегмента.
		/// </summary>
		public IReadOnlyList<MatroskaTrackInfo> Tracks { get; }

		/// <summary>
		/// Получает присоединённые элементы сегмента.
		/// </summary>
		public IReadOnlyList<MatroskaAttachedFileInfo> Attachments { get; }

		/// <summary>
		/// Инициализирует новый экземпляр класса MatroskaSegmentInfo на основе указанных данных.
		/// </summary>
		/// <param name="title">Название сегмента</param>
		/// <param name="date">Дата/время сегмента</param>
		/// <param name="duration">Продолжительность сегмента</param>
		/// <param name="timeCodeScale">Масштаб времени сегмента</param>
		/// <param name="tracks">Список трэков сегмента</param>
		/// <param name="attachments">Присоединённые элементы сегмента</param>
		public MatroskaSegmentInfo (
			string title,
			DateTime? date,
			double? duration,
			ulong? timeCodeScale,
			IReadOnlyList<MatroskaTrackInfo> tracks,
			IReadOnlyList<MatroskaAttachedFileInfo> attachments)
		{
			this.Title = title;
			this.Date = date;
			this.Duration = duration;
			this.TimeCodeScale = timeCodeScale;
			this.Tracks = tracks;
			this.Attachments = attachments;
		}

		/// <summary>
		/// Создаёт cуммарную информация о сегменте matroska-файла на основе указанной коллекции EBML-элементов.
		/// </summary>
		/// <param name="source">Коллекция EBML-элементов, представляющая информацию о сегменте matroska-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, представляющая операцию. Результатом будет выполнения задачи
		/// суммарная информация о сегменте matroska-файла на основе указанной коллекции EBML-элементов.</returns>
		public static Task<MatroskaSegmentInfo> ParseAsync (
			EbmlElementCollectionEnumerator source,
			CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			return ParseAsyncStateMachine (source, cancellationToken);
		}

		private static async Task<MatroskaSegmentInfo> ParseAsyncStateMachine (
			EbmlElementCollectionEnumerator source,
			CancellationToken cancellationToken)
		{
			string title = null;
			DateTime? date = null;
			double? duration = null;
			ulong? timeCodeScale = null;
			var tracks = new ArrayList<MatroskaTrackInfo> ();
			var attachments = new ArrayList<MatroskaAttachedFileInfo> ();
			bool clusterFound = false; // признак начала данных (окончания заголовка)
			do
			{
				var isMovedToNext = await source.MoveNextAsync (cancellationToken).ConfigureAwait (false);
				if (!isMovedToNext)
				{
					break;
				}
				switch (source.Current.Id)
				{
					case 0x1f43b675UL: // Cluster
						clusterFound = true; // начались данные, просмотр заголовка оканчиваем
						break;
					case 0x1549a966L: // Info
						var reader1 = source.Current.ReadSubElements ();
						while (true)
						{
							var isMovedToNext2 = await reader1.MoveNextAsync (cancellationToken).ConfigureAwait (false);
							if (!isMovedToNext2)
							{
								break;
							}
							switch (reader1.Current.Id)
							{
								case 0x2ad7b1UL: // TimeCodeScale
									timeCodeScale = reader1.Current.ReadUInt ();
									break;
								case 0x4489UL: // Duration
									duration = reader1.Current.ReadFloat ();
									break;
								case 0x4461UL: // DateUTC
									date = reader1.Current.ReadDate ();
									break;
								case 0x7ba9UL: // Title
									title = reader1.Current.ReadUtf ();
									break;
							}
						}
						break;
					case 0x1654ae6bUL: // Tracks
						await ProcessTracksEntryAsync (source.Current.ReadSubElements (), tracks, cancellationToken).ConfigureAwait (false);
						break;
					case 0x1941a469UL: // Attachments
						var reader6 = source.Current.ReadSubElements ();
						while (true)
						{
							var isMovedToNext2 = await reader6.MoveNextAsync (cancellationToken).ConfigureAwait (false);
							if (!isMovedToNext2)
							{
								break;
							}
							if (reader6.Current.Id == 0x61a7UL) // AttachedFile
							{
								var reader7 = reader6.Current.ReadSubElements ();
								string fileName = null;
								string fileMimeType = null;
								while (true)
								{
									var isMovedToNext3 = await reader7.MoveNextAsync (cancellationToken).ConfigureAwait (false);
									if (!isMovedToNext3)
									{
										break;
									}
									switch (reader7.Current.Id)
									{
										case 0x466eUL: // FileName
											fileName = reader7.Current.ReadUtf ();
											break;
										case 0x4660UL: // FileMimeType
											fileMimeType = reader7.Current.ReadAscii ();
											break;
									}
								}
								attachments.Add (new MatroskaAttachedFileInfo (fileName, fileMimeType));
							}
						}
						break;
				}
			} while (!clusterFound);

			return new MatroskaSegmentInfo (
				title,
				date,
				duration,
				timeCodeScale,
				tracks,
				attachments);
		}

		private static async Task ProcessTracksEntryAsync (
			EbmlElementCollectionEnumerator reader,
			IAdjustableCollection<MatroskaTrackInfo> tracks,
			CancellationToken cancellationToken)
		{
			while (true)
			{
				var isMovedToNext = await reader.MoveNextAsync (cancellationToken).ConfigureAwait (false);
				if (!isMovedToNext)
				{
					break;
				}
				if (reader.Current.Id == 0xaeUL) // TrackEntry
				{
					ulong? trackType = null;
					var forced = false;
					string codec = null;
					ulong? defaultDuration = null;
					string language = null;
					string name = null;
					MatroskaTrackInfoVideoFormat videoFormat = null;
					MatroskaTrackInfoAudioFormat audioFormat = null;
					var reader3 = reader.Current.ReadSubElements ();
					while (true)
					{
						var isMovedToNext2 = await reader3.MoveNextAsync (cancellationToken).ConfigureAwait (false);
						if (!isMovedToNext2)
						{
							break;
						}
						switch (reader3.Current.Id)
						{
							case 0x83UL: // TrackType
								trackType = reader3.Current.ReadUInt ();
								break;
							case 0x55aaUL: // FlagForced
								forced = reader3.Current.ReadUInt () != 0;
								break;
							case 0x86UL: // CodecId
								codec = reader3.Current.ReadAscii ();
								break;
							case 0x23e383UL: // DefaultDuration
								defaultDuration = reader3.Current.ReadUInt ();
								break;
							case 0x22b59cUL: // Language
								language = reader3.Current.ReadAscii ();
								break;
							case 0x536eUL: // Name
								name = reader3.Current.ReadUtf ();
								break;
							case 0xe0UL: // Video
								var reader4 = reader3.Current.ReadSubElements ();
								ulong? pixelWidth = null;
								ulong? pixelHeight = null;
								ulong? displayWidth = null;
								ulong? displayHeight = null;
								while (true)
								{
									var isMovedToNext3 = await reader4.MoveNextAsync (cancellationToken).ConfigureAwait (false);
									if (!isMovedToNext3)
									{
										break;
									}
									switch (reader4.Current.Id)
									{
										case 0xb0UL: // PixelWidth
											pixelWidth = reader4.Current.ReadUInt ();
											break;
										case 0xbaUL: // PixelHeight
											pixelHeight = reader4.Current.ReadUInt ();
											break;
										case 0x54b0UL: // DisplayWidth
											displayWidth = reader4.Current.ReadUInt ();
											break;
										case 0x54baUL: // DisplayHeight
											displayHeight = reader4.Current.ReadUInt ();
											break;
									}
								}
								videoFormat = new MatroskaTrackInfoVideoFormat (
									pixelWidth,
									pixelHeight,
									displayWidth,
									displayHeight);
								break;
							case 0xe1UL: // Audio
								var reader5 = reader3.Current.ReadSubElements ();
								double? samplingFrequency = null;
								ulong? channels = null;
								while (true)
								{
									var isMovedToNext3 = await reader5.MoveNextAsync (cancellationToken).ConfigureAwait (false);
									if (!isMovedToNext3)
									{
										break;
									}
									switch (reader5.Current.Id)
									{
										case 0xb5UL: // SamplingFrequency
											samplingFrequency = reader5.Current.ReadFloat ();
											break;
										case 0x9fUL: // Channels
											channels = reader5.Current.ReadUInt ();
											break;
									}
								}
								audioFormat = new MatroskaTrackInfoAudioFormat (samplingFrequency, channels);
								break;
						}
					}
					var track = new MatroskaTrackInfo (
						(MatroskaTrackType)trackType,
						forced,
						codec,
						defaultDuration,
						language,
						name,
						videoFormat,
						audioFormat);
					tracks.Add (track);
				}
			}
		}

		[DebuggerBrowsable (DebuggerBrowsableState.Never),
		SuppressMessage ("Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay
		{
			get
			{
				return FormattableString.Invariant ($"Date = {this.Date}, Duration = {this.Duration}, Tracks = {this.Tracks.Count}, Attachments = {this.Attachments.Count}");
			}
		}
	}
}
