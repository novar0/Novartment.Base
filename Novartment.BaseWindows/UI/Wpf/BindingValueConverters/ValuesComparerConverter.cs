using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Конвертирует два значения в результат их сравнения.
	/// </summary>
	[ValueConversion (typeof (string), typeof (BitmapImage))]
	public sealed class ValuesComparerConverter :
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
			if (values == null || values.Length != 2)
			{
				return false;
			}

			return values[0] == null ? values[1] == null : (object)values[0].Equals (values[1]);
		}

		/// <summary>Преобразует значение цели связывания в исходные значения привязки.</summary>
		/// <param name="value">Значение, производимое целью связывания.</param>
		/// <param name="targetTypes">Массив типов, в которые выполняется преобразование.</param>
		/// <param name="parameter">Используемый параметр преобразователя.</param>
		/// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
		/// <returns>Массив значений, преобразованных из целевых значений назад в исходные значения.</returns>
		public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}
