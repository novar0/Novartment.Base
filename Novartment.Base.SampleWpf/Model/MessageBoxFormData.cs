namespace Novartment.Base.SampleWpf
{
	public sealed class MessageBoxFormData
	{
		public MessageBoxFormData (string title, string message)
		{
			this.Title = title;
			this.Message = message;
		}

		public string Title { get; }

		public string Message { get; }
	}
}
