using System.IO;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class EnumComparerTests
	{
		[Fact]
		[Trait ("Category", "EnumComparer")]
		public void AllBitsChanged ()
		{
			Assert.False (EnumComparer.AllBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden));
			Assert.False (EnumComparer.AllBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Compressed));
			Assert.True (EnumComparer.AllBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed,
				FileAttributes.Archive | FileAttributes.Hidden));
			Assert.True (EnumComparer.AllBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden));
		}

		[Fact]
		[Trait ("Category", "EnumComparer")]
		public void AllBitsSet ()
		{
			Assert.False (EnumComparer.AllBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden));
			Assert.False (EnumComparer.AllBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Compressed));
			Assert.True (EnumComparer.AllBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden));
		}

		[Fact]
		[Trait ("Category", "EnumComparer")]
		public void AllBitsUnset ()
		{
			Assert.False (EnumComparer.AllBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden));
			Assert.False (EnumComparer.AllBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed,
				FileAttributes.Archive));
			Assert.True (EnumComparer.AllBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive));
		}

		[Fact]
		[Trait ("Category", "EnumComparer")]
		public void AnyBitsChanged ()
		{
			Assert.False (EnumComparer.AnyBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.ReadOnly));
			Assert.True (EnumComparer.AnyBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden));
			Assert.True (EnumComparer.AnyBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Compressed));
			Assert.True (EnumComparer.AnyBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed,
				FileAttributes.Archive | FileAttributes.Hidden));
			Assert.True (EnumComparer.AnyBitsChanged (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden));
		}

		[Fact]
		[Trait ("Category", "EnumComparer")]
		public void AnyBitsSet ()
		{
			Assert.False (EnumComparer.AnyBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden));
			Assert.False (EnumComparer.AnyBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.ReadOnly));
			Assert.True (EnumComparer.AnyBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed));
			Assert.True (EnumComparer.AnyBitsSet (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive,
				FileAttributes.Archive | FileAttributes.Hidden));
		}

		[Fact]
		[Trait ("Category", "EnumComparer")]
		public void AnyBitsUnset ()
		{
			Assert.False (EnumComparer.AnyBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Hidden));
			Assert.False (EnumComparer.AnyBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.ReadOnly,
				FileAttributes.Archive));
			Assert.True (EnumComparer.AnyBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed,
				FileAttributes.Archive | FileAttributes.Hidden));
			Assert.True (EnumComparer.AnyBitsUnset (
				FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive | FileAttributes.Compressed | FileAttributes.Hidden,
				FileAttributes.Archive));
		}
	}
}
