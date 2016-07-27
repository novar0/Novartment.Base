using System;
using System.IO;
using static System.Linq.Enumerable;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Конвертирует части строки пути к изображению по формату в параметре -> BitmapImage.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Multi",
		Justification = "Name 'MultiValue*' inherited from library inerface."),
	ValueConversion (typeof (string), typeof (BitmapImage))]
	public class MultiValuePathToBitmapImageConverter :
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
			if (values == null)
			{
				throw new ArgumentNullException (nameof (values));
			}
			if (parameter == null)
			{
				throw new ArgumentNullException (nameof (parameter));
			}
			Contract.EndContractBlock ();

			var isContainsNull = values.Contains (null);
			if (isContainsNull)
			{
				return DependencyProperty.UnsetValue;
			}
			var path = string.Format (CultureInfo.InvariantCulture, (string)parameter, values);
			var uri = new Uri (path);
			var isFileNotExists = (uri.Scheme == "file") && !File.Exists (path);
			if (isFileNotExists)
			{
				return DependencyProperty.UnsetValue;
			}
			var source = new BitmapImage (uri);
			return source;
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
