using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Novartment.Base.Test
{
	internal class SynchronizationContextMock : SynchronizationContext, IDisposable
	{
		private readonly Thread _thread;
		private readonly BlockingCollection<Tuple<SendOrPostCallback, object>> _tasks = new BlockingCollection<Tuple<SendOrPostCallback, object>> ();

		public SynchronizationContextMock ()
		{
			_thread = new Thread (ExecuteTaskFromQueue);
			_thread.Start ();
		}

		internal int ThreadId => _thread.ManagedThreadId;

		public override void Post (SendOrPostCallback d, object state)
		{
			_tasks.Add (Tuple.Create (d, state));
		}

		public override void Send (SendOrPostCallback d, object state)
		{
			_tasks.Add (Tuple.Create (d, state));
		}

		public void Dispose ()
		{
			_tasks.Dispose ();
		}

		private void ExecuteTaskFromQueue ()
		{
			foreach (var task in _tasks.GetConsumingEnumerable ())
			{
				task.Item1.Invoke (task.Item2);
			}
		}
	}
}
