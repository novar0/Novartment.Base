using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// конвертирует в обе стороны bool / System.Windows.Visibility, параметр (любое значение) указывает на инверсную логику.
	/// </summary>
	[ValueConversion (typeof (bool), typeof (Visibility))]
	public sealed class BoolToVisibilityConverter :
		IValueConverter
	{
		/// <summary>Преобразует значение.</summary>
		/// <param name="value">Значение, произведенное исходной привязкой.</param>
		/// <param name="targetType">Тип свойства цели связывания.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return Visibility.Collapsed;
			}

			if ((bool)value ^ (parameter != null))
			{
				return Visibility.Visible;
			}

			return Visibility.Collapsed;
		}

		/// <summary>Преобразует значение.</summary>
		/// <param name="value">Значение, произведенное целью привязки.</param>
		/// <param name="targetType">Тип, в который выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((Visibility)value == Visibility.Visible) ^ (parameter != null);
		}
	}
}
