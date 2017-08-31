using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Базовый класс хранения настроек программы.
	/// Дополнительно к ApplicationSettingsBase поддерживает автоматический перенос из более старой версии (апгрейд)
	/// и восстановление в состояние по-умолчанию в случае повреждёния файла.
	/// </summary>
	public class FailsafeApplicationSettingsBase : ApplicationSettingsBase
	{
		private static readonly string _SettingsAreDefaultString = "SettingsAreDefault5F2D9E7B15D04090BFFBC4396EE7ED7D";
		private bool _saveOnAnyChange = true;
		private int _integrityChecked = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса FailsafeApplicationSettingsBase.
		/// </summary>
		public FailsafeApplicationSettingsBase ()
		{
		}

		/// <summary>
		/// Получает или устанавливает свойство автоматического сохранения настроек после любого их изменения.
		/// </summary>
		public bool SaveOnAnyChange
		{
			get => _saveOnAnyChange;

			set
			{
				if (_saveOnAnyChange != value)
				{
					_saveOnAnyChange = value;
					base.OnPropertyChanged (this, new PropertyChangedEventArgs (nameof (this.SaveOnAnyChange)));
				}
			}
		}

		/// <summary>
		/// Получает или устанавливает признак того,
		/// что все настройки приложения имеют значение по-умолчанию,
		/// то есть ещё не сохранялись.
		/// </summary>
		/// <remarks>
		/// Не предназначено для внешнего использования.
		/// </remarks>
		[UserScopedSetting]
		[DefaultSettingValue ("True")]
		public bool SettingsAreDefault5F2D9E7B15D04090BFFBC4396EE7ED7D
		{
			get
			{
				try
				{
					return (bool)this[_SettingsAreDefaultString];
				}
				catch (SettingsPropertyNotFoundException)
				{
					return true;
				}
			}

			set
			{
				this[_SettingsAreDefaultString] = value;
			}
		}

		/// <summary>
		/// Получает или устанавливает значение свойства.
		/// </summary>
		/// <param name="propertyName">Имя свойства.</param>
		/// <returns>Значение свойства.</returns>
		public override object this[string propertyName]
		{
			get
			{
				if (propertyName == null)
				{
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
					throw new ArgumentNullException (nameof (propertyName));
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
				}

				Contract.EndContractBlock ();

				var oldValue = Interlocked.CompareExchange (ref _integrityChecked, 1, 0);
				if (oldValue == 0)
				{
					UpgradeIfNeeded ();
				}

				return base[propertyName];
			}

			set
			{
				if (propertyName == null)
				{
					throw new ArgumentNullException (nameof (propertyName));
				}

				Contract.EndContractBlock ();

				var oldValue = Interlocked.CompareExchange (ref _integrityChecked, 1, 0);
				if (oldValue == 0)
				{
					UpgradeIfNeeded ();
				}

				var propVal = base[propertyName];
				var needToUpdate = true;
				if ((propVal == null) && (value == null))
				{
					needToUpdate = false;
				}

				var isProvValueEqualsValue = (propVal != null) && propVal.Equals (value);
				if (isProvValueEqualsValue)
				{
					needToUpdate = false;
				}

				if (needToUpdate)
				{
					base[propertyName] = value;
				}
			}
		}

		/// <summary>
		/// Вызывает событие PropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="sender">Объект-источник уведомления.</param>
		/// <param name="e">Аргументы события PropertyChanged.</param>
		protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (_saveOnAnyChange)
			{
				Save ();
			}

			base.OnPropertyChanged (sender, e);
		}

		private void UpgradeIfNeeded ()
		{
			try
			{
				if (this.SettingsAreDefault5F2D9E7B15D04090BFFBC4396EE7ED7D)
				{
					Upgrade ();
					this.SettingsAreDefault5F2D9E7B15D04090BFFBC4396EE7ED7D = false;
					Save ();
				}
			}
			catch (ConfigurationErrorsException excpt)
			{
				// повреждён файл конфигурации. удаляем его и сбрасываем все настройки в состояние по умолчанию, требуя перезагрузки приложения
				var configFileName = excpt.Filename ?? ((ConfigurationErrorsException)excpt.InnerException).Filename;
				File.Delete (configFileName);
				Reload ();
				throw new ApplicationRestartRequiredException (
					"Failed to load application settings. Setting have been resetted and will take effect after a application restart.",
					excpt);
			}
		}
	}
}
