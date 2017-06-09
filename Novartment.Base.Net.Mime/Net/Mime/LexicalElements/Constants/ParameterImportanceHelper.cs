using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	internal static class ParameterImportanceHelper
	{
		/// <summary>
		/// Gets string name of ParameterImportance enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of ParameterImportance enumeration value.</returns>
		internal static string GetName (this DispositionNotificationParameterImportance value)
		{
			switch (value)
			{
				case DispositionNotificationParameterImportance.Required: return ParameterImportanceNames.Required;
				case DispositionNotificationParameterImportance.Optional: return ParameterImportanceNames.Optional;
				default:
					throw new NotSupportedException ("Unsupported value of DispositionNotificationParameterImportance '" + value + "'.");
			}
		}

		/// <summary>
		/// Parses string representation of ParameterImportance enumeration value.
		/// </summary>
		/// <param name="source">String representation of ParameterImportance enumeration value.</param>
		/// <param name="result">When this method returns, contains the ParameterImportance value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out DispositionNotificationParameterImportance result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var isRequired = ParameterImportanceNames.Required.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isRequired)
			{
				result = DispositionNotificationParameterImportance.Required;
				return true;
			}

			var isOptional = ParameterImportanceNames.Optional.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isOptional)
			{
				result = DispositionNotificationParameterImportance.Optional;
				return true;
			}

			result = DispositionNotificationParameterImportance.Unspecified;
			return false;
		}
	}
}
