using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Media
{
	/// <summary>
	/// RIFF-порция.
	/// </summary>
	/// <remarks>
	/// Базовый блок, из которых состоят такие файлы как AVI, ANI, WAV и WebP.
	/// </remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class RiffChunk
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса RiffChunk на основе указанных данных.
		/// </summary>
		/// <param name="id">FOURCC-код, идентифицирующий тип данных порции.</param>
		/// <param name="data">Исходные данные порции.</param>
		public RiffChunk (string id, IBufferedSource data)
		{
			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			Contract.EndContractBlock ();

			this.Id = id;
			this.Source = data;
		}

		/// <summary>Получает FOURCC-код, идентифицирующий тип данных порции.</summary>
		public string Id { get; }

		/// <summary>Получает исходные данные порции.</summary>
		public IBufferedSource Source { get; }

		/// <summary>Получает признак того, что порция содержит список вложенных порций.</summary>
		public bool IsSubChunkList => (this.Id == "RIFF") || (this.Id == "LIST");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => FormattableString.Invariant ($"ID = {this.Id}");

		/// <summary>
		/// Считывает RIFF-порцию из указанного буфера.
		/// </summary>
		/// <param name="source">Буфер данных содержащий RIFF-порцию.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>RIFF-порция, считанный из указанного буфера.</returns>
		public static Task<RiffChunk> ParseAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.BufferMemory.Length < 8)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Contract.EndContractBlock ();

			Task task;
			try
			{
				task = source.EnsureBufferAsync (8, cancellationToken);
			}
			catch (NotEnoughDataException exception)
			{
				throw new FormatException ("Specified source is too small for Riff-chunk. Expected minimum 8 bytes.", exception);
			}

			return ParseAsyncFinalizer ();

			async Task<RiffChunk> ParseAsyncFinalizer ()
			{
				try
				{
					await task.ConfigureAwait (false);
				}
				catch (NotEnoughDataException exception)
				{
					throw new FormatException ("Specified source is too small for Riff-chunk. Expected minimum 8 bytes.", exception);
				}

				var id = AsciiCharSet.GetString (source.BufferMemory.Span.Slice (source.Offset, 4));

				// TODO: сделать проще, без использования BitConverter
#if NETCOREAPP2_1
				var size = (long)BitConverter.ToUInt32 (source.BufferMemory.Span.Slice (source.Offset + 4));
#else
				var tempBuf = new byte[4];
				source.BufferMemory.Slice (source.Offset + 4, 4).CopyTo (tempBuf);
				var size = (long)BitConverter.ToUInt32 (tempBuf, 0);
#endif
				source.SkipBuffer (8);

				var data = new SizeLimitedBufferedSource (source, size);

				return new RiffChunk (id, data);
			}
		}
	}
}
