using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Кодирует строку в кавычках, предваряя префиксом '\' символы '\' и '"'.
	/// </summary>
	public sealed class QuotedStringEstimatingEncoder :
		IEstimatingEncoder
	{
		private readonly AsciiCharClasses _enabledClasses;

		/// <summary>
		/// Инициализирует новый экземпляр класса QuotedStringEstimatingEncoder с классом допустимых символов по умолчанию.
		/// </summary>
		public QuotedStringEstimatingEncoder ()
			: this (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса QuotedStringEstimatingEncoder с указанием класса допустимых символов.
		/// </summary>
		/// <param name="enabledClasses">Комбинация классов символов, разрешённых для кодирования.</param>
		public QuotedStringEstimatingEncoder (AsciiCharClasses enabledClasses)
		{
			_enabledClasses = enabledClasses;
		}

		/// <summary>
		/// Получает количество байтов, которые кодировщик записывает в качестве пролога перед данными.
		/// </summary>
		public int PrologSize => 1;

		/// <summary>
		/// Получает количество байтов, которые кодировщик записывает в качестве эпилога после данных.
		/// </summary>
		public int EpilogSize => 1;

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
			int dstPos = 1; // начальная кавычка
			maxOutCount--; // уменьшаем лимит на конечную кавычку
			if (dstPos >= maxOutCount)
			{ // ничего кроме кавычек не влезет
				return new EncodingBalance (0, 0);
			}

			var asciiClasses = AsciiCharSet.ValueClasses.Span;
			while ((srcPos < source.Length) && (dstPos < maxOutCount))
			{
				var octet = source[srcPos];
				var isEnabledClass = (octet < asciiClasses.Length) && ((asciiClasses[octet] & _enabledClasses) != 0);
				if (!isEnabledClass)
				{
					break;
				}

				if ((octet == 34) || (octet == 92))
				{
					// кавычка либо косая черта предваряется косой чертой
					if ((dstPos + 2) > maxOutCount)
					{
						break;
					}

					dstPos += 2;
				}
				else
				{
					dstPos++;
				}

				srcPos++;
			}

			if (srcPos < 1)
			{
				// ничего кроме кавычек
				return new EncodingBalance (0, 0);
			}

			dstPos++; // конечная кавычка
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
			int srcPos = 0;
			var maxOutCount = destination.Length;
			if (maxOutCount <= 2)
			{ // ничего кроме кавычек не влезет
				return new EncodingBalance (0, 0);
			}

			destination[outOffset++] = (byte)'"'; // начальная кавычка
			maxOutCount--; // уменьшаем лимит на конечную кавычку
			var asciiClasses = AsciiCharSet.ValueClasses.Span;
			while ((srcPos < source.Length) && (outOffset < maxOutCount))
			{
				var octet = source[srcPos];
				var isEnabledClass = (octet < asciiClasses.Length) && ((asciiClasses[octet] & _enabledClasses) != 0);
				if (!isEnabledClass)
				{
					break;
				}

				if ((octet == 34) || (octet == 92))
				{
					// кавычка либо косая черта предваряется косой чертой
					if ((outOffset + 2) > maxOutCount)
					{
						break;
					}

					destination[outOffset++] = (byte)'\\';
				}

				destination[outOffset++] = octet;
				srcPos++;
			}

			if (srcPos < 1)
			{
				// ничего кроме кавычек
				return new EncodingBalance (0, 0);
			}

			destination[outOffset++] = (byte)'"'; // конечная кавычка
			return new EncodingBalance (outOffset, srcPos);
		}
	}
}
