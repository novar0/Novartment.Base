using System;

namespace Novartment.Base
{
	/// <summary>
	/// Кодировщик порций массива байтов в другой формат,
	/// позволяющий заранее оценить валидность исходных данных и размер результата.
	/// </summary>
	public interface IEstimatingEncoder
	{
		/// <summary>
		/// Проверяет что указанный сегмент массива байтов с исходными данными выглядит как результат кодирования.
		/// В ситуациях где метод кодирования определяется по виду данных, приведёт к ошибочному декодированию.
		/// </summary>
		/// <param name="source">Массив байтов для проверки.</param>
		/// <param name="offset">Позиция начала данных в массиве.</param>
		/// <param name="count">Количество байтов в массиве.</param>
		/// <returns>True если указанный сегмент массива байтов с исходными данными выглядит как результат кодирования.</returns>
		bool MayConfuseDecoder (byte[] source, int offset, int count);

		/// <summary>
		/// В указанном массиве байтов ищет ближайшую позицию данных,
		/// подходящих для кодировщика.
		/// </summary>
		/// <param name="source">Исходный массив байтов.</param>
		/// <param name="offset">Позиция начала исходных данных в массиве.</param>
		/// <param name="count">Количество байтов исходных данных в массиве.</param>
		/// <returns>Ближайшая позиция данных, подходящих для кодировщика,
		/// либо -1 если подходящих данных не найдено.</returns>
		int FindValid (byte[] source, int offset, int count);

		/// <summary>
		/// Оценивает потенциальный результат кодирования указанной порции массива байтов.
		/// </summary>
		/// <param name="source">Массив байтов, содержащий порцию исходных данных.</param>
		/// <param name="offset">Позиция начала порции исходных данных.</param>
		/// <param name="count">Количество байтов в порции исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признако того, что указанная порция исходных данных является последней.</param>
		/// <returns>Кортеж из количества байтов, необходимых для результата кодирования и
		/// количества байтов источника, которое было использовано для кодирования.</returns>
		EncodingBalance Estimate (
			byte[] source, int offset, int count,
			int maxOutCount,
			int segmentNumber,
			bool isLastSegment);

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
		/// <param name="isLastSegment">Признако того, что указанная порция исходных данных является последней.</param>
		/// <returns>Кортеж из количества байтов, записанных в массив для результата кодирования и
		/// количества байтов источника, которое было использовано для кодирования.</returns>
		EncodingBalance Encode (
			byte[] source, int offset, int count,
			byte[] destination, int outOffset, int maxOutCount,
			int segmentNumber,
			bool isLastSegment);
	}

	/// <summary>
	/// Баланс операции кодирования в виде количества использованных и произведённых байтов.
	/// </summary>
	public struct EncodingBalance
	{
		/// <summary>
		/// Получает количество байтов, произведённых в результате кодирования.
		/// </summary>
		public int BytesProduced { get; }

		/// <summary>
		/// Получает количество байтов, использованных при кодировании.
		/// </summary>
		public int BytesConsumed { get; }

		/// <summary>
		/// Инициализирует новый экземпляр EncodingBalance с указанным
		/// количеством использованных и произведённых байтов.
		/// </summary>
		/// <param name="bytesProduced"></param>
		/// <param name="bytesConsumed"></param>
		public EncodingBalance (int bytesProduced, int bytesConsumed)
		{
			this.BytesProduced = bytesProduced;
			this.BytesConsumed = bytesConsumed;
		}
	}
}
