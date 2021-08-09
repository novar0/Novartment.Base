using System;
using System.Buffers.Binary;
using System.Diagnostics;
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
	public sealed class RiffChunk
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса RiffChunk на основе указанных данных.
		/// </summary>
		/// <param name="id">FOURCC-код, идентифицирующий тип данных порции.</param>
		/// <param name="data">Исходные данные порции.</param>
		public RiffChunk (string id, IBufferedSource data)
		{
			this.Source = data ?? throw new ArgumentNullException (nameof (data));
			this.Id = id;
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
		public static Task<RiffChunk> ParseAsync (IBufferedSource source, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.BufferMemory.Length < 8)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Task task;
			try
			{
				task = source.EnsureAvailableAsync (8, cancellationToken).AsTask ();
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

				var size = (long)BinaryPrimitives.ReadUInt32LittleEndian (source.BufferMemory.Span[(source.Offset + 4)..]);
				source.Skip (8);

				var data = new SizeLimitedBufferedSource (source, size);

				return new RiffChunk (id, data);
			}
		}
	}
}
