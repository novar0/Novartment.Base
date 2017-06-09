using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Linq;
using static System.Linq.Enumerable;

namespace Novartment.Base.Reflection
{
	/// <summary>
	/// Сервис получения информации рефлексии.
	/// </summary>
	public static class ReflectionService
	{
		/// <summary>Получает строковое название типа, включающее список дженерик-параметров.</summary>
		/// <param name="type">Тип, для которого нужно получить имя.</param>
		/// <returns>Строковое описание типа.</returns>
		public static string GetFormattedFullName (Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			Contract.EndContractBlock ();

			if (type.IsConstructedGenericType)
			{
				var arguments = type.GenericTypeArguments.Select (GetFormattedFullName);
				return type.GetGenericTypeDefinition ().FullName + "<" + string.Join (", ", arguments) + ">";
			}

			return type.FullName;
		}

		/// <summary>Получает версию указанной сборки System.Reflection.Assembly.</summary>
		/// <param name="assembly">Сборка, для которой надо получить версию.</param>
		/// <returns>Строковое представление версии.</returns>
		public static string GetAssemblyVersion (Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException (nameof (assembly));
			}

			Contract.EndContractBlock ();

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
		/// Получает список аргументов конструктора атрибута члена класса.
		/// </summary>
		/// <typeparam name="T">Тип атрибута.</typeparam>
		/// <param name="member">Член класса.</param>
		/// <returns>Коллекция аргументов конструктора атрибута.</returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1004:GenericMethodsShouldProvideTypeParameter",
			Justification = "Type parameter can not be provided.")]
		public static IReadOnlyList<AttributeArgument> GetAttributeArguments<T> (MemberInfo member)
			where T : Attribute
		{
			if (member == null)
			{
				throw new ArgumentNullException (nameof (member));
			}

			Contract.EndContractBlock ();

			var attrs = member.CustomAttributes;
			var attr = attrs.SingleOrDefault (item => item.AttributeType == typeof (T));
			if (attr == null)
			{
				return ReadOnlyList.Empty<AttributeArgument> ();
			}

			return attr.ConstructorArguments.AsReadOnlyList ()
				.Select (item => new AttributeArgument (null, item.Value))
				.Concat (attr.NamedArguments.AsReadOnlyList ().Select (item =>
					new AttributeArgument (item.MemberName, item.TypedValue.Value)));
		}

		/// <summary>
		/// Получает список аргументов конструктора атрибута.
		/// </summary>
		/// <typeparam name="T">Тип атрибута.</typeparam>
		/// <param name="value">Значение перечисления, для которого надо получить аргумент конструктора атрибута.</param>
		/// <returns>Коллекция аргументов конструктора атрибута.</returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1062:Validate arguments of public methods",
			Justification = "Method correctly works with ANY value.")]
		[SuppressMessage (
			"Microsoft.Design",
			"CA1004:GenericMethodsShouldProvideTypeParameter",
			Justification = "Type parameter can not be provided.")]
		public static IReadOnlyList<AttributeArgument> GetAttributeArguments<T> (Enum value)
			where T : Attribute
		{
			var name = value.ToString ();
			var fieldInfo = value.GetType ().GetTypeInfo ().GetDeclaredField (name);
			return (fieldInfo == null) ?
				ReadOnlyList.Empty<AttributeArgument> () :
				GetAttributeArguments<T> (fieldInfo);
		}
	}
}
