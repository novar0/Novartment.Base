using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Novartment.Base.Tasks
{
	/// <summary>
	/// Параметры завершённой задачи.
	/// </summary>
	public class CompletedTaskData
	{
		/// <summary>
		/// Инициализирует новый экземпляр CompletedTaskData на основе указанного состояния, исключения и состояния.
		/// </summary>
		/// <param name="status">Состояние задачи.</param>
		/// <param name="exception">Исключения задачи.</param>
		/// <param name="state">Объект-состояние задачи.</param>
		public CompletedTaskData(TaskStatus status, AggregateException exception, object state)
		{
			if ((status != TaskStatus.RanToCompletion) &&
				(status != TaskStatus.Canceled) &&
				(status != TaskStatus.Faulted))
			{
				throw new ArgumentOutOfRangeException(nameof(status));
			}

			Contract.EndContractBlock();

			this.Status = status;
			this.Exception = exception;
			this.State = state;
		}

		/// <summary>
		/// Инициализирует новый экземпляр CompletedTaskData на основе указанной задачи.
		/// </summary>
		/// <param name="task">Завершённая задача, параметры которой будет содержать создаваемый экземпляр.</param>
		public CompletedTaskData(Task task)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			if (!task.IsCompleted)
			{
				throw new ArgumentOutOfRangeException(nameof(task));
			}

			Contract.EndContractBlock();

			this.Status = task.Status;
			this.Exception = task.Exception;
			this.State = task.AsyncState;
		}

		/// <summary>
		/// Получает статус задачи: RanToCompletion, Canceled или Faulted.
		/// </summary>
		public TaskStatus Status { get; }

		/// <summary>
		/// Получает исключение, возникшее в ходе выполнения задачи. Null если исключений не было.
		/// </summary>
		public AggregateException Exception { get; }

		/// <summary>
		/// Получает объект-состояние задачи.
		/// </summary>
		public object State { get; }
	}
}
