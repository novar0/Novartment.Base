using System;
using System.Globalization;
using System.Windows.Data;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Конвертирует в обе стороны enum / номер.
	/// </summary>
	[ValueConversion (typeof (Enum), typeof (IConvertible))]
	public class EnumerableToNumberConverter :
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
			if (!(value is IConvertible conv))
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}

			return conv.ToType (targetType, (culture ?? CultureInfo.InvariantCulture).NumberFormat);
		}

		/// <summary>Преобразует значение.</summary>
		/// <param name="value">Значение, произведенное целью привязки.</param>
		/// <param name="targetType">Тип, в который выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is IConvertible conv))
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}

			var enumType = Enum.GetUnderlyingType (targetType);
			var newValue = conv.ToType (enumType, (culture ?? CultureInfo.InvariantCulture).NumberFormat);
			var res = Enum.ToObject (targetType, newValue);
			return res;
		}
	}
}
