using System.Collections.Generic;

namespace Novartment.Base
{
	/// <summary>
	/// Объект для обмена данными между приложениями.
	/// </summary>
	/// <remarks>
	/// Создан чтобы не привязываться к конкретным технологиям,
	/// в которых есть аналоги (System.Windows.DataObject и System.Windows.Forms.DataObject).
	/// </remarks>
	public interface IDataContainer
	{
		/// <summary>
		/// Gets a collection of strings, with each string specifying the name of a format supported by this data object.
		/// </summary>
		IReadOnlyList<string> AvailableFormats { get; }

		/// <summary>
		/// Retrieves a data object in a specified format; the data format is specified by a string.
		/// </summary>
		/// <param name="format">A string that specifies what format to retrieve the data as.</param>
		/// <param name="autoConvert">True to attempt to automatically convert the data to the specified format;
		/// False for no data format conversion.</param>
		/// <returns>A data object with the data in the specified format, or null if the data is not available in the specified format.</returns>
		object GetData (string format, bool autoConvert);

		/// <summary>
		/// Stores the specified data in this data object, along with one or more specified data formats.
		/// The data format is specified by a string.
		/// </summary>
		/// <param name="format">A string that specifies what format to store the data in.</param>
		/// <param name="value">The data to store in this data object.</param>
		/// <param name="autoConvert">True to allow the data to be converted to another format on retrieval;
		/// False to prohibit the data from being converted to another format on retrieval.</param>
		void SetData (string format, object value, bool autoConvert);
	}
}
