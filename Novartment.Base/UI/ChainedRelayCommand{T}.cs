using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Команда, выполняющая указанный делегат.
	/// Может быть частью цепи связанных команд.
	/// </summary>
	/// <typeparam name="T">Тип параметра команды.</typeparam>
	public sealed class ChainedRelayCommand<T> : ChainedCommandBase
	{
		private readonly Action<T> _execute;
		private readonly Func<T, bool> _canExecute;

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат
		/// и использовать другой указанный делегат для получения статуса готовности команды.
		/// Команда не будет являтся частью цепи связанных команд.
		/// </summary>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		public ChainedRelayCommand (Action<T> execute, Func<T, bool> canExecute = null)
			: base (null)
		{
			_execute = execute ?? throw new ArgumentNullException (nameof (execute)); ;
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
		public ChainedRelayCommand (CommandChain commandChain, Action<T> execute, Func<T, bool> canExecute)
			: base (commandChain)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Определяет готовность отдельно взятой команды (без учёта цепи) к исполнению с указанным параметром.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		/// <returns>Признак готовности команды к исполнению.</returns>
		protected override bool CanExecuteThis (object parameter)
		{
			return (_canExecute == null) || _canExecute.Invoke ((T)parameter);
		}

		/// <summary>
		/// Исполняет отдельно взятую команду (без учёта цепи) с указанным параметром.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		protected override void ExecuteThis (object parameter)
		{
			_execute.Invoke ((T)parameter);
		}
	}
}
