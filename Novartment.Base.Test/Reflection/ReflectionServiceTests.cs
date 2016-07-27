using System;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using Novartment.Base.Reflection;
using Xunit;

namespace Novartment.Base.Test
{
	public class Mock5<T1, T2>
	{
		public T1 Prop1 { get; set; }
		public T2 Prop2 { get; set; }
	}

	public enum TestEnum
	{
		[DefaultValueAttribute ("Test Value 1")]
		TestVal1,
		TestVal2,
		[DefaultValueAttribute ("Test Value Три")]
		TestVal3
	}

	[DefaultValue (typeof (DateTime), "Много не связанных тестов")] // этот аттрибут не удалять, он используется тестом
	public class ReflectionServiceTests
	{
		[Fact, Trait ("Category", "ReflectionService")]
		public void GetAssemblyVersion ()
		{
			Assert.Equal ("1.1.15037.9256", ReflectionService.GetAssemblyVersion (this.GetType ().GetTypeInfo ().Assembly));
		}

		[Fact, Trait ("Category", "ReflectionService")]
		public void GetFormattedFullName ()
		{
			Assert.Equal ("Novartment.Base.Test.ReflectionServiceTests", 
				ReflectionService.GetFormattedFullName (this.GetType ()));
			Assert.Equal ("Novartment.Base.Test.Mock5`2", 
				ReflectionService.GetFormattedFullName (typeof (Mock5<int, string>).GetGenericTypeDefinition ()));
			Assert.Equal ("Novartment.Base.Test.Mock5`2<System.Int32, System.String>",
				ReflectionService.GetFormattedFullName (typeof (Mock5<int, string>)));
			Assert.Equal ("Novartment.Base.Test.Mock5`2<System.Int32, System.Tuple`2<System.Int32, System.String>>",
				ReflectionService.GetFormattedFullName (typeof (Mock5<int, Tuple<int, string>>)));
		}

		[Fact, Trait ("Category", "ReflectionService")]
		[DefaultValue ("test")] // этот аттрибут не удалять, он используется тестом
		public void GetAttributeArguments ()
		{
			var args = ReflectionService.GetAttributeArguments<DefaultValueAttribute> (new Action (GetAttributeArguments).GetMethodInfo ());
			Assert.Equal (1, args.Count);
			Assert.IsType<String> (args[0].Value);
			Assert.Null (args[0].Name);
			Assert.Equal ("test", (string)args[0].Value);

			args = ReflectionService.GetAttributeArguments<DefaultValueAttribute> (this.GetType ().GetTypeInfo ());
			Assert.Equal (2, args.Count);

			Assert.IsAssignableFrom<Type> (args[0].Value);
			Assert.Null (args[0].Name);
			Assert.Equal (typeof (DateTime), (Type)args[0].Value);

			Assert.IsType<String> (args[1].Value);
			Assert.Null (args[1].Name);
			Assert.Equal ("Много не связанных тестов", (string)args[1].Value);
		}

		[Fact, Trait ("Category", "ReflectionService")]
		public void GetAttributeArgumentsEnum ()
		{
			Assert.Equal ("Test Value 1", ReflectionService.GetAttributeArguments<DefaultValueAttribute> (TestEnum.TestVal1)[0].Value);
			Assert.Equal ("Test Value Три", ReflectionService.GetAttributeArguments<DefaultValueAttribute> (TestEnum.TestVal3)[0].Value);
			Assert.Equal (0, ReflectionService.GetAttributeArguments<DefaultValueAttribute> (FileAccess.ReadWrite).Count);
		}
	}
}
