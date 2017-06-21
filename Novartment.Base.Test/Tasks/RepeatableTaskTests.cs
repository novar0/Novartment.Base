using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Tasks;
using Xunit;

namespace Novartment.Base.Test
{
	public class RepeatableTaskTests
	{
		[Fact]
		[Trait ("Category", "Tasks")]
		public void SwitchContextFromTaskFactory ()
		{
			var context = new SynchronizationContextMock ();
			var oldContext = SynchronizationContext.Current;
			int currentThreadId = Thread.CurrentThread.ManagedThreadId;
			int contextThreadId = context.ThreadId;
			try
			{
				SynchronizationContext.SetSynchronizationContext (context);
				var monitor = new RepeatableTaskMock ();
				using (var task = new RepeatableTask (monitor.TaskFactory))
				{
					task.TaskStarting += monitor.OnStarting;
					task.TaskStarted += monitor.OnStarted;
					task.TaskEnded += monitor.OnEnded;
					var arg = TaskArgument.SampleValue3;
					Assert.Null (monitor.CreatedEvent[(int)arg]);
					Assert.Null (monitor.StartingEvent[(int)arg]);
					Assert.Null (monitor.StartedEvent[(int)arg]);
					Assert.Null (monitor.EndedEvent[(int)arg]);
					task.Start (arg);
					Assert.True (task.IsRunning);
					monitor.CreatedEvent[(int)arg].Task.Wait ();
					monitor.StartingEvent[(int)arg].Task.Wait ();
					monitor.StartedEvent[(int)arg].Task.Wait ();
					Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
					Assert.Equal (currentThreadId, monitor.CreationThreadId[(int)arg]);
					Assert.Equal (currentThreadId, monitor.StartingThreadId[(int)arg]);
					Assert.Equal (currentThreadId, monitor.StartedThreadId[(int)arg]);
					monitor.Complete (arg);
					monitor.EndedEvent[(int)arg].Task.Wait ();
					Assert.Equal (contextThreadId, monitor.EndedThreadId[(int)arg]);
				}
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext (oldContext);
			}
		}

		[Fact]
		[Trait ("Category", "Tasks")]
		public void SwitchContextFromAction ()
		{
			var context = new SynchronizationContextMock ();
			var scheduler = new TaskSchedulerMock ();
			var oldContext = SynchronizationContext.Current;
			int currentThreadId = Thread.CurrentThread.ManagedThreadId;
			int schedulerThreadId = scheduler.ThreadId;
			int contextThreadId = context.ThreadId;
			try
			{
				SynchronizationContext.SetSynchronizationContext (context);
				var monitor = new RepeatableTaskMock ();
				using (var task = new RepeatableTask (monitor.TaskAction, scheduler))
				{
					task.TaskStarting += monitor.OnStarting;
					task.TaskStarted += monitor.OnStarted;
					task.TaskEnded += monitor.OnEnded;
					var arg = TaskArgument.SampleValue2;
					Assert.Null (monitor.CreatedEvent[(int)arg]);
					Assert.Null (monitor.StartingEvent[(int)arg]);
					Assert.Null (monitor.StartedEvent[(int)arg]);
					Assert.Null (monitor.EndedEvent[(int)arg]);
					task.Start (arg);
					Assert.True (task.IsRunning);
					monitor.CreatedEvent[(int)arg].Task.Wait ();
					monitor.StartingEvent[(int)arg].Task.Wait ();
					monitor.StartedEvent[(int)arg].Task.Wait ();
					Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
					Assert.Equal (schedulerThreadId, monitor.CreationThreadId[(int)arg]);
					Assert.Equal (currentThreadId, monitor.StartingThreadId[(int)arg]);
					Assert.Equal (currentThreadId, monitor.StartedThreadId[(int)arg]);
					monitor.Complete (arg);
					monitor.EndedEvent[(int)arg].Task.Wait ();
					Assert.Equal (contextThreadId, monitor.EndedThreadId[(int)arg]);
				}
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext (oldContext);
			}
		}

