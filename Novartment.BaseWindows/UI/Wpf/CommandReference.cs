using System;
using System.Windows;
using System.Windows.Input;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Ссылка на команду.
	/// </summary>
	/// <remarks>
	/// This class facilitates associating a key binding in XAML markup to a command
	/// defined in a View Model by exposing a Command dependency property.
	/// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
	/// </remarks>
	public sealed class CommandReference : Freezable,
		ICommand
	{
		/// <summary>
		/// Команда, хранящаяся в ссылке.
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register (
			"Command",
			typeof (ICommand),
			typeof (CommandReference),
			new PropertyMetadata (OnCommandChanged));

		private EventHandler _commandCanExecuteChangedHandler;

		/// <summary>
		/// Инициализирует новый экземпляр класса CommandReference.
		/// </summary>
		public CommandReference ()
		{
		}

		/// <summary>Происходит при изменениях, влияющих на то, должна ли выполняться данная команда.</summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Получает или устанавливает команду, хранящуюся в ссылке.
		/// </summary>
		public ICommand Command
		{
			get => (ICommand)GetValue (CommandProperty);
			set { SetValue (CommandProperty, value); }
		}

		/// <summary>Определяет метод, который определяет, может ли данная команда выполняться в ее текущем состоянии.</summary>
		/// <param name="parameter">Данные, используемые данной командой.</param>
		/// <returns>Значение true, если команда может быть выполнена; в противном случае — значение false.</returns>
		public bool CanExecute (object parameter)
		{
			return this.Command?.CanExecute (parameter) ?? false;
		}

		/// <summary>Определяет метод, вызываемый при вызове данной команды.</summary>
		/// <param name="parameter">Данные, используемые данной командой.</param>
		public void Execute (object parameter)
		{
			this.Command.Execute (parameter);
		}

		/// <summary>Не реализовано.</summary>
		/// <returns>Исключение NotImplementedException.</returns>
		protected override Freezable CreateInstanceCore ()
		{
			throw new NotImplementedException ();
		}

		private static void OnCommandChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var commandReference = d as CommandReference;

			if (e.OldValue is ICommand oldCommand)
			{
				oldCommand.CanExecuteChanged -= commandReference._commandCanExecuteChangedHandler;
			}

			if (e.NewValue is ICommand newCommand)
			{
				commandReference._commandCanExecuteChangedHandler = commandReference.CommandCanExecuteChanged;
				newCommand.CanExecuteChanged += commandReference._commandCanExecuteChangedHandler;
			}
		}

		private void CommandCanExecuteChanged (object sender, EventArgs e)
		{
			this.CanExecuteChanged?.Invoke (this, e);
		}
	}
}
