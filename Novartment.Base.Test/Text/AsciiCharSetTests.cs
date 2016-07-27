using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class AsciiCharSetTests
	{
		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void AllOfClass ()
		{
			Assert.True (AsciiCharSet.IsAllOfClass ("89235460", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAllOfClass ("8923 460", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAllOfClass ("🔓8923🔧460", AsciiCharClasses.Digit));
		}
		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void AnyOfClass ()
		{
			Assert.True (AsciiCharSet.IsAnyOfClass ("klOI H2 JtfKU+_*(^#f f", AsciiCharClasses.Digit));
			Assert.True (AsciiCharSet.IsAnyOfClass ("🔒👎klOI H👍2 JtfKU+_*(^#f f", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAnyOfClass ("", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAnyOfClass ("klOI H JtfKU+_*(^#f f🔨", AsciiCharClasses.Digit));
		}

		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void GetBytes ()
		{
			var buf = new byte[100];
			buf[0] = 123;
			AsciiCharSet.GetBytes (" !09Az}~", 0, " !09Az}~".Length, buf, 1);
			Assert.Equal (buf[0], 123);
			Assert.Equal (buf[1], 32);
			Assert.Equal (buf[2], 33);
			Assert.Equal (buf[3], 48);
			Assert.Equal (buf[4], 57);
			Assert.Equal (buf[5], 65);
			Assert.Equal (buf[6], 122);
			Assert.Equal (buf[7], 125);
			Assert.Equal (buf[8], 126);
		}

		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void GetBytes_Exception ()
		{
			var buf = new byte[100];
			Assert.Throws<FormatException> (() => AsciiCharSet.GetBytes (" Ж ", 0, " Ж ".Length, buf, 0));
		}

		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void GetString ()
		{
			var buf = new byte[] { 123, 32, 33, 48, 57, 65, 122, 125, 126 };
			Assert.Equal ("", AsciiCharSet.GetString (buf, 0, 0));
			Assert.Equal ("{ !09Az}~", AsciiCharSet.GetString (buf, 0, buf.Length));
			Assert.Equal ("z}~", AsciiCharSet.GetString (buf, 6, 3));
		}

		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void GetString_Exception ()
		{
			var buf = new byte[] { 33, 133, 32 };
			Assert.Throws<FormatException> (() => AsciiCharSet.GetString (buf, 0, buf.Length));
		}

		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void Quote ()
		{
			Assert.Equal ("\"source\"", AsciiCharSet.Quote ("source"));
			Assert.Equal (
				"\"\t !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\"",
				AsciiCharSet.Quote ("\t !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~"));
			Assert.Equal ("\"source\taa\\\\bb \\\"net\\\"\"", AsciiCharSet.Quote ("source\taa\\bb \"net\""));
		}

		[Fact, Trait ("Category", "Text.AsciiCharSet")]
		public void Quote_Exception ()
		{
			Assert.Throws<FormatException> (() => AsciiCharSet.Quote ("sourceЖ"));
		}

		[Fact, Trait ("Category", "AsciiCharSet")]
		public void IsValidInternetDomainName ()
		{
			// legal
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("serv01"));
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("www.asus.com"));
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("forum.ru-board.com"));
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("www.asus!$#&%+.com"));
			// illegal
			Assert.False (AsciiCharSet.IsValidInternetDomainName (""));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("."));
			Assert.False (AsciiCharSet.IsValidInternetDomainName (".."));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.asus."));
			Assert.False (AsciiCharSet.IsValidInternetDomainName (".asus.com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www..com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.фсюс.com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.asus(2).com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.asus:com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.asus;com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.asus[a]com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www,asus.com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.asus.com<3>"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www@asus.com"));
			Assert.False (AsciiCharSet.IsValidInternetDomainName ("www.\"asus\".com"));
		}
	}
}
