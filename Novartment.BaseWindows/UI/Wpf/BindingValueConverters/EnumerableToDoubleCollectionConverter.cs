using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static System.Linq.Enumerable;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Конвертирует любой список в коллекцию System.Double значений.
	/// </summary>
	[ValueConversion (typeof (IEnumerable), typeof (DoubleCollection))]
	public sealed class EnumerableToDoubleCollectionConverter :
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
			return (value is IEnumerable list) ?
				new DoubleCollection(list.Cast<object>().Select(System.Convert.ToDouble)) :
				DependencyProperty.UnsetValue;
		}

		/// <summary>Преобразует значение.</summary>
		/// <param name="value">Значение, произведенное целью привязки.</param>
		/// <param name="targetType">Тип, в который выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException ();
	}
}
