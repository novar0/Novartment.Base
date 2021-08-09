using System;
using System.Windows.Threading;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Реализация таймера на основе System.Windows.Threading.DispatcherTimer.
	/// При срабатывании вызывает указанный делегат.
	/// </summary>
	public sealed class WindowsDispatcherTimer : WindowsDispatcherTimer<Action>
	{
		/// <summary>
		/// Инициализирует новый экземпляр WindowsFormsTimer, вызывающий при срабатывании указанный делегат.
		/// </summary>
		/// <param name="callback">Делегат, вызываемый при срабатывании таймера.</param>
		/// <param name="dispatcher">Диспетчер, используемый при создании таймера.</param>
		/// <param name="priority">Приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.</param>
		public WindowsDispatcherTimer (Action callback, Dispatcher dispatcher, DispatcherPriority priority)
			: base (DoCallbackWithParam, callback, dispatcher, priority)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}
		}

		private static void DoCallbackWithParam (Action callback)
		{
			callback.Invoke ();
		}
	}
}
