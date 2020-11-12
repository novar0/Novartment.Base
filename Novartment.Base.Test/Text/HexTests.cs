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
			Assert.Equal (0, Hex.ParseByte ('0', '0'));
			Assert.Equal (0xa1, Hex.ParseByte ('a', '1'));
			Assert.Equal (0x9a, Hex.ParseByte ('9', 'A'));
			Assert.Equal (0xbc, Hex.ParseByte ('B', 'C'));
			Assert.Equal (0xff, Hex.ParseByte ('F', 'f'));
		}

		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void ParseArray ()
		{
			var buf = new byte[1024];
			buf[0] = 1;
			buf[1] = 2;
			buf[2] = 3;

			Assert.Equal (0, Hex.ParseArray ("", buf));
			Assert.Equal (1, buf[0]);

			Assert.Equal (1, Hex.ParseArray ("00", buf));
			Assert.Equal (0, buf[0]);
			Assert.Equal (2, buf[1]);

			Assert.Equal (2, Hex.ParseArray ("FFBC", buf));
			Assert.Equal (0xFF, buf[0]);
			Assert.Equal (0xBC, buf[1]);
			Assert.Equal (3, buf[2]);
		}

		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void OctetsUpper ()
		{
			Assert.Equal ("00", Hex.OctetsUpper.Span[0]);
			Assert.Equal ("BC", Hex.OctetsUpper.Span[0xbc]);
			Assert.Equal ("FF", Hex.OctetsUpper.Span[0xff]);
		}

		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void ToHexStringUpper ()
		{
			var buf = new char[1024];
			buf[0] = 'v';
			buf[1] = 'w';
			buf[2] = 'x';
			buf[3] = 'y';
			buf[4] = 'z';

			Assert.Equal (0, Hex.ToHexStringUpper (Array.Empty<byte> (), buf));
			Assert.Equal ('v', buf[0]);

			Assert.Equal (2, Hex.ToHexStringUpper (new byte[] { 0 }, buf));
			Assert.Equal ('0', buf[0]);
			Assert.Equal ('0', buf[1]);
			Assert.Equal ('x', buf[2]);

			Assert.Equal (4, Hex.ToHexStringUpper (new byte[] { 255, 160 }, buf));
			Assert.Equal ('F', buf[0]);
			Assert.Equal ('F', buf[1]);
			Assert.Equal ('A', buf[2]);
			Assert.Equal ('0', buf[3]);
			Assert.Equal ('z', buf[4]);
		}

		[Fact]
		[Trait ("Category", "Text.Hex")]
		public void ParseByte_Exception ()
		{
			Assert.Throws<FormatException> (() => Hex.ParseByte ('1', 'g'));
			Assert.Throws<FormatException> (() => Hex.ParseByte (' ', 'B'));
		}
	}
}
