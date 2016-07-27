using System;
using static System.Linq.Enumerable;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Конвертирует (базовое имя + идентификатор) ресурса из директории в ресурс.
	/// </summary>
	public class ParameterToResourceConverter :
		IMultiValueConverter
	{
		/// <summary>Преобразует исходные значения в значение для цели связывания.</summary>
		/// <param name="values">Массив значений, производимых исходными привязками.</param>
		/// <param name="targetType">Тип свойства цели связывания.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Преобразованное значение.</returns>
		public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values == null || parameter == null)
			{
				return DependencyProperty.UnsetValue;
			}
			var isContainsNull = values.Contains (null) || values.Contains (DependencyProperty.UnsetValue);
			if (isContainsNull)
			{
				return DependencyProperty.UnsetValue;
			}
			var element = values[0] as FrameworkElement;
			return (element != null) ?
				element.TryFindResource (string.Format (CultureInfo.InvariantCulture, (string)parameter, values)) :
				DependencyProperty.UnsetValue;
		}

		/// <summary>Преобразует значение цели связывания в исходные значения привязки.</summary>
		/// <param name="value">Значение, производимое целью связывания.</param>
		/// <param name="targetTypes">Массив типов, в которые выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Массив значений, преобразованных из целевых значений назад в исходные значения.</returns>
		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException ();
		}
	}
}
