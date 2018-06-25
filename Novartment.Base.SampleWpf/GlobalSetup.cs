using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Novartment.Base.IO;
using Novartment.Base.UI.Wpf;

// ВНИМАНИЕ!
// Этот исходник должен лежать в корне проекта
// чтобы корректно работало преобразование путей к исходным файлам при показе трассировки стэка исключения.
namespace Novartment.Base.SampleWpf
{
	public static class GlobalSetup
	{
		// Константы должны быть заполнены уникальными значениями
		// _ClickOnceURL укажите null если технология ClickOnce не используется
		private static readonly string _UniqueApplicationName = "SampleWpf_intance_1262C4C7-D476-4E54-9BE1-5320177AD0E3";
		private static readonly string _ClickOnceURL = @"http://someserver.com/SampleWpf/Novartment.Base.SampleWpf.application#Novartment.Base.SampleWpf.application, Culture=ru, PublicKeyToken=0000000000000000";

		private static readonly int _InstanceSearchTimeout = 1000;

		[STAThread]
		public static int Main ()
		{
			return MainEx ();
		}

		public static int MainEx ([CallerFilePath]string sourceFileName = "")
		{
			foreach (var listener in Debug.Listeners.OfType<DefaultTraceListener> ())
			{
				listener.AssertUiEnabled = true;
			}

			if ((_ClickOnceURL != null) && !Debugger.IsAttached && !ApplicationDeployment.IsNetworkDeployed)
			{// запрещаем запускать EXE напрямую, перенаправляем через ClickOnce-запускатель
				RelaunchThroughClickOnce ();
				return (int)ProcessExitCode.Ok;
			}

			// проверка уже запущеных экземпляров
			using (var locker = new SystemWideSingleton (_UniqueApplicationName))
			{
				var listener = new NamedPipeSignaler (_UniqueApplicationName);
				if (!locker.IsOriginal)
				{
					// посылаем сигнал ранее запущенному экземпляру чтобы он предстал перед пользователем взамен этого
					try
					{
						listener.SendSignalAsync (_InstanceSearchTimeout).Wait ();
					}
					catch
					{
						// исключения тут не важны, всё равно процесс завершается
					}

					return (int)ProcessExitCode.AlreadyRunningInstanceDetected;
				}

				using (var exitProgramCTS = new CancellationTokenSource ())
				{
					int result = (int)ProcessExitCode.Ok;
					var app = CompositionRoot.ComposeApplication ();

					// сохраняем путь к исходникам проекта чтобы исключать его в трассировке стэка
					if (!string.IsNullOrWhiteSpace (sourceFileName))
					{
						app.SourceDirectory = Path.GetDirectoryName (sourceFileName);
					}

					// запуск бесконечного ожидания попыток соединения (команд проявить главное окно)
					ExecuteOnSignal (listener, app.MakeVisibleAndScheduleBringToFront, exitProgramCTS.Token);

					result = (int)app.Run ();
					exitProgramCTS.Cancel ();
					return result;
				}
			}
		}

		private static async void ExecuteOnSignal (NamedPipeSignaler listener, Action action, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					await listener.WaitForSignalAsync (cancellationToken).ConfigureAwait (false);
				}
				catch (OperationCanceledException)
				{
					return;
				}

				var notUsed = ComponentApplication.Current.Dispatcher.InvokeAsync (action);
			}
		}

		private static void RelaunchThroughClickOnce ()
		{
			string newFile = Path.Combine (Path.GetTempPath (), _UniqueApplicationName + ".appref-ms");
			File.WriteAllText (newFile, _ClickOnceURL, Encoding.Unicode);
			Process.Start (newFile);
		}
	}
}
