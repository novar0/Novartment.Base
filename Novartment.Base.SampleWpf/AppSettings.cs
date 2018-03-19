using System.Configuration;

namespace Novartment.Base.SampleWpf
{
	public class AppSettings : Novartment.Base.FailsafeApplicationSettingsBase
	{
		public AppSettings ()
		{
		}

		[UserScopedSetting]
		[DefaultSettingValue ("0")]
		public int IntParameter
		{
			get => (int)this[nameof (IntParameter)];

			set
			{
				this[nameof (IntParameter)] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue ("Строковое значение")]
		public string StringParameter
		{
			get => (string)this[nameof (StringParameter)];

			set
			{
				this[nameof (StringParameter)] = value;
			}
		}
	}
}