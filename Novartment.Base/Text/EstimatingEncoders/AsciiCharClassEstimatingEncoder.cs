using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Кодирует последовательность ASCII-символов указанного класса.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Ascii",
		Justification = "'ASCII' represents standard term.")]
	public class AsciiCharClassEstimatingEncoder :
		IEstimatingEncoder
	{
		private readonly AsciiCharClasses _enabledClass;

		/// <summary>
		/// Инициализирует новый экземпляр класса AsciiCharClassEstimatingEncoder использующий указанный класс символов.
		/// </summary>
		/// <param name="enabledClass">Классы стмволов, разрешённые для прямой предеачи без кодирования.</param>
		public AsciiCharClassEstimatingEncoder (AsciiCharClasses enabledClass)
		{
			_enabledClass = enabledClass;
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
				var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)source[offset + pos], _enabledClass);
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

			int pos = 0;
			while ((pos < count) && (pos < maxOutCount) && AsciiCharSet.IsCharOfClass ((char)source[offset + pos], _enabledClass))
			{
				pos++;
			}
			return new EncodingBalance (pos, pos);
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
			int pos = 0;
			while ((pos < count) && (pos < maxOutCount))
			{
				var c = source[offset + pos];
				var isEnabledClass = AsciiCharSet.IsCharOfClass ((char)c, _enabledClass);
				if (!isEnabledClass)
				{
					break;
				}
				destination[outOffset++] = c;
				pos++;
			}
			return new EncodingBalance (outOffset - outStartOffset, pos);
		}
	}
}
