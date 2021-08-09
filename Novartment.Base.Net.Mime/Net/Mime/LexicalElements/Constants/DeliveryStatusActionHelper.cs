using System;

namespace Novartment.Base.Net.Mime
{
	internal static class DeliveryStatusActionHelper
	{
		/// <summary>
		/// Gets string name of DeliveryStatusAction enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of DeliveryStatusAction enumeration value.</returns>
		internal static string GetName (this DeliveryAttemptResult value)
		{
			return value switch
			{
				DeliveryAttemptResult.Failed => DeliveryStatusActionNames.Failed,
				DeliveryAttemptResult.Delayed => DeliveryStatusActionNames.Delayed,
				DeliveryAttemptResult.Delivered => DeliveryStatusActionNames.Delivered,
				DeliveryAttemptResult.Relayed => DeliveryStatusActionNames.Relayed,
				DeliveryAttemptResult.Expanded => DeliveryStatusActionNames.Expanded,
				_ => throw new NotSupportedException ("Unsupported value of Delivery Status Action: '" + value + "'."),
			};
		}

		/// <summary>
		/// Parses string representation of DeliveryStatusAction enumeration value.
		/// </summary>
		/// <param name="source">String representation of DeliveryStatusAction enumeration value.</param>
		/// <param name="result">When this method returns, contains the DeliveryStatusAction value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (ReadOnlySpan<char> source, out DeliveryAttemptResult result)
		{
			var isFailed = DeliveryStatusActionNames.Failed.AsSpan ().SequenceEqual (source);
			if (isFailed)
			{
				result = DeliveryAttemptResult.Failed;
				return true;
			}

			var isDelayed = DeliveryStatusActionNames.Delayed.AsSpan ().SequenceEqual (source);
			if (isDelayed)
			{
				result = DeliveryAttemptResult.Delayed;
				return true;
			}

			var isDelivered = DeliveryStatusActionNames.Delivered.AsSpan ().SequenceEqual (source);
			if (isDelivered)
			{
				result = DeliveryAttemptResult.Delivered;
				return true;
			}

			var isRelayed = DeliveryStatusActionNames.Relayed.AsSpan ().SequenceEqual (source);
			if (isRelayed)
			{
				result = DeliveryAttemptResult.Relayed;
				return true;
			}

			var isExpanded = DeliveryStatusActionNames.Expanded.AsSpan ().SequenceEqual (source);
			if (isExpanded)
			{
				result = DeliveryAttemptResult.Expanded;
				return true;
			}

			result = DeliveryAttemptResult.Unspecified;
			return false;
		}
	}
}
