using System;
using System.Collections;
using System.Collections.Generic;

namespace Novartment.Base.Collections.Linq
{
	/// <summary>
	/// Extension methods for structurally comparable objects.
	/// </summary>
	public static class StructuralEquatable
	{
		/// <summary>
		/// Determines whether two sequences are equal by comparing their elements by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
		/// <param name="first">A structurally comparable sequence to compare to second.</param>
		/// <param name="second">A sequence to compare to the first sequence.</param>
		/// <param name="comparer">A comparer to use to compare elements..</param>
		/// <returns>True if the two source sequences are of equal length and their corresponding elements compare equal according to comparer;
		/// otherwise, False.</returns>
		public static bool SequenceEqual<TSource> (
			this IStructuralEquatable first,
			IEnumerable<TSource> second,
			IEqualityComparer comparer = null)
		{
			if (first == null)
			{
				throw new ArgumentNullException (nameof (first));
			}

			if (second == null)
			{
				throw new ArgumentNullException (nameof (second));
			}

			return first.Equals (second, comparer ?? EqualityComparer<TSource>.Default);
		}
	}
}
