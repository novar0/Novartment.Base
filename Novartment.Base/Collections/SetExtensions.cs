using System;
using System.Collections.Generic;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Extension methods for the IAdjustableFiniteSet interface
	/// that mimic the ISet library interface.
	/// </summary>
	// TODO: учесть что в set элементы могут повторятся
	public static class SetExtensions
	{
		/// <summary>
		/// Removes all elements in the specified collection from the current set.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set on which the operation is performed.</param>
		/// <param name="other">The collection of items to remove from the set.</param>
		public static void ExceptWith<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			foreach (var item in other)
			{
				set.Remove (item);
			}
		}

		/// <summary>
		/// Modifies the current set so that it contains only elements that are present either
		/// in the current set or in the specified collection, but not both.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set on which the operation is performed.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		public static void SymmetricExceptWith<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			foreach (var item in other)
			{
				var setContainsItem = set.Contains (item);
				if (setContainsItem)
				{
					set.Remove (item);
				}
				else
				{
					set.Add (item);
				}
			}
		}

		/// <summary>
		/// Modifies the current set so that it contains only elements that are also in a
		/// specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set on which the operation is performed.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		public static void IntersectWith<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			set.Clear ();
			foreach (var item in other)
			{
				set.Add (item);
			}
		}

		/// <summary>
		/// Modifies the current set so that it contains all elements that are present in
		/// the current set, in the specified collection, or in both.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set on which the operation is performed.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		public static void UnionWith<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			foreach (var item in other)
			{
				set.Add (item);
			}
		}

		/// <summary>
		/// Determines whether the current set is a proper (strict) subset of a specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set for which the definition occurs.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		/// <returns>True if the current collection is a proper subset of other; otherwise, False.</returns>
		public static bool IsProperSubsetOf<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			int otherCount = 0;
			int containsCount = 0;
			foreach (var item in other)
			{
				var setContainsItem = set.Contains (item);
				if (setContainsItem)
				{
					containsCount++;
					if ((containsCount >= set.Count) && (otherCount > set.Count))
					{
						return true;
					}
				}

				otherCount++;
			}

			return false;
		}

		/// <summary>
		/// Determines whether a set is a subset of a specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set for which the definition occurs.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		/// <returns>True if the current set is a subset of other; otherwise, False.</returns>
		public static bool IsSubsetOf<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			int containsCount = 0;
			foreach (var item in other)
			{
				var setContainsItem = set.Contains (item);
				if (setContainsItem)
				{
					containsCount++;
					if (containsCount >= set.Count)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		///  Determines whether the current set is a proper (strict) superset of a specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set for which the definition occurs.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		/// <returns>True if the current set is a proper superset of other; otherwise, False.</returns>
		public static bool IsProperSupersetOf<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			int otherCount = 0;
			foreach (var item in other)
			{
				otherCount++;
				if (otherCount >= set.Count)
				{
					return false;
				}

				var setContainsItem = set.Contains (item);
				if (!setContainsItem)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the current set is a superset of a specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set for which the definition occurs.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		/// <returns>True if the current set is a superset of other; otherwise, False.</returns>
		public static bool IsSupersetOf<T> (this IReadOnlyFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			foreach (var item in other)
			{
				var setContainsItem = set.Contains (item);
				if (!setContainsItem)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the current set overlaps with the specified collection.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set for which the definition occurs.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		/// <returns>True if the current set and other share at least one common element; otherwise, False.</returns>
		public static bool Overlaps<T> (this IReadOnlyFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			foreach (var item in other)
			{
				var setContainsItem = set.Contains (item);
				if (setContainsItem)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the current set and the specified collection contain the same elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="set">The set for which the definition occurs.</param>
		/// <param name="other">The collection to compare to the current set.</param>
		/// <returns>True if the current set is equal to other; otherwise, False.</returns>
		public static bool SetEquals<T> (this IAdjustableFiniteSet<T> set, IEnumerable<T> other)
		{
			if (set == null)
			{
				throw new ArgumentNullException (nameof (set));
			}

			if (other == null)
			{
				throw new ArgumentNullException (nameof (other));
			}

			int otherCount = 0;
			int containsCount = 0;
			foreach (var item in other)
			{
				var setContainsItem = set.Contains (item);
				if (setContainsItem)
				{
					containsCount++;
				}

				otherCount++;
				if (otherCount > set.Count)
				{
					return false;
				}
			}

			return containsCount == set.Count;
		}
	}
}
