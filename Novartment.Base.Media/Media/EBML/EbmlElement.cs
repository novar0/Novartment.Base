using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Элемент EBML.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public sealed class EbmlElement
	{
		private static readonly DateTime MilleniumStart = new DateTime (2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static readonly sbyte[] ExtraBytesSize = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
		private static readonly ulong[] DataBitsMask =
		{
			(1L << 0) - 1,
			(1L << 7) - 1,
			(1L << 14) - 1,
			(1L << 21) - 1,
			(1L << 28) - 1,
			(1L << 35) - 1,
			(1L << 42) - 1,
			(1L << 49) - 1,
			(1L << 56) - 1,
		};

		private readonly ulong _size;
		private readonly IBufferedSource _source;
		private readonly bool _allDataBuffered;
		private ulong _readed;

		/// <summary>
		/// Инициализирует новый экземпляр класса EbmlElement на основе указанных данных.
		/// </summary>
		/// <param name="id">Идентификатор элемента.</param>
		/// <param name="size">Размер содержимого элемента.</param>
		/// <param name="source">Данные элемента.</param>
		public EbmlElement (ulong id, ulong size, IBufferedSource source)
		{
			if (size < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			this.Id = id;
			_size = size;
			_source = source;
			_allDataBuffered = size <= (ulong)source.Count;
		}

		/// <summary>Получает идентификатор элемента.</summary>
		public ulong Id { get; }

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => FormattableString.Invariant ($"ID = {this.Id}, Size = {_size}");

		/// <summary>
		/// Считывает EBML-элемент из указанного источника.
		/// </summary>
		/// <param name="source">Источник, содержащий EBML-элемент.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>EBML-элемент, считанный из указанного источника.</returns>
		public static Task<EbmlElement> ParseAsync (IBufferedSource source, CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return ParseAsyncStateMachine ();

			async Task<EbmlElement> ParseAsyncStateMachine ()
			{
				var id = await ReadVIntAsync (source, cancellationToken).ConfigureAwait (false);
				var size = await ReadVIntValueAsync (source, cancellationToken).ConfigureAwait (false);
				await source.LoadAsync (cancellationToken).ConfigureAwait (false);
				return new EbmlElement (id, size, source);
			}
		}

		/// <summary>
		/// Пропускает всё содержимое элемента.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public async Task SkipAllAsync (CancellationToken cancellationToken = default)
		{
			var toSkip = (long)_size - (long)_readed;
			if (toSkip > 0)
			{
				var skipped = await _source.TrySkipAsync ((long)toSkip, cancellationToken).ConfigureAwait (false);
				if (toSkip != skipped)
				{
					throw new InvalidOperationException ("Insufficient data in source to skip element.");
				}
			}
		}

		/// <summary>
		/// Reads the element data as a signed integer.
		/// </summary>
		/// <returns>the element data as a signed integer.</returns>
		public long ReadInt ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			var buf = _source.BufferMemory.Span.Slice (_source.Offset, _source.Count);
			var result = _size switch
			{
				8 => (long)buf[7] |
						(long)buf[6] << 8 |
						(long)buf[5] << 16 |
						(long)buf[4] << 24 |
						(long)buf[3] << 32 |
						(long)buf[2] << 40 |
						(long)buf[1] << 48 |
						(long)buf[0] << 56,
				7 => (long)buf[6] |
						(long)buf[5] << 8 |
						(long)buf[4] << 16 |
						(long)buf[3] << 24 |
						(long)buf[2] << 32 |
						(long)buf[1] << 40 |
						(long)buf[0] << 48,
				6 => (long)buf[5] |
						(long)buf[4] << 8 |
						(long)buf[3] << 16 |
						(long)buf[2] << 24 |
						(long)buf[1] << 32 |
						(long)buf[0] << 40,
				5 => (long)buf[4] |
						(long)buf[3] << 8 |
						(long)buf[2] << 16 |
						(long)buf[1] << 24 |
						(long)buf[0] << 32,
				4 => (long)buf[3] |
						(long)buf[2] << 8 |
						(long)buf[1] << 16 |
						(long)buf[0] << 24,
				3 => (long)buf[2] |
						(long)buf[1] << 8 |
						(long)buf[0] << 16,
				2 => (long)buf[1] |
						(long)buf[0] << 8,
				1 => (long)buf[0],
				_ => throw new FormatException (FormattableString.Invariant ($"Ivalid size ({_size}) of data of type Int. Expected 1 to 8 bytes.")),
			};
			_source.Skip ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as an unsigned integer.
		/// </summary>
		/// <returns>the element data as an unsigned integer.</returns>
		public ulong ReadUInt ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			var buf = _source.BufferMemory.Span.Slice (_source.Offset, _source.Count);
			var result = _size switch
			{
				8 => (ulong)buf[7] |
						(ulong)buf[6] << 8 |
						(ulong)buf[5] << 16 |
						(ulong)buf[4] << 24 |
						(ulong)buf[3] << 32 |
						(ulong)buf[2] << 40 |
						(ulong)buf[1] << 48 |
						(ulong)buf[0] << 56,
				7 => (ulong)buf[6] |
						(ulong)buf[5] << 8 |
						(ulong)buf[4] << 16 |
						(ulong)buf[3] << 24 |
						(ulong)buf[2] << 32 |
						(ulong)buf[1] << 40 |
						(ulong)buf[0] << 48,
				6 => (ulong)buf[5] |
						(ulong)buf[4] << 8 |
						(ulong)buf[3] << 16 |
						(ulong)buf[2] << 24 |
						(ulong)buf[1] << 32 |
						(ulong)buf[0] << 40,
				5 => (ulong)buf[4] |
						(ulong)buf[3] << 8 |
						(ulong)buf[2] << 16 |
						(ulong)buf[1] << 24 |
						(ulong)buf[0] << 32,
				4 => (ulong)buf[3] |
						(ulong)buf[2] << 8 |
						(ulong)buf[1] << 16 |
						(ulong)buf[0] << 24,
				3 => (ulong)buf[2] |
						(ulong)buf[1] << 8 |
						(ulong)buf[0] << 16,
				2 => (ulong)buf[1] |
						(ulong)buf[0] << 8,
				1 => (ulong)buf[0],
				_ => throw new FormatException (FormattableString.Invariant ($"Ivalid size ({_size}) of data of type UInt. Expected 1 to 8 bytes.")),
			};
			_source.Skip ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as a floating-point number.
		/// If the element data size is equal to <code>4</code>, then an instance of the <code>Float</code> is returned. If
		/// the element data size is equal to <code>8</code>, then an instance of the <code>Double</code> is returned.
		/// </summary>
		/// <returns>the element data as a floating-point number.</returns>
		public double ReadFloat ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			if ((_size != 4) && (_size != 8))
			{
				throw new FormatException (FormattableString.Invariant ($"Ivalid size ({_size}) of data of type Float. Expected 4 or 8 bytes."));
			}

			double result;
			var span = _source.BufferMemory.Span.Slice (_source.Offset, (int)_size);
			if (_size == 4)
			{
#if NETSTANDARD2_0
				var buf = new byte[4];
				buf[0] = span[3];
				buf[1] = span[2];
				buf[2] = span[1];
				buf[3] = span[0];
				result = BitConverter.ToSingle (buf, 0);
#else
				result = BitConverter.Int32BitsToSingle (BinaryPrimitives.ReadInt32BigEndian (span));
#endif
			}
			else
			{
				result = BitConverter.Int64BitsToDouble (BinaryPrimitives.ReadInt64BigEndian (span));
			}

			_source.Skip ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as a date.
		/// </summary>
		/// <returns>the element data as a date.</returns>
		public DateTime ReadDate ()
		{
			return MilleniumStart.AddTicks (ReadInt () / 100L);
		}

		/// <summary>
		/// Reads the element data as an ASCII string.
		/// </summary>
		/// <returns>the element data as an ASCII string.</returns>
		public string ReadAscii ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			if (!_allDataBuffered)
			{
				throw new NotSupportedException (FormattableString.Invariant ($"Text data size {_size} is too big. Supported maximum is {_source.BufferMemory.Length}."));
			}

			var result = AsciiCharSet.GetString (_source.BufferMemory.Span.Slice (_source.Offset, (int)_size));
			_source.Skip ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as an UTF8 string.
		/// </summary>
		/// <returns>the element data as an UTF8 string.</returns>
		public string ReadUtf ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			if (!_allDataBuffered)
			{
				throw new NotSupportedException (FormattableString.Invariant ($"Text data size {_size} is too big. Supported maximum is {_source.BufferMemory.Length}."));
			}

#if NETSTANDARD2_0
			var result = Encoding.UTF8.GetString (_source.BufferMemory.ToArray (), _source.Offset, (int)_size);
#else
			var result = Encoding.UTF8.GetString (_source.BufferMemory.Span.Slice (_source.Offset, (int)_size));
#endif
			_source.Skip ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Получает данные элемента в виде источника данных, представленного байтовым буфером.
		/// </summary>
		/// <returns>Данные элемента в виде источника данных.</returns>
		public IBufferedSource ReadBinary ()
		{
			_readed += _size;
			return new SizeLimitedBufferedSource (_source, (long)_size);
		}

		/// <summary>
		/// Получает перечислитель дочерних элементов.
		/// </summary>
		/// <returns>Перечислитель дочерних элементов.</returns>
		public EbmlElementCollectionEnumerator ReadSubElements ()
		{
			_readed += _size;
			return new EbmlElementCollectionEnumerator (new SizeLimitedBufferedSource (_source, (long)_size));
		}

		private static async Task<ulong> ReadVIntAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			await source.EnsureAvailableAsync (1, cancellationToken).ConfigureAwait (false);

			var byte0 = source.BufferMemory.Span[source.Offset];

			if (byte0 == 0)
			{
				throw new NotSupportedException ("VInt values more then 8 bytes are not suppoted.");
			}

			var extraBytes = ((byte0 & 0xf0) != 0) ?
				ExtraBytesSize[byte0 >> 4] :
				(4 + ExtraBytesSize[byte0]);

			var size = extraBytes + 1;
			if (size > source.BufferMemory.Length)
			{
				throw new FormatException ();
			}

			await source.EnsureAvailableAsync (size, cancellationToken).ConfigureAwait (false);
			ulong encodedValue = DecodeVInt (source.BufferMemory.Span.Slice (source.Offset, source.Count), extraBytes);
			source.Skip (size);

			return encodedValue;
		}

		private static async Task<ulong> ReadVIntValueAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			await source.EnsureAvailableAsync (1, cancellationToken).ConfigureAwait (false);

			var byte0 = source.BufferMemory.Span[source.Offset];
			if (byte0 == 0)
			{
				throw new NotSupportedException ("Size of VInt more than 8 bytes is not supported.");
			}

			var extraBytes = ((byte0 & 0xf0) != 0) ?
				ExtraBytesSize[byte0 >> 4] :
				(4 + ExtraBytesSize[byte0]);

			var size = extraBytes + 1;
			if (size > source.BufferMemory.Length)
			{
				throw new FormatException ();
			}

			await source.EnsureAvailableAsync (size, cancellationToken).ConfigureAwait (false);
			ulong encodedValue = DecodeVInt (source.BufferMemory.Span.Slice (source.Offset, source.Count), extraBytes);
			source.Skip (size);

			return encodedValue & DataBitsMask[extraBytes + 1];
		}

		private static ulong DecodeVInt (ReadOnlySpan<byte> buf, int size)
		{
			ulong encodedValue = buf[0];
			for (var i = 0; i < size; i++)
			{
				encodedValue = encodedValue << 8 | buf[i + 1];
			}

			return encodedValue;
		}
	}
}
