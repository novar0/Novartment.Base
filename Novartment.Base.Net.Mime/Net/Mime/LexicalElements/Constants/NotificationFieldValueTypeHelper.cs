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
		internal static bool TryParse (ReadOnlySpan<char> source, out NotificationFieldValueKind result)
		{
			var isRfc822 = NotificationFieldValueTypeNames.Rfc822.AsSpan ().SequenceEqual (source);
			if (isRfc822)
			{
				result = NotificationFieldValueKind.Mailbox;
				return true;
			}

			var isSmtp = NotificationFieldValueTypeNames.Smtp.AsSpan ().SequenceEqual (source);
			if (isSmtp)
			{
				result = NotificationFieldValueKind.Status;
				return true;
			}

			var isDns = NotificationFieldValueTypeNames.Dns.AsSpan ().SequenceEqual (source);
			if (isDns)
			{
				result = NotificationFieldValueKind.Host;
				return true;
			}

			result = NotificationFieldValueKind.Unspecified;
			return false;
		}
	}
}
