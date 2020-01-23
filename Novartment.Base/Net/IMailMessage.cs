using System;
using System.Collections.Generic;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// The generic internet message defined in RFC 822.
	/// </summary>
	/// <typeparam name="TMailbox">The type of mailboxes specified in the message properties.</typeparam>
	public interface IMailMessage<out TMailbox> :
		IBinarySerializable
	{
		/// <summary>Gets the origination date of the message.</summary>
		DateTimeOffset? OriginationDate { get; }

		/// <summary>Gets the subject of the message.</summary>
		string Subject { get; }

		/// <summary>Gets the collection of the mailboxes of authors of the message.</summary>
		IReadOnlyList<TMailbox> Originators { get; }

		/// <summary>Gets the collection of the mailboxes of recipients of the message.</summary>
		IReadOnlyList<TMailbox> Recipients { get; }

		/// <summary>Gets the content transfer encoding required for message.</summary>
		ContentTransferEncoding TransferEncoding { get; }
	}
}
