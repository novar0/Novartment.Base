using System.Configuration;

namespace Novartment.Base.SampleWpf
{

	public class AppSettings : Novartment.Base.FailsafeApplicationSettingsBase
	{
		public AppSettings ()
		{
		}

		#region свойства которые пишутся в конфиг. файл. может быть сгенерирован автоматически

		[UserScopedSetting]
		[DefaultSettingValue ("0")]
		public int IntParameter
		{
			get => ((int)(this["IntParameter"]));

			set
			{
				this["IntParameter"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue ("Строковое значение")]
		public string StringParameter
		{
			get => ((string)(this["StringParameter"]));

			set
			{
				this["StringParameter"] = value;
			}
		}

		#endregion
	}
}