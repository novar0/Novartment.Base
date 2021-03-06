using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.UnsafeWin32;

namespace Novartment.Base.Tasks
{
	/// <summary>
	/// Планировщик, исполняющий поставленные в очередь задачи
	/// в Single-threaded apartments (STA) потоках исполнения,
	/// инициализированных с помощью OleInitialize ().
	/// </summary>
	/// <remarks>
	/// Такая среда выполнения требуется для всех Win32 Ole*-функций и многих COM-объектах.
	/// </remarks>
	public sealed class OleStaTaskScheduler : TaskScheduler,
		IDisposable
	{
		private readonly Thread[] _threads;
		private BlockingCollection<Task> _tasks;

		/// <summary>
		/// Инициализирует новый экземпляр OleStaTaskScheduler, использующий указанное количество поток исполнения.
		/// </summary>
		/// <param name="numberOfThreads">Количество потоков исполнения, которые будут созданы и использованы.</param>
		public OleStaTaskScheduler (int numberOfThreads)
		{
			if (numberOfThreads < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (numberOfThreads));
			}

			_tasks = new BlockingCollection<Task> ();

			_threads = new Thread[numberOfThreads];
			ThreadStart threadStart = ExecuteTaskFromQueue;
			for (var i = 0; i < numberOfThreads; i++)
			{
				var thread = new Thread (threadStart)
				{
					IsBackground = true,
					Name = GetType ().Name + i,
				};
				thread.SetApartmentState (ApartmentState.STA);
				thread.Start ();
				_threads[i] = thread;
			}
		}

		/// <summary>Указывает максимальный уровень параллелизма, который поддерживается данным планировщиком.</summary>
		public override int MaximumConcurrencyLevel => _threads.Length;

		/// <summary>
		/// Освобождает планировщик и останавливает приём новых задач на выполение.
		/// Метод блокиуется до тех пор, пока все потоки не закончат выполнение.
		/// </summary>
		public void Dispose ()
		{
			var oldValue = Interlocked.Exchange (ref _tasks, null);
			if (oldValue != null)
			{
				oldValue.CompleteAdding ();
				foreach (var thread in _threads)
				{
					thread.Join ();
				}

				oldValue.Dispose ();
			}
		}

		/// <summary>Ставит задачу в очередь планировщика.</summary>
		/// <param name="task">Помещаемая в очередь задача.</param>
		protected override void QueueTask (Task task)
		{
			var tasks = _tasks;
			if (tasks == null)
			{
				throw new ObjectDisposedException (GetType ().FullName);
			}

			tasks.Add (task);
		}

		/// <summary>Создает перечислитель задач, которые в настоящее время находятся в очереди исполнения.</summary>
		/// <returns>Перечисляемый объект, позволяющий отладчику перемещаться по задачам, которые находятся в очереди данного планировщика.</returns>
		protected override IEnumerable<Task> GetScheduledTasks ()
		{
			var tasks = _tasks;
			if (tasks == null)
			{
				throw new ObjectDisposedException (GetType ().FullName);
			}

			return tasks.ToArray ();
		}

		/// <summary>Определяет, можно ли выполнить указанную задачу синхронно, и если возможно, выполняет её.</summary>
		/// <param name="task">Задача, которую требуется выполнить.</param>
		/// <param name="taskWasPreviouslyQueued">Логическое значение, указывающее, была ли задача ранее поставлена в очередь.</param>
		/// <returns>Логическое значение, определяющее, была ли задача выполнена на месте.</returns>
		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			return (Thread.CurrentThread.GetApartmentState () == ApartmentState.STA) && TryExecuteTask (task);
		}

		private void ExecuteTaskFromQueue ()
		{
			var hr = NativeMethods.Ole32.OleInitialize (IntPtr.Zero);
			if ((hr != 0) && (hr != 1))
			{
				// S_OK или S_FALSE
				throw new InvalidOperationException ("OleInitialize() failed.", Marshal.GetExceptionForHR (hr));
			}

			foreach (var task in _tasks.GetConsumingEnumerable ())
			{
				TryExecuteTask (task);
			}

			NativeMethods.Ole32.OleUninitialize ();
		}
	}
}
