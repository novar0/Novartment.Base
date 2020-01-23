using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Novartment.Base.Tasks
{
	/// <summary>
	/// A completed task parameters.
	/// </summary>
	public class CompletedTaskData
	{
		/// <summary>
		/// Initializes a new instance of the CompletedTaskData class
		/// with the specified status, the exception and the state object.
		/// </summary>
		/// <param name="status">The status of the task.</param>
		/// <param name="exception">The exception of the task.</param>
		/// <param name="state">The state object of the task.</param>
		public CompletedTaskData (TaskStatus status, AggregateException exception, object state)
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
		/// Initializes a new instance of the CompletedTaskData class
		/// with the parameters of the specified task.
		/// </summary>
		/// <param name="task">A completed task, the parameters of which will contain the created instance.</param>
		public CompletedTaskData (Task task)
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
		/// Gets the status of the task: RanToCompletion, Canceled или Faulted.
		/// </summary>
		public TaskStatus Status { get; }

		/// <summary>
		/// Gets an exception that occurred during the execution of a task.
		/// The null-reference indicates that no exceptions have occured.
		/// </summary>
		public AggregateException Exception { get; }

		/// <summary>
		/// Gets the state object of the task.
		/// </summary>
		public object State { get; }
	}
}
