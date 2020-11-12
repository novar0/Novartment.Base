using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class AsciiCharSetTests
	{
		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void AllOfClass ()
		{
			Assert.True (AsciiCharSet.IsAllOfClass ("89235460", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAllOfClass ("8923 460", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAllOfClass ("🔓8923🔧460", AsciiCharClasses.Digit));
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void AnyOfClass ()
		{
			Assert.True (AsciiCharSet.IsAnyOfClass ("klOI H2 JtfKU+_*(^#f f", AsciiCharClasses.Digit));
			Assert.True (AsciiCharSet.IsAnyOfClass ("🔒👎klOI H👍2 JtfKU+_*(^#f f", AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAnyOfClass (string.Empty, AsciiCharClasses.Digit));
			Assert.False (AsciiCharSet.IsAnyOfClass ("klOI H JtfKU+_*(^#f f🔨", AsciiCharClasses.Digit));
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void GetBytes ()
		{
			var buf = new byte[100];
			buf[0] = 123;
			AsciiCharSet.GetBytes (" !09Az}~".AsSpan (), buf.AsSpan (1));
			Assert.Equal (123, buf[0]);
			Assert.Equal (32, buf[1]);
			Assert.Equal (33, buf[2]);
			Assert.Equal (48, buf[3]);
			Assert.Equal (57, buf[4]);
			Assert.Equal (65, buf[5]);
			Assert.Equal (122, buf[6]);
			Assert.Equal (125, buf[7]);
			Assert.Equal (126, buf[8]);
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void GetBytes_Exception ()
		{
			var buf = new byte[100];
			Assert.Throws<FormatException> (() => AsciiCharSet.GetBytes (" Ж ".AsSpan (), buf));
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void GetString ()
		{
			var buf = new byte[] { 123, 32, 33, 48, 57, 65, 122, 125, 126 };
			Assert.Equal (string.Empty, AsciiCharSet.GetString (default));
			Assert.Equal ("{ !09Az}~", AsciiCharSet.GetString (buf.AsSpan ()));
			Assert.Equal ("z}~", AsciiCharSet.GetString (buf.AsSpan (6, 3)));
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void GetString_Exception ()
		{
			var buf = new byte[] { 33, 133, 32 };
			Assert.Throws<FormatException> (() => AsciiCharSet.GetString (buf.AsSpan (0, buf.Length)));
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void Quote ()
		{
			Assert.Equal ("\"source\"", AsciiCharSet.Quote ("source"));

			Assert.Equal (
				"\"\t !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\"",
				AsciiCharSet.Quote ("\t !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~"));
			Assert.Equal ("\"source\taa\\\\bb \\\"net\\\"\"", AsciiCharSet.Quote ("source\taa\\bb \"net\""));

			Assert.Throws<FormatException> (() => AsciiCharSet.Quote ("sourceЖ"));
		}

		[Fact]
		[Trait ("Category", "Text.AsciiCharSet")]
		public void QuoteSpan ()
		{
			var buf = new char[1000];
			var size = AsciiCharSet.Quote ("source", buf);
			Assert.Equal ("\"source\"", new string (buf.AsSpan (0, size)));

			size = AsciiCharSet.Quote ("\t !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~", buf);
			Assert.Equal (
				"\"\t !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\"",
				new string (buf.AsSpan (0, size)));

			size = AsciiCharSet.Quote ("source\taa\\bb \"net\"", buf);
			Assert.Equal ("\"source\taa\\\\bb \\\"net\\\"\"", new string (buf.AsSpan (0, size)));

			Assert.Throws<FormatException> (() => AsciiCharSet.Quote ("sourceЖ", buf));
		}

		[Fact]
		[Trait ("Category", "AsciiCharSet")]
		public void IsValidInternetDomainName ()
		{
			// legal
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("serv01"));
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("www.asus.com"));
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("forum.ru-board.com"));
			Assert.True (AsciiCharSet.IsValidInternetDomainName ("www.asus!$#&%+.com"));

			// illegal
			Assert.False (AsciiCharSet.IsValidInternetDomainName (string.Empty));
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
