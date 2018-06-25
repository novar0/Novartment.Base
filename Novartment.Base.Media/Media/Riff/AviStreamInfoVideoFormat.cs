using System;
using System.Diagnostics;
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
		private string DebuggerDisplay => FormattableString.Invariant ($"Width = {this.Width}, Height = {this.Height}, Compression = {this.Compression}");

		/// <summary>
		/// Считывает подробности видео-формата потока AVI-файла. из указанного буфера.
		/// </summary>
		/// <param name="source">Буфер данных содержащий подробности видео-формата потока AVI-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Подробности видео-формата потока AVI-файла, считанные из указанного буфера.</returns>
		public static Task<AviStreamInfoVideoFormat> ParseAsync (IBufferedSource source, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.BufferMemory.Length < 36)
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

				// TODO: сделать проще, без использования BitConverter
				var sourceBuf = source.BufferMemory;
#if NETCOREAPP2_1
				var compressionNumber = BitConverter.ToUInt32 (sourceBuf.Span.Slice (source.Offset + 16));
				var width = BitConverter.ToUInt32 (sourceBuf.Span.Slice (source.Offset + 4));
				var height = BitConverter.ToUInt32 (sourceBuf.Span.Slice (source.Offset + 8));
				var planes = BitConverter.ToUInt32 (sourceBuf.Span.Slice (source.Offset + 12));
				var bitCount = BitConverter.ToUInt32 (sourceBuf.Span.Slice (source.Offset + 14));
				var sizeImage = BitConverter.ToUInt32 (sourceBuf.Span.Slice (source.Offset + 20));
#else
				var tempBuf = new byte[4];
				sourceBuf.Slice (source.Offset + 16, 4).CopyTo (tempBuf);
				var compressionNumber = BitConverter.ToUInt32 (tempBuf, 0);
				sourceBuf.Slice (source.Offset + 4, 4).CopyTo (tempBuf);
				var width = BitConverter.ToUInt32 (tempBuf, 0);
				sourceBuf.Slice (source.Offset + 8, 4).CopyTo (tempBuf);
				var height = BitConverter.ToUInt32 (tempBuf, 0);
				sourceBuf.Slice (source.Offset + 12, 4).CopyTo (tempBuf);
				var planes = BitConverter.ToUInt32 (tempBuf, 0);
				sourceBuf.Slice (source.Offset + 14, 4).CopyTo (tempBuf);
				var bitCount = BitConverter.ToUInt32 (tempBuf, 0);
				sourceBuf.Slice (source.Offset + 20, 4).CopyTo (tempBuf);
				var sizeImage = BitConverter.ToUInt32 (tempBuf, 0);
#endif
				var codecId = (compressionNumber >= 0x20202020) ?
					AsciiCharSet.GetString (source.BufferMemory.Span.Slice (source.Offset + 16, 4)) :
					compressionNumber.ToString (CultureInfo.InvariantCulture);
				return new AviStreamInfoVideoFormat (width, height, planes, bitCount, codecId, sizeImage);
			}
		}
	}
}
