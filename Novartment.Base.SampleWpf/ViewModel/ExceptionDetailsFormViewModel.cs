using System;
using Novartment.Base;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

namespace Novartment.Base.SampleWpf
{
	public class ExceptionDetailsFormViewModel : BaseViewModel, IDialogViewModel<bool?>
	{
		private readonly string ExceptionStringSeparator = Environment.NewLine + "------------------------------" + Environment.NewLine;
		private readonly UserLevelExceptionData _data;
		private readonly IValueHolder<string> _details;

		public bool? Result { get { return true; } }

		public string FailedUser { get { return _data.FailedUser; } }
		public string FailedFramework { get { return _data.FailedFramework; } }
		public string FailedAssemblyName { get { return _data.FailedAssemblyName; } }
		public string FailedAssemblyVersion { get { return _data.FailedAssemblyVersion; } }
		public string FailedAction { get { return _data.FailedAction; } }
		public string RecommendedSoltion { get { return _data.RecommendedSolution; } }
		public string Message { get { return _data.Exception.Message; } }
		public string Details { get { return _details.Value; } }

		public ExceptionDetailsFormViewModel (UserLevelExceptionData data)
		{
			_data = data;
			_details = new LazyValueHolder<string> (InitializeDetails);
		}

		private string InitializeDetails ()
		{
			var fullDetails = string.Join (ExceptionStringSeparator, _data.Exception.GetFullInfo (ComponentApplication.Current.SourceDirectory));
			return
				$"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\nПользователь: {FailedUser}\r\nСистема: {FailedFramework}\r\nПрограмма: {FailedAssemblyName} {FailedAssemblyVersion}\r\nОперация: {FailedAction}\r\nИсключения:{ExceptionStringSeparator}{fullDetails}";
		}
	}
}
