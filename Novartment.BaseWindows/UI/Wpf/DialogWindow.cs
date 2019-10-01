using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Диалоговое окно.
	/// </summary>
	public class DialogWindow : Window,
		INotifyPropertyChanged
	{
		private UIElement _defaultButton;
		private int _nErrors;

		/// <summary>Инициализирует новый экземпляр класса DialogWindow.</summary>
		public DialogWindow ()
			: this (null)
		{
		}

		/// <summary>Инициализирует новый экземпляр класса DialogWindow с указанием родительского окна.</summary>
		/// <param name="owner">Родительское окно.</param>
		public DialogWindow (Window owner)
			: base ()
		{
			if (owner == null)
			{
				var mainWindow = Application.Current.MainWindow;
				if ((mainWindow != this) && mainWindow.IsLoaded && mainWindow.IsVisible)
				{
					owner = mainWindow;
				}
			}

			if (owner != null)
			{
				this.Owner = owner;
				this.ShowInTaskbar = false;
			}

			Validation.AddErrorHandler (this, ValidationErrorHandler);
		}

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Получает признак того, что все элементы диалогового окна успешно прошли валидацию данных.
		/// </summary>
		public bool DataAreValid => _nErrors < 1;

		/// <summary>Создает событие Initialized.</summary>
		/// <param name="e">Данные для события.</param>
		protected override void OnInitialized (EventArgs e)
		{
			var button = this.FindLogicalChild<Button> (item => item.IsDefault);
			if (button != null)
			{
				button.Click += CommitButtonClickHandler;
				if (button.IsEnabled)
				{
					_defaultButton = button;
				}
			}

			base.OnInitialized (e);
		}

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="name">Имя свойства.</param>
		protected virtual void OnPropertyChanged (string name)
		{
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
		}

		private void ValidationErrorHandler (object sender, ValidationErrorEventArgs e)
		{
			switch (e.Action)
			{
				case ValidationErrorEventAction.Added: _nErrors++; break;
				case ValidationErrorEventAction.Removed: _nErrors--; break;
			}

			if (_defaultButton != null)
			{
				_defaultButton.IsEnabled = this.DataAreValid;
			}

			OnPropertyChanged ("DataAreValid");
		}

		private void CommitButtonClickHandler (object sender, RoutedEventArgs e)
		{
			if (this.DataAreValid)
			{
				this.DialogResult = true;
			}
		}
	}
}