		[Fact]
		[Trait ("Category", "Tasks")]
		public void MultiRunFromTaskFactory ()
		{
			var monitor = new RepeatableTaskMock ();
			using (var task = new RepeatableTask (monitor.TaskFactory))
			{
				task.TaskStarting += monitor.OnStarting;
				task.TaskStarted += monitor.OnStarted;
				task.TaskEnded += monitor.OnEnded;
				Assert.False (task.IsRunning);

				// нормальный запуск
				var arg = TaskArgument.SampleValue1;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.False (task.IsRunning);

				// исключение
				arg = TaskArgument.ProcessingException;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.Equal (TaskStatus.Faulted, monitor.CompletedTaskData[(int)arg].Status);
				Assert.IsType<AggregateException> (monitor.CompletedTaskData[(int)arg].Exception);
				Assert.IsType<ArithmeticException> (monitor.CompletedTaskData[(int)arg].Exception.InnerException);
				Assert.False (task.IsRunning);

				// отмена из за запуска новой
				arg = TaskArgument.SampleValue2;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				var arg2 = TaskArgument.SampleValue3;
				task.Start (arg2); // запуск новой задачи пока не завершена предыдущая
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg2].Task.Wait ();
				monitor.StartingEvent[(int)arg2].Task.Wait ();
				monitor.StartedEvent[(int)arg2].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg2].Task.IsCompleted);
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.Equal (TaskStatus.Canceled, monitor.CompletedTaskData[(int)arg].Status);
				monitor.Complete (arg2);
				monitor.EndedEvent[(int)arg2].Task.Wait ();
				Assert.False (task.IsRunning);

				// отмена в процессе
				arg = TaskArgument.ProcessingCancel;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				task.Cancel ();
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.Equal (TaskStatus.Canceled, monitor.CompletedTaskData[(int)arg].Status);
				Assert.False (task.IsRunning);

				// отмена при подтверждении
				arg = TaskArgument.StartNotConfirmed;
				task.Start (arg);
				monitor.StartingEvent[(int)arg].Task.Wait ();
				Assert.False (task.IsRunning);
				Assert.False (monitor.CreatedEvent[(int)arg].Task.IsCompleted);
				Assert.False (monitor.StartedEvent[(int)arg].Task.IsCompleted);
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
			}
		}

		[Fact]
		[Trait ("Category", "Tasks")]
		public void MultiRunFromAction ()
		{
			var monitor = new RepeatableTaskMock ();
			using (var task = new RepeatableTask (monitor.TaskAction, TaskScheduler.Default))
			{
				task.TaskStarting += monitor.OnStarting;
				task.TaskStarted += monitor.OnStarted;
				task.TaskEnded += monitor.OnEnded;
				Assert.False (task.IsRunning);

				// нормальный запуск
				var arg = TaskArgument.SampleValue1;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.False (task.IsRunning);

				// исключение
				arg = TaskArgument.ProcessingException;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.Equal (TaskStatus.Faulted, monitor.CompletedTaskData[(int)arg].Status);
				Assert.IsType<AggregateException> (monitor.CompletedTaskData[(int)arg].Exception);
				Assert.IsType<ArithmeticException> (monitor.CompletedTaskData[(int)arg].Exception.InnerException);
				Assert.False (task.IsRunning);

				// отмена из за запуска новой
				arg = TaskArgument.SampleValue2;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				var arg2 = TaskArgument.SampleValue3;
				task.Start (arg2); // запуск новой задачи пока не завершена предыдущая
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg2].Task.Wait ();
				monitor.StartingEvent[(int)arg2].Task.Wait ();
				monitor.StartedEvent[(int)arg2].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg2].Task.IsCompleted);
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.Equal (TaskStatus.Canceled, monitor.CompletedTaskData[(int)arg].Status);
				monitor.Complete (arg2);
				monitor.EndedEvent[(int)arg2].Task.Wait ();
				Assert.False (task.IsRunning);

				// отмена в процессе
				arg = TaskArgument.ProcessingCancel;
				task.Start (arg);
				Assert.True (task.IsRunning);
				monitor.CreatedEvent[(int)arg].Task.Wait ();
				monitor.StartingEvent[(int)arg].Task.Wait ();
				monitor.StartedEvent[(int)arg].Task.Wait ();
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
				task.Cancel ();
				monitor.Complete (arg);
				monitor.EndedEvent[(int)arg].Task.Wait ();
				Assert.Equal (TaskStatus.Canceled, monitor.CompletedTaskData[(int)arg].Status);
				Assert.False (task.IsRunning);

				// отмена при подтверждении
				arg = TaskArgument.StartNotConfirmed;
				task.Start (arg);
				monitor.StartingEvent[(int)arg].Task.Wait ();
				Assert.False (task.IsRunning);
				Assert.False (monitor.CreatedEvent[(int)arg].Task.IsCompleted);
				Assert.False (monitor.StartedEvent[(int)arg].Task.IsCompleted);
				Assert.False (monitor.EndedEvent[(int)arg].Task.IsCompleted);
			}
		}
	}
}
