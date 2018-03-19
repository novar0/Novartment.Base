using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base
{
	/// <summary>
	/// Методы расширения для System.Diagnostics.Process.
	/// </summary>
	public static class ProcessExtensions
	{
		/// <summary>
		/// По возможности корректно завершает указанный процесс.
		/// В случае провала завершает его принудительно по истечении указанного промежутка времени.
		/// </summary>
		/// <param name="process">Процесс, который необходимо завершить.</param>
		/// <param name="timeoutMilliseconds">
		/// Максимальное время ожидания закрытия процесса в ответ на сигнал,
		/// после чего процесс будет завершён принудительно.
		/// </param>
		public static void Terminate (this Process process, int timeoutMilliseconds)
		{
			if (process == null)
			{
				throw new ArgumentNullException (nameof (process));
			}

			if (timeoutMilliseconds < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (timeoutMilliseconds));
			}

			Contract.EndContractBlock ();

			if (process.HasExited)
			{
				return;
			}

			try
			{
				var isClosed = process.CloseMainWindow ();
				if (isClosed)
				{
					process.WaitForExit (timeoutMilliseconds);
				}

				if (!process.HasExited)
				{
					process.Kill ();
				}
			}
			catch (InvalidOperationException)
			{
			}
		}

		/// <summary>
		/// Проверяет, выполняется ли указанный процесс.
		/// </summary>
		/// <param name="process">Процесс, состояние которого необходимо выяснить.</param>
		/// <returns>True если указанный процесс в настоящий момент выполняется.</returns>
		public static bool IsRunning (this Process process)
		{
			if (process == null)
			{
				throw new ArgumentNullException (nameof (process));
			}

			Contract.EndContractBlock ();

			try
			{
				return !process.HasExited && (process.Id != 0);
			}
			catch (InvalidOperationException)
			{
				return false;
			}
			catch (Win32Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Создаёт задачу, представляющую собой процесс, запущенный с указанными параметрами.
		/// </summary>
		/// <param name="process">Созданный, но не запущенный процесс.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, состояние которой отражает состояние запущенного процесса.</returns>
		public static Task StartAsync (this Process process, CancellationToken cancellationToken)
		{
			if (process == null)
			{
				throw new ArgumentNullException (nameof (process));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			var procData = new ProcessTaskCompletionSourceWrapper (process, cancellationToken);
			procData.Start ();
			return procData.Task;
		}

		/// <summary>
		/// Создаёт задачу, представляющую собой процесс, запущенный с указанными параметрами.
		/// Корректное завершение задачи произойдёт при создании запущенным процессом мьютекса с указанным именем.
		/// </summary>
		/// <param name="process">Созданный, но не запущенный процесс.</param>
		/// <param name="completionMutexName">Имя мьютекса, создание которого запущенным процессом будет означать успешное завершение задачи.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, состояние которой отражает состояние запущенного процесса.</returns>
		public static Task StartAsync (this Process process, string completionMutexName, CancellationToken cancellationToken)
		{
			if (process == null)
			{
				throw new ArgumentNullException (nameof (process));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			var procData = new ProcessTaskCompletionSourceWrapper (process, completionMutexName, cancellationToken);
			procData.Start ();
			return procData.Task;
		}

		internal class ProcessTaskCompletionSourceWrapper : TaskCompletionSource<object>,
			IDisposable
		{
			private static readonly TimeSpan _MutexCheckPeriod = new TimeSpan (0, 0, 0, 0, 100);
			private readonly CancellationToken _cancellationToken;
			private readonly ReusableDisposable _cTokenReg = new ReusableDisposable ();
			private readonly ReusableDisposable<ITimer> _mutexQueryTimer = new ReusableDisposable<ITimer> ();
			private readonly string _mutexName;
			private readonly Process _process;

			internal ProcessTaskCompletionSourceWrapper (Process process, CancellationToken cancellationToken)
				: this (process, null, cancellationToken)
			{
			}

			internal ProcessTaskCompletionSourceWrapper (Process process, string completionMutexName, CancellationToken cancellationToken)
				: base (process)
			{
				_process = process;
				_mutexName = completionMutexName;
				_cancellationToken = cancellationToken;
				if (cancellationToken.CanBeCanceled)
				{
					_cTokenReg.Value = cancellationToken.Register (Terminate);
				}

				process.EnableRaisingEvents = true;
				process.Exited += ProcessExited;
			}

#pragma warning disable CA1063 // Implement IDisposable Correctly
			/// <summary>
			/// Освобождает занятые объектом ресурсы.
			/// </summary>
			public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
			{
				_mutexQueryTimer.Dispose ();
				_cTokenReg.Dispose ();
				_process.Exited -= ProcessExited;
			}

			internal void Start ()
			{
				if (_cancellationToken.IsCancellationRequested)
				{
					Dispose ();
					TrySetCanceled ();
					return;
				}

				try
				{
					_process.Start ();
					if (_mutexName != null)
					{
						var timer = new AppDomainQueueTimer<bool> (TimerTick, false)
						{
							Interval = _MutexCheckPeriod,
						};
						_mutexQueryTimer.Value = timer;
						_mutexQueryTimer.Value.Start ();
					}
				}
				catch (Win32Exception excpt)
				{
					switch (excpt.NativeErrorCode)
					{
						case 740: // ERROR_ELEVATION_REQUIRED The requested operation requires elevation.
							TrySetException (new ElevationRequiredException ());
							break;
						case 1223: // ERROR_CANCELLED The operation was canceled by the user.
							TrySetCanceled ();
							break;
						default:
							throw;
					}
				}
				finally
				{
					Dispose ();
				}
			}

#pragma warning disable CA1801 // Review unused parameters
			private void TimerTick (bool notUsed)
#pragma warning restore CA1801 // Review unused parameters
			{
				if (this.Task.IsCompleted)
				{
					return;
				}

				bool notExists;
				try
				{
					using (new Mutex (false, _mutexName, out notExists))
					{
					}
				}
				catch (UnauthorizedAccessException)
				{
					notExists = false;
				}

				if (!notExists)
				{
					_mutexQueryTimer.Value.Stop ();
					Dispose ();
					TrySetResult (null);
				}
			}

			private void Terminate ()
			{
				_process.Terminate (5000);
			}

			private void ProcessExited (object sender, EventArgs e)
			{
				Dispose ();

				if (this.Task.IsCompleted)
				{
					return;
				}

				if (_cancellationToken.IsCancellationRequested)
				{
					TrySetCanceled ();
					return;
				}

				if (_process.ExitCode != 0)
				{
					TrySetException (new ProcessFailedException (_process.ExitCode));
					return;
				}

				TrySetResult (null);
			}
		}
	}
}
