using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	internal static class NotificationFieldValueTypeHelper
	{
		/// <summary>
		/// Gets string name of NotificationFieldValueKind enumeration value.
		/// </summary>
		/// <param name="value">Value to get name.</param>
		/// <returns>String name of NotificationFieldValueKind enumeration value.</returns>
		internal static string GetName (this NotificationFieldValueKind value)
		{
			switch (value)
			{
				case NotificationFieldValueKind.Mailbox: return NotificationFieldValueTypeNames.Rfc822;
				case NotificationFieldValueKind.Status: return NotificationFieldValueTypeNames.Smtp;
				case NotificationFieldValueKind.Host: return NotificationFieldValueTypeNames.Dns;
				default:
					throw new NotSupportedException ("Unsupported value of NotificationFieldValueType '" + value + "'.");
			}
		}

		/// <summary>
		/// Parses string representation of NotificationFieldValueKind enumeration value.
		/// </summary>
		/// <param name="source">String representation of NotificationFieldValueKind enumeration value.</param>
		/// <param name="result">When this method returns, contains the NotificationFieldValueKind value.</param>
		/// <returns>True was value parsed successfully; otherwise, false.</returns>
		internal static bool TryParse (string source, out NotificationFieldValueKind result)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			var isRfc822 = NotificationFieldValueTypeNames.Rfc822.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isRfc822)
			{
				result = NotificationFieldValueKind.Mailbox;
				return true;
			}
			var isSmtp = NotificationFieldValueTypeNames.Smtp.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isSmtp)
			{
				result = NotificationFieldValueKind.Status;
				return true;
			}
			var isDns = NotificationFieldValueTypeNames.Dns.Equals (source, StringComparison.OrdinalIgnoreCase);
			if (isDns)
			{
				result = NotificationFieldValueKind.Host;
				return true;
			}
			result = NotificationFieldValueKind.Unspecified;
			return false;
		}
	}

	/// <summary>
	/// Тип значения поля уведомления.
	/// Определено в RFC 3464 часть 2.1.2.
	/// </summary>
	internal static class NotificationFieldValueTypeNames
	{
		/// <summary>Mailbox address.</summary>
		internal static readonly string Rfc822 = "rfc822";

		/// <summary>Status code.</summary>
		internal static readonly string Smtp = "smtp";

		/// <summary>Name of the host.</summary>
		internal static readonly string Dns = "dns";
	}
}
