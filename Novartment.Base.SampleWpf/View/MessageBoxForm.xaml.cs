using System.Windows;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

namespace Novartment.Base.SampleWpf
{
	/// <summary>
	/// Interaction logic for OneFieldEnterForm.xaml
	/// </summary>
	public partial class MessageBoxForm : DialogWindow, IDialogView<MessageBoxResult>
	{
		public MessageBoxForm (IDialogViewModel<MessageBoxResult> viewModel)
			: base ()
		{
			this.DataContext = viewModel;
			ViewModel = viewModel;
			InitializeComponent ();
		}

		public MessageBoxForm (IDialogViewModel<MessageBoxResult> viewModel, Window owner)
			: base (owner)
		{
			this.DataContext = viewModel;
			ViewModel = viewModel;
			InitializeComponent ();
		}

		public IDialogViewModel<MessageBoxResult> ViewModel { get; private set; }
	}
}
