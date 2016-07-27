using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Кодирует строку в кавычках, предваряя префиксом '\' символы '\' и '"'.
	/// </summary>
	public class QuotedStringEstimatingEncoder :
		IEstimatingEncoder
	{
		private readonly AsciiCharClasses _enabledClasses;

		/// <summary>
		/// Получает количество байтов, которые кодировщик записывает в качестве пролога перед данными.
		/// </summary>
		[SuppressMessage ("Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "Cant be static because implements interface memeber.")]
		public int PrologSize => 1;

		/// <summary>
		/// Получает количество байтов, которые кодировщик записывает в качестве эпилога после данных.
		/// </summary>
		[SuppressMessage ("Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "Cant be static because implements interface memeber.")]
		public int EpilogSize => 1;

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
		/// Проверяет что указанный сегмент массива байтов с исходными данными выглядит как результат кодирования.
		/// В ситуациях где метод кодирования определяется по виду данных, приведёт к ошибочному декодированию.
		/// </summary>
		/// <param name="source">Массив байтов для проверки.</param>
		/// <param name="offset">Позиция начала данных в массиве.</param>
		/// <param name="count">Количество байтов в массиве.</param>
		/// <returns>True если указанный сегмент массива байтов с исходными данными выглядит как результат кодирования.</returns>
		public bool MayConfuseDecoder (byte[] source, int offset, int count)
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
			Contract.EndContractBlock ();

			if (count < 2)
			{
				return false;
			}
			var endPos = offset + count;
			while (offset <= (endPos - 2))
			{
				if (source[offset] == (byte)'"')
				{
					var offset2 = offset + 1;
					while (offset2 < endPos)
					{
						if (source[offset2] == (byte)'"')
						{
							return true;
						}
						var c = (char)source[offset2++];
						if (c != '\\')
						{
							var isEnabledClass = AsciiCharSet.IsCharOfClass (c, _enabledClasses);
							if (!isEnabledClass)
							{
								break;
							}
						}
					}
				}
				offset++;
			}
			return false;
		}

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
			if ((count < 0) || (count > source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}
			Contract.EndContractBlock ();

			int pos = 0;
			while (pos < count)
			{
				var c = source[offset + pos];
				var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)c, _enabledClasses);
				if (isEnabledClass)
				{
					return offset + pos;
				}
				pos++;
			}
			return -1;
		}

		/// <summary>
		/// Оценивает потенциальный результат кодирования указанной порции массива байтов.
		/// </summary>
		/// <param name="source">Массив байтов, содержащий порцию исходных данных.</param>
		/// <param name="offset">Позиция начала порции исходных данных.</param>
		/// <param name="count">Количество байтов в порции исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">Не используется.</param>
		/// <param name="isLastSegment">Не используется.</param>
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
			int dstPos = 1; // начальная кавычка
			maxOutCount--; // уменьшаем лимит на конечную кавычку
			if (dstPos >= maxOutCount)
			{ // ничего кроме кавычек не влезет
				return new EncodingBalance (0, 0);
			}
			while ((srcPos < count) && (dstPos < maxOutCount))
			{
				var c = source[offset + srcPos];
				var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)c, _enabledClasses);
				if (!isEnabledClass)
				{
					break;
				}
				if ((c == 34) || (c == 92))
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
			{ // ничего кроме кавычек
				return new EncodingBalance (0, 0);
			}
			dstPos++; // конечная кавычка
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
		/// <param name="segmentNumber">Не используется.</param>
		/// <param name="isLastSegment">Не используется.</param>
		/// <returns>Кортеж из количества байтов, записанных в массив для результата кодирования и
		/// количества байтов источника, которое было использовано для кодирования.</returns>
		public EncodingBalance Encode (
			byte[] source, int offset, int count,
			byte[] destination, int outOffset, int maxOutCount,
			int segmentNumber, bool isLastSegment)
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
			int srcPos = 0;
			if (maxOutCount <= 2)
			{ // ничего кроме кавычек не влезет
				return new EncodingBalance (0, 0);
			}
			destination[outOffset++] = (byte)'"'; // начальная кавычка
			maxOutCount--; // уменьшаем лимит на конечную кавычку
			while ((srcPos < count) && ((outOffset - outStartOffset) < maxOutCount))
			{
				var c = source[offset + srcPos];
				var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)c, _enabledClasses);
				if (!isEnabledClass)
				{
					break;
				}
				if ((c == 34) || (c == 92))
				{
					// кавычка либо косая черта предваряется косой чертой
					if ((outOffset - outStartOffset + 2) > maxOutCount)
					{
						break;
					}
					destination[outOffset++] = (byte)'\\';
				}
				destination[outOffset++] = c;
				srcPos++;
			}
			if (srcPos < 1)
			{ // ничего кроме кавычек
				return new EncodingBalance (0, 0);
			}
			destination[outOffset++] = (byte)'"'; // конечная кавычка
			return new EncodingBalance (outOffset - outStartOffset, srcPos);
		}
	}
}
