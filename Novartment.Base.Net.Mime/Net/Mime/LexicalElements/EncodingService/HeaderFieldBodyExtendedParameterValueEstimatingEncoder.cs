using System;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Кодирует 'extended-initial-value' и 'extended-other-values' согласно RFC 2231.
	/// </summary>
	internal class HeaderFieldBodyExtendedParameterValueEstimatingEncoder :
		IEstimatingEncoder
	{
		private readonly Encoding _encoding;

		/// <summary>
		/// Инициализирует новый экземпляр класса ExtendedParameterValueEstimatingEncoder с указанием используемой кодировки.
		/// </summary>
		/// <param name="encoding">Кодировка, используемая для двоичного представления символов.</param>
		internal HeaderFieldBodyExtendedParameterValueEstimatingEncoder (Encoding encoding)
		{
			if (encoding == null)
			{
				throw new ArgumentNullException (nameof (encoding));
			}

			Contract.EndContractBlock ();

			_encoding = encoding;
		}

		/// <summary>
		/// Получает количество байтов, которые будут вставлены перед данными.
		/// </summary>
		public int PrologSize => 0;

		/// <summary>
		/// Получает количество байтов, которые будут вставлены после данных.
		/// </summary>
		public int EpilogSize => 0;

		/// <summary>
		/// Оценивает потенциальный результат кодирования диапазона байтов.
		/// </summary>
		/// <param name="source">Диапазон байтов исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признак того, что указанный диапазон исходных данных является последним.</param>
		/// <returns>Баланс потенциальной операции кодирования.</returns>
		public EncodingBalance Estimate (ReadOnlySpan<byte> source, int maxOutCount, int segmentNumber, bool isLastSegment)
		{
			if (maxOutCount < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (maxOutCount));
			}

			Contract.EndContractBlock ();

			int srcPos = 0;
			var dstPos = (segmentNumber == 0) ? (_encoding.WebName.Length + 2) : 0;
			if (dstPos >= maxOutCount)
			{
				// ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			while ((srcPos < source.Length) && (dstPos < maxOutCount))
			{
				var octet = source[srcPos];
				var isToken = (octet != '%') && (octet < AsciiCharSet.Classes.Count) && ((AsciiCharSet.Classes[octet] & (short)AsciiCharClasses.Token) != 0);
				if (!isToken)
				{
					if ((dstPos + 3) > maxOutCount)
					{
						break;
					}

					dstPos += 3; // знак процента вместо символа, потом два шест.знака
				}
				else
				{
					dstPos++;
				}

				srcPos++;
			}

			return new EncodingBalance (dstPos, srcPos);
		}

		/// <summary>
		/// Кодирует указанную порцию диапазона байтов.
		/// </summary>
		/// <param name="source">Диапазон байтов, содержащий порцию исходных данных.</param>
		/// <param name="destination">Диапазон байтов, куда будет записываться результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признако того, что указанный диапазон исходных данных является последним.</param>
		/// <returns>Баланс операции кодирования.</returns>
		public EncodingBalance Encode (ReadOnlySpan<byte> source, Span<byte> destination, int segmentNumber, bool isLastSegment)
		{
			var outOffset = 0;
			var encodingName = _encoding.WebName;
			var maxOutCount = destination.Length;
			if (maxOutCount <= ((segmentNumber == 0) ? (encodingName.Length + 2) : 0))
			{
				// ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			if (segmentNumber == 0)
			{
				AsciiCharSet.GetBytes (encodingName.AsSpan (), destination);
				outOffset += encodingName.Length;
				destination[outOffset++] = (byte)'\'';
				destination[outOffset++] = (byte)'\'';
			}

			int srcPos = 0;
			while ((srcPos < source.Length) && (outOffset < maxOutCount))
			{
				var octet = source[srcPos];
				var isToken = (octet != '%') && (octet < AsciiCharSet.Classes.Count) && ((AsciiCharSet.Classes[octet] & (short)AsciiCharClasses.Token) != 0);
				if (!isToken)
				{
					// знак процента вместо символа, потом два шест.знака
					if ((outOffset + 3) > maxOutCount)
					{
						break;
					}

					destination[outOffset++] = (byte)'%';
					var hex = Hex.OctetsUpper[octet];
					destination[outOffset++] = (byte)hex[0];
					destination[outOffset++] = (byte)hex[1];
				}
				else
				{
					destination[outOffset++] = octet;
				}

				srcPos++;
			}

			return new EncodingBalance (outOffset, srcPos);
		}
	}
}
