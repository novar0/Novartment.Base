using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// The field of the header of the generic message format defined in RFC 822.
	/// The body of the field is represented in encoded form, used for transmission over network protocols.
	/// </summary>
	public class EncodedHeaderField
	{
		/// <summary>
		/// Initializes a new instance of the EncodedHeaderField class with a specified name and a body.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="body">The body of the field in encoded form, used for transmission over network protocols.</param>
		public EncodedHeaderField (HeaderFieldName name, ReadOnlySpan<byte> body)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			Contract.EndContractBlock ();

			this.Name = name;
			var buf = new byte[body.Length];
			body.CopyTo (buf);
			this.Body = buf;
		}

		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public HeaderFieldName Name { get; }

		/// <summary>
		/// Gets the body of the field.
		/// The body is in encoded form, used for transmission over network protocols.
		/// </summary>
		public ReadOnlyMemory<byte> Body { get; }

		/// <summary>
		/// Returns a string that represents the this object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString ()
		{
			return (this.Body.Length > 0) ? $"{this.Name}: {this.Body.Length} bytes" : $"{this.Name}:";
		}
	}
}
