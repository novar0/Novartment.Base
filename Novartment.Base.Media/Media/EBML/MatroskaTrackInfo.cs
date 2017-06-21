using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Информация о трэке матрёшка-файла.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Matroska",
		Justification = "'Matroska' represents standard term.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public class MatroskaTrackInfo
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса MatroskaTrackInfo на основе указанных данных.
		/// </summary>
		/// <param name="trackType">Разновидность трэка.</param>
		/// <param name="forced">Признак принудительного использования трэка.</param>
		/// <param name="codec">Кодек трэка.</param>
		/// <param name="defaultDuration">Продолжительность трэка по умолчанию.</param>
		/// <param name="language">Язык трэка.</param>
		/// <param name="name">Название трэка.</param>
		/// <param name="videoFormat">Видео-формат.</param>
		/// <param name="audioFormat">Аудио-формат.</param>
		public MatroskaTrackInfo (
			MatroskaTrackType? trackType,
			bool forced,
			string codec,
			ulong? defaultDuration,
			string language,
			string name,
			MatroskaTrackInfoVideoFormat videoFormat,
			MatroskaTrackInfoAudioFormat audioFormat)
		{
			this.TrackType = trackType;
			this.Forced = forced;
			this.Codec = codec;
			this.DefaultDuration = defaultDuration;
			this.Language = language;
			this.Name = name;
			this.VideoFormat = videoFormat;
			this.AudioFormat = audioFormat;
		}

		/// <summary>
		/// Получает разновидность трэка.
		/// </summary>
		public MatroskaTrackType? TrackType { get; }

		/// <summary>
		/// Получает признак принудительного использования трэка.
		/// </summary>
		public bool Forced { get; }

		/// <summary>
		/// Получает кодек трэка.
		/// </summary>
		public string Codec { get; }

		/// <summary>
		/// Получает продолжительность трэка по умолчанию.
		/// </summary>
		public ulong? DefaultDuration { get; }

		/// <summary>
		/// Получает язык трэка.
		/// </summary>
		public string Language { get; }

		/// <summary>
		/// Получает название трэка.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает видео-формат.
		/// </summary>
		public MatroskaTrackInfoVideoFormat VideoFormat { get; }

		/// <summary>
		/// Получает аудио-формат.
		/// </summary>
		public MatroskaTrackInfoAudioFormat AudioFormat { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay => FormattableString.Invariant ($"Type = {this.TrackType}, Codec = {this.Codec}");
	}
}
