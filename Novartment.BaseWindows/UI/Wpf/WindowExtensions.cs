using System;
using System.Windows;
using System.Windows.Threading;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для System.Windows.Window.
	/// </summary>
	public static class WindowExtensions
	{
		/// <summary>
		/// Проявляет окно и показывает его перед другими окнами.
		/// </summary>
		/// <param name="window">Окно, которое необходимо показать.</param>
		public static void MakeVisibleAndScheduleBringToFront (this Window window)
		{
			if (window == null)
			{
				throw new ArgumentNullException (nameof (window));
			}

			if (!window.IsLoaded)
			{
				return;
			}

			window.Visibility = Visibility.Visible;
			window.Dispatcher.BeginInvoke (DispatcherPriority.Background, new Action<Window> (BringToFront), window);
		}

		/// <summary>
		/// Показывает окно перед другими окнами.
		/// </summary>
		/// <param name="window">Окно, которое необходимо показать.</param>
		public static void BringToFront (this Window window)
		{
			if (window == null)
			{
				throw new ArgumentNullException (nameof (window));
			}

			if (!window.IsLoaded)
			{
				return;
			}

			window.WindowState = WindowState.Normal;
			window.Activate ();
		}
	}
}
