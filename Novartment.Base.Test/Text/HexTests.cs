using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class HexTests
	{
		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void ParseByte ()
		{
			Assert.Equal (0, Hex.ParseByte ("00"));
			Assert.Equal (0xa1, Hex.ParseByte ("00a1", 2));
			Assert.Equal (0x9a, Hex.ParseByte ('9', 'A'));
			Assert.Equal (0xbc, Hex.ParseByte ("BC"));
			Assert.Equal (0xbc, Hex.ParseByte (" bc", 1));
			Assert.Equal (0xff, Hex.ParseByte ("Ff"));
		}

		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void OctetsUpper ()
		{
			Assert.Equal ("00", Hex.OctetsUpper[0]);
			Assert.Equal ("BC", Hex.OctetsUpper[0xbc]);
			Assert.Equal ("FF", Hex.OctetsUpper[0xff]);
		}

		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void ParseByte_Exception ()
		{
			Assert.Throws<FormatException> (() => Hex.ParseByte ("1g"));
			Assert.Throws<FormatException> (() => Hex.ParseByte (" BC"));
		}
	}
}
