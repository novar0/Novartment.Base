using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности с простым (discrete) содержимым согласно RFC 2045.
	/// </summary>
	public interface IDiscreteEntityBody :
		IEntityBody
	{
		/// <summary>
		/// Возвращает декодированное тело сущности в виде источника данных.
		/// </summary>
		/// <returns>Декодированное тело сущности в виде источника данных.</returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1024:UsePropertiesWhereAppropriate",
			Justification = "Every time method creates new mutable result.")]
		IBufferedSource GetDataSource ();

		/// <summary>
		/// Устанавливает тело сущности считывая данные из указанного источника.
		/// </summary>
		/// <param name="data">Источник данных, содержимое которого станет телом сущности.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является количество байтов,
		/// которое заняли данные в теле сущности в закодированном виде.</returns>
		Task<int> SetDataAsync (IBufferedSource data, CancellationToken cancellationToken);
	}
}
