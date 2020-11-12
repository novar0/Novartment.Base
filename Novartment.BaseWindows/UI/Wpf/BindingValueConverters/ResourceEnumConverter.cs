using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Resources;
using System.Windows.Data;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Defines a type converter for enum values that converts enum values to and from string representations using resources.
	/// </summary>
	/// <remarks>
	/// This class makes localization of display values for enums in a project easy.
	/// Simply derive a class from this class and pass the ResourceManager in the constructor.
	/// <code lang="C#" escaped="true">
	/// class LocalizedEnumConverter : ResourceEnumConverter
	/// {
	///    public LocalizedEnumConverter (Type type)
	///        : base(type, Properties.Resources.ResourceManager)
	///    {
	///    }
	/// }
	/// </code>
	/// Then define the enum values in the resource editor.
	/// The names of the resources are simply the enum value prefixed by the
	/// enum type name with an underscore separator eg MyEnum_MyValue.
	/// You can then use the TypeConverter attribute to make
	/// the LocalizedEnumConverter the default TypeConverter for the enums in your project.
	/// </remarks>
	public class ResourceEnumConverter : EnumConverter,
		IValueConverter
	{
		private readonly IDictionary<CultureInfo, LookupTable> _lookupTables = new Dictionary<CultureInfo, LookupTable> ();
		private readonly ResourceManager _resourceManager;
		private readonly bool _isFlagEnum = false;
		private readonly Array _flagValues;

		/// <summary>
		/// Initializes a new instance of the converter using translations from the given resource manager.
		/// </summary>
		/// <param name="type">Enum type.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		public ResourceEnumConverter (Type type, ResourceManager resourceManager)
			: base (type)
		{
			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			if (resourceManager == null)
			{
				throw new ArgumentNullException (nameof (resourceManager));
			}

			Contract.EndContractBlock ();

			_resourceManager = resourceManager;
			var flagAttributes = type.GetCustomAttributes (typeof (FlagsAttribute), true);
			_isFlagEnum = flagAttributes.Length > 0;
			if (_isFlagEnum)
			{
				_flagValues = Enum.GetValues (type);
			}
		}

		/// <summary>
		/// Convert the given enum value to string using the registered type converter.
		/// </summary>
		/// <param name="value">The enum value to convert to string.</param>
		/// <returns>The localized string value for the enum.</returns>
		public static string ConvertToString (Enum value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			var converter = TypeDescriptor.GetConverter (value.GetType ());
			return converter.ConvertToString (value);
		}

		/// <summary>
		/// Return a list of the enum values and their associated display text for the given enum type in the current UI Culture.
		/// </summary>
		/// <param name="enumType">The enum type to get the values for.</param>
		/// <returns>
		/// A list of KeyValuePairs where the key is the enum value and the value is the text to display.
		/// </returns>
		/// <remarks>
		/// This method can be used to provide localized binding to enums in ASP.NET applications.
		/// Unlike windows forms the standard ASP.NET controls do not use TypeConverters to convert from enum values to the displayed text.
		/// You can bind an ASP.NET control to the list returned by this method
		/// by setting the DataValueField to "Key" and theDataTextField to "Value".
		/// </remarks>
		public static IReadOnlyList<EnumeratedKeyAndValue> GetValues (Type enumType)
		{
			return GetValues (enumType, CultureInfo.CurrentUICulture);
		}

		/// <summary>
		/// Return a list of the enum values and their associated display text for the given enum type.
		/// </summary>
		/// <param name="enumType">The enum type to get the values for.</param>
		/// <param name="culture">The culture to get the text for.</param>
		/// <returns>
		/// A list of KeyValuePairs where the key is the enum value and the value is the text to display.
		/// </returns>
		/// <remarks>
		/// This method can be used to provide localized binding to enums in ASP.NET applications.
		/// Unlike windows forms the standard ASP.NET controls do not use TypeConverters to convert from enum values to the displayed text.
		/// You can bind an ASP.NET control to the list returned by this method
		/// by setting the DataValueField to "Key" and theDataTextField to "Value".
		/// </remarks>
		public static IReadOnlyList<EnumeratedKeyAndValue> GetValues (Type enumType, CultureInfo culture)
		{
			var result = new ArrayList<EnumeratedKeyAndValue> ();
			var converter = TypeDescriptor.GetConverter (enumType);
			foreach (Enum value in Enum.GetValues (enumType))
			{
				result.Add (new EnumeratedKeyAndValue (value, converter.ConvertToString (null, culture, value)));
			}

			return result;
		}

		/// <summary>
		/// Convert string values to enum values.
		/// </summary>
		/// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
		/// <param name="culture">An optional System.Globalization.CultureInfo. If not supplied, the current culture is assumed.</param>
		/// <param name="value">The System.Object to convert.</param>
		/// <returns>An System.Object that represents the converted value.</returns>
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}

			if (value is string strValue)
			{
				var result = (_isFlagEnum ? GetFlagValue (culture, strValue) : GetValue (culture, strValue))
					?? base.ConvertFrom (context, culture, value);
				return result;
			}

			return base.ConvertFrom (context, culture, value);
		}

		/// <summary>
		/// Convert the enum value to a string.
		/// </summary>
		/// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
		/// <param name="culture">An optional System.Globalization.CultureInfo. If not supplied, the current culture is assumed.</param>
		/// <param name="value">The System.Object to convert.</param>
		/// <param name="destinationType">The System.Type to convert the value to.</param>
		/// <returns>An System.Object that represents the converted value.</returns>
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}

			if (value == null)
			{
				return null;
			}

			if ((destinationType == typeof (string)) || (destinationType == typeof (object)))
			{
				object result = _isFlagEnum ? GetFlagValueText (culture, value) : GetValueText (culture, value);
				return result;
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}

		/// <summary>
		/// Handle XAML Conversion from this type to other types.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="notUsed">not used.</param>
		/// <param name="culture">The culture to convert.</param>
		/// <returns>The converted value.</returns>
		object IValueConverter.Convert (object value, Type targetType, object notUsed, CultureInfo culture)
		{
			return ConvertTo (null, culture, value, targetType);
		}

		/// <summary>
		/// Handle XAML Conversion from other types back to this type.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="notUsed">not used.</param>
		/// <param name="culture">The culture to convert.</param>
		/// <returns>The converted value.</returns>
		object IValueConverter.ConvertBack (object value, Type targetType, object notUsed, CultureInfo culture)
		{
			return ConvertFrom (null, culture, value);
		}

		/// <summary>
		/// Return the name of the resource to use.
		/// </summary>
		/// <param name="value">The value to get.</param>
		/// <returns>The name of the resource to use.</returns>
		protected virtual string GetResourceName (object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return value.GetType ().Name + "_" + value;
		}

		// Return true if the given value is can be represented using a single bit.
		private static bool IsSingleBitValue (ulong value)
		{
			return value switch
			{
				0 => false,
				1 => true,
				_ => (value & (value - 1)) == 0,
			};
		}

		// Get the lookup table for the given culture (creating if necessary).
		private LookupTable GetLookupTable (CultureInfo culture)
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}

			var isValueGetted = _lookupTables.TryGetValue (culture, out LookupTable result);
			if (!isValueGetted)
			{
				result = new LookupTable ();
				foreach (var value in GetStandardValues ())
				{
					var text = GetValueText (culture, value);
					if (text != null)
					{
						result.Add (text, value);
					}
				}

				_lookupTables.Add (culture, result);
			}

			return result;
		}

		/// <summary>
		/// Return the text to display for a simple value in the given culture.
		/// </summary>
		/// <param name="culture">The culture to get the text for.</param>
		/// <param name="value">The enum value to get the text for.</param>
		/// <returns>The localized text.</returns>
		private string GetValueText (CultureInfo culture, object value)
		{
			var resourceName = GetResourceName (value);
			return _resourceManager.GetString (resourceName, culture) ?? resourceName;
		}

		/// <summary>
		/// Return the text to display for a flag value in the given culture.
		/// </summary>
		/// <param name="culture">The culture to get the text for.</param>
		/// <param name="value">The flag enum value to get the text for.</param>
		/// <returns>The localized text.</returns>
		private string GetFlagValueText (CultureInfo culture, object value)
		{
			// if there is a standard value then use it
			var isValueDefined = Enum.IsDefined (value.GetType (), value);
			if (isValueDefined)
			{
				return GetValueText (culture, value);
			}

			// otherwise find the combination of flag bit values
			// that makes up the value
			ulong lValue = Convert.ToUInt32 (value, CultureInfo.InvariantCulture);
			string result = null;
			foreach (var flagValue in _flagValues)
			{
				ulong lFlagValue = Convert.ToUInt32 (flagValue, CultureInfo.InvariantCulture);
				var isSingleBitValue = IsSingleBitValue (lFlagValue);
				if (isSingleBitValue)
				{
					if ((lFlagValue & lValue) == lFlagValue)
					{
						var valueText = GetValueText (culture, flagValue);
						result = (result != null) ? (result + ", " + valueText) : valueText;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Return the Enum value for a simple (non-flagged enum).
		/// </summary>
		/// <param name="culture">The culture to convert using.</param>
		/// <param name="text">The text to convert.</param>
		/// <returns>The enum value.</returns>
		private object GetValue (CultureInfo culture, string text)
		{
			var lookupTable = GetLookupTable (culture);
			lookupTable.TryGetValue (text, out object result);
			return result;
		}

		/// <summary>
		/// Return the Enum value for a flagged enum.
		/// </summary>
		/// <param name="culture">The culture to convert using.</param>
		/// <param name="text">The text to convert.</param>
		/// <returns>The enum value.</returns>
		private object GetFlagValue (CultureInfo culture, string text)
		{
			var lookupTable = GetLookupTable (culture);
			var textValues = text.Split (',');
			ulong result = 0;
			foreach (var textValue in textValues)
			{
				var trimmedTextValue = textValue.Trim ();
				var isValueGetted = lookupTable.TryGetValue (trimmedTextValue, out object value);
				if (!isValueGetted)
				{
					return null;
				}

				result |= Convert.ToUInt32 (value, CultureInfo.InvariantCulture);
			}

			return Enum.ToObject (this.EnumType, result);
		}

		private class LookupTable : Dictionary<string, object>
		{
		}
	}
}
