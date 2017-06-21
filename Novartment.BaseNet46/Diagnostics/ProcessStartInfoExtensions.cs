using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base
{
	/// <summary>
	/// Методы расширения для System.Diagnostics.ProcessStartInfo.
	/// </summary>
	public static class ProcessStartInfoExtensions
	{
		/// <summary>
		/// Создаёт задачу, представляющую собой процесс, запущенный с указанными параметрами.
		/// </summary>
		/// <param name="startInfo">Параметры запуска процесса.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, состояние которой отражает состояние запущенного процесса.</returns>
		[SuppressMessage (
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "'proc' can not be disposed here, it represents running process.")]
		public static Task StartProcessAsync (this ProcessStartInfo startInfo, CancellationToken cancellationToken)
		{
			if (startInfo == null)
			{
				throw new ArgumentNullException (nameof (startInfo));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			var proc = new Process ()
			{
				StartInfo = startInfo,
			};
			return proc.StartAsync (cancellationToken);
		}

		/// <summary>
		/// Создаёт задачу, представляющую собой процесс, запущенный с указанными параметрами.
		/// Корректное завершение задачи произойдёт при создании запущенным процессом мьютекса с указанным именем.
		/// </summary>
		/// <param name="startInfo">Параметры запуска процесса.</param>
		/// <param name="completionMutexName">Имя мьютекса, создание которого запущенным процессом будет означать успешное завершение задачи.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющаю собой запущенный процесс.</returns>
		[SuppressMessage (
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "'proc' can not be disposed here, it represents running process.")]
		public static Task StartProcessAsync (this ProcessStartInfo startInfo, string completionMutexName, CancellationToken cancellationToken)
		{
			if (startInfo == null)
			{
				throw new ArgumentNullException (nameof (startInfo));
			}

			if (completionMutexName == null)
			{
				throw new ArgumentNullException (nameof (completionMutexName));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			var proc = new Process ()
			{
				StartInfo = startInfo,
			};
			return proc.StartAsync (completionMutexName, cancellationToken);
		}
	}
}
