using System;
using Novartment.Base.Text;

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
			return value switch
			{
				DispositionNotificationParameterImportance.Required => ParameterImportanceNames.Required,
				DispositionNotificationParameterImportance.Optional => ParameterImportanceNames.Optional,
				_ => throw new NotSupportedException ("Unsupported value of DispositionNotificationParameterImportance '" + value + "'."),
			};
		}

		/// <summary>
		/// Parses string representation of ParameterImportance enumeration value.
		/// </summary>
		/// <param name="source">String representation of ParameterImportance enumeration value.</param>
		/// <param name="result">When this method returns, contains the ParameterImportance value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (ReadOnlySpan<char> source, out DispositionNotificationParameterImportance result)
		{
			var isRequired = ParameterImportanceNames.Required.AsSpan ().SequenceEqual (source);
			if (isRequired)
			{
				result = DispositionNotificationParameterImportance.Required;
				return true;
			}

			var isOptional = ParameterImportanceNames.Optional.AsSpan ().SequenceEqual (source);
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
