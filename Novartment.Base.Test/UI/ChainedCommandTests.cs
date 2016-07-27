using System.Collections.Generic;
using Xunit;
using Novartment.Base.UI;

namespace Novartment.Base.Test
{
	public class ChainedCommandTests
	{
		internal class ChainedCommandMock : ChainedCommandBase
		{
			internal List<object> CanExecutes = new List<object> ();
			internal List<object> Executes = new List<object> ();

			internal ChainedCommandMock (CommandChain commandChain) : base (commandChain)
			{
			}

			protected override bool CanExecuteThis (object parameter)
			{
				CanExecutes.Add (parameter);
				return parameter != null;
			}

			protected override void ExecuteThis (object parameter)
			{
				Executes.Add (parameter);
			}
		}

		[Fact, Trait ("Category", "UI")]
		public void NoChain ()
		{
			CommandChain commandChain = null;
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
		}

		[Fact, Trait ("Category", "UI")]
		public void ExecuteOnlyThis_CanExecuteWhenThis ()
		{
			var commandChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenThis);
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
		}

		[Fact, Trait ("Category", "UI")]
		public void ExecuteOnlyThis_CanExecuteWhenAny ()
		{
			var commandChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenAny);
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (3, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (4, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (5, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (6, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);

			cmd1.CanExecutes.Clear ();
			cmd2.CanExecutes.Clear ();
			cmd3.CanExecutes.Clear ();
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (3, cmd1.CanExecutes.Count);
			Assert.Equal (3, cmd2.CanExecutes.Count);
			Assert.Equal (3, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (4, cmd1.CanExecutes.Count);
			Assert.Equal (4, cmd2.CanExecutes.Count);
			Assert.Equal (4, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (5, cmd1.CanExecutes.Count);
			Assert.Equal (5, cmd2.CanExecutes.Count);
			Assert.Equal (5, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (6, cmd1.CanExecutes.Count);
			Assert.Equal (6, cmd2.CanExecutes.Count);
			Assert.Equal (6, cmd3.CanExecutes.Count);
		}

		[Fact, Trait ("Category", "UI")]
		public void ExecuteOnlyThis_CanExecuteWhenAll ()
		{
			var commandChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenAll);
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (3, cmd1.CanExecutes.Count);
			Assert.Equal (3, cmd2.CanExecutes.Count);
			Assert.Equal (3, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (4, cmd1.CanExecutes.Count);
			Assert.Equal (4, cmd2.CanExecutes.Count);
			Assert.Equal (4, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (5, cmd1.CanExecutes.Count);
			Assert.Equal (5, cmd2.CanExecutes.Count);
			Assert.Equal (5, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (6, cmd1.CanExecutes.Count);
			Assert.Equal (6, cmd2.CanExecutes.Count);
			Assert.Equal (6, cmd3.CanExecutes.Count);

			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 1, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 2, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 3, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 4, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 5, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 6, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
		}

		[Fact, Trait ("Category", "UI")]
		public void ExecuteAll_CanExecuteWhenThis ()
		{
			var commandChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenThis);
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (3, cmd1.Executes.Count);
			Assert.Equal (3, cmd2.Executes.Count);
			Assert.Equal (3, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (4, cmd1.Executes.Count);
			Assert.Equal (4, cmd2.Executes.Count);
			Assert.Equal (4, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (5, cmd1.Executes.Count);
			Assert.Equal (5, cmd2.Executes.Count);
			Assert.Equal (5, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (6, cmd1.Executes.Count);
			Assert.Equal (6, cmd2.Executes.Count);
			Assert.Equal (6, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
		}

		[Fact, Trait ("Category", "UI")]
		public void ExecuteAll_CanExecuteWhenAny ()
		{
			var commandChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenAny);
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (3, cmd1.Executes.Count);
			Assert.Equal (3, cmd2.Executes.Count);
			Assert.Equal (3, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (4, cmd1.Executes.Count);
			Assert.Equal (4, cmd2.Executes.Count);
			Assert.Equal (4, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (5, cmd1.Executes.Count);
			Assert.Equal (5, cmd2.Executes.Count);
			Assert.Equal (5, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (6, cmd1.Executes.Count);
			Assert.Equal (6, cmd2.Executes.Count);
			Assert.Equal (6, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (3, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (4, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (5, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (6, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);

			cmd1.CanExecutes.Clear ();
			cmd2.CanExecutes.Clear ();
			cmd3.CanExecutes.Clear ();
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (3, cmd1.CanExecutes.Count);
			Assert.Equal (3, cmd2.CanExecutes.Count);
			Assert.Equal (3, cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (4, cmd1.CanExecutes.Count);
			Assert.Equal (4, cmd2.CanExecutes.Count);
			Assert.Equal (4, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (5, cmd1.CanExecutes.Count);
			Assert.Equal (5, cmd2.CanExecutes.Count);
			Assert.Equal (5, cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (6, cmd1.CanExecutes.Count);
			Assert.Equal (6, cmd2.CanExecutes.Count);
			Assert.Equal (6, cmd3.CanExecutes.Count);
		}

		[Fact, Trait ("Category", "UI")]
		public void ExecuteAll_CanExecuteWhenAll ()
		{
			var commandChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenAll);
			var cmd1 = new ChainedCommandMock (commandChain);
			var cmd2 = new ChainedCommandMock (commandChain);
			var cmd3 = new ChainedCommandMock (commandChain);

			Assert.Equal (0, cmd1.Executes.Count);
			Assert.Equal (0, cmd2.Executes.Count);
			Assert.Equal (0, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (1, cmd1.Executes.Count);
			Assert.Equal (1, cmd2.Executes.Count);
			Assert.Equal (1, cmd3.Executes.Count);
			cmd1.Execute (null);
			Assert.Equal (2, cmd1.Executes.Count);
			Assert.Equal (2, cmd2.Executes.Count);
			Assert.Equal (2, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (3, cmd1.Executes.Count);
			Assert.Equal (3, cmd2.Executes.Count);
			Assert.Equal (3, cmd3.Executes.Count);
			cmd2.Execute (null);
			Assert.Equal (4, cmd1.Executes.Count);
			Assert.Equal (4, cmd2.Executes.Count);
			Assert.Equal (4, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (5, cmd1.Executes.Count);
			Assert.Equal (5, cmd2.Executes.Count);
			Assert.Equal (5, cmd3.Executes.Count);
			cmd3.Execute (null);
			Assert.Equal (6, cmd1.Executes.Count);
			Assert.Equal (6, cmd2.Executes.Count);
			Assert.Equal (6, cmd3.Executes.Count);

			Assert.Equal (0, cmd1.CanExecutes.Count);
			Assert.Equal (0, cmd2.CanExecutes.Count);
			Assert.Equal (0, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (1, cmd1.CanExecutes.Count);
			Assert.Equal (1, cmd2.CanExecutes.Count);
			Assert.Equal (1, cmd3.CanExecutes.Count);
			Assert.True (cmd1.CanExecute (1));
			Assert.Equal (2, cmd1.CanExecutes.Count);
			Assert.Equal (2, cmd2.CanExecutes.Count);
			Assert.Equal (2, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (3, cmd1.CanExecutes.Count);
			Assert.Equal (3, cmd2.CanExecutes.Count);
			Assert.Equal (3, cmd3.CanExecutes.Count);
			Assert.True (cmd2.CanExecute (1));
			Assert.Equal (4, cmd1.CanExecutes.Count);
			Assert.Equal (4, cmd2.CanExecutes.Count);
			Assert.Equal (4, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (5, cmd1.CanExecutes.Count);
			Assert.Equal (5, cmd2.CanExecutes.Count);
			Assert.Equal (5, cmd3.CanExecutes.Count);
			Assert.True (cmd3.CanExecute (1));
			Assert.Equal (6, cmd1.CanExecutes.Count);
			Assert.Equal (6, cmd2.CanExecutes.Count);
			Assert.Equal (6, cmd3.CanExecutes.Count);

			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 1, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd1.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 2, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 3, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd2.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 4, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 5, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
			Assert.False (cmd3.CanExecute (null));
			Assert.Equal (6 + 6 + 6 + 6, cmd1.CanExecutes.Count + cmd2.CanExecutes.Count + cmd3.CanExecutes.Count);
		}
	}
}
