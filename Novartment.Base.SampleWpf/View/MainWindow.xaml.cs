using System;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

namespace Novartment.Base.SampleWpf
{
	public enum DropTargetEventType
	{
		Enter,
		Over,
		Leave,
		Drop,
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : System.Windows.Window
	{
		private readonly AppSettings _appSettings;
		private readonly IDisposable _dropSourceRegistration;
		private readonly IDisposable _dropTargetRegistration;

		public MainWindow (
			BaseViewModel viewModel,
			AppSettings appSettings,
			IDragDropSource dragDropSourceHandler,
			IDragDropTarget dragDropTargetHandler)
		{
			this.DataContext = viewModel;
			_appSettings = appSettings;
			InitializeComponent ();

			_dropSourceRegistration = DropSourceArea.RegisterAsDragDropSource (dragDropSourceHandler);
			_dropTargetRegistration = DropTargetArea.RegisterAsDragDropTarget (dragDropTargetHandler);
		}

		protected override void OnClosed (EventArgs e)
		{
			_dropTargetRegistration.Dispose ();
			_dropSourceRegistration.Dispose ();
			base.OnClosed (e);
		}
	}
}
