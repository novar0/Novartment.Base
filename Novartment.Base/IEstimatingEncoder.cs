using System;

namespace Novartment.Base
{
	/// <summary>
	/// Кодировщик диапазона байтов в другой формат,
	/// позволяющий заранее оценить валидность исходных данных и размер результата.
	/// </summary>
	public interface IEstimatingEncoder
	{
		/// <summary>
		/// Получает количество байтов, которые будут вставлены перед данными.
		/// </summary>
		int PrologSize { get; }

		/// <summary>
		/// Получает количество байтов, которые будут вставлены после данных.
		/// </summary>
		int EpilogSize { get; }

		/// <summary>
		/// Оценивает потенциальный результат кодирования диапазона байтов.
		/// </summary>
		/// <param name="source">Диапазон байтов исходных данных.</param>
		/// <param name="maxOutCount">Максимальное количество байтов, которое может содержать результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признак того, что указанный диапазон исходных данных является последним.</param>
		/// <returns>Баланс потенциальной операции кодирования.</returns>
		EncodingBalance Estimate (ReadOnlySpan<byte> source, int maxOutCount, int segmentNumber, bool isLastSegment);

		/// <summary>
		/// Кодирует указанную порцию диапазона байтов.
		/// </summary>
		/// <param name="source">Диапазон байтов, содержащий порцию исходных данных.</param>
		/// <param name="destination">Диапазон байтов, куда будет записываться результат кодирования.</param>
		/// <param name="segmentNumber">Номер порции с результирующими данными.</param>
		/// <param name="isLastSegment">Признако того, что указанный диапазон исходных данных является последним.</param>
		/// <returns>Баланс операции кодирования.</returns>
		EncodingBalance Encode (ReadOnlySpan<byte> source, Span<byte> destination, int segmentNumber, bool isLastSegment);
	}

#pragma warning disable CA1815 // Override equals and operator equals on value types
	/// <summary>
	/// Баланс операции кодирования в виде количества использованных и произведённых байтов.
	/// </summary>
	public readonly struct EncodingBalance
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		/// <summary>
		/// Инициализирует новый экземпляр EncodingBalance с указанным
		/// количеством использованных и произведённых байтов.
		/// </summary>
		/// <param name="bytesProduced">Количество использованных байтов.</param>
		/// <param name="bytesConsumed">Количество произведённых байтов.</param>
		public EncodingBalance (int bytesProduced, int bytesConsumed)
		{
			this.BytesProduced = bytesProduced;
			this.BytesConsumed = bytesConsumed;
		}

		/// <summary>
		/// Получает количество байтов, произведённых в результате кодирования.
		/// </summary>
		public readonly int BytesProduced { get; }

		/// <summary>
		/// Получает количество байтов, использованных при кодировании.
		/// </summary>
		public readonly int BytesConsumed { get; }

		/// <summary>
		/// Деконструирует данные.
		/// </summary>
		/// <param name="bytesProduced">Получает количество байтов, произведённых в результате кодирования.</param>
		/// <param name="bytesConsumed">Получает количество байтов, использованных при кодировании.</param>
		public readonly void Deconstruct (out int bytesProduced, out int bytesConsumed)
		{
			bytesProduced = this.BytesProduced;
			bytesConsumed = this.BytesConsumed;
		}
	}
}
