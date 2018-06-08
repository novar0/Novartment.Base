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
		/// <param name="source">Массив байтов, содержащий порцию исходных данных.</param>
		/// <param name="offset">Позиция начала порции исходных данных.</param>
		/// <param name="count">Количество байтов в порции исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">segmentNumber не используется.</param>
		/// <param name="isLastSegment">isLastSegment не используется.</param>
		/// <returns>Кортеж из количества байтов, необходимых для результата кодирования и
		/// количества байтов источника, которое было использовано для кодирования.</returns>
		public EncodingBalance Estimate (byte[] source, int offset, int count, int maxOutCount, int segmentNumber, bool isLastSegment)
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

			while ((srcPos < count) && (dstPos < maxOutCount))
			{
				var c = source[offset + srcPos];
				var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)c, _enabledClasses);
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

					dstPos += 3; // знак процента вместо символа, потом два шест.знака
				}

				srcPos++;
			}

			dstPos += this.EpilogSize; // эпилог
			return new EncodingBalance (dstPos, srcPos);
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
			AsciiCharSet.GetBytes (_encoding.WebName.AsSpan (), destination.AsSpan (outOffset));
			outOffset += _encoding.WebName.Length;
			destination[outOffset++] = (byte)'?';
			destination[outOffset++] = (byte)'Q';
			destination[outOffset++] = (byte)'?';
			maxOutCount -= 2; // уменьшаем лимит на размер эпилога
			if ((outOffset - outStartOffset) >= maxOutCount)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}

			int srcPos = 0;
			while ((srcPos < count) && ((outOffset - outStartOffset) < maxOutCount))
			{
				var c = source[offset + srcPos];
				if (c == 32)
				{
					destination[outOffset++] = (byte)'_';
				}
				else
				{
					var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)c, _enabledClasses);
					if (isEnabledClass)
					{
						destination[outOffset++] = c;
					}
					else
					{
						// знак процента вместо символа, потом два шест.знака
						if ((outOffset - outStartOffset + 3) > maxOutCount)
						{
							break;
						}

						destination[outOffset++] = (byte)'=';
						var hex = Hex.OctetsUpper[c];
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
			return new EncodingBalance (outOffset - outStartOffset, srcPos);
		}
	}
}
