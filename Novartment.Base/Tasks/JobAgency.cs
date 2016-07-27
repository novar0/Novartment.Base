using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base
{
	/// <summary>
	/// Служба, собирающая задания и асинхронно раздающая их выполнение.
	/// </summary>
	/// <typeparam name="TItem">Тип входного параметра заданий.</typeparam>
	/// <typeparam name="TResult">Тип результата, возвращаемого заданиями.</typeparam>
	public class JobAgency<TItem, TResult> :
		IJobProvider<TItem, TResult>
	{
		private readonly Queue<JobCompletionSource<TItem, TResult>> _producers = new Queue<JobCompletionSource<TItem, TResult>> ();
		private readonly Queue<TaskCompletionSource<JobCompletionSource<TItem, TResult>>> _consumers =
			new Queue<TaskCompletionSource<JobCompletionSource<TItem, TResult>>> ();

		/// <summary>
		/// Инициализирует новый экземпляр JobAgency.
		/// </summary>
		public JobAgency ()
		{
		}

		/// <summary>
		/// Ставит задание в очередь для последующего выполнения.
		/// Выполнение задания начинается асинхронно после вызова TakeJobAsync().
		/// </summary>
		/// <param name="jobParameter">Входной параметр задания.</param>
		/// <returns>Задача, представляющая выполнение задания.</returns>
		/// <remarks>
		/// Метод совершенно синхронный и не может быть использован по шаблону Task-based Asynchronous Pattern (TAP).
		/// Возвращаемая задача не имеет отношения к смысловой нагрузке метода.
		/// </remarks>
		public Task<TResult> OfferJob (TItem jobParameter)
		{
			var completionSource = new JobCompletionSource<TItem, TResult> (jobParameter);
			OfferJobCompletionSource (completionSource);
			return completionSource.Task;
		}

		/// <summary>
		/// Добавляет маркер в очередь заданий.
		/// При получении маркера в методе TakeJobAsync(), у него будет установлен флаг IsMarker.
		/// </summary>
		/// <returns>Задача, представляющая выполнение задания-маркера.</returns>
		public Task PutMarker ()
		{
			var completionSource = JobCompletionSource<TItem, TResult>.Marker;
			OfferJobCompletionSource (completionSource);
			return completionSource.Task;
		}

		private void OfferJobCompletionSource (JobCompletionSource<TItem, TResult> completionSource)
		{
			TaskCompletionSource<JobCompletionSource<TItem, TResult>> consumer;
			lock (_producers)
			{
				do
				{
					if (_consumers.Count < 1)
					{
						// нет ожиданий потребителей, складываем запрос в производственную очередь
						_producers.Enqueue (completionSource);
						return;
					}
					// берём самое старое ожидание потребителя
					consumer = _consumers.Dequeue ();
				} while (consumer.Task.IsCompleted); // пропускаем отменённые ожидания потребителей
			}
			// удовлетворяем самое старое ожидание потребителя
			consumer.TrySetResult (completionSource);
		}

		/// <summary>
		/// Асинхронно запрашивает задание у службы.
		/// При получении маркера, добавленного методом PutMarker(), у него будет установлен флаг IsMarker.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, представляющая получение задания.
		/// Результатом задачи будет производитель выполнения полученного задания.
		/// </returns>
		public Task<JobCompletionSource<TItem, TResult>> TakeJobAsync (CancellationToken cancellationToken)
		{
			lock (_producers)
			{
				if (_producers.Count > 0)
				{
					// возвращаем самое старое задание из производственной очереди
					return Task.FromResult (_producers.Dequeue ());
				}
				// производственная очередь пуста, регистрируем ожидание потребителя
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<JobCompletionSource<TItem, TResult>> (cancellationToken);
				}
				var consumer = new TaskCompletionSource<JobCompletionSource<TItem, TResult>> ();
				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.Register (() => consumer.TrySetCanceled (), false);
				}
				_consumers.Enqueue (consumer);
				return consumer.Task;
			}
		}
	}
}
