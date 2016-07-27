using System;
using System.Reflection;
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

			var typeInfo = type.GetTypeInfo ();
			if (!typeInfo.IsValueType)
			{
				return true;
			}
			if (typeInfo.IsEnum)
			{
				return IsAtomicallyAssignable (Enum.GetUnderlyingType (type));
			}
			if ((type == typeof (System.Boolean)) || // специальный тип, размер может быть различный но всегда приспособлен для атомарного присвоения
				(type == typeof (System.Byte)) || // тип размером 8 бит
				(type == typeof (System.SByte)) || // тип размером 8 бит
				(type == typeof (IntPtr))) // указатель платформы
			{
				return true;
			}
			if ((type == typeof (System.Char)) || // тип размером 16 бит
				(type == typeof (System.Int16)) || // тип размером 16 бит
				(type == typeof (System.UInt16))) // тип размером 16 бит
			{
				return (IntPtr.Size >= 2);
			}
			if ((type == typeof (System.Int32)) || // тип размером 32 бит
				(type == typeof (System.UInt32)) || // тип размером 32 бит
				(type == typeof (System.Single))) // тип размером 32 бит
			{
				return (IntPtr.Size >= 4);
			}
			if ((type == typeof (System.Int64)) || // тип размером 64 бит
				(type == typeof (System.UInt64)) || // тип размером 64 бит
				(type == typeof (System.Double)) || // тип размером 64 бит
				(type == typeof (System.DateTime))) // тип размером 64 бит
			{
				return (IntPtr.Size >= 8);
			}
			return false;
		}
	}
}
