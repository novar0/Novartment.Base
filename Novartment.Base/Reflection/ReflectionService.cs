using System;
using System.Reflection;
using Novartment.Base.Collections.Linq;
using static System.Linq.Enumerable;

namespace Novartment.Base.Reflection
{
	/// <summary>
	/// The reflection information service.
	/// </summary>
	public static class ReflectionService
	{
		/// <summary>Gets a string representation of a type that includes a list of generic parameters.</summary>
		/// <param name="type">The type to get the name for.</param>
		/// <returns>The string representation of a type</returns>
		public static string GetDisplayName (Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			if (type.IsConstructedGenericType)
			{
				var arguments = type.GenericTypeArguments.Select (GetDisplayName);
				return type.GetGenericTypeDefinition ().FullName + "<" + string.Join (", ", arguments) + ">";
			}

			return type.FullName;
		}

		/// <summary>Creates string representation of the version of the specfified Assembly.</summary>
		/// <param name="assembly">The Assembly for which string representation of the version will be created.</param>
		/// <returns>The string representation of the version of the specfified Assembly.</returns>
		public static string GetDisplayVersion (Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException (nameof (assembly));
			}

			// у текущей сборки ищем атрибут AssemblyFileVersionAttribute
			// если не удалось найти, то берём просто версию сборки
			// это будет не точная версия, поскольку версию сборки меняют только при больших разрушающих изменениях
			var attrs = assembly.CustomAttributes;
			var attr = attrs.FirstOrDefault (item => item.AttributeType == typeof (AssemblyFileVersionAttribute));
			if (attr != null)
			{
				var args = attr.ConstructorArguments;
				if (args.Count == 1)
				{
					return (string)args[0].Value;
				}
			}

			return assembly.GetName ().Version.ToString ();
		}

		/// <summary>
		/// Checks whether values can be assigned atomically to variables of the specified type.
		/// </summary>
		/// <param name="type">
		/// The type for which it checks whether it is possible to atomically assign values to variables.
		/// </param>
		/// <returns>True if values for variables of the specified type can be assigned atomically; otherwise False.</returns>
		public static bool IsAtomicallyAssignable (Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

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
