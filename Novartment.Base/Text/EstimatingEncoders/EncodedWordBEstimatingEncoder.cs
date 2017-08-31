using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Кодирует 'encoded-word' способом 'B' согласно RFC 2047.
	/// </summary>
	public class EncodedWordBEstimatingEncoder :
		IEstimatingEncoder
	{
		private static readonly byte[] _Base64Table =
		{
			0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a,
			0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a,
			0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x2b, 0x2f, 0x3d,
		};

		private readonly Encoding _encoding;

		/// <summary>
		/// Инициализирует новый экземпляр класса EncodedWordBEstimatingEncoder с указанием используемой кодировки.
		/// </summary>
		/// <param name="encoding">Кодировка, используемая для двоичного представления символов.</param>
		public EncodedWordBEstimatingEncoder(Encoding encoding)
		{
			if (encoding == null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			Contract.EndContractBlock();

			_encoding = encoding;
		}

		/// <summary>
		/// Получает количество байтов, которые будут вставлены перед данными.
		/// </summary>
		public int PrologSize => _encoding.WebName.Length + 5;

		/// <summary>
		/// Получает количество байтов, которые будут вставлены после данных.
		/// </summary>
		public int EpilogSize => 2;

		/// <summary>
		/// В указанном массиве байтов ищет ближайшую позицию данных,
		/// подходящих для кодировщика.
		/// </summary>
		/// <param name="source">Исходный массив байтов.</param>
		/// <param name="offset">Позиция начала исходных данных в массиве.</param>
		/// <param name="count">Количество байтов исходных данных в массиве.</param>
		/// <returns>Ближайшая позиция данных, подходящих для кодировщика,
		/// либо -1 если подходящих данных не найдено.</returns>
		public int FindValid (byte[] source, int offset, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if ((offset < 0) || (offset > source.Length) || ((offset == source.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			Contract.EndContractBlock ();

			return offset;
		}

		/// <summary>
		/// Оценивает потенциальный результат кодирования указанной порции массива байтов.
		/// </summary>
		/// <param name="source">source не используется.</param>
		/// <param name="offset">offset не используется.</param>
		/// <param name="count">Количество байтов в порции исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">segmentNumber не используется.</param>
		/// <param name="isLastSegment">isLastSegment не используется.</param>
		/// <returns>Кортеж из количества байтов, необходимых для результата кодирования и
		/// количества байтов источника, которое было использовано для кодирования.</returns>
		public EncodingBalance Estimate (byte[] source, int offset, int count, int maxOutCount, int segmentNumber, bool isLastSegment)
		{
			if (maxOutCount < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (maxOutCount));
			}

			Contract.EndContractBlock ();

			// The encoding process represents 24-bit groups of input bits as output strings of 4 encoded characters.
			var maxGroups = (maxOutCount - this.PrologSize - this.EpilogSize) / 4;
			if (maxGroups < 1)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			var groupsRequested = (int)Math.Ceiling ((double)count / 3.0D);
			var groups = Math.Min (groupsRequested, maxGroups);
			return new EncodingBalance (
				this.PrologSize + this.EpilogSize + (groups * 4),
				Math.Min (count, groups * 3));
		}

		/// <summary>
		/// Кодирует указанную порцию массива байтов.
		/// </summary>
		/// <param name="source">Массив байтов, содержащий порцию исходных данных.</param>
		/// <param name="offset">Позиция начала порции исходных данных.</param>
		/// <param name="count">Количество байтов в порции исходных данных.</param>
		/// <param name="destination">Массив байтов, куда будет записываться результат кодирования.</param>
		/// <param name="outOffset">Позиция в destination куда будет записываться результат кодирования.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">segmentNumber не используется.</param>
		/// <param name="isLastSegment">isLastSegment не используется.</param>
		/// <returns>Кортеж из количества байтов, записанных в массив для результата кодирования и
		/// количества байтов источника, которое было использовано для кодирования.</returns>
		public EncodingBalance Encode (
			byte[] source,
			int offset,
			int count,
			byte[] destination,
			int outOffset,
			int maxOutCount,
			int segmentNumber,
			bool isLastSegment)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if ((offset < 0) || (offset > source.Length) || ((offset == source.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (offset));
			}

			if ((count < 0) || (count > source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			if ((outOffset < 0) || (outOffset > destination.Length) || ((outOffset == destination.Length) && (maxOutCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (outOffset));
			}

			if ((maxOutCount < 0) || (maxOutCount > destination.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (maxOutCount));
			}

			Contract.EndContractBlock ();

			var outStartOffset = outOffset;
			destination[outOffset++] = (byte)'=';
			destination[outOffset++] = (byte)'?';
			AsciiCharSet.GetBytes (_encoding.WebName, 0, _encoding.WebName.Length, destination, outOffset);
			outOffset += _encoding.WebName.Length;
			destination[outOffset++] = (byte)'?';
			destination[outOffset++] = (byte)'B';
			destination[outOffset++] = (byte)'?';
			maxOutCount -= 2; // уменьшаем лимит на размер эпилога
			var maxGroups = (maxOutCount - (outOffset - outStartOffset)) / 4;
			if (maxGroups < 1)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			var groupsRequested = (int)Math.Ceiling (count / 3.0);
			var groups = Math.Min (groupsRequested, maxGroups);
			var sourceCount = Math.Min (count, groups * 3);
			ConvertToBase64Array (source, offset, sourceCount, destination, ref outOffset);
			destination[outOffset++] = (byte)'?'; // эпилог
			destination[outOffset++] = (byte)'=';
			return new EncodingBalance (outOffset - outStartOffset, sourceCount);
		}

		private static void ConvertToBase64Array (byte[] inData, int offset, int length, byte[] outData, ref int outOffset)
		{
			var tail = length % 3;
			var tailPos = offset + (length - tail);
			while (offset < tailPos)
			{
				outData[outOffset++] = _Base64Table[(inData[offset] & 0xfc) >> 2];
				outData[outOffset++] = _Base64Table[((inData[offset] & 0x03) << 4) | ((inData[offset + 1] & 0xf0) >> 4)];
				outData[outOffset++] = _Base64Table[((inData[offset + 1] & 0x0f) << 2) | ((inData[offset + 2] & 0xc0) >> 6)];
				outData[outOffset++] = _Base64Table[inData[offset + 2] & 0x3f];
				offset += 3;
			}

			switch (tail)
			{
				case 1:
					outData[outOffset] = _Base64Table[(inData[tailPos] & 0xfc) >> 2];
					outData[outOffset + 1] = _Base64Table[(inData[tailPos] & 0x03) << 4];
					outData[outOffset + 2] = _Base64Table[0x40];
					outData[outOffset + 3] = _Base64Table[0x40];
					outOffset += 4;
					break;
				case 2:
					outData[outOffset] = _Base64Table[(inData[tailPos] & 0xfc) >> 2];
					outData[outOffset + 1] = _Base64Table[((inData[tailPos] & 0x03) << 4) | ((inData[tailPos + 1] & 0xf0) >> 4)];
					outData[outOffset + 2] = _Base64Table[(inData[tailPos + 1] & 0x0f) << 2];
					outData[outOffset + 3] = _Base64Table[0x40];
					outOffset += 4;
					break;
			}
		}
	}
}
