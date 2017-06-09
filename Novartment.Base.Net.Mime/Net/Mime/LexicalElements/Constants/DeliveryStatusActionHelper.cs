using System;
using System.Diagnostics.Contracts;

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
			switch (value)
			{
				case DeliveryAttemptResult.Failed: return DeliveryStatusActionNames.Failed;
				case DeliveryAttemptResult.Delayed: return DeliveryStatusActionNames.Delayed;
				case DeliveryAttemptResult.Delivered: return DeliveryStatusActionNames.Delivered;
				case DeliveryAttemptResult.Relayed: return DeliveryStatusActionNames.Relayed;
				case DeliveryAttemptResult.Expanded: return DeliveryStatusActionNames.Expanded;
				default:
					throw new NotSupportedException ("Unsupported value of Delivery Status Action: '" + value + "'.");
			}
		}

		/// <summary>
		/// Parses string representation of DeliveryStatusAction enumeration value.
		/// </summary>
		/// <param name="source">String representation of DeliveryStatusAction enumeration value.</param>
		/// <param name="result">When this method returns, contains the DeliveryStatusAction value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out DeliveryAttemptResult result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			var isFailed = DeliveryStatusActionNames.Failed.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isFailed)
			{
				result = DeliveryAttemptResult.Failed;
				return true;
			}

			var isDelayed = DeliveryStatusActionNames.Delayed.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isDelayed)
			{
				result = DeliveryAttemptResult.Delayed;
				return true;
			}

			var isDelivered = DeliveryStatusActionNames.Delivered.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isDelivered)
			{
				result = DeliveryAttemptResult.Delivered;
				return true;
			}

			var isRelayed = DeliveryStatusActionNames.Relayed.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isRelayed)
			{
				result = DeliveryAttemptResult.Relayed;
				return true;
			}

			var isExpanded = DeliveryStatusActionNames.Expanded.Equals (source, StringComparison.OrdinalIgnoreCase);
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
