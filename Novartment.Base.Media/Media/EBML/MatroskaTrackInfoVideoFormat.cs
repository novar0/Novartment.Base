using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Информация об видео-трэке матрёшка-файла.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Matroska",
		Justification = "'Matroska' represents standard term.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public class MatroskaTrackInfoVideoFormat
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса MatroskaTrackInfoVideoFormat на основе указанных данных.
		/// </summary>
		/// <param name="pixelWidth">Количество пикселей по горизонтали.</param>
		/// <param name="pixelHeight">Количество отображаемых пикселей по горизонтали.</param>
		/// <param name="displayWidth">Количество пикселей по горизонтали для отображения.</param>
		/// <param name="displayHeight">Количество пикселей по вертикали для отображения.</param>
		public MatroskaTrackInfoVideoFormat (ulong? pixelWidth, ulong? pixelHeight, ulong? displayWidth, ulong? displayHeight)
		{
			this.PixelWidth = pixelWidth;
			this.PixelHeight = pixelHeight;
			this.DisplayWidth = displayWidth;
			this.DisplayHeight = displayHeight;
		}

		/// <summary>
		/// Получает количество пикселей по горизонтали.
		/// </summary>
		public ulong? PixelWidth { get; }

		/// <summary>
		/// Получает количество пикселей по вертикали.
		/// </summary>
		public ulong? PixelHeight { get; }

		/// <summary>
		/// Получает количество пикселей по горизонтали для отображения.
		/// </summary>
		public ulong? DisplayWidth { get; }

		/// <summary>
		/// Получает количество пикселей по вертикали для отображения.
		/// </summary>
		public ulong? DisplayHeight { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		[SuppressMessage (
		"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay => FormattableString.Invariant ($"Width = {this.PixelWidth}, Height = {this.PixelHeight}");
	}
}
