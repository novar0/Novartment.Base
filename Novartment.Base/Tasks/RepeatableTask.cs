using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Tasks
{
	/// <summary>
	/// A repeatable cancellable task based on the task creation factory.
	/// In case of the cancellation and when starting a new one, already running tasks are canceled.
	/// </summary>
	/// <remarks>
	/// Suitable for tasks such as opening a URL in a browser.
	/// Key Features.
	/// It is created in advance (and not during startup) and then supports multiple launches with different parameter values,
	/// which is convenient for declaratively assigning actions to user interface elements.
	/// Automatically creates/releases the necessary CancellationTokenSorce.
	/// Monitors the status of previous tasks (to which a cancel signal was sent) even if new ones are already running.
	/// </remarks>
	/// Official documentaion of the Task.Dispose method:
	/// «if your app targets the .NET Framework 4.5 or later, there is no need to call Dispose».
	/// Therefore, for objects of type such as Task and CancellationTokenSource, no release is performed that would greatly complicate the class.
#pragma warning disable CA1063 // Implement IDisposable Correctly
	public class RepeatableTask :
#pragma warning restore CA1063 // Implement IDisposable Correctly
		IDisposable
	{
		private readonly Func<object, CancellationToken, Task> _createTaskFunc;
		private readonly Action<object, CancellationToken> _taskAction;
		private readonly TaskScheduler _taskScheduler;
		private CancellationTokenSource _cancellationTokenSource = null;
		private int _tasksInProgressCount = 0;

		/// <summary>
		/// Initializes a new instance of the RepeatableTask class
		/// that will use the tasks returned by the specified task creation function.
		/// </summary>
		/// <param name="taskFactory">
		/// The task creation function. Will be called every time RepeatableTask starts.
		/// The task returned by the function should already be running.
		/// </param>
		public RepeatableTask (Func<object, CancellationToken, Task> taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException (nameof (taskFactory));
			}

			Contract.EndContractBlock ();

			_createTaskFunc = taskFactory;
		}

		/// <summary>
		/// Initializes a new instance of the RepeatableTask class
		/// that will create tasks by asynchronously invoking the specified method
		/// using the specified task scheduler.
		/// </summary>
		/// <param name="taskAction">
		/// The method, that will be asynchronously invoked every time RepeatableTask starts.
		/// </param>
		/// <param name="taskScheduler">
		/// The task scheduler, that will be used to asynchronously invoke starting action.
		/// Specify null-reference to use the default scheduler.
		/// </param>
		public RepeatableTask (Action<object, CancellationToken> taskAction, TaskScheduler taskScheduler = null)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException (nameof (taskAction));
			}

			Contract.EndContractBlock ();

			_taskScheduler = taskScheduler ?? TaskScheduler.Default;
			_taskAction = taskAction;
		}

		/// <summary>Occurs before the task starts.</summary>
		public event EventHandler<TaskStartingEventArgs> TaskStarting;

		/// <summary>Occurs after starting a task.</summary>
		public event EventHandler<DataEventArgs<object>> TaskStarted;

		/// <summary>Occurs after a task completes.</summary>
		public event EventHandler<DataEventArgs<CompletedTaskData>> TaskEnded;

		/// <summary>
		/// Gets a value indicating whether the task is currently running.
		/// </summary>
		public bool IsRunning => _tasksInProgressCount > 0;

		/// <summary>
		/// Starts a task with the specified state object.
		/// A previously started task is canceled.
		/// </summary>
		/// <param name="state">State object passed to the task being started.</param>
		public void Start (object state)
		{
			// уведомляем о запуске задачи. обработчик может установить флаг отмены запуска и поменять объект-состояние
			var startingArgs = new TaskStartingEventArgs (state);
			OnTaskStarting (startingArgs);
			if (startingArgs.Cancel)
			{
				return;
			}

			state = startingArgs.State;

			Interlocked.Increment (ref _tasksInProgressCount);

			// создаём новый токен запроса отмены, запрашивая отмену в предыдущем
			var newCts = new CancellationTokenSource ();
			Interlocked.Exchange (ref _cancellationTokenSource, newCts)?.Cancel ();
			var cancellationToken = newCts.Token;

			var task = _createTaskFunc?.Invoke (state, cancellationToken) ??
				Task.Factory.StartNew (
					st => _taskAction.Invoke (st, cancellationToken),
					state,
					cancellationToken,
					TaskCreationOptions.None,
					_taskScheduler);

			OnTaskStarted (new DataEventArgs<object> (state));

			if (task.IsCompleted)
			{
				Interlocked.Decrement (ref _tasksInProgressCount);
				OnTaskEnded (new DataEventArgs<CompletedTaskData> (new CompletedTaskData (task.Status, task.Exception, state)));
			}
			else
			{
				// для созданной задачи задаём продолжение для уведомления о выполнении, которое будет выполнено в текущем контексте
				var scheduler = (SynchronizationContext.Current != null) ?
					TaskScheduler.FromCurrentSynchronizationContext () :
					TaskScheduler.Current;

				task.ContinueWith (
					prevTask =>
					{
						Interlocked.Decrement (ref _tasksInProgressCount);
						var taskData = new CompletedTaskData (prevTask.Status, prevTask.Exception, state);
						OnTaskEnded (new DataEventArgs<CompletedTaskData> (taskData));
					},
					default,
					TaskContinuationOptions.None,
					scheduler);
			}
		}

		/// <summary>
		/// Cancels all previously started tasks.
		/// </summary>
		public void Cancel ()
		{
			Interlocked.Exchange (ref _cancellationTokenSource, null)?.Cancel ();
		}

#pragma warning disable CA1063 // Implement IDisposable Correctly
		/// <summary>
		/// Performs freeing and releasing resources.
		/// </summary>
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			TaskStarting = null;
			TaskStarted = null;
			TaskEnded = null;
			Cancel ();
		}

		/// <summary>
		/// Calls the TaskStarting event with the specified arguments.
		/// </summary>
		/// <param name="args">Аргументы события TaskStarting.</param>
		protected virtual void OnTaskStarting (TaskStartingEventArgs args)
		{
			this.TaskStarting?.Invoke (this, args);
		}

		/// <summary>
		/// Calls the TaskStarted event with the specified arguments.
		/// </summary>
		/// <param name="args">Аргументы события TaskStarted.</param>
		protected virtual void OnTaskStarted (DataEventArgs<object> args)
		{
			this.TaskStarted?.Invoke (this, args);
		}

		/// <summary>
		/// Calls the TaskEnded event with the specified arguments.
		/// </summary>
		/// <param name="args">Аргументы события TaskEnded.</param>
		protected virtual void OnTaskEnded (DataEventArgs<CompletedTaskData> args)
		{
			this.TaskEnded?.Invoke (this, args);
		}
	}
}
