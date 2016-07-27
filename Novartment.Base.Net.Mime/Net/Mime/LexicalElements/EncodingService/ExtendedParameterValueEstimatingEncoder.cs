using System;
using System.Text;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Кодирует 'extended-initial-value' и 'extended-other-values' согласно RFC 2231.
	/// </summary>
	internal class ExtendedParameterValueEstimatingEncoder :
		IEstimatingEncoder
	{
		private readonly Encoding _encoding;

		/// <summary>
		/// Инициализирует новый экземпляр класса ExtendedParameterValueEstimatingEncoder с указанием используемой кодировки.
		/// </summary>
		/// <param name="encoding">Кодировка, используемая для двоичного представления символов.</param>
		internal ExtendedParameterValueEstimatingEncoder (Encoding encoding)
		{
			if (encoding == null)
			{
				throw new ArgumentNullException (nameof (encoding));
			}
			Contract.EndContractBlock ();

			_encoding = encoding;
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
			Contract.EndContractBlock ();

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
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
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
			var dstPos = (segmentNumber == 0) ? (_encoding.WebName.Length + 2) : 0;
			if (dstPos >= maxOutCount)
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}
			while ((srcPos < count) && (dstPos < maxOutCount))
			{
				var c = source[offset + srcPos];
				var isToken = (c != '%') && AsciiCharSet.IsCharOfClass ((char)c, AsciiCharClasses.Token);
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
		/// Кодирует указанную порцию массива байтов.
		/// </summary>
		/// <param name="source">Массив байтов, содержащий порцию исходных данных.</param>
		/// <param name="offset">Позиция начала порции исходных данных.</param>
		/// <param name="count">Количество байтов в порции исходных данных.</param>
		/// <param name="destination">Массив байтов, куда будет записываться результат кодирования.</param>
		/// <param name="outOffset">Позиция в destination куда будет записываться результат кодирования.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
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
			var encodingName = _encoding.WebName;
			if (maxOutCount <= ((segmentNumber == 0) ? (encodingName.Length + 2) : 0))
			{ // ничего кроме пролога не влезет
				return new EncodingBalance (0, 0);
			}
			if (segmentNumber == 0)
			{
				AsciiCharSet.GetBytes (encodingName, 0, encodingName.Length, destination, outOffset);
				outOffset += encodingName.Length;
				destination[outOffset++] = (byte)'\'';
				destination[outOffset++] = (byte)'\'';
			}
			int srcPos = 0;
			while ((srcPos < count) && ((outOffset - outStartOffset) < maxOutCount))
			{
				var c = source[offset + srcPos];
				var isToken = (c != '%') && AsciiCharSet.IsCharOfClass ((char)c, AsciiCharClasses.Token);
				if (!isToken)
				{
					// знак процента вместо символа, потом два шест.знака
					if ((outOffset - outStartOffset + 3) > maxOutCount)
					{
						break;
					}
					destination[outOffset++] = (byte)'%';
					var hex = Hex.OctetsUpper[c];
					destination[outOffset++] = (byte)hex[0];
					destination[outOffset++] = (byte)hex[1];
				}
				else
				{
					destination[outOffset++] = c;
				}
				srcPos++;
			}
			return new EncodingBalance (outOffset - outStartOffset, srcPos);
		}
	}
}
