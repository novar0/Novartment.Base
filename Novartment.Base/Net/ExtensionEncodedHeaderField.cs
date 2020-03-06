using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// The field of the header of the generic message format defined in RFC 822,
	/// for which an extension is required (non-standard field).
	/// </summary>
	public class ExtensionHeaderField : EncodedHeaderField
	{
		/// <summary>
		/// Initializes a new instance of the ExtensionHeaderField class with a specified name and a value.
		/// </summary>
		/// <param name="extensionName">The name of the field.</param>
		/// <param name="body">The body of the field in encoded form, used for transmission over network protocols.</param>
		public ExtensionHeaderField (string extensionName, ReadOnlySpan<byte> body)
			: base (HeaderFieldName.Extension, body)
		{
			if (extensionName == null)
			{
				throw new ArgumentNullException (nameof (extensionName));
			}

			Contract.EndContractBlock ();

			this.ExtensionName = extensionName;
		}

		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public string ExtensionName { get; }
	}
}
