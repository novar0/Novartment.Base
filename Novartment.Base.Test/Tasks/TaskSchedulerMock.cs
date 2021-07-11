using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Test
{
	internal sealed class TaskSchedulerMock : TaskScheduler, IDisposable
	{
		private readonly Thread _thread;
		private readonly BlockingCollection<Task> _tasks = new ();

		internal TaskSchedulerMock ()
		{
			_thread = new Thread (ExecuteTaskFromQueue);
			_thread.Start ();
		}

		internal int ThreadId => _thread.ManagedThreadId;

		public void Dispose ()
		{
			_tasks.Dispose ();
		}

		protected override IEnumerable<Task> GetScheduledTasks ()
		{
			return null;
		}

		protected override void QueueTask (Task task)
		{
			_tasks.Add (task);
		}

		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			return false;
		}

		private void ExecuteTaskFromQueue ()
		{
			foreach (var task in _tasks.GetConsumingEnumerable ())
			{
				TryExecuteTask (task);
			}
		}
	}
}
