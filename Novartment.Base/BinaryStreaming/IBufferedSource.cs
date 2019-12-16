using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных для последовательного чтения, представленный байтовым буфером.
	/// </summary>
	[ContractClass (typeof (IBufferedSourceContracts))]
	public interface IBufferedSource
	{
		/// <summary>
		/// Получает буфер, в котором содержится некоторая часть данных источника.
		/// Текущая начальная позиция и количество доступных данных содержатся в свойствах Offset и Count,
		/// при этом сам буфер остаётся неизменным всё время жизни источника.
		/// </summary>
		ReadOnlyMemory<byte> BufferMemory { get; }

		/// <summary>
		/// Получает начальную позицию данных, доступных в BufferMemory.
		/// Количество данных, доступных в BufferMemory, содержится в Count.
		/// </summary>
		int Offset { get; }

		/// <summary>
		/// Получает количество данных, доступных в BufferMemory.
		/// Начальная позиция доступных данных содержится в Offset.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Получает признак исчерпания источника.
		/// Возвращает True если источник больше не поставляет данных.
		/// Содержимое буфера при этом остаётся верным, но больше не будет меняться.
		/// </summary>
		bool IsExhausted { get; }

		/// <summary>
		/// Асинхронно заполняет буфер данными источника, дополняя уже доступные там данные.
		/// В результате буфер может быть заполнен не полностью, если источник поставляет данные блоками,
		/// либо пуст, если источник исчерпался.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.
		/// Если после завершения в Count будет ноль,
		/// то источник исчерпан и доступных данных в буфере больше не будет.</returns>
		ValueTask FillBufferAsync (CancellationToken cancellationToken = default);

		/// <summary>
		/// Асинхронно запрашивает у источника указанное количество данных в буфере.
		/// В результате запроса в буфере может оказаться данных больше, чем запрошено.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="size">Требуемый размер данных в буфере.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Происходит если size меньше нуля или больше размера буфера данных.
		/// </exception>
		/// <exception cref="Novartment.Base.BinaryStreaming.NotEnoughDataException">
		/// Происходит если источник не может предоставить указанного количества данных.
		/// </exception>
		/// <returns>Задача, представляющая операцию.</returns>
		ValueTask EnsureBufferAsync (int size, CancellationToken cancellationToken = default);

		/// <summary>
		/// Пропускает указанное количество данных из начала доступных данных буфера.
		/// При выполнении может измениться свойство Offset.
		/// </summary>
		/// <param name="size">Размер данных для пропуска в начале доступных данных буфера.
		/// Должен быть меньше, чем размер доступных в буфере данных.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Происходит если size меньше нуля или больше размера доступных в буфере данных.
		/// </exception>
		void SkipBuffer (int size);
	}

	/// <summary>
	/// Содержит только метаданные контрактов для IBufferedSource.
	/// </summary>
	[ContractClassFor (typeof (IBufferedSource))]
	internal abstract class IBufferedSourceContracts :
		IBufferedSource
	{
		private IBufferedSourceContracts ()
		{
		}

		public ReadOnlyMemory<byte> BufferMemory => default;

		public int Offset => 0;

		public int Count => 0;

		public bool IsExhausted => false;

		public ValueTask FillBufferAsync (CancellationToken cancellationToken = default)
		{
			Contract.Ensures (this.BufferMemory.Equals (Contract.OldValue (this.BufferMemory)));
			Contract.Ensures ((this.Count > 0) || this.IsExhausted);
			Contract.EndContractBlock ();
			return default;
		}

		public ValueTask EnsureBufferAsync (int size, CancellationToken cancellationToken = default)
		{
			Contract.Requires (size >= 0);
			Contract.Requires (size <= this.BufferMemory.Length);

			Contract.Ensures (this.BufferMemory.Equals (Contract.OldValue (this.BufferMemory)));
			Contract.EndContractBlock ();
			return default;
		}

		public void SkipBuffer (int size)
		{
			Contract.Requires ((size >= 0) && (size <= this.Count));

			Contract.Ensures (this.BufferMemory.Equals (Contract.OldValue (this.BufferMemory)));
			Contract.Ensures ((this.Offset + this.Count) == (Contract.OldValue (this.Offset) + Contract.OldValue (this.Count)));
			Contract.Ensures (this.IsExhausted == Contract.OldValue (this.IsExhausted));
			Contract.EndContractBlock ();
		}
	}
}
