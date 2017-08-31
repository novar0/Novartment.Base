using System.Threading.Tasks;

namespace Novartment.Base
{
	/// <summary>
	/// Производитель выполнения задания, используемый для отражения состояния выполнения задания.
	/// </summary>
	/// <typeparam name="TItem">Тип входного параметра заданий.</typeparam>
	/// <typeparam name="TResult">Тип, возвращаемый заданиями.</typeparam>
	public class JobCompletionSource<TItem, TResult> : TaskCompletionSource<TResult>
	{
		/// <summary>
		/// Инициализирует новый экземпляр JobCompletionSource с укзанным входным параметром.
		/// </summary>
		/// <param name="state">Входной параметр задания.</param>
		public JobCompletionSource(TItem state)
			: base(state)
		{
		}

		/// <summary>
		/// Получает признак того, экземпляр является производителем выполнения особого задания-маркера.
		/// </summary>
		public virtual bool IsMarker => false;

		/// <summary>
		/// Получает входной параметр задания.
		/// </summary>
		public TItem Item => (TItem)this.Task.AsyncState;
	}
}
