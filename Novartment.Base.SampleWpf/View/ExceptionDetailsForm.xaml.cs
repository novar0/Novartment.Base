using System;
using System.Windows;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

namespace Novartment.Base.SampleWpf
{
	/// <summary>
	/// Interaction logic for OneFieldEnterForm.xaml
	/// </summary>
	public partial class ExceptionDetailsForm : DialogWindow, IDialogView<bool?>
	{
		public ExceptionDetailsForm (IDialogViewModel<bool?> viewModel)
			: base ()
		{
			InitCore (viewModel);
		}

		public ExceptionDetailsForm (IDialogViewModel<bool?> viewModel, Window owner)
			: base (owner)
		{
			InitCore (viewModel);
		}

		public IDialogViewModel<bool?> ViewModel { get; private set; }

		protected override void OnContentRendered (EventArgs e)
		{
			double bigSize = MaxWidthTemplate.ActualWidth;
			BigText.MaxHeight = bigSize * 2;
			MainContent.MaxWidth = bigSize * 4;
		}

		private void InitCore (IDialogViewModel<bool?> viewModel)
		{
			this.DataContext = viewModel;
			this.ViewModel = viewModel;
			InitializeComponent ();
			System.Drawing.Icon icon = System.Drawing.SystemIcons.Error;
			InformationalImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon (icon.Handle, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions ());
		}

		private void CopyButtonClick (object sender, RoutedEventArgs e)
		{
			try
			{
				Clipboard.SetText (BigText.Text);
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
			}
		}
	}
}
