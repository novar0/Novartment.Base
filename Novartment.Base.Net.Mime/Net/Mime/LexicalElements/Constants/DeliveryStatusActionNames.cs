namespace Novartment.Base.Net.Mime
{
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
