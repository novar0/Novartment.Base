using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Конвертирует в обе стороны число / табличная длина.
	/// </summary>
	[ValueConversion (typeof (int), typeof (GridLength))]
	public sealed class NumberToGridLengthConverter :
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
			return new GridLength (System.Convert.ToDouble (value, CultureInfo.InvariantCulture));
		}

		/// <summary>Преобразует значение.</summary>
		/// <param name="value">Значение, произведенное целью привязки.</param>
		/// <param name="targetType">Тип, в который выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var gv = (GridLength)value;
			return gv.IsAbsolute ? gv.Value : DependencyProperty.UnsetValue;
		}
	}
}
