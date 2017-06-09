using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Цепь связанных команд.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "This collection is more chain than collection.")]
	public class CommandChain
	{
		private SingleLinkedListNode<ChainedCommandBase> _firstCommand = null;

		/// <summary>
		/// Инициализирует новый экземпляр класса CommandChain
		/// с указанным поведением в цепи связанных команд.
		/// </summary>
		/// <param name="executionChained">Признак выполнения всех команд цепи при выполнении одной.</param>
		/// <param name="executionAbilityChainBehavior">Поведение при запросе готовности выполнения команды связанное с другими командами цепи.</param>
		public CommandChain (bool executionChained, ExecutionAbilityChainBehavior executionAbilityChainBehavior)
		{
			this.ExecutionChained = executionChained;
			this.ExecutionAbilityChainBehavior = executionAbilityChainBehavior;
		}

		/// <summary>
		/// Получает признак выполнения всех команд цепи при выполнении одной.
		/// </summary>
		public bool ExecutionChained { get; }

		/// <summary>
		/// Получает поведение при запросе готовности выполнения команды связанное с другими командами цепи.
		/// </summary>
		public ExecutionAbilityChainBehavior ExecutionAbilityChainBehavior { get; }

		/// <summary>
		/// Получает начальный узел односвязного списка команд цепи.
		/// </summary>
		public SingleLinkedListNode<ChainedCommandBase> FirstCommand => _firstCommand;

		/// <summary>Очищает цепь.</summary>
		public void Clear ()
		{
			_firstCommand = null;
		}

		/// <summary>
		/// Добавляет указанную команду в цепь.
		/// </summary>
		/// <param name="command">Команда для добавления в цепь.</param>
		public void Add (ChainedCommandBase command)
		{
			if (command == null)
			{
				throw new ArgumentNullException (nameof (command));
			}

			Contract.EndContractBlock ();

			_firstCommand = new SingleLinkedListNode<ChainedCommandBase> (command, _firstCommand);
		}
	}
}
