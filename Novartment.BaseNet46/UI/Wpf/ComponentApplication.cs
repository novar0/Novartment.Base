﻿using System;
using System.IO;
using System.Windows;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Security;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Windows.Markup;
using System.Windows.Data;
using Novartment.Base.UnsafeWin32;
using Novartment.Base.Reflection;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Инкапсулирует Windows Presentation Foundation (WPF) приложение.
	/// </summary>
	/// <remarks>
	/// Дополнительно к библиотечному Application содержит:
	/// метод создания таймеров основанных на диспетчере,
	/// метод отображения информации об исключении,
	/// метод проявляния на переднем плане окна приложения,
	/// событие об обновлении содержимого буфера обмена.
	/// </remarks>
	public class ComponentApplication : Application
	{
		private readonly Func<Window> _mainWindowFactory;
		private readonly Func<UserLevelExceptionData, IDialogView<bool?>> _exceptionDialogFactory;
		private readonly string _version;
		private string _sourceDirectory;
		private int _messageHookSet = 0;
		private EventHandler _onClipboardUpdated;

		/// <summary>
		/// Получает строковое представление версии приложения.
		/// </summary>
		public string Version => _version;

		/// <summary>
		/// Получает текущее приложение.
		/// </summary>
		public static new ComponentApplication Current => Application.Current as ComponentApplication;

		/// <summary>
		/// Получает путь к директории, в которой расположены исходные файлы приложения.
		/// </summary>
		public string SourceDirectory { get { return _sourceDirectory; } set { _sourceDirectory = value; } }

		/// <summary>
		/// Инициализирует новый экземпляр класса ComponentApplication использующий указанные параметры.
		/// </summary>
		/// <param name="mainWindowFactory">Фабрика, которая производит главное окно приложения.</param>
		/// <param name="exceptionDialogFactory">Фабрика, которая производит окно диалога об исключительной ситуации.</param>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller doesn't have to cope with nested generics, he is just passing a lambda expression.")]
		public ComponentApplication (Func<Window> mainWindowFactory, Func<UserLevelExceptionData, IDialogView<bool?>> exceptionDialogFactory)
			: base ()
		{
			if (mainWindowFactory == null)
			{
				throw new ArgumentNullException (nameof (mainWindowFactory));
			}
			if (exceptionDialogFactory == null)
			{
				throw new ArgumentNullException (nameof (exceptionDialogFactory));
			}
			Contract.EndContractBlock ();

			_mainWindowFactory = mainWindowFactory;
			_exceptionDialogFactory = exceptionDialogFactory;
			_version = ReflectionService.GetAssemblyVersion (Assembly.GetEntryAssembly () ?? Assembly.GetCallingAssembly ());
		}

		/// <summary>
		/// Создаёт таймер, вызывающий при срабатывании указанный делегат с указанным параметром.
		/// Опционально можно указать приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.
		/// </summary>
		/// <param name="callback">Делегат, вызываемый при срабатывании таймера.</param>
		/// <param name="state">Объект, передавамый в делегат при срабатывании таймера.</param>
		/// <param name="priority">Приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.
		/// По-умолчанию используется фоновый приоритет.</param>
		/// <returns>Создайнный таймер.</returns>
		public WindowsDispatcherTimer<TState> CreateTimer<TState> (
			Action<TState> callback,
			TState state,
			DispatcherPriority priority)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}
			Contract.EndContractBlock ();

			return new WindowsDispatcherTimer<TState> (callback, state, this.Dispatcher, priority);
		}

		/// <summary>
		///Создаёт таймер, вызывающий при срабатывании указанный делегат.
		/// Опционально можно указать приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.
		/// </summary>
		/// <param name="callback">Делегат, вызываемый при срабатывании таймера.</param>
		/// <param name="priority">Приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.
		/// По-умолчанию используется фоновый приоритет.</param>
		/// <returns>Создайнный таймер.</returns>
		public WindowsDispatcherTimer CreateTimer (Action callback, DispatcherPriority priority)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}
			Contract.EndContractBlock ();

			return new WindowsDispatcherTimer (callback, this.Dispatcher, priority);
		}

		/// <summary>
		/// Проявляет на переднем плане главное окно приложения.
		/// </summary>
		public void MakeVisibleAndScheduleBringToFront ()
		{
			this.MainWindow?.MakeVisibleAndScheduleBringToFront ();
		}

		/// <summary>
		/// Запускает приложение: создаёт и показывает пользователю окно, созданное фабрикой окон приложения,
		/// затем обрабатывает очередь сообщений этого окна до тех пор пока не будет получена команда его закрытия.
		/// </summary>
		/// <returns>Код завершения приложения.</returns>
		/// <remarks>
		/// Дополнительно к библиотечному Application:
		/// устанавливается значение по умолчанию для FrameworkElement.Language,
		/// устанавливается глобальный обработчик неперехваченных исключений,
		/// устанавливается режим приёма сообщений об обновлении буфера обмена, транслируемый в события ClipboardUpdated.
		/// </remarks>
		[SuppressMessage ("Microsoft.Design",
			"CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "The exception is top-level and will be presented to user.")]
		public new ProcessExitCode Run ()
		{
			// ставим язык по-умолчанию для XAML-объектов
			var lang = XmlLanguage.GetLanguage (CultureInfo.CurrentCulture.Name);
			FrameworkElement.LanguageProperty.OverrideMetadata (typeof (FrameworkElement), new FrameworkPropertyMetadata (lang));

			// установка глобального обработчика неперехваченных исключений
			AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

			// установка обработчика неперехваченных исключений для WPF-диспетчера
			DispatcherUnhandledException += OnDispatcherUnhandledException;

			Window mainWindow;
			try
			{
				mainWindow = _mainWindowFactory.Invoke ();
				mainWindow.Loaded += AddClipboardFormatListener;
				mainWindow.Closed += RemoveClipboardFormatListener;
			}
			catch (ApplicationRestartRequiredException)
			{
				return ProcessExitCode.NeedRestart;
			}
			catch (Exception excpt)
			{
				ReportException (
					excpt,
					_mainWindowFactory.GetType ().Assembly,
					Wpf.Resources.CreateMainWindowFailedAction,
					Wpf.Resources.CreateMainWindowFailedRecommendations);
				return ProcessExitCode.UnhandledException;
			}

			Run (mainWindow);

			return ProcessExitCode.Ok;
		}

		/// <summary>
		/// Показывает пользователю диалог с информацией о произошедшем в приложении исключении.
		/// </summary>
		/// <param name="exception">Произошедшее исключение.</param>
		/// <param name="failedAssembly">Сборка, в которой произошло исключение.</param>
		/// <param name="failedAction">Выполняемое программой действие, при выполнении которого произошло исключение.</param>
		/// <param name="recommendedSolution">Рекомендуемое пользователю действие.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameters have clear right 'default' values and there is no plausible reason why the default might need to change.")]
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public void ReportException (
			Exception exception,
			Assembly failedAssembly = null,
			string failedAction = null,
			string recommendedSolution = null)
		{
			if (exception == null)
			{
				throw new ArgumentNullException (nameof (exception));
			}
			Contract.EndContractBlock ();

			var assembly = failedAssembly ?? Assembly.GetEntryAssembly () ?? Assembly.GetCallingAssembly ();
			ReportException (new UserLevelExceptionData
			(
				ExceptionDescriptionProvider.CreateDescription (exception),
				WindowsEnvironmentDescription.CurrentUser,
				WindowsEnvironmentDescription.Framework,
				Path.GetFileName (assembly.Location),
				ReflectionService.GetAssemblyVersion (assembly),
				failedAction ?? Wpf.Resources.NotSpecifiedAction,
				recommendedSolution ?? Wpf.Resources.NotSpecifiedRecommendations
			));
		}

		/// <summary>
		/// Показывает пользователю диалог с информацией о произошедшем в приложении исключении.
		/// </summary>
		/// <param name="dataToShow">Набор данных об исключении.</param>
		public void ReportException (UserLevelExceptionData dataToShow)
		{
			Dispatcher.Invoke (new Action<UserLevelExceptionData> (ShowExceptionDialog), dataToShow);
		}

		/// <summary>
		/// Разрешает для операций биндинга прямое конкурентное обращение к коллекции.
		/// Подразумевается что синхронизация конкурентного доступа встроена в коллекцию.
		/// </summary>
		/// <param name="collection">Коллекция, для которой требуется разрешить прямое конкурентное обращение.</param>
		public static void EnableBindingOperationsWithoutSynchronization (IEnumerable collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}
			Contract.EndContractBlock ();

			BindingOperations.EnableCollectionSynchronization (collection, null, (nu1, nu2, action, nu3) => action.Invoke ());
		}

		/// <summary>Происходит когда меняется содержимое буфера обмена.</summary>
		public event EventHandler ClipboardUpdated
		{
			add
			{
				EventHandler oldHandler;
				EventHandler newHandler;
				var prevHandler = _onClipboardUpdated;
				do
				{
					oldHandler = prevHandler;
					newHandler = (EventHandler)Delegate.Combine (oldHandler, value);
					prevHandler = Interlocked.CompareExchange (ref _onClipboardUpdated, newHandler, oldHandler);
				} while (prevHandler != oldHandler);

				if (newHandler != null)
				{
					var oldValue = Interlocked.CompareExchange (ref _messageHookSet, 1, 0);
					if (oldValue == 0)
					{
						ComponentDispatcher.ThreadPreprocessMessage += PreprocessMessage;
					}
				}
			}
			remove
			{
				EventHandler oldHandler;
				var prevHandler = _onClipboardUpdated;
				do
				{
					oldHandler = prevHandler;
					var newHandler = (EventHandler)Delegate.Remove (oldHandler, value);
					prevHandler = Interlocked.CompareExchange (ref _onClipboardUpdated, newHandler, oldHandler);
				}
				while (prevHandler != oldHandler);
			}
		}

		/// <summary>
		/// Вызывает событие ClipboardUpdated.
		/// </summary>
		protected virtual void OnClipboardUpdated ()
		{
			_onClipboardUpdated?.Invoke (this, EventArgs.Empty);
		}

		#region private methods

		[SecurityCritical]
		private static void AddClipboardFormatListener (object sender, RoutedEventArgs e)
		{
			NativeMethods.User32.AddClipboardFormatListener (new WindowInteropHelper ((Window)sender).EnsureHandle ());
		}

		[SecurityCritical]
		private static void RemoveClipboardFormatListener (object sender, EventArgs e)
		{
			NativeMethods.User32.RemoveClipboardFormatListener (new WindowInteropHelper ((Window)sender).Handle);
		}

		private void PreprocessMessage (ref MSG msg, ref bool handled)
		{
			// конвертируем сообщение WM_CLIPBOARDUPDATE в событие ClipboardUpdated
			if (!handled && (msg.message == 0x031D)) // WM_CLIPBOARDUPDATE
			{
				this.Dispatcher.InvokeAsync (OnClipboardUpdated, DispatcherPriority.Background);
			}
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		private void OnDomainUnhandledException (object sender, UnhandledExceptionEventArgs args)
		{
			var assembly = Assembly.GetCallingAssembly ();
			ReportException ((Exception)args.ExceptionObject, assembly);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		private void OnDispatcherUnhandledException (object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			var assembly = Assembly.GetCallingAssembly ();
			args.Handled = true;
			ReportException (args.Exception, assembly);
		}

		private void ShowExceptionDialog (UserLevelExceptionData dataToShow)
		{
			if (_exceptionDialogFactory == null)
			{
				var window = this.MainWindow;
				if (window == null)
				{
					MessageBox.Show (
						string.Join ("; ", dataToShow.Exception.GetDescription ()),
						Wpf.Resources.ExceptionOccured,
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
				else
				{
					MessageBox.Show (
						window,
						string.Join ("; ", dataToShow.Exception.GetDescription ()),
						Wpf.Resources.ExceptionOccured,
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			}
			else
			{
				var dialog = _exceptionDialogFactory.Invoke (dataToShow);
				dialog.ShowDialog ();
			}
		}

		#endregion
	}
}
