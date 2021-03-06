using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Команда, выполняющая указанный делегат.
	/// Может быть частью цепи связанных команд.
	/// </summary>
	public sealed class ChainedRelayCommand : ChainedCommandBase
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат
		/// и использовать другой указанный делегат для получения статуса готовности команды.
		/// Команда не будет являтся частью цепи связанных команд.
		/// </summary>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		public ChainedRelayCommand (Action execute, Func<bool> canExecute = null)
			: base (null)
		{
			_execute = execute ?? throw new ArgumentNullException (nameof (execute));
			_canExecute = canExecute;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат,
		/// использовать другой указанный делегат для получения статуса готовности команды и
		/// будет являться звеном указанной цепи команд.
		/// </summary>
		/// <param name="commandChain">Цепь связанных комманд, звеном которого станет текущая команда.</param>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		public ChainedRelayCommand (CommandChain commandChain, Action execute, Func<bool> canExecute = null)
			: base (commandChain)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Определяет готовность отдельно взятой команды (без учёта цепи) к исполнению.
		/// </summary>
		/// <param name="parameter">Не используется.</param>
		/// <returns>Признак готовности команды к исполнению.</returns>
		protected override bool CanExecuteThis (object parameter)
		{
			return (_canExecute == null) || _canExecute.Invoke ();
		}

		/// <summary>
		/// Исполняет отдельно взятую команду (без учёта цепи).
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		protected override void ExecuteThis (object parameter)
		{
			_execute.Invoke ();
		}
	}
}
