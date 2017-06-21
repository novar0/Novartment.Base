using System;
using Novartment.Base;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

namespace Novartment.Base.SampleWpf
{
	public class ExceptionDetailsFormViewModel : BaseViewModel,
		IDialogViewModel<bool?>
	{
		private readonly string exceptionStringSeparator = Environment.NewLine + "------------------------------" + Environment.NewLine;
		private readonly UserLevelExceptionData _data;
		private readonly IValueHolder<string> _details;

		public ExceptionDetailsFormViewModel (UserLevelExceptionData data)
		{
			_data = data;
			_details = new LazyValueHolder<string> (InitializeDetails);
		}

		public bool? Result => true;

		public string FailedUser => _data.FailedUser;

		public string FailedFramework => _data.FailedFramework;

		public string FailedAssemblyName => _data.FailedAssemblyName;

		public string FailedAssemblyVersion => _data.FailedAssemblyVersion;

		public string FailedAction => _data.FailedAction;

		public string RecommendedSoltion => _data.RecommendedSolution;

		public string Message => _data.Exception.Message;

		public string Details => _details.Value;

		private string InitializeDetails ()
		{
			var fullDetails = string.Join (exceptionStringSeparator, _data.Exception.GetFullInfo (ComponentApplication.Current.SourceDirectory));
			return
				$"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\nПользователь: {FailedUser}\r\nСистема: {FailedFramework}\r\nПрограмма: {FailedAssemblyName} {FailedAssemblyVersion}\r\nОперация: {FailedAction}\r\nИсключения:{exceptionStringSeparator}{fullDetails}";
		}
	}
}
