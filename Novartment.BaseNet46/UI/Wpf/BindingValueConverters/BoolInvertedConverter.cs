using System;
using System.Globalization;
using System.Windows.Data;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Инвертирует в обе стороны булевые значение
	/// </summary>
	[ValueConversion (typeof (bool), typeof (bool))]
	public class BoolInvertedConverter :
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
			return !(bool)value;
		}

		/// <summary>Преобразует значение.</summary>
		/// <param name="value">Значение, произведенное целью привязки.</param>
		/// <param name="targetType">Тип, в который выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}
}
