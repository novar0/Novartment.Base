using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Кодирует последовательность ASCII-символов указанного класса.
	/// </summary>
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

			int pos = 0;
			var asciiClasses = AsciiCharSet.Classes.Span;
			while ((pos < source.Length) && (pos < maxOutCount) && (source[pos] < asciiClasses.Length) && ((asciiClasses[source[pos]] & _enabledClass) != 0))
			{
				pos++;
			}

			return new EncodingBalance (pos, pos);
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
			var pos = 0;
			var outOffset = 0;
			var asciiClasses = AsciiCharSet.Classes.Span;
			while ((pos < source.Length) && (pos < destination.Length))
			{
				var octet = source[pos];
				if ((octet >= asciiClasses.Length) || ((asciiClasses[octet] & _enabledClass) == 0))
				{
					break;
				}

				destination[outOffset++] = octet;
				pos++;
			}

			return new EncodingBalance (outOffset, pos);
		}
	}
}
