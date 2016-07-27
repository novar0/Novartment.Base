using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Методы расширения, имитирующие библиотечный ISet.
	/// </summary>
	// TODO: учесть что в set элементы могут повторятся
	public static class SetExtensions
	{
		/// <summary>
		/// Удаляет все элементы указанной коллекции из текущего множества.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, над которым производится операция.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
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
			Contract.EndContractBlock ();

			foreach (var item in other)
			{
				set.Remove (item);
			}
		}

		/// <summary>
		/// Изменяет текущее множество, чтобы оно содержало только элементы, которые имеются либо в текущем множестве,
		/// либо в указанной коллекции, но не одновременно в них обоих. 
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, над которым производится операция.</param>
		/// <param name="other">Множество для сравнения с текущим множеством.</param>
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
			Contract.EndContractBlock ();

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
		/// Изменяет текущее множество, чтобы оно содержало только элементы, которые также имеются в заданной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, над которым производится операция.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
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
			Contract.EndContractBlock ();

			set.Clear ();
			foreach (var item in other)
			{
				set.Add (item);
			}
		}

		/// <summary>
		/// Изменяет текущий множество, чтобы оно содержало все элементы, которые имеются как в текущем множестве,
		/// так и в указанной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, над которым производится операция.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
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
			Contract.EndContractBlock ();

			foreach (var item in other)
			{
				set.Add (item);
			}
		}

		/// <summary>
		/// Определяет, является ли текущее множество строгим подмножеством заданной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, для которого происходит опеределение.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
		/// <returns> Значение true, если текущее множество является строгим подмножеством объекта other;
		/// в противном случае — значение false.</returns>
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
			Contract.EndContractBlock ();

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
		/// Определяет, является ли множеств подмножеством заданной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, для которого происходит опеределение.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
		/// <returns>Значение true, если текущее множество является подмножеством объекта other; в противном случае — значение false.</returns>
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
			Contract.EndContractBlock ();

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
		/// Определяет, является ли текущее множество строгим надмножеством заданной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, для которого происходит опеределение.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
		/// <returns> Значение true, если текущее множество является строгим надмножеством объекта other; в противном случае — значение false.</returns>
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
			Contract.EndContractBlock ();

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
		/// Определяет, является ли текущее множество надмножеством заданной коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, для которого происходит опеределение.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
		/// <returns> Значение true, если текущее множество является надмножеством объекта other; в противном случае — значение false.</returns>
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
			Contract.EndContractBlock ();

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
		/// Определяет, пересекаются ли текущее множество и указанная коллекция.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, для которого происходит опеределение.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
		/// <returns> Значение true, если в текущем множестве и объекте other имеется по крайней мере один общий элемент;
		/// в противном случае — значение false.</returns>
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
			Contract.EndContractBlock ();

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
		/// Определяет, содержат ли текущее множество и указанная коллекция одни и те же элементы.
		/// </summary>
		/// <typeparam name="T">Тип элементов множества.</typeparam>
		/// <param name="set">Множество, для которого происходит опеределение.</param>
		/// <param name="other">Коллекция для сравнения с текущим множеством.</param>
		/// <returns> Значение true, если текущее множество равно объекту other; в противном случае — значение false.</returns>
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
			Contract.EndContractBlock ();

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
