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
		private static readonly byte[] _base64Table =
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
		/// Оценивает потенциальный результат кодирования диапазона байтов.
		/// </summary>
		/// <param name="source">Диапазон байтов исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признак того, что указанный диапазон исходных данных является последним.</param>
		/// <returns>Баланс потенциальной операции кодирования.</returns>
		public EncodingBalance Estimate (ReadOnlySpan<byte> source, int maxOutCount, int segmentNumber = 0, bool isLastSegment = false)
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

			var groupsRequested = (int)Math.Ceiling ((double)source.Length / 3.0D);
			var groups = Math.Min (groupsRequested, maxGroups);
			return new EncodingBalance (
				this.PrologSize + this.EpilogSize + (groups * 4),
				Math.Min (source.Length, groups * 3));
		}

		/// <summary>
		/// Кодирует указанную порцию диапазона байтов.
		/// </summary>
		/// <param name="source">Диапазон байтов, содержащий порцию исходных данных.</param>
		/// <param name="destination">Диапазон байтов, куда будет записываться результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признако того, что указанный диапазон исходных данных является последним.</param>
		/// <returns>Баланс операции кодирования.</returns>
		public EncodingBalance Encode (ReadOnlySpan<byte> source, Span<byte> destination, int segmentNumber = 0, bool isLastSegment = false)
		{
			var prologLen = 5 + _encoding.WebName.Length; // размер пролога
			var epilogLen = 2;
			var maxGroups = (destination.Length - prologLen - epilogLen) / 4;  // уменьшаем лимит на размер эпилога
			if (maxGroups < 1)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			// пролог
			var outOffset = 0;
			destination[outOffset++] = (byte)'=';
			destination[outOffset++] = (byte)'?';
			AsciiCharSet.GetBytes (_encoding.WebName.AsSpan (), destination.Slice (outOffset));
			outOffset += _encoding.WebName.Length;
			destination[outOffset++] = (byte)'?';
			destination[outOffset++] = (byte)'B';
			destination[outOffset++] = (byte)'?';

			var groupsRequested = (int)Math.Ceiling (source.Length / 3.0);
			var groups = Math.Min (groupsRequested, maxGroups);
			var sourceCount = Math.Min (source.Length, groups * 3);
			var size = ConvertToBase64Array (source.Slice (0, sourceCount), destination.Slice (outOffset));
			outOffset += size;

			// эпилог
			destination[outOffset++] = (byte)'?';
			destination[outOffset++] = (byte)'=';
			return new EncodingBalance (outOffset, sourceCount);
		}

		private static int ConvertToBase64Array (ReadOnlySpan<byte> inData, Span<byte> outData)
		{
			var tail = inData.Length % 3;
			var tailPos = inData.Length - tail;
			var offset = 0;
			var outOffset = 0;
			while (offset < tailPos)
			{
				outData[outOffset++] = _base64Table[(inData[offset] & 0xfc) >> 2];
				outData[outOffset++] = _base64Table[((inData[offset] & 0x03) << 4) | ((inData[offset + 1] & 0xf0) >> 4)];
				outData[outOffset++] = _base64Table[((inData[offset + 1] & 0x0f) << 2) | ((inData[offset + 2] & 0xc0) >> 6)];
				outData[outOffset++] = _base64Table[inData[offset + 2] & 0x3f];
				offset += 3;
			}

			switch (tail)
			{
				case 1:
					outData[outOffset] = _base64Table[(inData[tailPos] & 0xfc) >> 2];
					outData[outOffset + 1] = _base64Table[(inData[tailPos] & 0x03) << 4];
					outData[outOffset + 2] = _base64Table[0x40];
					outData[outOffset + 3] = _base64Table[0x40];
					outOffset += 4;
					break;
				case 2:
					outData[outOffset] = _base64Table[(inData[tailPos] & 0xfc) >> 2];
					outData[outOffset + 1] = _base64Table[((inData[tailPos] & 0x03) << 4) | ((inData[tailPos + 1] & 0xf0) >> 4)];
					outData[outOffset + 2] = _base64Table[(inData[tailPos + 1] & 0x0f) << 2];
					outData[outOffset + 3] = _base64Table[0x40];
					outOffset += 4;
					break;
			}

			return outOffset;
		}
	}
}
