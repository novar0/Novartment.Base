using System.Collections.Generic;
using Novartment.Base.UI;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class ChainedRelayCommandTests
	{
		private int _canExecuteCount;
		private int _executeCount;
		private List<string> _canExecuteCalls;
		private List<string> _executeCalls;

		[Fact]
		[Trait ("Category", "UI")]
		public void ChainedRelayCommand_CallDelegates ()
		{
			_canExecuteCount = 0;
			_executeCount = 0;
			var cmd = new ChainedRelayCommand (Execute1, CanExecute1);
			Assert.Equal (0, _canExecuteCount);
			Assert.Equal (0, _executeCount);
			Assert.True (cmd.CanExecute (null));
			Assert.Equal (1, _canExecuteCount);
			Assert.Equal (0, _executeCount);
			Assert.True (cmd.CanExecute (null));
			Assert.Equal (2, _canExecuteCount);
			Assert.Equal (0, _executeCount);
			cmd.Execute (null);
			Assert.Equal (2, _canExecuteCount);
			Assert.Equal (1, _executeCount);
			cmd.Execute (null);
			Assert.Equal (2, _canExecuteCount);
			Assert.Equal (2, _executeCount);
		}

		[Fact]
		[Trait ("Category", "UI")]
		public void ChainedRelayCommand_CallDelegatesParam ()
		{
			_canExecuteCalls = new List<string> ();
			_executeCalls = new List<string> ();
			var cmd = new ChainedRelayCommand<string> (Execute2, CanExecute2);
			Assert.Empty (_canExecuteCalls);
			Assert.Empty (_executeCalls);
			Assert.True (cmd.CanExecute ("123"));
			Assert.Single (_canExecuteCalls);
			Assert.Equal ("123", _canExecuteCalls[0]);
			Assert.Empty (_executeCalls);
			Assert.False (cmd.CanExecute (null));
			Assert.Equal (2, _canExecuteCalls.Count);
			Assert.Null (_canExecuteCalls[1]);
			Assert.Empty (_executeCalls);
			cmd.Execute ("abc");
			Assert.Equal (2, _canExecuteCalls.Count);
			Assert.Single (_executeCalls);
			Assert.Equal ("abc", _executeCalls[0]);
			cmd.Execute ("01234ABC");
			Assert.Equal (2, _canExecuteCalls.Count);
			Assert.Equal (2, _executeCalls.Count);
			Assert.Equal ("01234ABC", _executeCalls[1]);
		}

		private bool CanExecute1 ()
		{
			_canExecuteCount++;
			return true;
		}

		private void Execute1 ()
		{
			_executeCount++;
		}

		private bool CanExecute2 (string parameter)
		{
			_canExecuteCalls.Add (parameter);
			return parameter != null;
		}

		private void Execute2 (string parameter)
		{
			_executeCalls.Add (parameter);
		}
	}
}
