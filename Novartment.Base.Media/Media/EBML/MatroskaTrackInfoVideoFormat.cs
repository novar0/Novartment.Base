using System;
using System.Diagnostics;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Информация об видео-трэке матрёшка-файла.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public sealed class MatroskaTrackInfoVideoFormat
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
		private string DebuggerDisplay => FormattableString.Invariant ($"Width = {this.PixelWidth}, Height = {this.PixelHeight}");
	}
}
