﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Novartment.Base;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;
using Novartment.Base.Shell;
using Novartment.Base.Tasks;

namespace Novartment.Base.SampleWpf
{
	public enum CheckSeverity : int
	{
		[System.ComponentModel.Description ("ОК")]
		OK = 0,
		[System.ComponentModel.Description ("Не проверено")]
		NotChecked = 1,
		[System.ComponentModel.Description ("Предупреждение")]
		Warning = 2,
		[System.ComponentModel.Description ("Ошибка")]
		Error = 3
	}

	public class MainViewModel : BaseViewModel,
		IDragDropSource,
		IDragDropTarget
	{
		private readonly SimpleEventLog _eventLog;
		private readonly ICollectionView _eventsListView;
		private readonly IClipboard _clipBoardService;
		private readonly Func<IDataContainer> _clipboardObjectFactory;
		private readonly ComponentApplication _application;
		private readonly AppSettings _appSettings;
		private readonly System.ServiceModel.Channels.IOutputChannel _dataService;
		private readonly Func<MessageBoxFormData, Autofac.Features.OwnedInstances.Owned<IDialogView<System.Windows.MessageBoxResult>>> _messageBoxFactory;
		private readonly TaskScheduler _taskSchedulerShell = new OleStaTaskScheduler (1);
		private string _dataFormats = "drop here";

		public ICollectionView EventsList { get { return _eventsListView; } }
		public string Version { get { return ComponentApplication.Current.Version; } }
		public string DataFormats { get { return _dataFormats; } }

		#region tasks

		private readonly CommandedRepeatableTask _refreshDataTask;
		private readonly CommandedRepeatableTask _copyItemTask;
		private readonly CommandedRepeatableTask _clearItemsTask;

		public CommandedRepeatableTask RrefreshDataTask { get { return _refreshDataTask; } }
		public CommandedRepeatableTask CopyItemTask { get { return _copyItemTask; } }
		public CommandedRepeatableTask ClearItemsTask { get { return _clearItemsTask; } }

		#endregion

		public MainViewModel (
			ComponentApplication application,
			AppSettings appSettings,
			SimpleEventLog eventLog,
			IClipboard clipBoardService,
			Func<IDataContainer> clipboardObjectFactory,
			System.ServiceModel.Channels.IOutputChannel dataService,
			Func<MessageBoxFormData, Autofac.Features.OwnedInstances.Owned<IDialogView<System.Windows.MessageBoxResult>>> messageBoxFactory)
		{
			_application = application;
			_appSettings = appSettings;
			_eventLog = eventLog;
			_clipBoardService = clipBoardService;
			_clipboardObjectFactory = clipboardObjectFactory;
			_dataService = dataService;
			_messageBoxFactory = messageBoxFactory;

			ComponentApplication.EnableBindingOperationsWithoutSynchronization (_eventLog);
			_eventsListView = new LiteListCollectionView<SimpleEventRecord> ((IReadOnlyList<SimpleEventRecord>)_eventLog);

			_refreshDataTask = new CommandedRepeatableTask (ProcessData, TaskScheduler.Default);
			_refreshDataTask.TaskStarting += (sender, e) =>
			{
				using (var dialog = _messageBoxFactory.Invoke (new MessageBoxFormData ()
				{
					Title = Properties.Resources.TaskStartingPromptTitle,
					Message = Properties.Resources.TaskStartingPromptText
				}))
				{
					if (dialog.Value.ShowDialog () != true) e.Cancel = true;
				}
				Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo ("en-US");
				Novartment.Base.UI.Wpf.ResxExtension.UpdateAllTargets ();
			};
			_refreshDataTask.TaskEnded += (e, args) => LogTaskEnded (args, Properties.Resources.TaskProcessingTitle);

			_copyItemTask = new CollectionContextCommandedRepeatableTask<SimpleEventRecord> (CopyItem, _taskSchedulerShell);//, Properties.Resources.TaskCopyEventLogTitle);
			_clearItemsTask = new CollectionContextCommandedRepeatableTask<SimpleEventRecord> (ClearItems, TaskScheduler.Default);//, Properties.Resources.TaskClearEventLogTitle);

			_eventLog.Info (string.Format ("Запущена программа версии {0}", Version));
		}

		#region IDragSource

		DragStartData IDragDropSource.DragStart (double x, double y, DragControl mouseButton)
		{
			System.Diagnostics.Trace.WriteLine (string.Format ("Источник: подцеплено в {0}/{1} кнопкой {2}", x, y, mouseButton));
			return new DragStartData ()
				{
					Object = new WpfDataContainer (DataContainerFormats.Text, "hello from another app!"),
					AllowedEffects = DragDropEffects.All
				};
		}

		bool IDragDropSource.GiveFeedback (DragDropEffects effects)
		{
			return true;
		}

		DragDropAction IDragDropSource.QueryContinueDrag (bool escapePressed, DragDropKeyStates keyStates)
		{
			DragDropAction result = DragDropAction.Continue;
			if (escapePressed)
			{
				result = DragDropAction.Cancel;
			}
			else
			{
				if ((keyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.LeftMouseButton)
				{
					result = DragDropAction.Drop;
				}
			}
			return result;
		}

		void IDragDropSource.DragEnd (DragDropEffects effects)
		{
			System.Diagnostics.Trace.WriteLine (string.Format ("Источник: отцеплено с эффектом {0}", effects));
			if ((effects & DragDropEffects.Move) == DragDropEffects.Move)
			{
				// удалить перемещенный объект
			}
		}

		#endregion

		#region IDragDropTarget

		void IDragDropTarget.DragLeave ()
		{
		}

		DragDropEffects IDragDropTarget.DragEnter (IDataContainer data, DragDropKeyStates keyStates, double x, double y, DragDropEffects allowedEffects)
		{
			System.Diagnostics.Trace.WriteLine (string.Format ("Приемник: заехало в {0}/{1} с модификаторами {2}", x, y, keyStates));
			if (((keyStates & DragDropKeyStates.AltKey) == DragDropKeyStates.AltKey) && ((allowedEffects & DragDropEffects.Link) == DragDropEffects.Link))
			{
				return DragDropEffects.Link;
			}
			if (((keyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey) && ((allowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy))
			{
				return DragDropEffects.Copy;
			}
			if ((allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
			{
				return DragDropEffects.Move;
			}
			return DragDropEffects.None;
		}

		DragDropEffects IDragDropTarget.DragOver (DragDropKeyStates keyStates, double x, double y, DragDropEffects allowedEffects)
		{
			if (((keyStates & DragDropKeyStates.AltKey) == DragDropKeyStates.AltKey) && ((allowedEffects & DragDropEffects.Link) == DragDropEffects.Link))
			{
				return DragDropEffects.Link;
			}
			if (((keyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey) && ((allowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy))
			{
				return DragDropEffects.Copy;
			}
			if ((allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
			{
				return DragDropEffects.Move;
			}
			return DragDropEffects.None;
		}

		DragDropEffects IDragDropTarget.Drop (IDataContainer data, DragDropKeyStates keyStates, double x, double y, DragDropEffects allowedEffects)
		{
			System.Diagnostics.Trace.WriteLine (string.Format ("Приемник: брошено в {0}/{1} с модификаторами {2}", x, y, keyStates));
			// получаем список объектов оболочки. если их нет, то берем просто текст
			ShellItem[] shellItems;
			if ((shellItems = ShellItem.FromDataContainer (data)) != null)
			{
				_dataFormats = string.Join (",\r\n", shellItems.Select (item => item.DisplayNameRelative));
			}
			else
			{
				_dataFormats = (string)data.GetData (DataContainerFormats.UnicodeText, true);
			}
			RaisePropertyChanged ("DataFormats");
			if (((keyStates & DragDropKeyStates.AltKey) == DragDropKeyStates.AltKey) && ((allowedEffects & DragDropEffects.Link) == DragDropEffects.Link))
			{
				return DragDropEffects.Link;
			}
			if (((keyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey) && ((allowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy))
			{
				return DragDropEffects.Copy;
			}
			if ((allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
			{
				return DragDropEffects.Move;
			}
			return DragDropEffects.None;
		}

		#endregion

		private void CopyItem (ContextCollectionData<SimpleEventRecord> data, CancellationToken cancellationToken)
		{
			var items = data.ContextCollectionSelectedItems.Cast<SimpleEventRecord> ().OrderBy (item => item.Time);
			string report = string.Join (Environment.NewLine, items);
			var clipboardObject = _clipboardObjectFactory.Invoke ();
			clipboardObject.SetData (DataContainerFormats.Text, report, true);
			_clipBoardService.SetData (clipboardObject);
		}

		private void ClearItems (ContextCollectionData<SimpleEventRecord> data, CancellationToken cancellationToken)
		{
			_eventLog.Clear ();
		}

		private void ProcessData (object state, CancellationToken cancellationToken)
		{
			// выполнение фоновых работ, в том числе вызов сервисов типа _dataSerivce.DoSomeWork ();
			_eventLog.Info ("Запуск задачи");
			Thread.Sleep (1000);
			throw new ArgumentException ("Имитация исключительной ситуации", "state");
		}

		private void LogTaskEnded (DataEventArgs<CompletedTaskData> args, string title)
		{
			switch (args.Value.Status)
			{
				case TaskStatus.Faulted:
					_eventLog.Error (
						string.Format (Properties.Resources.TaskCompletedException, title));
					_application.ReportException (args.Value.Exception, null, title);
					break;
				case TaskStatus.Canceled:
					_eventLog.Warn (string.Format (Properties.Resources.TaskCompletedCanceled, title));
					break;
				case TaskStatus.RanToCompletion:
					_eventLog.Info (string.Format (Properties.Resources.TaskCompleted, title));
					break;
			}
		}

		public override void Dispose ()
		{
			_clearItemsTask.Dispose ();
			_copyItemTask.Dispose ();
			_refreshDataTask.Dispose ();
		}
	}
}