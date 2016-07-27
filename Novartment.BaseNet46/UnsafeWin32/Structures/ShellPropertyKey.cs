using System;
using System.Runtime.InteropServices;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Defines a unique key for a Shell Property
	/// </summary>
	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	internal struct ShellPropertyKey :
		IEquatable<ShellPropertyKey>
	{
		private readonly Guid _formatId;
		private readonly Int32 _propertyId;

/*
		/// <summary>
		/// A unique GUID for the property
		/// </summary>
		internal Guid FormatId
		{
			get
			{
				return _formatId;
			}
		}

		/// <summary>
		/// Property identifier (PID)
		/// </summary>
		internal Int32 PropertyId
		{
			get
			{
				return _propertyId;
			}
		}
*/

		/// <summary>
		/// ShellPropertyKey Constructor
		/// </summary>
		/// <param name="formatId">A unique GUID for the property</param>
		/// <param name="propertyId">Property identifier (PID)</param>
		internal ShellPropertyKey (Guid formatId, Int32 propertyId)
		{
			_formatId = formatId;
			_propertyId = propertyId;
		}

		/// <summary>
		/// Returns whether this object is equal to another. This is vital for performance of value types.
		/// </summary>
		/// <param name="other">The object to compare against.</param>
		/// <returns>Equality result.</returns>
		public bool Equals (ShellPropertyKey other)
		{
			return other.Equals ((object)this);
		}

		/// <summary>
		/// Returns the hash code of the object. This is vital for performance of value types.
		/// </summary>
		/// <returns>Hash code of specified object.</returns>
		public override int GetHashCode ()
		{
			return _formatId.GetHashCode () ^ _propertyId;
		}

		/// <summary>
		/// Returns whether this object is equal to another. This is vital for performance of value types.
		/// </summary>
		/// <param name="obj">The object to compare against.</param>
		/// <returns>Equality result.</returns>
		public override bool Equals (object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (!(obj is ShellPropertyKey))
			{
				return false;
			}

			var other = (ShellPropertyKey)obj;
			return other._formatId.Equals (_formatId) && (other._propertyId == _propertyId);
		}

		/// <summary>
		/// Implements the == (equality) operator.
		/// </summary>
		/// <param name="propKey1">First property key to compare.</param>
		/// <param name="propKey2">Second property key to compare.</param>
		/// <returns>true if object a equals object b. false otherwise.</returns>
		public static bool operator == (ShellPropertyKey propKey1, ShellPropertyKey propKey2)
		{
			return propKey1.Equals (propKey2);
		}

		/// <summary>
		/// Implements the != (inequality) operator.
		/// </summary>
		/// <param name="propKey1">First property key to compare</param>
		/// <param name="propKey2">Second property key to compare.</param>
		/// <returns>true if object a does not equal object b. false otherwise.</returns>
		public static bool operator != (ShellPropertyKey propKey1, ShellPropertyKey propKey2)
		{
			return !propKey1.Equals (propKey2);
		}

		/// <summary>
		/// Override ToString() to provide a user friendly string representation
		/// </summary>
		/// <returns>String representing the property key</returns>
		public override string ToString ()
		{
			return FormattableString.Invariant ($"{_formatId.ToString ("B")}, {_propertyId}");
		}
	}
}
