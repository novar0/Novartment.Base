using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Поставщик, у которого можно запрашивать задания.
	/// </summary>
	/// <typeparam name="TItem">Тип входного параметра заданий.</typeparam>
	/// <typeparam name="TResult">Тип, возвращаемый заданиями.</typeparam>
	public interface IJobProvider<TItem, TResult>
	{
		/// <summary>
		/// Асинхронно запрашивает задание у поставщика.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, представляющая получение задания.
		/// Результатом задачи будет производитель выполнения полученного задания.
		/// </returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller expects exactly this behavior.")]
		Task<JobCompletionSource<TItem, TResult>> TakeJobAsync (CancellationToken cancellationToken);
	}
}
