using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Reflection
{
	/// <summary>
	/// Методы расширения для System.Type.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Проверяет, можно ли атомарно присваивать значения переменным указанного типа.
		/// </summary>
		/// <param name="type">Тип, для которого проверяется, можно ли атомарно присваивать значения переменным.</param>
		/// <returns>True если значения переменным указанного типа можно присваивать атомарно, иначе False.</returns>
		public static bool IsAtomicallyAssignable (this Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			Contract.EndContractBlock ();

			if (!type.IsValueType)
			{
				return true;
			}

			if (type.IsEnum)
			{
				return IsAtomicallyAssignable (Enum.GetUnderlyingType (type));
			}

			if ((type == typeof (bool)) ||
				(type == typeof (byte)) ||
				(type == typeof (sbyte)) ||
				(type == typeof (IntPtr)))
			{
				// тип размером 8 бит и указатель платформы
				return true;
			}

			if ((type == typeof (char)) ||
				(type == typeof (short)) ||
				(type == typeof (ushort)))
			{
				// тип размером 16 бит
				return IntPtr.Size >= 2;
			}

			if ((type == typeof (int)) ||
				(type == typeof (uint)) ||
				(type == typeof (float)))
			{
				// тип размером 32 бит
				return IntPtr.Size >= 4;
			}

			if ((type == typeof (long)) ||
				(type == typeof (ulong)) ||
				(type == typeof (double)) ||
				(type == typeof (DateTime)))
			{
				// тип размером 64 бит
				return IntPtr.Size >= 8;
			}

			return false;
		}
	}
}
