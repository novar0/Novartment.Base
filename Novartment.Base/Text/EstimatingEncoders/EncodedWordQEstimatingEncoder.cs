using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Кодирует 'encoded-word' способом 'Q' согласно RFC 2047.
	/// </summary>
	public class EncodedWordQEstimatingEncoder :
		IEstimatingEncoder
	{
		private readonly Encoding _encoding;
		private readonly AsciiCharClasses _enabledClasses;

		/// <summary>
		/// Инициализирует новый экземпляр класса EncodedWordQEstimatingEncoder с указанием используемой кодировки.
		/// </summary>
		/// <param name="encoding">Кодировка, используемая для двоичного представления символов.</param>
		/// <param name="enabledClasses">Комбинация классов символов, разрешённых для прямого представления (без кодирования).</param>
		public EncodedWordQEstimatingEncoder (Encoding encoding, AsciiCharClasses enabledClasses)
		{
			if (encoding == null)
			{
				throw new ArgumentNullException (nameof (encoding));
			}

			Contract.EndContractBlock ();

			_encoding = encoding;
			_enabledClasses = enabledClasses;
		}

		/// <summary>
		/// Получает количество байтов, которые кодировщик записывает в качестве пролога перед данными.
		/// </summary>
		public int PrologSize => _encoding.WebName.Length + 5;

		/// <summary>
		/// Получает количество байтов, которые кодировщик записывает в качестве эпилога после данных.
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
		public EncodingBalance Estimate (ReadOnlySpan<byte> source, int maxOutCount, int segmentNumber, bool isLastSegment)
		{
			if (maxOutCount < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (maxOutCount));
			}

			Contract.EndContractBlock ();

			int srcPos = 0;
			var dstPos = this.PrologSize;
			maxOutCount -= 2; // уменьшаем лимит на размер эпилога
			if (dstPos >= maxOutCount)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			var asciiClasses = AsciiCharSet.ValueClasses.Span;
			while ((srcPos < source.Length) && (dstPos < maxOutCount))
			{
				var octet = source[srcPos];
				var isEnabledClass = (octet < asciiClasses.Length) && ((asciiClasses[octet] & _enabledClasses) != 0);
				if (isEnabledClass)
				{
					dstPos++;
				}
				else
				{
					if ((dstPos + 3) > maxOutCount)
					{
						break;
					}

					dstPos += 3; // знак процента, потом два шест.знака
				}

				srcPos++;
			}

			dstPos += this.EpilogSize; // эпилог
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
			destination[outOffset++] = (byte)'=';
			destination[outOffset++] = (byte)'?';
			AsciiCharSet.GetBytes (_encoding.WebName.AsSpan (), destination.Slice (outOffset));
			outOffset += _encoding.WebName.Length;
			destination[outOffset++] = (byte)'?';
			destination[outOffset++] = (byte)'Q';
			destination[outOffset++] = (byte)'?';
			var maxOutCount = destination.Length - 2; // уменьшаем лимит на размер эпилога
			if (outOffset >= maxOutCount)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			int srcPos = 0;
			var hexOctets = Hex.OctetsUpper.Span;
			var asciiClasses = AsciiCharSet.ValueClasses.Span;
			while ((srcPos < source.Length) && (outOffset < maxOutCount))
			{
				var octet = source[srcPos];
				if (octet == 32)
				{
					destination[outOffset++] = (byte)'_';
				}
				else
				{
					var isEnabledClass = (octet < asciiClasses.Length) && ((asciiClasses[octet] & _enabledClasses) != 0);
					if (isEnabledClass)
					{
						destination[outOffset++] = octet;
					}
					else
					{
						// знак процента вместо символа, потом два шест.знака
						if ((outOffset + 3) > maxOutCount)
						{
							break;
						}

						destination[outOffset++] = (byte)'=';
						var hex = hexOctets[octet];
						destination[outOffset++] = (byte)hex[0];
						destination[outOffset++] = (byte)hex[1];
					}
				}

				srcPos++;
			}

			if (srcPos < 1)
			{ // не влезло ничего кроме пролога и эпилога
				return new EncodingBalance (0, 0);
			}

			destination[outOffset++] = (byte)'?'; // эпилог
			destination[outOffset++] = (byte)'=';
			return new EncodingBalance (outOffset, srcPos);
		}
	}
}
