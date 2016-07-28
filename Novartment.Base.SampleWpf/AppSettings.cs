using System.Configuration;

namespace Novartment.Base.SampleWpf
{

	public class AppSettings : Novartment.Base.FailsafeApplicationSettingsBase
	{
		public AppSettings ()
		{
		}

		#region свойства которые пишутся в конфиг. файл. может быть сгенерирован автоматически

		[UserScopedSettingAttribute ()]
		[DefaultSettingValueAttribute ("0")]
		public int IntParameter
		{
			get
			{
				return ((int)(this["IntParameter"]));
			}
			set
			{
				this["IntParameter"] = value;
			}
		}

		[UserScopedSettingAttribute ()]
		[DefaultSettingValueAttribute ("Строковое значение")]
		public string StringParameter
		{
			get
			{
				return ((string)(this["StringParameter"]));
			}
			set
			{
				this["StringParameter"] = value;
			}
		}

		#endregion
	}
}