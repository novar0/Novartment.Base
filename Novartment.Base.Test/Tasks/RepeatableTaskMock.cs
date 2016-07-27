using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Tasks;

namespace Novartment.Base.Test
{
	internal enum TaskArgument : int
	{
		SampleValue1 = 0,
		SampleValue2 = 1,
		SampleValue3 = 2,
		StartNotConfirmed = 4,
		ProcessingException = 5,
		ProcessingCancel = 6
	}

	internal class RepeatableTaskMock
	{
		private const int MaxCount = 256;
		private readonly TaskCompletionSource<int>[] _taskCreated = new TaskCompletionSource<int>[MaxCount];
		private readonly TaskCompletionSource<int>[] _taskStarting = new TaskCompletionSource<int>[MaxCount];
		private readonly TaskCompletionSource<int>[] _taskStarted = new TaskCompletionSource<int>[MaxCount];
		private readonly TaskCompletionSource<int>[] _taskEnded = new TaskCompletionSource<int>[MaxCount];
		private readonly TaskCompletionSource<int>[] _taskCompletionSource = new TaskCompletionSource<int>[MaxCount];
		private readonly ManualResetEvent[] _endTaskSignaler = new ManualResetEvent[MaxCount];
		private readonly int[] _creationThreadId = new int[MaxCount];
		private readonly int[] _startingThreadId = new int[MaxCount];
		private readonly int[] _startedThreadId = new int[MaxCount];
		private readonly int[] _endedThreadId = new int[MaxCount];
		private readonly CompletedTaskData[] _completedTaskData = new CompletedTaskData[MaxCount];

		internal TaskCompletionSource<int>[] CreatedEvent => _taskCreated;
		internal TaskCompletionSource<int>[] StartingEvent => _taskStarting;
		internal TaskCompletionSource<int>[] StartedEvent => _taskStarted;
		internal TaskCompletionSource<int>[] EndedEvent => _taskEnded;
		internal CompletedTaskData[] CompletedTaskData => _completedTaskData;

		internal int[] CreationThreadId => _creationThreadId;
		internal int[] StartingThreadId => _startingThreadId;
		internal int[] StartedThreadId => _startedThreadId;
		internal int[] EndedThreadId => _endedThreadId;

		internal void Complete (TaskArgument arg)
		{
			int index = (int)arg;
			_endTaskSignaler[index].Set ();
			var tcs = _taskCompletionSource[index];
			if (tcs != null)
			{
				if (arg == TaskArgument.ProcessingException)
				{
					tcs.SetException (new ArithmeticException ("test exception"));
					return;
				}
				var cToken = (CancellationToken)tcs.Task.AsyncState;
				if (cToken.IsCancellationRequested || (arg == TaskArgument.ProcessingCancel))
				{
					tcs.SetCanceled ();
					return;
				}
				tcs.SetResult (0);
			}
		}

		internal Task TaskFactory (object state, CancellationToken cToken)
		{
			var index = (int)state;
			if (_taskCreated[index] == null) throw new InvalidOperationException ($"not started task {index}");
			_creationThreadId[index] = Thread.CurrentThread.ManagedThreadId;
			_taskCreated[index].SetResult (0);
			var tcs = new TaskCompletionSource<int> (cToken);
			_taskCompletionSource[index] = tcs;
			return tcs.Task;
		}
		internal void TaskAction (object state, CancellationToken cToken)
		{
			var index = (int)state;
			if (_taskCreated[index] == null) throw new InvalidOperationException ($"not started task {index}");
			_creationThreadId[index] = Thread.CurrentThread.ManagedThreadId;
			_taskCreated[index].SetResult (0);
			_endTaskSignaler[index].WaitOne ();
			if (((TaskArgument)state) == TaskArgument.ProcessingException) throw new ArithmeticException ("test exception");
			cToken.ThrowIfCancellationRequested ();
		}
		internal void OnStarting (object sender, TaskStartingEventArgs args)
		{
			var index = (int)args.State;
			if (_taskCreated[index] != null) throw new InvalidOperationException ($"already started task {index}");
			_taskCreated[index] = new TaskCompletionSource<int> ();
			_taskStarting[index] = new TaskCompletionSource<int> ();
			_taskStarted[index] = new TaskCompletionSource<int> ();
			_taskEnded[index] = new TaskCompletionSource<int> ();
			_endTaskSignaler[index] = new ManualResetEvent (false);
			_startingThreadId[index] = Thread.CurrentThread.ManagedThreadId;
			_taskStarting[index].SetResult (0);
			if (((TaskArgument)args.State) == TaskArgument.StartNotConfirmed)
			{
				args.Cancel = true;
			}
		}
		internal void OnStarted (object sender, DataEventArgs<object> args)
		{
			var index = (int)args.Value;
			if (_taskCreated[index] == null) throw new InvalidOperationException ($"not started task {index}");
			_startedThreadId[index] = Thread.CurrentThread.ManagedThreadId;
			_taskStarted[index].SetResult (0);
		}
		internal void OnEnded (object sender, DataEventArgs<CompletedTaskData> args)
		{
			var index = (int)args.Value.State;
			_completedTaskData[index] = args.Value;
			if (_taskCreated[index] == null) throw new InvalidOperationException ($"not started task {index}");
			_endedThreadId[index] = Thread.CurrentThread.ManagedThreadId;
			_taskEnded[index].SetResult (0);
		}
	}
}
