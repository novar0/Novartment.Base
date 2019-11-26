namespace Novartment.Base
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
	/// <summary>
	/// Позиция/размер порции данных и выбранный для неё кодировщик.
	/// </summary>
	public readonly ref struct EstimatingEncoderChunk
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса ChunkEncoderSelection с указанным
		/// кодировщиком, начальной позицией и размером порции.
		/// </summary>
		/// <param name="encoder">Получает кодировщик, который выбран для порции.</param>
		/// <param name="offset">Получает начальная позиция порции в исходных данных.</param>
		/// <param name="count">Получает размер порции.</param>
		public EstimatingEncoderChunk (IEstimatingEncoder encoder, int offset, int count)
		{
			this.Encoder = encoder;
			this.Offset = offset;
			this.Count = count;
		}

		/// <summary>
		/// Кодировщик, который выбран для порции.
		/// </summary>
		public readonly IEstimatingEncoder Encoder { get; }

		/// <summary>
		/// Начальная позиция порции в исходных данных.
		/// </summary>
		public readonly int Offset { get; }

		/// <summary>
		/// Размер порции.
		/// </summary>
		public readonly int Count { get; }

		/// <summary>
		/// Деконструирует данные.
		/// </summary>
		/// <param name="encoder">Получает кодировщик, который выбран для порции.</param>
		/// <param name="offset">Получает начальную позицию порции в исходных данных.</param>
		/// <param name="count">Получает размер порции.</param>
		public readonly void Deconstruct (out IEstimatingEncoder encoder, out int offset, out int count)
		{
			encoder = this.Encoder;
			offset = this.Offset;
			count = this.Count;
		}
	}
}
