using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Tasks;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Управляемая командами повторяемая задача для привязки к элементам интерфейса.
	/// </summary>
	public class CommandedRepeatableTask : RepeatableTask,
		INotifyPropertyChanged
	{
		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанной фабрики по производству задач.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		public CommandedRepeatableTask (Func<object, CancellationToken, Task> taskFactory)
			: base (taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException (nameof (taskFactory));
			}

			var startChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenAll);
			this.StartCommand = new ChainedRelayCommand<object> (startChain, StartInternal, CanStart);

			var stopChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenAny);
			this.StopCommand = new ChainedRelayCommand (stopChain, Cancel, CanCancel);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанного делегата и планировщика задач.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		public CommandedRepeatableTask (Action<object, CancellationToken> taskAction, TaskScheduler taskScheduler)
			: base (taskAction, taskScheduler)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException (nameof (taskAction));
			}

			if (taskScheduler == null)
			{
				throw new ArgumentNullException (nameof (taskScheduler));
			}

			var startChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenAll);
			this.StartCommand = new ChainedRelayCommand<object> (startChain, StartInternal, CanStart);

			var stopChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenAny);
			this.StopCommand = new ChainedRelayCommand (stopChain, Cancel, CanCancel);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанной фабрики по производству задач и предыдущей задачи в цепи.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		/// <param name="previousTask">Предыдущая задача, в цепь команд которой будут добавлены команды создаваемой задачи.</param>
		protected CommandedRepeatableTask (Func<object, CancellationToken, Task> taskFactory, CommandedRepeatableTask previousTask)
			: base (taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException (nameof (taskFactory));
			}

			if (previousTask == null)
			{
				throw new ArgumentNullException (nameof (previousTask));
			}

			this.StartCommand = new ChainedRelayCommand<object> (previousTask.StartCommand.Chain, StartInternal, CanStart);
			this.StopCommand = new ChainedRelayCommand (previousTask.StopCommand.Chain, Cancel, CanCancel);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанного делегата, планировщика задач и предыдущей задачи в цепи.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		/// <param name="previousTask">Предыдущая задача, в цепь команд которой будут добавлены команды создаваемой задачи.</param>
		protected CommandedRepeatableTask (
			Action<object, CancellationToken> taskAction,
			TaskScheduler taskScheduler,
			CommandedRepeatableTask previousTask)
			: base (taskAction, taskScheduler)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException (nameof (taskAction));
			}

			if (taskScheduler == null)
			{
				throw new ArgumentNullException (nameof (taskScheduler));
			}

			if (previousTask == null)
			{
				throw new ArgumentNullException (nameof (previousTask));
			}

			this.StartCommand = new ChainedRelayCommand<object> (previousTask.StartCommand.Chain, StartInternal, CanStart);
			this.StopCommand = new ChainedRelayCommand (previousTask.StopCommand.Chain, Cancel, CanCancel);
		}

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Получает команду запуска задачи.</summary>
		public ChainedRelayCommand<object> StartCommand { get; }

		/// <summary>Получает команду остановки задачи.</summary>
		public ChainedRelayCommand StopCommand { get; }

		/// <summary>
		/// Создаёт связанную задачу на основе указанной фабрики по производству задач.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		/// <returns>Повторяемая задача на основе указанного делегата создания задачи старта.</returns>
		public CommandedRepeatableTask CreateLinked (Func<object, CancellationToken, Task> taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException (nameof (taskFactory));
			}

			return new CommandedRepeatableTask (taskFactory, this);
		}

		/// <summary>
		/// Создаёт связанную задачу на основе указанного делегата и планировщика задач.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		/// <returns>Повторяемая задача на основе указанного делегата создания задачи старта.</returns>
		public CommandedRepeatableTask CreateLinked (Action<object, CancellationToken> taskAction, TaskScheduler taskScheduler)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException (nameof (taskAction));
			}

			if (taskScheduler == null)
			{
				throw new ArgumentNullException (nameof (taskScheduler));
			}

			return new CommandedRepeatableTask (taskAction, taskScheduler, this);
		}

		/// <summary>
		/// Запускает задачу. Преобразовывает объект-состояние при переопределении в наследованном классе.
		/// </summary>
		/// <param name="state">Объект-состояние, передаваемый в запускаемую задачу.</param>
		protected virtual void StartInternal (object state)
		{
			Start (state);
		}

		/// <summary>
		/// Вызывает событие TaskStarted с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskStarted.</param>
		protected override void OnTaskStarted (DataEventArgs<object> args)
		{
			base.OnTaskStarted (args);
			this.StartCommand.RaiseCanExecuteChanged ();
			this.StopCommand.RaiseCanExecuteChanged ();
			OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsRunning)));
		}

		/// <summary>
		/// Вызывает событие TaskEnded с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskEnded.</param>
		protected override void OnTaskEnded (DataEventArgs<CompletedTaskData> args)
		{
			base.OnTaskEnded (args);
			this.StartCommand.RaiseCanExecuteChanged ();
			this.StopCommand.RaiseCanExecuteChanged ();
			OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsRunning)));
		}

		/// <summary>
		/// Вызывает событие PropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события PropertyChanged.</param>
		protected virtual void OnPropertyChanged (PropertyChangedEventArgs args)
		{
			this.PropertyChanged?.Invoke (this, args);
		}

		private bool CanStart (object notUsed)
		{
			return !this.IsRunning;
		}

		private bool CanCancel ()
		{
			return this.IsRunning;
		}
	}
}
