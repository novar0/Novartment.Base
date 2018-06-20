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
	/// Коллекция RIFF-порций.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class RiffChunkListReader
	{
		private readonly IBufferedSource _source;
		private RiffChunk _current;

		/// <summary>
		/// Инициализирует новый экземпляр класса RiffChunkListReader на основе указанного источника данных.
		/// </summary>
		/// <param name="source">Источника данных.</param>
		public RiffChunkListReader (IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.BufferMemory.Length < 4)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Contract.EndContractBlock ();

			_source = source;

			if (_source.Count < 4)
			{
				throw new InvalidOperationException ("Specified source is too small to be list of Riff-chunk. Expected minimum 4 bytes");
			}

			this.ListId = AsciiCharSet.GetString (_source.BufferMemory.Span.Slice (_source.Offset, 4));
			_source.SkipBuffer (4);
		}

		/// <summary>Получает FOURCC-код, идентифицирующий коллекцию.</summary>
		public string ListId { get; }

		/// <summary>
		/// Получает текущий элемент перечислителя.
		/// </summary>
		public RiffChunk Current
		{
			get
			{
				if (_current == null)
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it not started or already ended");
				}

				return _current;
			}
		}

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => "Id = " + this.ListId;

		/// <summary>
		/// Перемещает перечислитель к следующему элементу строки.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
		/// false, если перечислитель достиг конца.</returns>
		public async Task<bool> MoveNextAsync (CancellationToken cancellationToken)
		{
			// skip all data in current
			if (_current != null)
			{
				await _current.Source.SkipToEndAsync (cancellationToken).ConfigureAwait (false);
			}

			await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
			if (_source.Count < 8)
			{
				// no more data
				_current = null;
				return false;
			}

			_current = await RiffChunk.ParseAsync (_source, cancellationToken).ConfigureAwait (false);
			return true;
		}
	}
}
