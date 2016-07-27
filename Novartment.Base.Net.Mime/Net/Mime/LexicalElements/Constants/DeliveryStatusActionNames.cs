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

	/// <summary>Value of 'Action' field in Delivery Status Notification entity.
	/// Indicates the action performed by the Reporting-MTA as a result of its attempt to deliver the message to this recipient address.
	/// Определено в RFC 3464 section 2.3.</summary>
	internal static class DeliveryStatusActionNames
	{
		/// <summary>"failed". Message could not be delivered to the recipient.</summary>
		internal static readonly string Failed = "failed";

		/// <summary>"delayed". Reporting MTA has so far been unable to deliver or relay the message,
		/// but it will continue to attempt to do so.</summary>
		internal static readonly string Delayed = "delayed";

		/// <summary>"delivered". Message was successfully delivered to the recipient address specified by the sender,
		/// which includes "delivery" to a mailing list exploder.</summary>
		internal static readonly string Delivered = "delivered";

		/// <summary>"relayed". Message has been relayed or gatewayed into an environment that does not accept responsibility for
		/// generating DSNs upon successful delivery.</summary>
		internal static readonly string Relayed = "relayed";

		/// <summary>"expanded". Message has been successfully delivered to the recipient address as specified by the sender,
		/// and forwarded by the Reporting-MTA beyond that destination to multiple additional recipient addresses.</summary>
		internal static readonly string Expanded = "expanded";
	}
}
