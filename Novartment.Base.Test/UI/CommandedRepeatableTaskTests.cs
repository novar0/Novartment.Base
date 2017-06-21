using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.UI;
using Xunit;

namespace Novartment.Base.Test
{
	public class CommandedRepeatableTaskTests
	{
		private static ManualResetEvent _taskEndedSignaler;
		private static int _propertyChangedCount1;
		private static int _propertyChangedCount2;
		private static int _propertyChangedCount3;
		private static int _startCommanCanExecuteChanged1;
		private static int _startCommanCanExecuteChanged2;
		private static int _startCommanCanExecuteChanged3;
		private static int _stopCommanCanExecuteChanged1;
		private static int _stopCommanCanExecuteChanged2;
		private static int _stopCommanCanExecuteChanged3;

		[Fact]
		[Trait ("Category", "UI")]
		public void Commands ()
		{
			_propertyChangedCount1 = 0;
			_propertyChangedCount2 = 0;
			_propertyChangedCount3 = 0;
			_startCommanCanExecuteChanged1 = 0;
			_startCommanCanExecuteChanged2 = 0;
			_startCommanCanExecuteChanged3 = 0;
			_stopCommanCanExecuteChanged1 = 0;
			_stopCommanCanExecuteChanged2 = 0;
			_stopCommanCanExecuteChanged3 = 0;
			using (var task1 = new CommandedRepeatableTask (CreateNewTask))
			{
				task1.PropertyChanged += OnPropertyChanged1;
				task1.TaskEnded += (e, args) => _taskEndedSignaler.Set ();
				task1.StartCommand.CanExecuteChanged += OnStartCommandCanExecuteChanged1;
				task1.StopCommand.CanExecuteChanged += OnStopCommandCanExecuteChanged1;
				using (var task2 = task1.CreateLinked (CreateNewTask))
				{
					task2.PropertyChanged += OnPropertyChanged2;
					task2.TaskEnded += (e, args) => _taskEndedSignaler.Set ();
					task2.StartCommand.CanExecuteChanged += OnStartCommandCanExecuteChanged2;
					task2.StopCommand.CanExecuteChanged += OnStopCommandCanExecuteChanged2;
					using (var task3 = task2.CreateLinked (CreateNewTask))
					{
						using (_taskEndedSignaler = new ManualResetEvent (false))
						{
							task3.PropertyChanged += OnPropertyChanged3;
							task3.TaskEnded += (e, args) => _taskEndedSignaler.Set ();
							task3.StartCommand.CanExecuteChanged += OnStartCommandCanExecuteChanged3;
							task3.StopCommand.CanExecuteChanged += OnStopCommandCanExecuteChanged3;

							Assert.False (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.True (task1.StartCommand.CanExecute (null));
							Assert.True (task2.StartCommand.CanExecute (null));
							Assert.True (task3.StartCommand.CanExecute (null));
							Assert.False (task1.StopCommand.CanExecute (null));
							Assert.False (task2.StopCommand.CanExecute (null));
							Assert.False (task3.StopCommand.CanExecute (null));
							Assert.Equal (0, _propertyChangedCount1);
							Assert.Equal (0, _propertyChangedCount2);
							Assert.Equal (0, _propertyChangedCount3);
							Assert.Equal (0, _startCommanCanExecuteChanged1);
							Assert.Equal (0, _startCommanCanExecuteChanged2);
							Assert.Equal (0, _startCommanCanExecuteChanged3);
							Assert.Equal (0, _stopCommanCanExecuteChanged1);
							Assert.Equal (0, _stopCommanCanExecuteChanged2);
							Assert.Equal (0, _stopCommanCanExecuteChanged3);

							var tcs = new TaskCompletionSource<int> (0);
							task1.StartCommand.Execute (tcs);
							Assert.True (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (1, _propertyChangedCount1);
							Assert.Equal (0, _propertyChangedCount2);
							Assert.Equal (0, _propertyChangedCount3);
							Assert.Equal (1, _startCommanCanExecuteChanged1);
							Assert.Equal (1, _startCommanCanExecuteChanged2);
							Assert.Equal (1, _startCommanCanExecuteChanged3);
							Assert.Equal (1, _stopCommanCanExecuteChanged1);
							Assert.Equal (1, _stopCommanCanExecuteChanged2);
							Assert.Equal (1, _stopCommanCanExecuteChanged3);
							Assert.False (task1.StartCommand.CanExecute (null));
							Assert.False (task2.StartCommand.CanExecute (null));
							Assert.False (task3.StartCommand.CanExecute (null));
							Assert.True (task1.StopCommand.CanExecute (null));
							Assert.True (task2.StopCommand.CanExecute (null));
							Assert.True (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.True (_taskEndedSignaler.WaitOne ());
							Assert.False (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (0, _propertyChangedCount2);
							Assert.Equal (0, _propertyChangedCount3);
							Assert.Equal (2, _startCommanCanExecuteChanged1);
							Assert.Equal (2, _startCommanCanExecuteChanged2);
							Assert.Equal (2, _startCommanCanExecuteChanged3);
							Assert.Equal (2, _stopCommanCanExecuteChanged1);
							Assert.Equal (2, _stopCommanCanExecuteChanged2);
							Assert.Equal (2, _stopCommanCanExecuteChanged3);
							Assert.True (task1.StartCommand.CanExecute (null));
							Assert.True (task2.StartCommand.CanExecute (null));
							Assert.True (task3.StartCommand.CanExecute (null));
							Assert.False (task1.StopCommand.CanExecute (null));
							Assert.False (task2.StopCommand.CanExecute (null));
							Assert.False (task3.StopCommand.CanExecute (null));

							tcs = new TaskCompletionSource<int> (0);
							task2.StartCommand.Execute (tcs);
							Assert.False (task1.IsRunning);
							Assert.True (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (1, _propertyChangedCount2);
							Assert.Equal (0, _propertyChangedCount3);
							Assert.Equal (3, _startCommanCanExecuteChanged1);
							Assert.Equal (3, _startCommanCanExecuteChanged2);
							Assert.Equal (3, _startCommanCanExecuteChanged3);
							Assert.Equal (3, _stopCommanCanExecuteChanged1);
							Assert.Equal (3, _stopCommanCanExecuteChanged2);
							Assert.Equal (3, _stopCommanCanExecuteChanged3);
							Assert.False (task1.StartCommand.CanExecute (null));
							Assert.False (task2.StartCommand.CanExecute (null));
							Assert.False (task3.StartCommand.CanExecute (null));
							Assert.True (task1.StopCommand.CanExecute (null));
							Assert.True (task2.StopCommand.CanExecute (null));
							Assert.True (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.True (_taskEndedSignaler.WaitOne ());
							Assert.False (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (2, _propertyChangedCount2);
							Assert.Equal (0, _propertyChangedCount3);
							Assert.Equal (4, _startCommanCanExecuteChanged1);
							Assert.Equal (4, _startCommanCanExecuteChanged2);
							Assert.Equal (4, _startCommanCanExecuteChanged3);
							Assert.Equal (4, _stopCommanCanExecuteChanged1);
							Assert.Equal (4, _stopCommanCanExecuteChanged2);
							Assert.Equal (4, _stopCommanCanExecuteChanged3);
							Assert.True (task1.StartCommand.CanExecute (null));
							Assert.True (task2.StartCommand.CanExecute (null));
							Assert.True (task3.StartCommand.CanExecute (null));
							Assert.False (task1.StopCommand.CanExecute (null));
							Assert.False (task2.StopCommand.CanExecute (null));
							Assert.False (task3.StopCommand.CanExecute (null));

							tcs = new TaskCompletionSource<int> (0);
							task3.StartCommand.Execute (tcs);
							Assert.False (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.True (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (2, _propertyChangedCount2);
							Assert.Equal (1, _propertyChangedCount3);
							Assert.Equal (5, _startCommanCanExecuteChanged1);
							Assert.Equal (5, _startCommanCanExecuteChanged2);
							Assert.Equal (5, _startCommanCanExecuteChanged3);
							Assert.Equal (5, _stopCommanCanExecuteChanged1);
							Assert.Equal (5, _stopCommanCanExecuteChanged2);
							Assert.Equal (5, _stopCommanCanExecuteChanged3);
							Assert.False (task1.StartCommand.CanExecute (null));
							Assert.False (task2.StartCommand.CanExecute (null));
							Assert.False (task3.StartCommand.CanExecute (null));
							Assert.True (task1.StopCommand.CanExecute (null));
							Assert.True (task2.StopCommand.CanExecute (null));
							Assert.True (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.True (_taskEndedSignaler.WaitOne ());
							Assert.False (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (2, _propertyChangedCount2);
							Assert.Equal (2, _propertyChangedCount3);
							Assert.Equal (6, _startCommanCanExecuteChanged1);
							Assert.Equal (6, _startCommanCanExecuteChanged2);
							Assert.Equal (6, _startCommanCanExecuteChanged3);
							Assert.Equal (6, _stopCommanCanExecuteChanged1);
							Assert.Equal (6, _stopCommanCanExecuteChanged2);
							Assert.Equal (6, _stopCommanCanExecuteChanged3);
							Assert.True (task1.StartCommand.CanExecute (null));
							Assert.True (task2.StartCommand.CanExecute (null));
							Assert.True (task3.StartCommand.CanExecute (null));
							Assert.False (task1.StopCommand.CanExecute (null));
							Assert.False (task2.StopCommand.CanExecute (null));
							Assert.False (task3.StopCommand.CanExecute (null));

							tcs = new TaskCompletionSource<int> (0);
							task2.StartCommand.Execute (tcs);
							Assert.False (task1.IsRunning);
							Assert.True (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (3, _propertyChangedCount2);
							Assert.Equal (2, _propertyChangedCount3);
							Assert.Equal (7, _startCommanCanExecuteChanged1);
							Assert.Equal (7, _startCommanCanExecuteChanged2);
							Assert.Equal (7, _startCommanCanExecuteChanged3);
							Assert.Equal (7, _stopCommanCanExecuteChanged1);
							Assert.Equal (7, _stopCommanCanExecuteChanged2);
							Assert.Equal (7, _stopCommanCanExecuteChanged3);
							Assert.False (task1.StartCommand.CanExecute (null));
							Assert.False (task2.StartCommand.CanExecute (null));
							Assert.False (task3.StartCommand.CanExecute (null));
							Assert.True (task1.StopCommand.CanExecute (null));
							Assert.True (task2.StopCommand.CanExecute (null));
							Assert.True (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							task2.StopCommand.Execute (null);
							Assert.True (_taskEndedSignaler.WaitOne ());
							Assert.False (task1.IsRunning);
							Assert.False (task2.IsRunning);
							Assert.False (task3.IsRunning);
							Assert.Equal (2, _propertyChangedCount1);
							Assert.Equal (4, _propertyChangedCount2);
							Assert.Equal (2, _propertyChangedCount3);
							Assert.Equal (8, _startCommanCanExecuteChanged1);
							Assert.Equal (8, _startCommanCanExecuteChanged2);
							Assert.Equal (8, _startCommanCanExecuteChanged3);
							Assert.Equal (8, _stopCommanCanExecuteChanged1);
							Assert.Equal (8, _stopCommanCanExecuteChanged2);
							Assert.Equal (8, _stopCommanCanExecuteChanged3);
							Assert.True (task1.StartCommand.CanExecute (null));
							Assert.True (task2.StartCommand.CanExecute (null));
							Assert.True (task3.StartCommand.CanExecute (null));
							Assert.False (task1.StopCommand.CanExecute (null));
							Assert.False (task2.StopCommand.CanExecute (null));
							Assert.False (task3.StopCommand.CanExecute (null));
						}
					}
				}
			}
		}

		private static Task CreateNewTask (object state, CancellationToken cToken)
		{
			var tcs = (TaskCompletionSource<int>)state;
			cToken.Register (() => tcs.TrySetCanceled ());
			return tcs.Task;
		}

		private void OnStartCommandCanExecuteChanged1 (object sender, EventArgs e)
		{
			_startCommanCanExecuteChanged1++;
		}

		private void OnStartCommandCanExecuteChanged2 (object sender, EventArgs e)
		{
			_startCommanCanExecuteChanged2++;
		}

		private void OnStartCommandCanExecuteChanged3 (object sender, EventArgs e)
		{
			_startCommanCanExecuteChanged3++;
		}

		private void OnStopCommandCanExecuteChanged1 (object sender, EventArgs e)
		{
			_stopCommanCanExecuteChanged1++;
		}

		private void OnStopCommandCanExecuteChanged2 (object sender, EventArgs e)
		{
			_stopCommanCanExecuteChanged2++;
		}

		private void OnStopCommandCanExecuteChanged3 (object sender, EventArgs e)
		{
			_stopCommanCanExecuteChanged3++;
		}

		private void OnPropertyChanged1 (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsRunning")
			{
				_propertyChangedCount1++;
			}
		}

		private void OnPropertyChanged2 (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsRunning")
			{
				_propertyChangedCount2++;
			}
		}

		private void OnPropertyChanged3 (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsRunning")
			{
				_propertyChangedCount3++;
			}
		}
	}
}
