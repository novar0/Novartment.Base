using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Информация об аудио-трэке матрёшка-файла.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Matroska",
		Justification = "'Matroska' represents standard term.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public class MatroskaTrackInfoAudioFormat
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса MatroskaTrackInfoAudioFormat на основе указанных данных.
		/// </summary>
		/// <param name="samplingFrequency">Частота дискретизации трэка.</param>
		/// <param name="channels">Количество каналов трэка.</param>
		public MatroskaTrackInfoAudioFormat (double? samplingFrequency, ulong? channels)
		{
			this.SamplingFrequency = samplingFrequency;
			this.Channels = channels;
		}

		/// <summary>
		/// Получает частоту дискретизации трэка.
		/// </summary>
		public double? SamplingFrequency { get; }

		/// <summary>
		/// Получает количество каналов трэка.
		/// </summary>
		public ulong? Channels { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		[SuppressMessage (
		"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay => FormattableString.Invariant ($"SamplingFrequency = {this.SamplingFrequency}, Channels = {this.Channels}");
	}
}
