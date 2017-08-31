using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// A base class for the ViewModel classes in the MVVM pattern.
	/// </summary>
	//// [ClassInfo(typeof(BaseViewModel))]
	public abstract class BaseViewModel : ModelBase
	{
		private static bool? _isInDesignMode;

		/// <summary>
		/// Initializes a new instance of the BaseViewModel class.
		/// </summary>
		protected BaseViewModel ()
		{
		}

		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running in Blend or Visual Studio).
		/// </summary>
		public static bool IsInDesignMode
		{
			get
			{
				if (!_isInDesignMode.HasValue)
				{
					var prop = DesignerProperties.IsInDesignModeProperty;
					_isInDesignMode
						= (bool)DependencyPropertyDescriptor
									 .FromProperty (prop, typeof (FrameworkElement))
									 .Metadata.DefaultValue;

					// Just to be sure
					if (!_isInDesignMode.Value
						&& Process.GetCurrentProcess ().ProcessName.StartsWith ("devenv", StringComparison.OrdinalIgnoreCase))
					{
						_isInDesignMode = true;
					}
				}

				return _isInDesignMode.Value;
			}
		}
	}
}
