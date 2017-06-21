﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Подробности видео-формата потока AVI-файла.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Avi",
		Justification = "'AVI' represents standard term.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public class AviStreamInfoVideoFormat
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса AviStreamInfoVideoFormat на основе указанных данных.
		/// </summary>
		/// <param name="width">Width of the bitmap, in pixels.</param>
		/// <param name="height">Height of the bitmap, in pixels.</param>
		/// <param name="planes">Number of planes for the target device.</param>
		/// <param name="bitCount">Number of bits per pixel (bpp).</param>
		/// <param name="compression">FOURCC code specifying format of pixel data.</param>
		/// <param name="sizeImage">Size, in bytes, of the image. This can be set to 0 for uncompressed RGB bitmaps.</param>
		public AviStreamInfoVideoFormat (
			uint width,
			uint height,
			uint planes,
			uint bitCount,
			string compression,
			uint sizeImage)
		{
			this.Width = width;
			this.Height = height;
			this.Planes = planes;
			this.BitCount = bitCount;
			this.Compression = compression;
			this.SizeImage = sizeImage;
		}

		/// <summary>Gets width of the bitmap, in pixels.</summary>
		public uint Width { get; }

		/// <summary>Gets height of the bitmap, in pixels.</summary>
		public uint Height { get; }

		/// <summary>Gets number of planes for the target device.</summary>
		public uint Planes { get; }

		/// <summary>Gets number of bits per pixel (bpp).</summary>
		public uint BitCount { get; }

		/// <summary>Gets FOURCC code specifying format of pixel data.</summary>
		public string Compression { get; }

		/// <summary>Gets size, in bytes, of the image. This can be set to 0 for uncompressed RGB bitmaps.</summary>
		public uint SizeImage { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay => FormattableString.Invariant ($"Width = {this.Width}, Height = {this.Height}, Compression = {this.Compression}");

		/// <summary>
		/// Считывает подробности видео-формата потока AVI-файла. из указанного буфера.
		/// </summary>
		/// <param name="source">Буфер данных содержащий подробности видео-формата потока AVI-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Подробности видео-формата потока AVI-файла, считанные из указанного буфера.</returns>
		public static Task<AviStreamInfoVideoFormat> ParseAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.Buffer.Length < 36)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Contract.EndContractBlock ();

			Task task;
			try
			{
				task = source.EnsureBufferAsync (36, cancellationToken);
			}
			catch (NotEnoughDataException exception)
			{
				throw new FormatException (
					"Insuficient size of RIFF-chunk 'strf' for stream of type 'vids'. Expected minimum 36 bytes.",
					exception);
			}

			return ParseAsyncFinalizer ();

			async Task<AviStreamInfoVideoFormat> ParseAsyncFinalizer ()
			{
				try
				{
					await task.ConfigureAwait (false);
				}
				catch (NotEnoughDataException exception)
				{
					throw new FormatException (
						"Insuficient size of RIFF-chunk 'strf' for stream of type 'vids'. Expected minimum 36 bytes.",
						exception);
				}

				/*
					LONG	biWidth;
					LONG	biHeight;
					WORD	biPlanes;
					WORD	biBitCount;
					DWORD	biCompression;
					DWORD	biSizeImage;
					LONG	biXPelsPerMeter;
					LONG	biYPelsPerMeter;
					DWORD	biClrUsed;
					DWORD	biClrImportant;
				*/
				var compressionNumber = BitConverter.ToUInt32 (source.Buffer, source.Offset + 16);
				var codecId = (compressionNumber >= 0x20202020) ?
					AsciiCharSet.GetString (source.Buffer, source.Offset + 16, 4) :
					compressionNumber.ToString (CultureInfo.InvariantCulture);
				var videoInfo = new AviStreamInfoVideoFormat (
					BitConverter.ToUInt32 (source.Buffer, source.Offset + 4),
					BitConverter.ToUInt32 (source.Buffer, source.Offset + 8),
					BitConverter.ToUInt32 (source.Buffer, source.Offset + 12),
					BitConverter.ToUInt32 (source.Buffer, source.Offset + 14),
					codecId,
					BitConverter.ToUInt32 (source.Buffer, source.Offset + 20));
				return videoInfo;
			}
		}
	}
}
