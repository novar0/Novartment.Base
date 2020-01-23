using System;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// The parameter of the field of the header of the generic message format defined in RFC 822.
	/// </summary>
	public class HeaderFieldParameter :
		IValueHolder<string>,
		IEquatable<HeaderFieldParameter>
	{
		private string _value;

		/// <summary>
		/// Initializes a new instance of the HeaderFieldParameter class with a specified name and a value.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		public HeaderFieldParameter (string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if ((name.Length < 1) ||
				!AsciiCharSet.IsAllOfClass(name, AsciiCharClasses.Token))
			{
				throw new ArgumentOutOfRangeException(nameof(name));
			}

			Contract.EndContractBlock();

			this.Name = name;
			_value = value;
		}

		/// <summary>
		/// Gets the name of the parameter.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the value of the parameter.
		/// </summary>
		public string Value
		{
			get => _value;
			set { _value = value; }
		}

		/// <summary>
		/// Returns a value that indicates whether two HeaderFieldParameter objects are equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two HeaderFieldParameter objects are equal; otherwise, False.</returns>
		public static bool operator ==(HeaderFieldParameter first, HeaderFieldParameter second)
		{
			return first is null ?
				second is null :
				first.Equals(second);
		}

		/// <summary>
		/// Returns a value that indicates whether two HeaderFieldParameter objects are not equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two HeaderFieldParameter objects are not equal; otherwise, False.</returns>
		public static bool operator !=(HeaderFieldParameter first, HeaderFieldParameter second)
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
			return this.Name + "=" + _value;
		}

		/// <summary>
		/// Returns the hash code for this object.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode ()
		{
#if NETSTANDARD2_0
			return StringComparer.OrdinalIgnoreCase.GetHashCode (this.Name) ^ (_value?.GetHashCode () ?? 0);
#else
			return StringComparer.OrdinalIgnoreCase.GetHashCode (this.Name) ^ (_value?.GetHashCode (StringComparison.Ordinal) ?? 0);
#endif
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as HeaderFieldParameter;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public bool Equals (HeaderFieldParameter other)
		{
			if (other == null)
			{
				return false;
			}

			return
				string.Equals (this.Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (_value, other._value, StringComparison.Ordinal);
		}
	}
}
