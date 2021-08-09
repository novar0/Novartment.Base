using System;
using System.Collections.Generic;
using System.Windows;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Объект для обмена данными между приложениями на основе System.Windows.DataObject.
	/// </summary>
	public sealed class WpfDataContainer :
		IDataContainer
	{
		private readonly DataObject _dataObject;

		/// <summary>
		/// Initializes a new instance of the WpfDataContainer class.
		/// </summary>
		public WpfDataContainer ()
		{
			_dataObject = new DataObject ();
		}

		/// <summary>
		/// Initializes a new instance of the WpfDataContainer class that contains the specified data.
		/// </summary>
		/// <param name="data">An object that represents the data to store in this data object.</param>
		public WpfDataContainer (object data)
		{
			_dataObject = new DataObject (data);
		}

		/// <summary>
		/// Initializes a new instance of the WpfDataContainer class that contains the specified data and its associated format;
		/// the format is specified by a string.
		/// </summary>
		/// <param name="format">A string that specifies the format for the data.</param>
		/// <param name="data">An object that represents the data to store in this data object.</param>
		public WpfDataContainer (string format, object data)
		{
			if (format == null)
			{
				throw new ArgumentNullException (nameof (format));
			}

			_dataObject = new DataObject (format, data, true);
		}

		/// <summary>
		/// Initializes a new instance of the WpfDataContainer class that contains the specified data and its associated format;
		/// the data format is specified by a System.Type object.
		/// </summary>
		/// <param name="format">A System.Type that specifies the format for the data.</param>
		/// <param name="data">The data to store in this data object.</param>
		public WpfDataContainer (Type format, object data)
		{
			if (format == null)
			{
				throw new ArgumentNullException (nameof (format));
			}

			_dataObject = new DataObject (format, data);
		}

		/// <summary>
		/// Initializes a new instance of the WpfDataContainer class that contains the specified data and its associated format;
		/// the format is specified by a string.
		/// This overload includes a Boolean flag to indicate whether the data may be converted to another format on retrieval.
		/// </summary>
		/// <param name="format">A string that specifies the format for the data.</param>
		/// <param name="data">The data to store in this data object.</param>
		/// <param name="autoConvert">true to allow the data to be converted to another format on retrieval;
		/// false to prohibit the data from being converted to another format on retrieval.</param>
		public WpfDataContainer (string format, object data, bool autoConvert)
		{
			if (format == null)
			{
				throw new ArgumentNullException (nameof (format));
			}

			_dataObject = new DataObject (format, data, autoConvert);
		}

		/// <summary>
		/// Initializes a new instance of the WpfDataContainer class that encapsulates specified System.Windows.DataObject.
		/// </summary>
		/// <param name="dataObject">DataObject that represents data to encapsulate.</param>
		public WpfDataContainer (DataObject dataObject)
		{
			if (dataObject == null)
			{
				throw new ArgumentNullException (nameof (dataObject));
			}

			_dataObject = dataObject;
		}

		/// <summary>
		/// Получает представление объекта данных в виде System.Windows.DataObject.
		/// </summary>
		public DataObject InternalDataObject => _dataObject;

		/// <summary>
		/// An array of strings, with each string specifying the name of a format supported by this data object.
		/// </summary>
		/// <returns>A collection of strings, with each string specifying the name of a format supported by this data object. </returns>
		public IReadOnlyList<string> AvailableFormats => _dataObject.GetFormats (true);

		/// <summary>
		/// Конвертирует объекты типа IDataContainer в объекты типа IDataObject.
		/// </summary>
		/// <param name="data">Исходный обоъект типа IDataContainer.</param>
		/// <returns>Конечный объект типа IDataObject.</returns>
		public static System.Runtime.InteropServices.ComTypes.IDataObject ToComDataObject (IDataContainer data)
		{
			var comDataObject = data as System.Runtime.InteropServices.ComTypes.IDataObject;
			if (comDataObject == null)
			{
				if (data is WpfDataContainer dataContainerWpf)
				{
					comDataObject = dataContainerWpf._dataObject;
				}
			}

			if (comDataObject == null)
			{
				throw new InvalidOperationException ("Specified object can not be converted in System.Runtime.InteropServices.ComTypes.IDataObject");
			}

			return comDataObject;
		}

		/// <summary>
		/// Конвертирует объекты типа IDataObject в объекты типа IDataContainer.
		/// </summary>
		/// <param name="comDataObject">Исходный обоъект типа IDataObject.</param>
		/// <returns>Конечный объект типа IDataContainer.</returns>
		public static IDataContainer FromComDataObject (System.Runtime.InteropServices.ComTypes.IDataObject comDataObject)
		{
			return new WpfDataContainer (comDataObject);
		}

		/// <summary>
		/// Retrieves a data object in a specified format; the data format is specified by a string.
		/// </summary>
		/// <param name="format">A string that specifies what format to retrieve the data as.</param>
		/// <param name="autoConvert">True to attempt to automatically convert the data to the specified format;
		/// false for no data format conversion.</param>
		/// <returns>A data object with the data in the specified format, or null if the data is not available in the specified format.</returns>
		public object GetData (string format, bool autoConvert)
		{
			if (format == null)
			{
				throw new ArgumentNullException (nameof (format));
			}

			return _dataObject.GetData (format, autoConvert);
		}

		/// <summary>
		/// Stores the specified data in this data object, along with one or more specified data formats.
		/// The data format is specified by a string.
		/// </summary>
		/// <param name="format">A string that specifies what format to store the data in.</param>
		/// <param name="value">The data to store in this data object.</param>
		/// <param name="autoConvert">true to allow the data to be converted to another format on retrieval; false
		/// to prohibit the data from being converted to another format on retrieval.</param>
		public void SetData (string format, object value, bool autoConvert)
		{
			if (format == null)
			{
				throw new ArgumentNullException (nameof (format));
			}

			_dataObject.SetData (format, value, autoConvert);
		}
	}
}
