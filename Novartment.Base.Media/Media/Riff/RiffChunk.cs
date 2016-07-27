using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;
using Novartment.Base.BinaryStreaming;

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
		/// <summary>Получает FOURCC-код, идентифицирующий тип данных порции.</summary>
		public string Id { get; }

		/// <summary>Получает исходные данные порции.</summary>
		public IBufferedSource Source { get; }

		/// <summary>Получает признак того, что порция содержит список вложенных порций.</summary>
		public bool IsSubChunkList => ((this.Id == "RIFF") || (this.Id == "LIST"));

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
			if (source.Buffer.Length < 8)
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
			return ParseAsyncFinalizer (task, source);
		}

		private static async Task<RiffChunk> ParseAsyncFinalizer (Task task, IBufferedSource source)
		{
			try
			{
				await task.ConfigureAwait (false);
			}
			catch (NotEnoughDataException exception)
			{
				throw new FormatException ("Specified source is too small for Riff-chunk. Expected minimum 8 bytes.", exception);
			}
			var id = AsciiCharSet.GetString (source.Buffer, source.Offset, 4);
			var size = (long)BitConverter.ToUInt32 (source.Buffer, source.Offset + 4);
			source.SkipBuffer (8);

			var data = new SizeLimitedBufferedSource (source, size);

			return new RiffChunk (id, data);
		}

		[DebuggerBrowsable (DebuggerBrowsableState.Never),
		SuppressMessage ("Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay
		{
			get
			{
				return FormattableString.Invariant ($"ID = {this.Id}");
			}
		}
	}
}
