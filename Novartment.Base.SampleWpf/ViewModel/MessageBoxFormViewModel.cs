using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

namespace Novartment.Base.SampleWpf
{
	public sealed class MessageBoxFormViewModel : BaseViewModel, IDialogViewModel<System.Windows.MessageBoxResult>
	{
		public MessageBoxFormViewModel (MessageBoxFormData data)
		{
			this.Message = data.Message;
			this.Title = data.Title;
		}

		public System.Windows.MessageBoxResult Result => System.Windows.MessageBoxResult.OK;

		public string Message { get; set; }

		public string Title { get; set; }
	}
}
