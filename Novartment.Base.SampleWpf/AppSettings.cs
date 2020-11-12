using System.Configuration;

namespace Novartment.Base.SampleWpf
{
	public sealed class AppSettings : Novartment.Base.FailsafeApplicationSettingsBase
	{
		public AppSettings ()
		{
		}

		[UserScopedSetting]
		[DefaultSettingValue ("0")]
		public int IntParameter
		{
			get => (int)this[nameof (this.IntParameter)];

			set
			{
				this[nameof (this.IntParameter)] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue ("Строковое значение")]
		public string StringParameter
		{
			get => (string)this[nameof (this.StringParameter)];

			set
			{
				this[nameof (this.StringParameter)] = value;
			}
		}
	}
}