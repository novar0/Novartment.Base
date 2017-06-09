using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Ebml",
		Justification = "'EBML' represents standard term.")]
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	[CLSCompliant (false)]
	public class EbmlElement
	{
		private static readonly DateTime _MilleniumStart = new DateTime (2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static readonly sbyte[] _ExtraBytesSize = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
		private static readonly ulong[] _DataBitsMask =
		{
			(1L << 0) - 1,
			(1L << 7) - 1,
			(1L << 14) - 1,
			(1L << 21) - 1,
			(1L << 28) - 1,
			(1L << 35) - 1,
			(1L << 42) - 1,
			(1L << 49) - 1,
			(1L << 56) - 1
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
		[SuppressMessage (
		"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		private string DebuggerDisplay => FormattableString.Invariant ($"ID = {this.Id}, Size = {_size}");

		/// <summary>
		/// Считывает EBML-элемент из указанного источника.
		/// </summary>
		/// <param name="source">Источник, содержащий EBML-элемент.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>EBML-элемент, считанный из указанного источника.</returns>
		public static Task<EbmlElement> ParseAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return ParseAsyncStateMachine (source, cancellationToken);
		}

		/// <summary>
		/// Пропускает всё содержимое элемента.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public async Task SkipAllAsync (CancellationToken cancellationToken)
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
		/// <returns>the element data as a signed integer</returns>
		[SuppressMessage (
		"Microsoft.Naming",
			"CA1720:IdentifiersShouldNotContainTypeNames",
			MessageId = "int",
			Justification = "The term 'Int' in method name refers to EBML specification, not C#.")]
		public long ReadInt ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			long result;
			switch (_size)
			{
				case 8:
					result = (long)_source.Buffer[_source.Offset + 7] |
						(long)_source.Buffer[_source.Offset + 6] << 8 |
						(long)_source.Buffer[_source.Offset + 5] << 16 |
						(long)_source.Buffer[_source.Offset + 4] << 24 |
						(long)_source.Buffer[_source.Offset + 3] << 32 |
						(long)_source.Buffer[_source.Offset + 2] << 40 |
						(long)_source.Buffer[_source.Offset + 1] << 48 |
						(long)_source.Buffer[_source.Offset] << 56;
					break;
				case 7:
					result = (long)_source.Buffer[_source.Offset + 6] |
						(long)_source.Buffer[_source.Offset + 5] << 8 |
						(long)_source.Buffer[_source.Offset + 4] << 16 |
						(long)_source.Buffer[_source.Offset + 3] << 24 |
						(long)_source.Buffer[_source.Offset + 2] << 32 |
						(long)_source.Buffer[_source.Offset + 1] << 40 |
						(long)_source.Buffer[_source.Offset] << 48;
					break;
				case 6:
					result = (long)_source.Buffer[_source.Offset + 5] |
						(long)_source.Buffer[_source.Offset + 4] << 8 |
						(long)_source.Buffer[_source.Offset + 3] << 16 |
						(long)_source.Buffer[_source.Offset + 2] << 24 |
						(long)_source.Buffer[_source.Offset + 1] << 32 |
						(long)_source.Buffer[_source.Offset] << 40;
					break;
				case 5:
					result = (long)_source.Buffer[_source.Offset + 4] |
						(long)_source.Buffer[_source.Offset + 3] << 8 |
						(long)_source.Buffer[_source.Offset + 2] << 16 |
						(long)_source.Buffer[_source.Offset + 1] << 24 |
						(long)_source.Buffer[_source.Offset] << 32;
					break;
				case 4:
					result = (long)_source.Buffer[_source.Offset + 3] |
						(long)_source.Buffer[_source.Offset + 2] << 8 |
						(long)_source.Buffer[_source.Offset + 1] << 16 |
						(long)_source.Buffer[_source.Offset] << 24;
					break;
				case 3:
					result = (long)_source.Buffer[_source.Offset + 2] |
						(long)_source.Buffer[_source.Offset + 1] << 8 |
						(long)_source.Buffer[_source.Offset] << 16;
					break;
				case 2:
					result = (long)_source.Buffer[_source.Offset + 1] |
						(long)_source.Buffer[_source.Offset] << 8;
					break;
				case 1:
					result = (long)_source.Buffer[_source.Offset];
					break;
				default:
					throw new FormatException (FormattableString.Invariant ($"Ivalid size ({_size}) of data of type Int. Expected 1 to 8 bytes."));
			}

			_source.SkipBuffer ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as an unsigned integer.
		/// </summary>
		/// <returns>the element data as an unsigned integer</returns>
		[SuppressMessage (
		"Microsoft.Naming",
			"CA1720:IdentifiersShouldNotContainTypeNames",
			MessageId = "uint",
			Justification = "The term 'UInt' in method name refers to EBML specification, not C#.")]
		public ulong ReadUInt ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			ulong result;
			switch (_size)
			{
				case 8:
					result = (ulong)_source.Buffer[_source.Offset + 7] |
						(ulong)_source.Buffer[_source.Offset + 6] << 8 |
						(ulong)_source.Buffer[_source.Offset + 5] << 16 |
						(ulong)_source.Buffer[_source.Offset + 4] << 24 |
						(ulong)_source.Buffer[_source.Offset + 3] << 32 |
						(ulong)_source.Buffer[_source.Offset + 2] << 40 |
						(ulong)_source.Buffer[_source.Offset + 1] << 48 |
						(ulong)_source.Buffer[_source.Offset] << 56;
					break;
				case 7:
					result = (ulong)_source.Buffer[_source.Offset + 6] |
						(ulong)_source.Buffer[_source.Offset + 5] << 8 |
						(ulong)_source.Buffer[_source.Offset + 4] << 16 |
						(ulong)_source.Buffer[_source.Offset + 3] << 24 |
						(ulong)_source.Buffer[_source.Offset + 2] << 32 |
						(ulong)_source.Buffer[_source.Offset + 1] << 40 |
						(ulong)_source.Buffer[_source.Offset] << 48;
					break;
				case 6:
					result = (ulong)_source.Buffer[_source.Offset + 5] |
						(ulong)_source.Buffer[_source.Offset + 4] << 8 |
						(ulong)_source.Buffer[_source.Offset + 3] << 16 |
						(ulong)_source.Buffer[_source.Offset + 2] << 24 |
						(ulong)_source.Buffer[_source.Offset + 1] << 32 |
						(ulong)_source.Buffer[_source.Offset] << 40;
					break;
				case 5:
					result = (ulong)_source.Buffer[_source.Offset + 4] |
						(ulong)_source.Buffer[_source.Offset + 3] << 8 |
						(ulong)_source.Buffer[_source.Offset + 2] << 16 |
						(ulong)_source.Buffer[_source.Offset + 1] << 24 |
						(ulong)_source.Buffer[_source.Offset] << 32;
					break;
				case 4:
					result = (ulong)_source.Buffer[_source.Offset + 3] |
						(ulong)_source.Buffer[_source.Offset + 2] << 8 |
						(ulong)_source.Buffer[_source.Offset + 1] << 16 |
						(ulong)_source.Buffer[_source.Offset] << 24;
					break;
				case 3:
					result = (ulong)_source.Buffer[_source.Offset + 2] |
						(ulong)_source.Buffer[_source.Offset + 1] << 8 |
						(ulong)_source.Buffer[_source.Offset] << 16;
					break;
				case 2:
					result = (ulong)_source.Buffer[_source.Offset + 1] |
						(ulong)_source.Buffer[_source.Offset] << 8;
					break;
				case 1:
					result = (ulong)_source.Buffer[_source.Offset];
					break;
				default:
					throw new FormatException (FormattableString.Invariant ($"Ivalid size ({_size}) of data of type UInt. Expected 1 to 8 bytes."));
			}

			_source.SkipBuffer ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as a floating-point number.
		/// If the element data size is equal to <code>4</code>, then an instance of the <code>Float</code> is returned. If
		/// the element data size is equal to <code>8</code>, then an instance of the <code>Double</code> is returned.
		/// </summary>
		/// <returns>the element data as a floating-point number</returns>
		[SuppressMessage (
		"Microsoft.Naming",
			"CA1720:IdentifiersShouldNotContainTypeNames",
			MessageId = "float",
			Justification = "The term 'Float' in method name refers to EBML specification, not C#.")]
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

			var buf = new byte[_size];
			Array.Copy (_source.Buffer, _source.Offset, buf, 0, (int)_size);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse (buf);
			}

			double result = (_size == 4) ?
				BitConverter.ToSingle (buf, 0) :
				BitConverter.ToDouble (buf, 0);
			_source.SkipBuffer ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as a date.
		/// </summary>
		/// <returns>the element data as a date</returns>
		public DateTime ReadDate ()
		{
			return _MilleniumStart.AddTicks (ReadInt () / 100L);
		}

		/// <summary>
		/// Reads the element data as an ASCII string.
		/// </summary>
		/// <returns>the element data as an ASCII string</returns>
		[SuppressMessage (
		"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Ascii",
			Justification = "'ASCII' represents standard term.")]
		public string ReadAscii ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			if (!_allDataBuffered)
			{
				throw new NotSupportedException (FormattableString.Invariant ($"Text data size {_size} is too big. Supported maximum is {_source.Buffer.Length}."));
			}

			var result = AsciiCharSet.GetString (_source.Buffer, _source.Offset, (int)_size);
			_source.SkipBuffer ((int)_size);
			_readed += _size;
			return result;
		}

		/// <summary>
		/// Reads the element data as an UTF8 string.
		/// </summary>
		/// <returns>the element data as an UTF8 string</returns>
		public string ReadUtf ()
		{
			if (_readed >= _size)
			{
				throw new InvalidOperationException ("Cant read data when all element's data already readed.");
			}

			if (!_allDataBuffered)
			{
				throw new NotSupportedException (FormattableString.Invariant ($"Text data size {_size} is too big. Supported maximum is {_source.Buffer.Length}."));
			}

			var result = Encoding.UTF8.GetString (_source.Buffer, _source.Offset, (int)_size);
			_source.SkipBuffer ((int)_size);
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

		private static async Task<EbmlElement> ParseAsyncStateMachine (IBufferedSource source, CancellationToken cancellationToken)
		{
			var id = await ReadVIntAsync (source, cancellationToken).ConfigureAwait (false);
			var size = await ReadVIntValueAsync (source, cancellationToken).ConfigureAwait (false);
			await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
			return new EbmlElement (id, size, source);
		}

		private static async Task<ulong> ReadVIntAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			await source.EnsureBufferAsync (1, cancellationToken).ConfigureAwait (false);

			if (source.Buffer[source.Offset] == 0)
			{
				throw new NotSupportedException ("VInt values more then 8 bytes are not suppoted.");
			}

			var extraBytes = ((source.Buffer[source.Offset] & 0xf0) != 0) ?
				_ExtraBytesSize[source.Buffer[source.Offset] >> 4] :
				(4 + _ExtraBytesSize[source.Buffer[source.Offset]]);

			var size = extraBytes + 1;
			if (size > source.Buffer.Length)
			{
				throw new FormatException ();
			}

			await source.EnsureBufferAsync (size, cancellationToken).ConfigureAwait (false);

			ulong encodedValue = source.Buffer[source.Offset];
			for (var i = 0; i < extraBytes; i++)
			{
				encodedValue = encodedValue << 8 | source.Buffer[source.Offset + i + 1];
			}

			source.SkipBuffer (size);

			return encodedValue;
		}

		private static async Task<ulong> ReadVIntValueAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			await source.EnsureBufferAsync (1, cancellationToken).ConfigureAwait (false);

			if (source.Buffer[source.Offset] == 0)
			{
				throw new NotSupportedException ("Size of VInt more than 8 bytes is not supported.");
			}

			var extraBytes = ((source.Buffer[source.Offset] & 0xf0) != 0) ?
				_ExtraBytesSize[source.Buffer[source.Offset] >> 4] :
				(4 + _ExtraBytesSize[source.Buffer[source.Offset]]);

			var size = extraBytes + 1;
			if (size > source.Buffer.Length)
			{
				throw new FormatException ();
			}

			await source.EnsureBufferAsync (size, cancellationToken).ConfigureAwait (false);

			ulong encodedValue = source.Buffer[source.Offset];
			for (var i = 0; i < extraBytes; i++)
			{
				encodedValue = encodedValue << 8 | source.Buffer[source.Offset + i + 1];
			}

			source.SkipBuffer (size);

			return encodedValue & _DataBitsMask[extraBytes + 1];
		}
	}
}
