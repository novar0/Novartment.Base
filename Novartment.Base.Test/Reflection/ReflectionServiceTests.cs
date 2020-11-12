using System;
using System.ComponentModel;
using Novartment.Base.Reflection;
using Xunit;

namespace Novartment.Base.Test
{
	[DefaultValue (typeof (DateTime), "Много не связанных тестов")] // этот аттрибут не удалять, он используется тестом
	public sealed class ReflectionServiceTests
	{
		[Fact]
		[Trait ("Category", "ReflectionService")]
		public void GetDisplayVersion ()
		{
			var actualVer = ReflectionService.GetDisplayVersion (this.GetType ().Assembly);
			Assert.Equal ("1.1.15037.9256", actualVer);
		}

		[Fact]
		[Trait ("Category", "ReflectionService")]
		public void GetDisplayName ()
		{
			Assert.Equal (
				"Novartment.Base.Test.ReflectionServiceTests",
				ReflectionService.GetDisplayName (this.GetType ()));
			Assert.Equal (
				"Novartment.Base.Test.Mock5`2",
				ReflectionService.GetDisplayName (typeof (Mock5<int, string>).GetGenericTypeDefinition ()));
			Assert.Equal (
				"Novartment.Base.Test.Mock5`2<System.Int32, System.String>",
				ReflectionService.GetDisplayName (typeof (Mock5<int, string>)));
			Assert.Equal (
				"Novartment.Base.Test.Mock5`2<System.Int32, System.Tuple`2<System.Int32, System.String>>",
				ReflectionService.GetDisplayName (typeof (Mock5<int, Tuple<int, string>>)));
		}
	}

	public enum TestEnum
	{
		[DefaultValue ("Test Value 1")]
		TestVal1,
		TestVal2,
		[DefaultValue ("Test Value Три")]
		TestVal3,
	}

	public sealed class Mock5<T1, T2>
	{
		public T1 Prop1 { get; set; }

		public T2 Prop2 { get; set; }
	}
}
