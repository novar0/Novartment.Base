using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// The field of the header of the generic message format defined in RFC 822.
	/// </summary>
	public class HeaderField :
		IEquatable<HeaderField>
	{
		/// <summary>
		/// Initializes a new instance of the HeaderField class with a specified name and a value.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="body">The body of the field in encoded form, used for transmission over network protocols.</param>
		public HeaderField (HeaderFieldName name, ReadOnlySpan<byte> body)
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
		/// Returns a value that indicates whether two HeaderField objects are equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two HeaderField objects are equal; otherwise, False.</returns>
		public static bool operator == (HeaderField first, HeaderField second)
		{
			return first is null ?
				second is null :
				first.Equals(second);
		}

		/// <summary>
		/// Returns a value that indicates whether two HeaderField objects are not equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two HeaderField objects are not equal; otherwise, False.</returns>
		public static bool operator != (HeaderField first, HeaderField second)
		{
			return !(first is null ?
				second is null :
				first.Equals(second));
		}

		/// <summary>
		/// Returns a string that represents the this object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString ()
		{
			return (this.Body.Length > 0) ? $"{this.Name}: {this.Body}" : $"{this.Name}:";
		}

		/// <summary>
		/// Returns the hash code for this object.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode ()
		{
			return this.Name.GetHashCode () ^ this.Body.GetHashCode ();
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as HeaderField;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public bool Equals (HeaderField other)
		{
			if (other == null)
			{
				return false;
			}

			return (this.Name == other.Name) && this.Body.Equals (other.Body);
		}
	}
}
